using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PacketInfo;

enum e_GroupNotify
{
    legend = 0,
    hero_rare = 1,
    hero_normal = 2,
    monster_normal = 3,
    monster_trash = 4,
}

public class HeroesInfoMenu : MenuBase
{
    public PrefabManager heroItemPrefabManager, partyItemPrefabManager, pieceItemPrefabManager, bookItemPrefabManager;
    public UIScrollView m_HeroScroll, m_PartyScroll, m_PieceScroll, m_BookScroll;
    public UIGrid m_HeroGrid, m_PartyGrid, m_PieceGrid;
    public UIToggle m_HeroToggle, m_PartyToggle, m_PieceToggle;
    public UISprite m_HeroNotify, m_PartyNotify, m_PieceNotify;
    public HeroSortInfo m_SortInfo;
    public UILabel m_HeroCount;

    public GameObject m_Help;
    
    public GameObject m_HeroSaleBtn, m_HeroSaleCancelBtn, m_HeroSalePanel, m_BookPanel;
    public UILabel m_LabelSaleBtn;
    public UILabel m_LabelSalePrice;
    public UILabel m_LabelSaleCount;

    public List<UISprite> m_NotifyIcons;

    List<HeroesInfoItem> m_SaleSelected = new List<HeroesInfoItem>();
    int m_SalePrice = 0;

    string m_selected_book_id = string.Empty;

    List<CreatureBookGroup> booksitem = new List<CreatureBookGroup>();
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {        
        Init(parms.bBack);
        return true;
    }

    override public void UpdateMenu()
    {
        Init(false);
    }

    void Init(bool bBack)
    {
        m_SortInfo.Init(OnSorted);

        if (bBack == true)
        {
            if (m_HeroToggle.value == true)
                InitHeroItem();
            else if (m_PartyToggle.value == true)
                InitPartyItem();
            else if (m_PieceToggle.value == true)
                InitPieceItem();
        }
        m_HeroNotify.gameObject.SetActive(CreatureManager.Instance.IsNotify);
        m_PieceNotify.gameObject.SetActive(ItemManager.Instance.IsPieceNotify);
        m_PartyNotify.gameObject.SetActive(CreatureBookManager.Instance.IsNotify);
    }

    ////////////////////////////////////////////////////////////////

    void Start()
    {
        if(GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();
        Localization.language = ConfigData.Instance.Language;
    }

    List<Creature> m_Creatures;
    void InitHeroItem()
    {
        m_HeroScroll.transform.parent.gameObject.SetActive(true);
        m_PartyScroll.gameObject.SetActive(false);
        m_PieceScroll.gameObject.SetActive(false);
        m_BookPanel.SetActive(false);

        heroItemPrefabManager.Clear();

        m_Creatures = m_SortInfo.GetSortedCreatures();

        foreach (Creature creature in m_Creatures)
        {
            if (creature.Grade == 0)
                continue;

            HeroesInfoItem item = heroItemPrefabManager.GetNewObject<HeroesInfoItem>(m_HeroGrid.transform, Vector3.zero);
            item.Init(creature, OnSelectCharacter);
        }

        int count = m_HeroGrid.transform.childCount;
        while (count++ % m_HeroGrid.maxPerLine != 0)
        {
            HeroesInfoItem item = heroItemPrefabManager.GetNewObject<HeroesInfoItem>(m_HeroGrid.transform, Vector3.zero);
            item.Init(null, null);
        }

        m_HeroGrid.Reposition();
        m_HeroScroll.ResetPosition();

        RefreshInfo();
        UpdateSalePrice(0);
    }

    void InitPieceItem()
    {
        m_HeroScroll.transform.parent.gameObject.SetActive(false);
        m_PartyScroll.gameObject.SetActive(false);
        m_BookPanel.SetActive(false);
        m_PieceScroll.gameObject.SetActive(true);

        pieceItemPrefabManager.Clear();

        var pieces = ItemManager.Instance.Items.Where(i => i.IsSoulStone && i.Count > 0);
        if (pieces.Count() > 0)
        {
            foreach (Item soulstone in pieces)
            {
                PieceHeroItem item = pieceItemPrefabManager.GetNewObject<PieceHeroItem>(m_PieceGrid.transform, Vector3.zero);
                item.Init(soulstone);
            }

            int count = pieceItemPrefabManager.Count;
            while (count++ % m_PieceGrid.maxPerLine != 0)
            {
                PieceHeroItem item = pieceItemPrefabManager.GetNewObject<PieceHeroItem>(m_PieceGrid.transform, Vector3.zero);
                item.Init(null);
            }

            m_PieceGrid.Reposition();
            m_PieceGrid.enabled = false;
            m_PieceScroll.ResetPosition();

            foreach (var transform in m_PieceGrid.GetChildList())
            {
                var soulstone = transform.gameObject.GetComponent<PieceHeroItem>();
                if (soulstone.m_SoulStone != null)
                {
                    soulstone.CheckNotify();
                }
            }

            m_Help.gameObject.SetActive(false);
        }
        else
            m_Help.gameObject.SetActive(true);
    }

    void InitBook()
    {   
        m_HeroScroll.transform.parent.gameObject.SetActive(false);
        m_PieceScroll.gameObject.SetActive(false);
        m_BookPanel.SetActive(true);

        booksitem.ForEach(i => i.m_CreaturePrefabManager.Destroy());
        booksitem = new List<CreatureBookGroup>();
        bookItemPrefabManager.Clear();


        if (string.IsNullOrEmpty(m_selected_book_id) == true)
        {
            m_selected_book_id = CreatureBookInfoManager.Instance.Values.First().ID;
            if (string.IsNullOrEmpty(m_selected_book_id) == true)
                return;
        }

        float last_obj_height = 0f;// = GetBookGroupPosition(creature_item);

        foreach (var group in CreatureBookInfoManager.Instance.GetInfoByID(m_selected_book_id).Groups)
        {              
            var sub_item = bookItemPrefabManager.GetNewObject<CreatureBookGroup>(m_BookScroll.transform, Vector3.zero);
            sub_item.Init(group);
            sub_item.transform.localPosition = new Vector3(0, last_obj_height);

            booksitem.Add(sub_item);
            last_obj_height += GetBookGroupPosition(sub_item);
        }
        RefreshBookBtnNotify();
        m_BookScroll.ResetPosition();
    }
    void InitPartyItem()
    {
        InitBook();
    }

    public void OnValueChanged(UIToggle toggle)
    {
        if (toggle.value == true)
        {            
            SetSaleMode(false);
            m_Help.gameObject.SetActive(false);
            switch (toggle.name)
            {
                case "MenuHero":
                    InitHeroItem();
                    break;

                case "MenuParty":
                    InitPartyItem();
                    break;

                case "MenuPiece":
                    InitPieceItem();
                    break;
                case "MenuBook":
                    InitBook();
                    break;
            }
        }
    }

    void OnSorted()
    {
        m_Creatures = m_SortInfo.GetSortedCreatures();

        List<HeroesInfoItem> items = new List<HeroesInfoItem>();
        for (int i = 0; i < m_HeroGrid.transform.childCount; ++i)
            items.Add(m_HeroGrid.transform.GetChild(i).gameObject.GetComponent<HeroesInfoItem>());
        m_HeroGrid.transform.DetachChildren();

        foreach (Creature creature in m_Creatures)
        {
            if (creature.Grade == 0)
                continue;

            HeroesInfoItem item = items.Find(i => i.Creature == creature);
            item.transform.SetParent(m_HeroGrid.transform, true);
            items.Remove(item);
        }

        foreach (var item in items)
        {
            item.transform.SetParent(m_HeroGrid.transform, true);
        }

        m_HeroGrid.Reposition();
        m_HeroScroll.ResetPosition();
    }

    public void OnClickSlotBuy()
    {
        Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Creature, (PopupSlotBuy.OnOkDeleage)OnSlotBuy);
    }

    void RefreshInfo()
    {
        m_HeroCount.text = Localization.Format("HeroCount", CreatureManager.Instance.Creatures.Count, Network.PlayerInfo.creature_count_max);
    }

    void OnSlotBuy()
    {
        RefreshInfo();
    }

    void OnSelectCharacter(HeroesInfoItem item)
    {
        if (m_HeroSalePanel.activeSelf == false)
        {
            MenuParams menu = new MenuParams();
            menu.AddParam("Creature", item.Creature);
            menu.AddParam("Creatures", m_Creatures);
            GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
        }
        else
        {
            if (item.m_SpriteSelected.gameObject.activeSelf == true)
            {
                item.SetSelect(false);
                m_SaleSelected.Remove(item);
                UpdateSalePrice(m_SalePrice - item.Creature.SalePrice);
            }
            else
            {//select
                if (item.Creature.IsLock)
                {
                    Popup.Instance.ShowMessageKey("HeroSaleConfirmTeamLocked");
                    return;
                }
                if (TeamDataManager.Instance.CheckTeam(item.Creature.Idx, PacketEnums.pe_Team.PVP_Defense) == true)
                {
                    m_SelectConfirmItem = item;
                    Popup.Instance.ShowMessageKey("HeroSaleConfirmTeamPVPDefense");
                    return;
                }
                if (TeamDataManager.Instance.CheckTeam(item.Creature.Idx) != PacketEnums.pe_Team.Invalid)
                {
                    m_SelectConfirmItem = item;
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "HeroSaleConfirmTeam");
                    return;
                }
                if (item.Creature.Enchant > 0)
                {
                    m_SelectConfirmItem = item;
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEnchanted");
                    return;
                }
                if (item.Creature.Armor.EnchantLevel > 0 || item.Creature.Weapon.EnchantLevel > 0)
                {
                    m_SelectConfirmItem = item;
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEquipEnchanted");
                    return;
                }
                if (item.Creature.Runes.Count > 0)
                {
                    m_SelectConfirmItem = item;
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEquipRune");
                    return;
                }

                item.SetSelect(true);
                m_SaleSelected.Add(item);
                UpdateSalePrice(m_SalePrice + item.Creature.SalePrice);
            }
        }
    }

    HeroesInfoItem m_SelectConfirmItem;
    void OnSelectSaleConfirm(bool is_confirm)
    {
        if (is_confirm)
        {
            m_SelectConfirmItem.SetSelect(true);
            m_SaleSelected.Add(m_SelectConfirmItem);
            UpdateSalePrice(m_SalePrice + m_SelectConfirmItem.Creature.SalePrice);
        }
    }

    public void OnClickSale()
    {
        if (m_HeroSalePanel.activeSelf == true)
        {
            OnClickSaleCancel();
        }
        else
        {
            SetSaleMode(true);
            InitHeroItem();
        }
    }

    void UpdateSalePrice(int price)
    {
        m_SalePrice = price;
        m_LabelSalePrice.text = Localization.Format("GoodsFormat", m_SalePrice);
        m_LabelSaleCount.text = Localization.Format("SelectedSaleCreatures", m_SaleSelected.Count);
    }

    public void OnClickSaleCancel()
    {        
        SetSaleMode(false);

        InitHeroItem();
    }
    public void OnClickSaleConfirm()
    {
        if(m_SaleSelected.Count == 0)
        {
            Tooltip.Instance.ShowMessageKey("SelectCreatureForSale");
            return;
        }
        if (m_SaleSelected.Exists(c => c.Creature.Grade >= 3) == true)
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(CallbackSale), "SaleGrade3CreatureConfirm");
        else
            CallbackSale(true);
    }
    void CallbackSale(bool is_confirm)
    {
        if(is_confirm)
        {
            C2G.CreatureSales packet = new C2G.CreatureSales();
            packet.creature_idxes = m_SaleSelected.Select(c => c.Creature.Idx).ToList();
            packet.creature_grades= m_SaleSelected.Select(c => (long)c.Creature.Grade).ToList();
            //packet.creature_grades[0] = 6;
            Network.GameServer.JsonAsync<C2G.CreatureSales, C2G.CreatureSalesAck>(packet, OnSale);
        }
    }

    void OnSale(C2G.CreatureSales packet, C2G.CreatureSalesAck ack)
    {
        Network.PlayerInfo.AddGoods(ack.add_goods);
        GameMain.Instance.UpdatePlayerInfo();
        Tooltip.Instance.ShowMessageKeyFormat("SaleCreatureResultFormat", ack.add_goods.goods_value);

        m_SaleSelected.ForEach(c =>
        {
            CreatureManager.Instance.Remove(c.Creature.Idx);
            //m_HeroGrid.RemoveChild(c.transform);
            //c.gameObject.SetActive(false);
            heroItemPrefabManager.Free(c.gameObject);
        });

        m_SaleSelected.Clear();
        UpdateSalePrice(0);
        RefreshInfo();

        CreatureManager.Instance.UpdateNotify();
        m_HeroNotify.gameObject.SetActive(CreatureManager.Instance.IsNotify);

        m_HeroGrid.Reposition();
    }

    void SetSaleMode(bool is_salemode)
    {
        m_SaleSelected.Clear();
        m_HeroSaleBtn.SetActive(!is_salemode);
        m_HeroSaleCancelBtn.SetActive(is_salemode);
        m_HeroSalePanel.SetActive(is_salemode);
        if (is_salemode)
            m_HeroScroll.GetComponent<UIPanel>().baseClipRegion = new Vector4(0f, 40f, 900f, 440f);
        else
            m_HeroScroll.GetComponent<UIPanel>().baseClipRegion = new Vector4(0f, 0f, 900f, 520f);

        m_SortInfo.SetSaleMode(is_salemode);
    }

    public void OnClickBookBtn(GameObject obj)
    {
        m_selected_book_id = obj.name;
        InitBook();
    }

    void RefreshBookBtnNotify()
    {
        m_NotifyIcons.ForEach(b => b.gameObject.SetActive(false));
        foreach (var item in CreatureBookManager.Instance.GetNotifyData().GroupBy(d => CreatureBookInfoManager.Instance.GetListIDByCreatureIdn(d.creature_idn)))
        {
            m_NotifyIcons[(int)(e_GroupNotify)System.Enum.Parse(typeof(e_GroupNotify), item.Key)].gameObject.SetActive(true);
        }

        m_PartyNotify.gameObject.SetActive(CreatureBookManager.Instance.IsNotify);
        
    }

    int GetLineCount(int grid_count, int per_line_count)
    {
        return (grid_count + per_line_count - 1) / per_line_count;
    }

    int GetBookGroupPosition(CreatureBookGroup before_group)
    {
        return -(int)((GetLineCount(before_group.grid_count, before_group.m_CreatureGrid.maxPerLine) * before_group.m_CreatureGrid.cellHeight) + before_group.m_CreatureGrid.transform.localPosition.y + 20);
    }
}
