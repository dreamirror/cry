using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketInfo;
using PacketEnums;

public class HeroRune : MenuBase
{
    public GameObject PopupRunePrefab;
    public PrefabManager runeItemPrefabManager;
    public UIScrollView m_RuneScroll;
    public UIGrid m_HeroRuneGrid, m_RuneGrid;
    public RuneSortInfo m_SortInfo;

    public Transform m_PopupIndicator, m_PopupEquippedIndicator;
    PopupRune m_PopupEquippedRune, m_PopupRune;

    public UILabel m_LabelRuneCount, m_LabelTitle, m_LabelTotalSellPrice;

    public GameObject m_BatchSellPopup, m_BatchSellBtn, m_BatchSellCancelBtn;

    public List<RuneItem> m_UpgradeMaterials;
    public RuneItem m_UpgradeRune;
    public UIToggle m_ToggleEquip, m_ToggleUpgrade;
    public UILabel m_UpgradePriceLabel;

    public GameObject m_RuneUpgradeEventTab;
    public GameObject m_RuneUpgradeEvent;
    Creature m_Creature;
    bool is_sellmode;
    
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        //if(m_Creature == null || parms.bBack == false)
        m_Creature = parms.GetObject<Creature>();
        InitBatchLabel();
        if (parms.bBack == true && CreatureManager.Instance.Contains(m_Creature) == false)
            return false;

        Init(parms.bBack);
        return true;
    }

    override public void UpdateMenu()
    {
        Init(false);
    }

    void UpdateEvent()
    {
        m_RuneUpgradeEvent.SetActive(EventHottimeManager.Instance.IsRuneUpgradeEvent);
        m_RuneUpgradeEventTab.SetActive(EventHottimeManager.Instance.IsRuneUpgradeEvent);

    }
    void Init(bool bBack)
    {
        m_SortInfo.Init(OnSorted);

        InitInfo();
        InitRuneItem();
        ReloadBlockCheckRunes();

        UpdateEvent();
    }

    ////////////////////////////////////////////////////////////////

    void Start()
    {
        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();
        Localization.language = ConfigData.Instance.Language;
    }

    public override bool Uninit(bool bBack)
    {
        runeItemPrefabManager.Clear();
        return true;
    }

    int m_RuneCount = 0;
    List<RuneItem> m_Runes = null;
    List<RuneItem> m_HeroRunes = null;
    List<RuneItem> m_selected_runes = null;
    void InitRuneItem()
    {
        runeItemPrefabManager.Clear();

        m_HeroRunes = new List<RuneItem>();
        for (int i = 0; i < 10; ++i)
        {
            var rune = runeItemPrefabManager.GetNewObject<RuneItem>(m_HeroRuneGrid.transform, Vector3.zero);
            if (i < m_Creature.Runes.Count)
                rune.Init(m_Creature.Runes[i], i >= m_Creature.RuneSlotCount, OnClickEquippedRune, i);
            else
                rune.Init(null, i >= m_Creature.RuneSlotCount, null, i);
            m_HeroRunes.Add(rune);
        }
        m_HeroRuneGrid.Reposition();

        m_Runes = new List<RuneItem>();

        var runes = m_SortInfo.GetSortedRunes();
        m_RuneCount = runes.Count;

        foreach (Rune rune in runes)
        {
            RuneItem item = runeItemPrefabManager.GetNewObject<RuneItem>(m_RuneGrid.transform, Vector3.zero);
            item.Init(rune, false, OnClickRune);

            m_Runes.Add(item);
        }

        for (int i = 0; i < 5; ++i)
        {
            var item = runeItemPrefabManager.GetNewObject<RuneItem>(m_RuneGrid.transform, Vector3.zero);
            item.InitDummy();
        }

        m_RuneGrid.Reposition();
        m_RuneScroll.ResetPosition();

        m_LabelTitle.text = string.Format("{0} {1} {2}", m_Creature.GetLevelText(), m_Creature.Info.Name, m_Creature.GetEnchantText());
        RefreshInfo();
    }

    void InitUpgrade()
    {

    }

    void OnClickRune(RuneItem rune)
    {
        if (is_sellmode == true)
        {
            int selected_node = m_selected_runes.FindIndex(r => r.Rune.RuneIdx == rune.Rune.RuneIdx);
            if (selected_node < 0)
            {
                rune.SetSelected(true);
                m_LabelTotalSellPrice.text = (int.Parse(m_LabelTotalSellPrice.text) + rune.Rune.Info.GradeInfo.SalePrice).ToString();
                m_selected_runes.Add(rune);
            }
            else
            {
                m_selected_runes[selected_node].SetSelected(false);
                m_LabelTotalSellPrice.text = (int.Parse(m_LabelTotalSellPrice.text) - rune.Rune.Info.GradeInfo.SalePrice).ToString();
                m_selected_runes.RemoveAt(selected_node);
            }
        }
        else
        {
            if (m_ToggleUpgrade.value == true)
            {                
                if (rune.Rune.Info.Grade >= 6)
                {
                    Tooltip.Instance.ShowMessageKey("RuneMaxGrade");
                    return;
                }
                int selected_node = m_selected_runes.FindIndex(r => r.Rune.RuneIdx == rune.Rune.RuneIdx);
                if (selected_node < 0)
                {
                    if (rune.m_SpriteBlock.gameObject.activeSelf == true)
                    {
                        Tooltip.Instance.ShowMessageKey("RuneUpgradeTip");
                        return;
                    }

                    if (m_UpgradeRune.Rune == null)
                    {
                        m_UpgradeRune.RefreshUpgradeRune(rune.Rune, true);
                        int upgrade_cost = rune.Rune.Info.GradeInfo.UpgradeCost;
                        var event_info = EventHottimeManager.Instance.GetInfoByID("rune_upgrade_discount");
                        if (event_info != null)
                            upgrade_cost = (int)(upgrade_cost* event_info.Percent);

                        m_UpgradePriceLabel.text = upgrade_cost.ToString();

                        m_Runes.ForEach(item => item.SetBlockSprite(item.Rune.Info.Grade != rune.Rune.Info.Grade));
                    }

                    foreach (RuneItem item in m_UpgradeMaterials)
                    {
                        if (item.Rune == null)
                        {
                            item.Init(rune.Rune, false, OnDeselectMaterial);
                            rune.SetSelected(true);
                            m_selected_runes.Add(rune);
                            return;
                        }
                    }
                    Tooltip.Instance.ShowMessageKey("RuneUpgradeFull");
                }
                else
                {
                    OnDeselectMaterial(rune);
                }   
            }
            else
                InitPopupRune(rune.Rune);

            ReloadBlockCheckRunes();
        }
    }

    void OnDeselectMaterial(RuneItem rune)
    {
        int selected_node = m_selected_runes.FindIndex(r => r.Rune.RuneIdx == rune.Rune.RuneIdx);
        if (selected_node < 0)
            return;

        m_UpgradeMaterials.Find(r => r.Rune != null && r.Rune.RuneIdx == rune.Rune.RuneIdx).Init(null, false, null);

        m_selected_runes[selected_node].SetSelected(false);
        m_selected_runes.RemoveAt(selected_node);


        foreach (RuneItem item in m_UpgradeMaterials)
            if (item.Rune != null)
                return;
        ReloadBlockCheckRunes();
    }

    void OnClickEquippedRune(RuneItem rune)
    {
        InitPopupEquippedRune(rune.Rune);
    }

    void OnSorted()
    {
        var runes = m_SortInfo.GetSortedRunes();

        List<RuneItem> items = new List<RuneItem>();
        for (int i = 0; i < m_RuneGrid.transform.childCount; ++i)
            items.Add(m_RuneGrid.transform.GetChild(i).gameObject.GetComponent<RuneItem>());
        m_RuneGrid.transform.DetachChildren();

        foreach (Rune rune in runes)
        {
            RuneItem item = items.Find(i => i.Rune == rune);
            if (item == null)
                continue;
            item.Init(item.Rune, item.m_lock_toggle.value, OnClickRune);
            m_RuneGrid.AddChild(item.transform, false);

            items.Remove(item);
        }

        foreach (var item in items)
        {
            m_RuneGrid.AddChild(item.transform, false);
        }

        m_RuneGrid.Reposition();
        m_RuneScroll.ResetPosition();
    }

    void InitInfo()
    {
        RefreshInfo();
    }

    void RefreshInfo()
    {
        m_LabelRuneCount.text = Localization.Format("RuneCount", m_RuneCount, Network.PlayerInfo.rune_count_max);
    }

    public void OnClickHelp()
    {
        if(m_ToggleEquip.value == true)
            Tooltip.Instance.ShowHelp(Localization.Get("Help_HeroRune_Title"), Localization.Get("Help_HeroRune"));
        else
            Tooltip.Instance.ShowHelp(Localization.Get("Help_HeroRune_Title"), Localization.Get("Help_UpgradeRune"));
    }

    public void OnClickSlotBuy()
    {
        Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Rune, (PopupSlotBuy.OnOkDeleage)OnSlotBuy);
    }

    void OnSlotBuy()
    {
        RefreshInfo();
    }

    void InitPopupRune(Rune rune)
    {
        if (m_PopupRune == null)
        {
            m_PopupRune = NGUITools.AddChild(m_PopupIndicator.gameObject, PopupRunePrefab).GetComponent<PopupRune>();
        }
        m_PopupRune.gameObject.SetActive(true);
        m_PopupRune.Init(rune, false, OnPopupClick);
    }

    void InitPopupEquippedRune(Rune rune)
    {
        if (m_PopupEquippedRune == null)
        {
            m_PopupEquippedRune = NGUITools.AddChild(m_PopupEquippedIndicator.gameObject, PopupRunePrefab).GetComponent<PopupRune>();
        }
        m_PopupEquippedRune.gameObject.SetActive(true);
        m_PopupEquippedRune.Init(rune, true, OnPopupClick);
    }

    void OnPopupClick(Rune rune, string name)
    {
        switch (name)
        {
            case "btn_sale":
                Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(rune, StoreConfirmParam.RuneType.Sale, new pd_GoodsData(pe_GoodsType.token_gold, rune.Info.GradeInfo.SalePrice), OnSaleConfirm, false));
                break;

            case "btn_equip":                
                if (rune.Info.CheckEquipType(m_Creature.Info.AttackType) == false)
                {
                    Tooltip.Instance.ShowMessageKeyFormat("RuneEquipTypeError", Localization.Get(rune.Info.EquipType.ToString()));
                }
                else if (m_HeroRunes.Count(r => r.Rune != null) >= m_Creature.RuneSlotCount)
                    Tooltip.Instance.ShowMessageKey("RuneSlotLimit");
                else
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnEquipConfirm), "RuneEquipConfirm");
                break;

            case "btn_unequip":
                int price = rune.Info.GradeInfo.UnequipPrice;
                var event_info = EventHottimeManager.Instance.GetInfoByID("rune_unequip_discount");
                if (event_info != null)
                    price = (int)(price * event_info.Percent);

                if (Network.PlayerInfo.GetGoodsValue(rune.Info.GradeInfo.UnequipPriceType) < price)
                {
                    Popup.Instance.Show(ePopupMode.MoveStore, PacketInfo.pe_GoodsType.token_gem);
                    return;
                }
                Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(rune, StoreConfirmParam.RuneType.Unequip, new pd_GoodsData(rune.Info.GradeInfo.UnequipPriceType, price), OnUnequipConfirm, event_info != null));
                break;

            case "btn_enchant":
                if (rune.Level == rune.Info.GradeInfo.MaxLevel)
                {
                    Tooltip.Instance.ShowMessageKey("MaxRuneLevel");
                    return;
                }
                Popup.Instance.Show(ePopupMode.RuneEnchant, rune, new PopupRuneEnchant.OnOkDeleagate(OnEnchantSuccess));
                InitBatchLabel();
                break;
        }
    }

    void OnSaleConfirm(StoreConfirmParam param)
    {
        C2G.RunesSale packet = new C2G.RunesSale();
        packet.rune_idxes = new List<long>();
        packet.rune_grades = new List<long>();
        packet.rune_idxes.Add(param.rune_item.RuneIdx);
        packet.rune_grades.Add(param.rune_item.Info.Grade);
        
        Network.GameServer.JsonAsync<C2G.RunesSale, C2G.RunesSaleAck>(packet, OnRunesSaleHandler);
    }

    void OnEquipConfirm(bool confirm)
    {
        if (confirm == false)
            return;

        C2G.RuneEquip packet = new C2G.RuneEquip();
        packet.rune_idx = m_PopupRune.Rune.RuneIdx;
        packet.rune_id = m_PopupRune.Rune.Info.ID;
        packet.creature_idx = m_Creature.Idx;
        packet.creature_id = m_Creature.Info.ID;
        if (Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = Network.PlayerInfo.tutorial_state;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.rune_equip = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnRuneEquipTutorial);
        }
        else
            Network.GameServer.JsonAsync<C2G.RuneEquip, NetworkCore.AckDefault>(packet, OnRuneEquip);
    }

    void OnUnequipConfirm(StoreConfirmParam param)
    {
        C2G.RuneUnequip packet = new C2G.RuneUnequip();
        packet.rune_idx = param.rune_item.RuneIdx;
        packet.rune_grade = param.rune_item.Info.Grade;
        Network.GameServer.JsonAsync<C2G.RuneUnequip, C2G.RuneUnequipAck>(packet, OnRuneUnequip);
    }

    void OnEnchantSuccess(Rune rune, bool is_success)
    {
        GameMain.Instance.UpdatePlayerInfo();

        if (is_success == false)
            return;

        if (m_Runes.Exists(r => r.Rune.RuneIdx == rune.RuneIdx))
        {
            m_Runes.Find(r => r.Rune.RuneIdx == rune.RuneIdx).RefreshRuneInfo(rune);
            return;
        }

        if (m_HeroRunes.Exists(r => r.Rune != null && r.Rune.RuneIdx == rune.RuneIdx))
        {
            m_HeroRunes.Find(r => r.Rune.RuneIdx == rune.RuneIdx).RefreshRuneInfo(rune);
            return;
        }
    }

    void RemoveRune(RuneItem item)
    {
        --m_RuneCount;

        m_Runes.Remove(item);
        //item.InitDummy();
        item.gameObject.SetActive(false);
        //item.transform.SetParent(null, false);
        //item.transform.SetParent(m_RuneGrid.transform, false);

        if(m_PopupRune != null)
            m_PopupRune.gameObject.SetActive(false);

        m_RuneGrid.Reposition();
        //OnSorted();
    }

    void RemoveHeroRune(RuneItem item)
    {
        for (int i = 0; i < 10; ++i)
        {
            if (i < m_Creature.Runes.Count)
                m_HeroRunes[i].Init(m_Creature.Runes[i], i >= m_Creature.RuneSlotCount, OnClickEquippedRune);
            else
                m_HeroRunes[i].Init(null, i >= m_Creature.RuneSlotCount, OnClickEquippedRune);
        }
        m_PopupEquippedRune.gameObject.SetActive(false);
    }

    void OnRuneEquipTutorial(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnRuneEquip(packet.rune_equip, null);
    }
    void OnRuneEquip(C2G.RuneEquip packet, NetworkCore.AckDefault ack)
    {
        RuneManager.Instance.EquipRune(packet.rune_idx, packet.creature_idx);
        RuneItem item = m_Runes.Find(r => r.Rune.RuneIdx == packet.rune_idx);

        int find_index = m_HeroRunes.FindIndex(r => r.Rune == null);
        m_HeroRunes[find_index].Init(item.Rune, find_index >= m_Creature.RuneSlotCount, OnClickEquippedRune);

        RemoveRune(item);
        RefreshInfo();

        if(Tutorial.Instance.Completed == false)
            Tutorial.Instance.AfterNetworking();
        Tooltip.Instance.ShowMessageKey("RuneEquipSuccess");
    }

    void OnRuneUnequip(C2G.RuneUnequip packet, C2G.RuneUnequipAck ack)
    {
        RuneManager.Instance.UnEquipRune(packet.rune_idx);
        RuneItem item = m_HeroRunes.Find(r => r.Rune != null && r.Rune.RuneIdx == packet.rune_idx);
        Rune rune = item.Rune;
        

        ++m_RuneCount;
        RemoveHeroRune(item);

        RuneItem new_item = runeItemPrefabManager.GetNewObject<RuneItem>(m_RuneGrid.transform, Vector3.zero);
        new_item.Init(rune, false, OnClickRune);
        m_Runes.Add(new_item);

        OnSorted();

        Network.PlayerInfo.UseGoods(ack.use_goods);
        GameMain.Instance.UpdatePlayerInfo();
        RefreshInfo();
    }

    public void OnClickBatchSellConfirm()
    {
        C2G.RunesSale packet = new C2G.RunesSale();
        packet.rune_idxes = new List<long>();
        packet.rune_grades = new List<long>();
        foreach (RuneItem item in m_selected_runes)
        {
            packet.rune_grades.Add(item.Rune.Info.Grade);
            packet.rune_idxes.Add(item.Rune.RuneIdx);
        }

        if (packet.rune_idxes.Count > 0)
        {
            Network.GameServer.JsonAsync<C2G.RunesSale, C2G.RunesSaleAck>(packet, OnRunesSaleHandler);
        }
        else
            InitBatchLabel();
    }

    void OnRunesSaleHandler(C2G.RunesSale send, C2G.RunesSaleAck recv)
    {
        foreach (long rune_idx in send.rune_idxes)
        {
            RuneManager.Instance.RemoveRune(rune_idx);
            RuneItem item = m_Runes.Find(r => r.Rune.RuneIdx == rune_idx);
            if (item == null)
            {
                item = m_HeroRunes.Find(r => r.Rune.RuneIdx == rune_idx);
                if (item == null)
                    continue;
            }

            RemoveRune(item);
        }
        m_selected_runes.Clear();
        Network.PlayerInfo.AddGoods(recv.add_goods);
        GameMain.Instance.UpdatePlayerInfo();
        RefreshInfo();
        //InitRuneItem();        
        InitBatchLabel();

        Tooltip.Instance.ShowMessageKey("RuneSaleSuccess");
    }

    public void OnClickBatchSellCancel()
    {
        CancelBatchSell();
    }

    public void OnClickBatchSell()
    {
        CancelBatchSell();

        if (m_ToggleUpgrade.value == true)
            ReloadBlockCheckRunes();

        if (is_sellmode == false)
        {   
            if (m_PopupRune != null)
                m_PopupRune.gameObject.SetActive(false);
            if (m_PopupEquippedRune != null)
                m_PopupEquippedRune.gameObject.SetActive(false);
            SetBatchSellPanel(true);
            is_sellmode = true;
        }
    }

    public void OnValueChange(UIToggle toggle)
    {
        if (toggle.value == true)
        {
            switch (toggle.name)
            {
                case "Equip":
                    ReloadBlockCheckRunes();
                    break;
                case "Upgrade":
                    if (m_PopupRune != null && m_PopupRune.gameObject.activeSelf == true)
                        m_PopupRune.gameObject.SetActive(false);
                    if (m_PopupEquippedRune != null && m_PopupEquippedRune.gameObject.activeSelf == true)
                        m_PopupEquippedRune.gameObject.SetActive(false);
                    m_UpgradeMaterials.ForEach(item => item.Init(null, false, null));
                    ReloadBlockCheckRunes();
                    break;
            }
        }
        UpdateEvent();
    }

    public void OnClickUpgradeConfirm()
    {
        
        foreach (RuneItem item in m_UpgradeMaterials)
        {
            if (item.Rune == null)
            {
                Tooltip.Instance.ShowMessageKey("RuneUpgradeTip");
                return;
            }
        }

        int upgrade_cost = m_UpgradeMaterials[0].Rune.Info.GradeInfo.UpgradeCost;
        var event_info = EventHottimeManager.Instance.GetInfoByID("rune_upgrade_discount");
        if (event_info != null)
            upgrade_cost = (int)(upgrade_cost * event_info.Percent);

        if (Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold) < m_UpgradeMaterials[0].Rune.Info.GradeInfo.UpgradeCost)
        {            
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }
        Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnUpgradeConfirm), "RuneUpgradeAlert");
    }

    void OnUpgradeConfirm(bool is_ok)
    {
        if (is_ok == false)
            return;
        C2G.RuneUpgrade packet = new C2G.RuneUpgrade();
        packet.material_grade = m_UpgradeMaterials[0].Rune.Info.Grade;
        packet.material_idxes = new List<long>();

        m_UpgradeMaterials.ForEach(item => packet.material_idxes.Add(item.Rune.RuneIdx));

        Network.GameServer.JsonAsync<C2G.RuneUpgrade, C2G.RuneUpgradeAck>(packet, OnRuneUpgradeAckHandler);
    }

    void OnRuneUpgradeAckHandler(C2G.RuneUpgrade send, C2G.RuneUpgradeAck recv)
    {
        Network.PlayerInfo.UseGoods(recv.use_goods);
        GameMain.Instance.UpdatePlayerInfo();
        m_UpgradeRune.Init(null, false, null);
        m_UpgradeMaterials.ForEach(item => { RuneManager.Instance.RemoveRune(item.Rune); item.Init(null, false, null); } );

        RuneManager.Instance.Add(recv.rune_info);

        Popup.Instance.Show(ePopupMode.LootItem, new LootItemInfo(recv.rune_info.rune_idn, 1));

        RefreshInfo();
        InitRuneItem();        
        InitBatchLabel();
        ReloadBlockCheckRunes();
    }

    void CancelBatchSell()
    {
        m_selected_runes.Clear();
        m_Runes.ForEach(item => item.SetSelected(false));
        InitBatchLabel();
    }
    
    void InitBatchLabel()
    {
        if (m_PopupRune != null && m_PopupRune.gameObject.activeSelf == true)
            m_PopupRune.gameObject.SetActive(false);

        if (m_PopupEquippedRune != null && m_PopupEquippedRune.gameObject.activeSelf == true)
            m_PopupEquippedRune.gameObject.SetActive(false);
        m_selected_runes = new List<RuneItem>();
        is_sellmode = false;
        SetBatchSellPanel(false);
        m_LabelTotalSellPrice.text = "0";
    }

    void SetBatchSellPanel(bool is_active)
    {
        var panel = m_RuneScroll.GetComponent<UIPanel>();
        if (is_active)
            panel.baseClipRegion = new Vector4(0, 50, panel.baseClipRegion.z, 420);
        else
            panel.baseClipRegion = new Vector4(0, 0, panel.baseClipRegion.z, 520);
        m_BatchSellPopup.SetActive(is_active);
        m_BatchSellCancelBtn.SetActive(is_active);
        m_BatchSellBtn.SetActive(!is_active);
    }

    public void OnClickRuneEquip()
    {
        ReloadBlockCheckRunes();
    }

    void ReloadBlockCheckRunes()
    {
        if (m_ToggleUpgrade.value == true)
        {
            m_selected_runes = new List<RuneItem>();

            foreach (var selected_rune in m_UpgradeMaterials)
            {
                if (selected_rune.Rune != null)
                {
                    RuneItem rune = m_Runes.Find(r => r.Rune.RuneIdx == selected_rune.Rune.RuneIdx);
                    m_selected_runes.Add(rune);
                }
            }

            if (m_selected_runes.Count > 0)
            {
                foreach (var rune in m_Runes)
                {
                    rune.SetSelected(false);

                    if (m_selected_runes.First().Rune.Info.Grade != rune.Rune.Info.Grade)
                        rune.SetBlockSprite(true);
                    else if (m_selected_runes.Exists(r => r.Rune.RuneIdx == rune.Rune.RuneIdx))
                        rune.SetSelected(true);
                }
            }
            else if (m_selected_runes.Count == 0)
            {
                m_Runes.ForEach(i => { i.SetSelected(false); i.SetBlockSprite(false); });
                m_UpgradePriceLabel.text = "-";
            }
        }
        else
        {
            foreach (var rune in m_Runes)
            {
                rune.SetBlockSprite(false);
                rune.SetSelected(false);
            }
        }
    }
}
