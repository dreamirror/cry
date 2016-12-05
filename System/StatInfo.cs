using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

public enum eStatType : byte
{
    MaxHP,

    PhysicAttack,
    MagicAttack,
    Heal,

    PhysicDefense,
    MagicDefense,

    CriticalPower,
    CriticalChance,

    AttackSpeed,
    DecreaseDamagePercent,
    IncreaseDamagePercent,
    ManaGainPercent,
    ManaRegenPercent,
    ManaInit,

    HitRate,
    EvadeRate,

    Attack = 100,
    Defense,

    HP = 200,
    Mana,
}

public class StatTypeInfo
{
    public int HashCode { get; private set; }
    public Dictionary<eStatType, PropertyInfo> Fields { get; private set; }

    public StatTypeInfo()
    {
        Type type = typeof(StatInfo);
        HashCode = type.GetHashCode();

        PropertyInfo[] fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Fields = new Dictionary<eStatType, PropertyInfo>();
        foreach (PropertyInfo info in fields)
        {
            eStatType stat_type = (eStatType)Enum.Parse(typeof(eStatType), info.Name);
            Fields.Add(stat_type, info);
        }
    }

    public void Copy(StatInfo source, StatInfo target)
    {
        foreach (PropertyInfo field in Fields.Values)
        {
            field.SetValue(target, field.GetValue(source, null), null);
        }
    }

    public StatInfo Clone(StatInfo info)
    {
        StatInfo new_info = new StatInfo();
        Copy(info, new_info);
        return new_info;
    }

    public void SetValue(StatInfo info, eStatType type, int value)
    {
        PropertyInfo field;
        if (Fields.TryGetValue(type, out field))
        {
            field.SetValue(info, value, null);
        }
    }

    public int GetValue(StatInfo info, eStatType type)
    {
        PropertyInfo field;
        if (Fields.TryGetValue(type, out field))
        {
            return (int)field.GetValue(info, null);
        }
        return 0;
    }

    public void AddValue(StatInfo info, eStatType type, int add_value)
    {
        PropertyInfo field;
        if (Fields.TryGetValue(type, out field))
        {
            int value = (int)field.GetValue(info, null);
            field.SetValue(info, Math.Max(0, value+add_value), null);
        }
    }

    public void AddRange(StatInfo info, StatInfo source, int multi_value)
    {
        foreach (PropertyInfo field in Fields.Values)
        {
            field.SetValue(info, (int)field.GetValue(info, null) + (int)field.GetValue(source, null) * multi_value, null);
        }
    }

    public void CheckType(eStatType type)
    {
        PropertyInfo field;
        if (Fields.TryGetValue(type, out field) == false)
        {
            UnityEngine.Debug.LogErrorFormat("Invalid Stat Type : {0}", type);
        }
    }
}

public class StatInfo
{
    static readonly public float DefenseTypeRatio = 0.9f;

    static public StatTypeInfo TypeInfo { get; private set; }

    ObscuredInt _MaxHP;
    public int MaxHP
    {
        get { return _MaxHP; }
        set { _MaxHP = value; }
    }

    ObscuredInt _PhysicAttack;
    ObscuredInt _MagicAttack;
    ObscuredInt _Heal;
    public int PhysicAttack { get { return _PhysicAttack; } set { _PhysicAttack = value; } }
    public int MagicAttack { get { return _MagicAttack; } set { _MagicAttack = value; } }
    public int Heal { get { return _Heal; } set { _Heal = value; } }

    ObscuredInt _PhysicDefense;
    ObscuredInt _MagicDefense;
    public int PhysicDefense { get { return _PhysicDefense; } set { _PhysicDefense = value; } }
    public int MagicDefense { get { return _MagicDefense; } set { _MagicDefense = value; } }

    ObscuredInt _CriticalChance;
    ObscuredInt _CriticalPower;
    public int CriticalChance { get { return _CriticalChance; } set { _CriticalChance = value; } }
    public int CriticalPower { get { return _CriticalPower; } set { _CriticalPower = value; } }

    ObscuredInt _AttackSpeed;
    public int AttackSpeed { get { return _AttackSpeed; } set { _AttackSpeed = value; } }

    ObscuredInt _DecreaseDamagePercent;
    ObscuredInt _IncreaseDamagePercent;
    public int DecreaseDamagePercent { get { return _DecreaseDamagePercent; } set { _DecreaseDamagePercent = value; } }
    public int IncreaseDamagePercent { get { return _IncreaseDamagePercent; } set { _IncreaseDamagePercent = value; } }

    ObscuredInt _ManaGainPercent;
    public int ManaGainPercent { get { return _ManaGainPercent; } set { _ManaGainPercent = value; } }
    ObscuredInt _ManaRegenPercent;
    public int ManaRegenPercent { get { return _ManaRegenPercent; } set { _ManaRegenPercent = value; } }
    ObscuredInt _ManaInit;
    public int ManaInit { get { return _ManaInit; } set { _ManaInit = value; } }

    ObscuredInt _HitRate;
    public int HitRate { get { return _HitRate; } set { _HitRate = value; } }
    ObscuredInt _EvadeRate;
    public int EvadeRate { get { return _EvadeRate; } set { _EvadeRate = value; } }


    public int GetAttack()
    {
        int value = PhysicAttack;
        if (MagicAttack > value) value = MagicAttack;
        if (Heal > value) value = Heal;

        return value;
    }

    public int GetDefense()
    {
        int value = PhysicDefense;
        if (MagicDefense > value) value = MagicDefense;

        return value;
    }

    void CheckTypeInfo()
    {
        if (TypeInfo == null || TypeInfo.HashCode != GetType().GetHashCode())
            TypeInfo = new StatTypeInfo();
    }

    void CheckType(eStatType type)
    {
        if (TypeInfo == null || TypeInfo.HashCode != GetType().GetHashCode())
            TypeInfo = new StatTypeInfo();
        TypeInfo.CheckType(type);
    }

    static public eStatType GetStatType(string stat_name)
    {
        return (eStatType)Enum.Parse(typeof(eStatType), stat_name);
    }

    public StatInfo()
    {
        CheckTypeInfo();
    }

    public StatInfo(XmlNode node)
    {
        CheckTypeInfo();

        foreach (XmlAttribute attr in node.Attributes)
        {
            eStatType stat_type = GetStatType(attr.Name);
            CheckType(stat_type);
            TypeInfo.SetValue(this, stat_type, int.Parse(attr.Value));
        }
    }

    public StatInfo(StatInfo info)
    {
        CheckTypeInfo();

        TypeInfo.Copy(info, this);
    }

    public void SetValue(eStatType type, int value)
    {
        CheckType(type);

        TypeInfo.SetValue(this, type, value);
    }

    public int GetValue(eStatType type)
    {
        CheckType(type);

        return TypeInfo.GetValue(this, type);
    }

    public void AddValue(eStatType type, int add_value)
    {
        CheckType(type);

        TypeInfo.AddValue(this, type, add_value);
    }

    public void AddRange(StatInfo info, int multi_value = 1)
    {
        CheckTypeInfo();

        TypeInfo.AddRange(this, info, multi_value);
    }

    public void Multiply(float percent)
    {
        CheckTypeInfo();

        foreach (var field in TypeInfo.Fields)
        {
            if (IsPercentValue(field.Key) == true)
                continue;

            field.Value.SetValue(this, Mathf.RoundToInt(((int)field.Value.GetValue(this, null) * percent)), null);
        }
    }

    static public bool IsPercentValue(eStatType stat_type)
    {
        switch(stat_type)
        {
            case eStatType.CriticalChance:
            case eStatType.CriticalPower:
            case eStatType.AttackSpeed:
            case eStatType.DecreaseDamagePercent:
            case eStatType.IncreaseDamagePercent:
            case eStatType.HitRate:
            case eStatType.EvadeRate:
            case eStatType.ManaInit:
                return true;
        }
        return false;
    }
#if !SH_ASSETBUNDLE
    static public bool IsDefaultValue(SharedData.eAttackType attack_type, eStatType stat_type)
    {
        switch (stat_type)
        {
            case eStatType.PhysicAttack:
                return attack_type == SharedData.eAttackType.physic;

            case eStatType.MagicAttack:
                return attack_type == SharedData.eAttackType.magic;

            case eStatType.Heal:
                return attack_type == SharedData.eAttackType.heal;

            case eStatType.MaxHP:
            case eStatType.PhysicDefense:
            case eStatType.MagicDefense:
            case eStatType.CriticalPower:
                return true;
        }
        return false;
    }

    public string Tooltip(SharedData.eAttackType attack_type)
    {
        string res = "";
        foreach (eStatType type in Enum.GetValues(typeof(eStatType)))
        {
            if ((int)type >= 100) continue;
            int value = GetValue(type);
            if (value == 0 || IsDefaultValue(attack_type, type) == false) continue;

            if (IsPercentValue(type) == true)
            {
                res += string.Format("{0} +{1}%\n", Localization.Get(string.Format("StatType_{0}", type)), value / 100f);
            }
            else
            {
                res += string.Format("{0} +{1}\n", Localization.Get(string.Format("StatType_{0}", type)), value);
            }
        }
        return res.Trim();
    }
    public eStatType GetStatType(int index, SharedData.eAttackType attack_type)
    {
        int idx = 0;
        foreach (eStatType type in Enum.GetValues(typeof(eStatType)))
        {
            if ((int)type >= 100) continue;
            int value = GetValue(type);
            if (value == 0 || IsDefaultValue(attack_type, type) == false) continue;
            if (idx++ == index) return type;
        }
        return eStatType.Attack;
    }
#endif
}

public class StatValue
{
    public eStatType Type { get; private set; }
    public ObscuredInt Value { get; private set; }

    public StatValue(XmlNode node)
    {
        Type = (eStatType)Enum.Parse(typeof(eStatType), node.Attributes["stat_type"].Value);
        Value = int.Parse(node.Attributes["value"].Value);
    }

    public StatValue(eStatType type, int value)
    {
        this.Type = type;
        this.Value = value;
    }
}