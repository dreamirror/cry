using UnityEngine;
using System.Collections;

public class PopupEquipEnchant : PopupBase
{
    public GameObject StuffItemPrefab, HeroEquipPrefab;
    public GameObject m_WeaponIndicator, m_ArmorIndicator;
    HeroEquip m_Weapon, m_Armor;
    Creature m_Creature;

    System.Action OnEquipEnchantCallback = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_Creature = parms[0] as Creature;
        OnEquipEnchantCallback = parms[1] as System.Action;
        Init();
    }

    void Init()
    {
        if (m_Weapon == null) m_Weapon = NGUITools.AddChild(m_WeaponIndicator, HeroEquipPrefab).GetComponent<HeroEquip>();
        if (m_Armor == null) m_Armor = NGUITools.AddChild(m_ArmorIndicator, HeroEquipPrefab).GetComponent<HeroEquip>();
        m_Weapon.Init(m_Creature, m_Creature.Weapon, StuffItemPrefab, EquipEnchantCallback);
        m_Armor.Init(m_Creature, m_Creature.Armor, StuffItemPrefab, EquipEnchantCallback);
    }

    void EquipEnchantCallback()
    {
        m_Weapon.Reinit();
        m_Armor.Reinit();
        if (OnEquipEnchantCallback != null)
            OnEquipEnchantCallback();
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_EquipEnchant_Title"), Localization.Get("Help_EquipEnchant"));
    }
}
