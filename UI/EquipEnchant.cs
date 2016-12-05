using PacketInfo;
using System.Collections.Generic;
using UnityEngine;

//public delegate void OnEquipEnchantDelegate(Equip equip);

public class EquipEnchant : MonoBehaviour
{
    public PrefabManager RewardItemPrefab;
    public PrefabManager EquipItemPrefab;

    public UILabel m_LabelEquipName;
    public GameObject m_StuffIndicator;
    public GameObject m_EquipIndicator, m_EquipEnchantIndicator;
    //public UISprite m_SpriteEquipIcon;
    public UILabel m_LabelEquipEnchantGrade, m_LabelEquipEnchant, m_LabelEquipEnchantChanged;
    public UILabel m_LabelEquipValue1, m_LabelEquipValue1Current, m_LabelEquipValue1Changed;
    public UILabel m_LabelEquipValue2, m_LabelEquipValue2Current, m_LabelEquipValue2Changed;

    public UILabel m_Label;
    public UILabel m_LabelEquipEnchantPrice;

    public UIParticleContainer m_EquipReady;
    //public UIButton m_btnOk;
    public GameObject m_MaxEnchant;

    public UIParticleContainer m_ParticleEnchant;
    public UIPlayTween m_PlayTween;

    //panel
    public GameObject[] m_Texts;

    public GameObject m_BtnEnchant;
    public GameObject m_BtnUpgrade;

    System.Action<Equip> OnEquipEnchantCallback = null;
    Equip m_Equip = null;
    RewardItem m_Stuff = null;
    //////////////////////////////////////////////////////////////////////////////////////
    public void Init(Equip equip, System.Action<Equip> _OnEquipEnchantCallback, bool bPlayTween = false)
    {
        m_Equip = equip;
        OnEquipEnchantCallback = _OnEquipEnchantCallback;
        m_LabelEquipName.text = m_Equip.GetName();

        if (m_Equip.Stuffs.Count > 0)
        {
            RewardItemPrefab.Clear();
            m_Stuff = RewardItemPrefab.GetNewObject<RewardItem>(m_StuffIndicator.transform, Vector3.zero);
            m_Stuff.Init(m_Equip.Stuffs[0]);
            m_Stuff.OnClickItem = OnClickStuff;
        }


        StatInfo stat_info = new StatInfo();
        StatInfo stat_info2 = new StatInfo();
        EquipInfoManager.Instance.AddStats(m_Equip.Info, m_Equip.EnchantLevel, stat_info);

        EquipItemPrefab.Clear();
        if (m_Equip.EnchantLevel < 5)
        {
            var item1 = EquipItemPrefab.GetNewObject<EquipItem>(m_EquipIndicator.transform, Vector3.zero);
            item1.Init(m_Equip);
            Equip equip_enchant = m_Equip.Clone();
            equip_enchant.Enchant();
            var item2 = EquipItemPrefab.GetNewObject<EquipItem>(m_EquipEnchantIndicator.transform, Vector3.zero);
            item2.Init(equip_enchant);

            m_LabelEquipEnchantGrade.text = Localization.Get("EquipEnchantGrade");
            m_LabelEquipEnchant.text = string.Format("+{0}", m_Equip.EnchantLevel);
            m_LabelEquipEnchantChanged.text = string.Format("+{0}", m_Equip.EnchantLevel + 1);
            EquipInfoManager.Instance.AddStats(m_Equip.Info, m_Equip.EnchantLevel + 1, stat_info2);
            m_EquipReady.gameObject.SetActive(m_Equip.IsNotify);

            m_BtnEnchant.SetActive(true);
            m_BtnEnchant.GetComponent<BoxCollider2D>().enabled = true;
            m_BtnUpgrade.SetActive(false);
            m_MaxEnchant.SetActive(false);
        }
        else if(m_Equip.Info.Grade < 6)
        {
            var item1 = EquipItemPrefab.GetNewObject<EquipItem>(m_EquipIndicator.transform, Vector3.zero);
            item1.Init(m_Equip);

            var equip_info = EquipInfoManager.Instance.GetInfoByID(m_Equip.Info.NextEquipID);
            Equip equip_enchant = new Equip(equip_info);
            var item2 = EquipItemPrefab.GetNewObject<EquipItem>(m_EquipEnchantIndicator.transform, Vector3.zero);
            item2.Init(equip_enchant);

            m_LabelEquipEnchantGrade.text = Localization.Get("EquipGrade");
            m_LabelEquipEnchant.text = string.Format("{0}", m_Equip.Info.Grade);
            m_LabelEquipEnchantChanged.text = string.Format("{0}", equip_info.Grade);
            EquipInfoManager.Instance.AddStats(equip_info, 0, stat_info2);
            m_EquipReady.gameObject.SetActive(m_Equip.IsNotify);

            m_BtnEnchant.SetActive(false);
            m_BtnUpgrade.SetActive(true);
            m_BtnUpgrade.GetComponent<BoxCollider2D>().enabled = true;
            m_MaxEnchant.SetActive(false);
        }
        else
        {
            var item1 = EquipItemPrefab.GetNewObject<EquipItem>(m_EquipIndicator.transform, Vector3.zero);
            item1.Init(m_Equip);
            m_LabelEquipEnchantGrade.text = Localization.Get("EquipEnchantGrade");
            m_LabelEquipEnchant.text = string.Format("+{0}", m_Equip.EnchantLevel);
            m_LabelEquipEnchantChanged.text = "";

            m_BtnEnchant.SetActive(false);
            m_BtnUpgrade.SetActive(false);
            m_MaxEnchant.SetActive(true);
        }

        eStatType stat_type = stat_info.GetStatType(0, m_Equip.Info.CategoryInfo.AttackType);
        if ((int)stat_type < 100)
        {
            m_LabelEquipValue1.text = Localization.Get(string.Format("StatType_{0}", stat_type));
            m_LabelEquipValue1Current.text = string.Format("+{0}", stat_info.GetValue(stat_type));
            if (m_MaxEnchant.activeSelf == true)
                m_LabelEquipValue1Changed.text = "";
            else
                m_LabelEquipValue1Changed.text = string.Format("+{0}", stat_info2.GetValue(stat_type));
        }

        stat_type = stat_info.GetStatType(1, m_Equip.Info.CategoryInfo.AttackType);
        if ((int)stat_type < 100)
        {
            m_LabelEquipValue2.text = Localization.Get(string.Format("StatType_{0}", stat_type));
            m_LabelEquipValue2Current.text = string.Format("+{0}", stat_info.GetValue(stat_type));
            if (m_MaxEnchant.activeSelf == true)
                m_LabelEquipValue2Changed.text = "";
            else
                m_LabelEquipValue2Changed.text = string.Format("+{0}", stat_info2.GetValue(stat_type)); 
            m_Texts[2].SetActive(true);
        }
        else
        {
            m_Texts[2].SetActive(false);
        }

        m_LabelEquipEnchantPrice.text = Localization.Format("GoodsFormat", m_Equip.EnchantCost);

        if (bPlayTween)
            m_PlayTween.Play(true);

    }

    public void OnEnchant()
    {
        if (m_Equip.AvailableEnchant() == false)
        {
            //Tooltip.Instance.ShowMessageKey("NotEnoughStuff");
            Popup.Instance.Show(ePopupMode.StuffConfirm, m_Equip.Stuffs[0], "NotEnoughStuff");
            return;
        }

        if (Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold) < m_Equip.EnchantCost)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }

        OnEquipEnchant();
    }

    public void OnClickUpgrade()
    {
        Upgrade();
    }

    public void OnClickEnchant()
    {
        OnEnchant();
    }

    void OnEquipEnchant()
    {
        m_BtnEnchant.GetComponent<BoxCollider2D>().enabled = false;
        C2G.EquipEnchant packet = new C2G.EquipEnchant();
        packet.equip_idx = m_Equip.EquipIdx;
        packet.creature_idx = m_Equip.CreatureIdx;
        packet.equip_id = m_Equip.Info.ID;
        packet.enchant_level = m_Equip.EnchantLevel;

        if (Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
            tutorial_packet.equip_enchant = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialEquipEnchant);
        }
        else
            Network.GameServer.JsonAsync<C2G.EquipEnchant, C2G.EquipEnchantAck>(packet, OnEquipEnchant);
    }
    void OnTutorialEquipEnchant(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnEquipEnchant(packet.equip_enchant, ack.equip_enchant);
        Tutorial.Instance.AfterNetworking();
    }
    void OnEquipEnchant(C2G.EquipEnchant packet, C2G.EquipEnchantAck ack)
    {
        ItemManager.Instance.Reset(ack.item);
        var creature = CreatureManager.Instance.GetInfoByIdx(m_Equip.CreatureIdx);
        CreatureManager.Instance.UpdateEquip(creature, ack.equip);
        Network.PlayerInfo.UseGoods(ack.use_gold);
        //Tooltip.Instance.ShowMessageKey("EquipEnchantSuccess");
        
        GameMain.Instance.UpdateMenu();
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateNotify(false);

        PlayEnchantEffect();
    }

    void Upgrade()
    {
        if (m_Equip.AvailableUpgrade() == false)
        {
            //Tooltip.Instance.ShowMessageKey("NotAvailableEquipUpgrade");
            Popup.Instance.Show(ePopupMode.StuffConfirm, m_Equip.Stuffs[0], "NotAvailableEquipUpgrade");
            return;
        }

        if (Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold) < m_Equip.EnchantCost)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }
        if (m_Equip.Info.NextEquipLevel == short.MaxValue)
        {
            Tooltip.Instance.ShowMessageKey("EquipUpgradeLimit");
            return;
        }

        OnEquipUpgradeCallback();
        //Popup.Instance.Show(ePopupMode.EquipUpgrade, equip, new OnEquipUpgradeDelegate(OnEquipUpgradeCallback));
    }

    void OnEquipUpgradeCallback()
    {
        m_BtnUpgrade.GetComponent<BoxCollider2D>().enabled = false;
        C2G.EquipUpgrade packet = new C2G.EquipUpgrade();
        packet.creature_idx = m_Equip.CreatureIdx;
        packet.equip_idx = m_Equip.EquipIdx;
        packet.equip_grade = m_Equip.Info.Grade;
        packet.equip_id = m_Equip.Info.ID;

        Network.GameServer.JsonAsync<C2G.EquipUpgrade, C2G.EquipUpgradeAck>(packet, OnEquipUpgrade);
    }

    void OnEquipUpgrade(C2G.EquipUpgrade packet, C2G.EquipUpgradeAck ack)
    {
        ItemManager.Instance.Reset(ack.item);
        var creature = CreatureManager.Instance.GetInfoByIdx(m_Equip.CreatureIdx);
        CreatureManager.Instance.UpdateEquip(creature, ack.equip);
        Network.PlayerInfo.UseGoods(ack.use_gold);

        GameMain.Instance.UpdateNotify(false);
        GameMain.Instance.UpdateMenu();
        GameMain.Instance.UpdatePlayerInfo();

        PlayEnchantEffect();
    }

    void PlayEnchantEffect()
    {
        m_ParticleEnchant.Play();
        OnEquipEnchantCallback(m_Equip);
    }

    void OnClickStuff(ItemInfoBase info)
    {
        Popup.Instance.Show(ePopupMode.Stuff, info);
    }

}
