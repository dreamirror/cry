using PacketInfo;
using System.Collections.Generic;
using UnityEngine;

//public delegate void OnEquipEnchantDelegate(Equip equip);

public class PopupEnchantNew : PopupBase
{
    public PrefabManager EquipEnchantPrefab;
    public PrefabManager EquipEnchantMaxPrefab;
    public GameObject m_WeaponIndicator;
    public GameObject m_ArmorIndicator;

    EquipEnchant m_Weapon = null;
    EquipEnchant m_Armor = null;
    System.Action OnEquipEnchantCallback = null;
    //OnEquipEnchantDelegate OnEquipEnchant = null;
    Creature m_Creature = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms == null || parms.Length != 2)
            throw new System.Exception("invalid parms");

        m_Creature = (Creature)parms[0];
        OnEquipEnchantCallback = parms[1] as System.Action;

        EquipEnchantPrefab.Clear();
        EquipEnchantMaxPrefab.Clear();
        m_Weapon = null;
        m_Armor = null;
        Init();
    }
    //////////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
    }
    public void Init(bool bTweenWeapon = false, bool bTweenArmor = false)
    {
        if (m_Creature.Weapon.Info.Grade == 6 && m_Creature.Weapon.EnchantLevel == 5)
        {
            if (m_Weapon != null)
            {
                if (EquipEnchantPrefab.Contains(m_Weapon.gameObject) == true)
                {
                    EquipEnchantPrefab.Free(m_Weapon.gameObject);
                    m_Weapon = null;
                }
            }
            if (m_Weapon == null)
                m_Weapon = EquipEnchantMaxPrefab.GetNewObject<EquipEnchant>(m_WeaponIndicator.transform, Vector3.zero);
        }
        else
        {
            if (m_Weapon != null)
            {
                if (EquipEnchantMaxPrefab.Contains(m_Weapon.gameObject) == true)
                {
                    EquipEnchantMaxPrefab.Free(m_Weapon.gameObject);
                    m_Weapon = null;
                }
            }
            if (m_Weapon == null)
                m_Weapon = EquipEnchantPrefab.GetNewObject<EquipEnchant>(m_WeaponIndicator.transform, Vector3.zero);
        }

        if (m_Creature.Armor.Info.Grade == 6 && m_Creature.Armor.EnchantLevel == 5)
        {
            if (m_Armor != null)
            {
                if (EquipEnchantPrefab.Contains(m_Armor.gameObject) == true)
                {
                    EquipEnchantPrefab.Free(m_Armor.gameObject);
                    m_Armor = null;
                }
            }
            if (m_Armor == null)
                m_Armor = EquipEnchantMaxPrefab.GetNewObject<EquipEnchant>(m_ArmorIndicator.transform, Vector3.zero);
        }
        else
        {
            if (m_Armor != null)
            {
                if (EquipEnchantMaxPrefab.Contains(m_Armor.gameObject) == true)
                {
                    EquipEnchantMaxPrefab.Free(m_Armor.gameObject);
                    m_Armor = null;
                }
            }
            if (m_Armor == null)
                m_Armor = EquipEnchantPrefab.GetNewObject<EquipEnchant>(m_ArmorIndicator.transform, Vector3.zero);
        }

        m_Weapon.Init(m_Creature.Weapon, OnEnchantCallback, bTweenWeapon);
        m_Armor.Init(m_Creature.Armor, OnEnchantCallback, bTweenArmor);
    }

    public void OnCancel()
    {
        m_Weapon = null;
        m_Armor = null;
        base.OnClose();
    }

    void OnEnchantCallback(Equip equip)
    {
//         EquipEnchantPrefab.Clear();
//         EquipEnchantMaxPrefab.Clear();
        Init(m_Creature.Weapon == equip, m_Creature.Armor == equip);
        OnEquipEnchantCallback();
    }
    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_EquipEnchant_Title"), Localization.Get("Help_EquipEnchant"));
    }

}
