using PacketInfo;
using UnityEngine;

public delegate void OnEquipUpgradeDelegate(Equip equip);

public class PopupEquipUpgrade : PopupBase
{
    //////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class UIEquipEvolve
    {
        public UILabel EquipName;
        public UISprite EquipIcon;
        public UILabel Desc;
        public UIGrid Grid;
        public void Init(EquipInfo info, int enchant, int value_color = -1)
        {
            string name = info.GetName();
            if (enchant > 0)
                name += string.Format("+{0}", enchant);
            EquipName.text = name;
            EquipIcon.spriteName = info.IconID;

            Desc.text = info.Tooltip(enchant);
        }
    }
    public UIEquipEvolve[] m_Equips;
    public UILabel m_LabelEquipUpgradePrice;


    OnEquipUpgradeDelegate OnEquipUpgrade = null;
    Equip m_Equip = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms == null || parms.Length != 2)
            throw new System.Exception("invalid parms");

        m_Equip = (Equip)parms[0];
        if (m_Equip.EnchantLevel != 5)
            throw new System.Exception("invalid equip for upgrade");

        OnEquipUpgrade = (OnEquipUpgradeDelegate)parms[1];

        Init();

    }
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
    }

    public void Init()
    {
        m_Equips[0].Init(m_Equip.Info, 5);
        m_Equips[1].Init(EquipInfoManager.Instance.GetInfoByID(m_Equip.Info.NextEquipID), 0);

        m_LabelEquipUpgradePrice.text = Localization.Format("GoodsFormat", m_Equip.EnchantCost);
    }


    public void OnCancel()
    {
        parent.Close();
    }

    public void OnUpgrade()
    {
        if (m_Equip.AvailableUpgrade() == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableEquipUpgrade");
            return;
        }

        if (Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold) < m_Equip.EnchantCost)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }

        OnEquipUpgrade(m_Equip);
        parent.Close();
    }
}
