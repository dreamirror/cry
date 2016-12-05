using PacketInfo;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnEquipEnchantDelegate(Equip equip);

public class PopupEnchant : PopupBase
{
    //////////////////////////////////////////////////////////////////////////
    //main
    public UILabel m_LabelEquipName;
    public UISprite m_SpriteEquipIcon;
    public UILabel m_LabelEquipEnchant , m_LabelEquipEnchantChanged;
    public UILabel m_LabelEquipValue1, m_LabelEquipValue1Current, m_LabelEquipValue1Changed;
    public UILabel m_LabelEquipValue2, m_LabelEquipValue2Current, m_LabelEquipValue2Changed;

    public UILabel m_LabelEquipEnchantPrice;

    public UIButton m_btnOk;

    //panel
    public GameObject[] m_Texts;

    OnEquipEnchantDelegate OnEquipEnchant = null;
    Equip m_Equip = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms == null || parms.Length != 2)
            throw new System.Exception("invalid parms");

        m_Equip = (Equip)parms[0];
        if (m_Equip.EnchantLevel >= 5 || m_Equip.EnchantInfo.Stuffs.Count == 0)
            throw new System.Exception("invalid equip for enchant");

        OnEquipEnchant = (OnEquipEnchantDelegate)parms[1];

        Init();
    }
    //////////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
    }
    public void Init()
    {

        m_LabelEquipName.text = m_Equip.GetName();

        m_SpriteEquipIcon.spriteName = m_Equip.Info.IconID;

        m_LabelEquipEnchant.text = string.Format("+{0}", m_Equip.EnchantLevel);
        m_LabelEquipEnchantChanged.text = string.Format("+{0}", m_Equip.EnchantLevel + 1);

        StatInfo stat_info = new StatInfo();
        StatInfo stat_info2 = new StatInfo();
        EquipInfoManager.Instance.AddStats(m_Equip.Info, m_Equip.EnchantLevel, stat_info);
        EquipInfoManager.Instance.AddStats(m_Equip.Info, m_Equip.EnchantLevel + 1, stat_info2);

        eStatType stat_type = stat_info.GetStatType(0, m_Equip.Info.CategoryInfo.AttackType);
        if ((int)stat_type < 100)
        {
            m_LabelEquipValue1.text = Localization.Get(string.Format("StatType_{0}", stat_type));
            m_LabelEquipValue1Current.text = string.Format("+{0}", stat_info.GetValue(stat_type));
            m_LabelEquipValue1Changed.text = string.Format("+{0}", stat_info2.GetValue(stat_type));
        }

        stat_type = stat_info.GetStatType(1, m_Equip.Info.CategoryInfo.AttackType);
        if ((int)stat_type < 100)
        {
            m_LabelEquipValue2.text = Localization.Get(string.Format("StatType_{0}", stat_type));
            m_LabelEquipValue2Current.text = string.Format("+{0}", stat_info.GetValue(stat_type));
            m_LabelEquipValue2Changed.text = string.Format("+{0}", stat_info2.GetValue(stat_type)); 
            m_Texts[2].SetActive(true);
        }
        else
        {
            m_Texts[2].SetActive(false);
        }

        m_LabelEquipEnchantPrice.text = Localization.Format("GoodsFormat", m_Equip.EnchantCost);
    }

    public void OnCancel()
    {
        parent.Close();
    }

    public void OnEnchant()
    {
        if (m_Equip.AvailableEnchant() == false)
        {
            Tooltip.Instance.ShowMessageKey("NotEnoughStuff");
            return;
        }

        if (Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold) < m_Equip.EnchantCost)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }

        OnEquipEnchant(m_Equip);
        parent.Close();
    }
}
