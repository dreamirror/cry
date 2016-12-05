using MNS;
using PacketInfo;
using SharedData;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class SlotInfo
{
    public int CashDefault { get; private set; }
    public int CashAdd { get; private set; }
    public int CashMax { get; private set; }

    public short AddCount { get; private set; }
    public short CountMax { get; private set; }

    public SlotInfo(XmlNode node)
    {
        CashDefault = int.Parse(node.Attributes["cash_default"].Value);
        CashAdd = int.Parse(node.Attributes["cash_add"].Value);
        CashMax = int.Parse(node.Attributes["cash_max"].Value);

        AddCount = short.Parse(node.Attributes["add_count"].Value);
        CountMax = short.Parse(node.Attributes["count_max"].Value);
    }
}

public class CreatureEnchantInfo
{
    public CreatureEnchantInfo(XmlNode node)
    {
        enchant_point = short.Parse(node.Attributes["enchant_point"].Value);
        enchant_gold = int.Parse(node.Attributes["enchant_gold"].Value);
        mix_gold = int.Parse(node.Attributes["mix_gold"].Value);
        evolve_gold = int.Parse(node.Attributes["evolve_gold"].Value);
    }

    public short enchant_point;
    public int enchant_gold, mix_gold, evolve_gold;
}

public class CreatureGradeEnchant
{
    public CreatureGradeEnchant(XmlNode node)
    {
        stat_percent = float.Parse(node.Attributes["stat_percent"].Value);

        XmlAttribute attr = node.Attributes["over_enchant_gold"];
        if (attr != null)
            over_enchant_gold = int.Parse(node.Attributes["over_enchant_gold"].Value);
        else
            over_enchant_gold = 0;
    }
    public float stat_percent;
    public int over_enchant_gold;
}

public class CreatureGrade
{
    public short level_max;
    public List<CreatureGradeEnchant> enchants;
    public int sale_price;
    public CreatureGrade(XmlNode node)
    {
        level_max = short.Parse(node.Attributes["level_max"].Value);
        sale_price = int.Parse(node.Attributes["sale_price"].Value);

        enchants = new List<CreatureGradeEnchant>();
        foreach (XmlNode child in node.SelectNodes("Enchant"))
        {
            enchants.Add(new CreatureGradeEnchant(child));
        }
    }
}

public class CreatureStatPreset
{
    public string type;
    XmlNode stat_node = null;
    XmlNode statIncrease_node = null;

    public CreatureStatPreset(XmlNode node)
    {
        type = node.Attributes["type"].Value;
        stat_node = node.SelectSingleNode("Stat");
        statIncrease_node = node.SelectSingleNode("IncreaseStatPerLevel");
    }

    public StatInfo Stat(eAttackType AttackType)
    {
        return GetStatInfo(stat_node, AttackType);
    }
    public StatInfo StatIncrease(eAttackType AttackType)
    {
        return GetStatInfo(statIncrease_node, AttackType);
    }

    StatInfo LoadStatPresetInfo(XmlNode node)
    {
        StatInfo info = new StatInfo();

        foreach (XmlAttribute attr in node.Attributes)
        {
            eStatType stat_type = StatInfo.GetStatType(attr.Name);
            int value = int.Parse(attr.Value);
            info.SetValue(stat_type, value);
        }

        return info;
    }

    static public StatInfo GetStatInfo(XmlNode stat_node, eAttackType AttackType)
    {
        StatInfo info = new StatInfo();
        foreach (XmlAttribute attr in stat_node.Attributes)
        {
            eStatType stat_type = StatInfo.GetStatType(attr.Name);
            int value = int.Parse(attr.Value);

            switch (stat_type)
            {
                case eStatType.Attack:
                    info.SetValue(eStatType.PhysicAttack, value);
                    info.SetValue(eStatType.MagicAttack, value);
                    info.SetValue(eStatType.Heal, value);
                    break;

                case eStatType.Defense:
                    switch (AttackType)
                    {
                        case eAttackType.physic:
                            info.SetValue(eStatType.PhysicDefense, value);
                            info.SetValue(eStatType.MagicDefense, (int)(value * StatInfo.DefenseTypeRatio));
                            break;

                        case eAttackType.magic:
                            info.SetValue(eStatType.PhysicDefense, (int)(value * StatInfo.DefenseTypeRatio));
                            info.SetValue(eStatType.MagicDefense, value);
                            break;

                        case eAttackType.heal:
                            info.SetValue(eStatType.PhysicDefense, (int)(value * StatInfo.DefenseTypeRatio));
                            info.SetValue(eStatType.MagicDefense, (int)(value * StatInfo.DefenseTypeRatio));
                            break;
                    }
                    break;

                default:
                    info.SetValue(stat_type, value);
                    break;
            }

        }
        return info;
    }
}

public class CreatureInfoManager : InfoManager<CreatureInfo, CreatureInfo, CreatureInfoManager>
{
    public SlotInfo Slot { get; private set; }
    public List<CreatureEnchantInfo> EnchantInfos { get; private set; }
    public List<CreatureGrade> Grades { get; private set; }
    public List<CreatureStatPreset> StatPresets { get; private set; }

    override protected void PreLoadData(XmlNode node)
    {
        base.PreLoadData(node);

        Slot = new SlotInfo(node.SelectSingleNode("SlotInfo"));

        Grades = new List<CreatureGrade>();
        foreach (XmlNode child in node.SelectSingleNode("CreatureGrade").ChildNodes)
        {
            Grades.Add(new CreatureGrade(child));
        }

        EnchantInfos = new List<CreatureEnchantInfo>();
        foreach (XmlNode child in node.SelectSingleNode("EnchantData").ChildNodes)
        {
            EnchantInfos.Add(new CreatureEnchantInfo(child));
        }

        XmlNode preset_node = node.SelectSingleNode("CreatureStatPreset");
        if (preset_node != null)
        {
            StatPresets = new List<CreatureStatPreset>();
            foreach (XmlNode child in preset_node.ChildNodes)
            {
                StatPresets.Add(new CreatureStatPreset(child));
            }
        }
    }

    public short MixLevelLimit(short grade)
    {
        return LevelLimit("mix_level_limit", grade);
    }

    public short MixBaseLevelLimit(short grade)
    {
        return LevelLimit("mix_base_level_limit", grade);
    }

    public short EvolveLevelLimit(short grade)
    {
        return LevelLimit("evolve_level_limit", grade);
    }

    public short EvolveBaseLevelLimit(short grade)
    {
        return LevelLimit("evolve_base_level_limit", grade);
    }

    short LevelLimit(string config, short grade)
    {
        short level_limit = GameConfig.Get<short>(config);
        return Math.Min(level_limit, Grades[grade].level_max);
    }
}

public class CreatureInfo : InfoBaseString
{
    public EquipCategory EquipWeaponCategory { get; private set; }
    public EquipCategory EquipArmorCategory { get; private set; }

    public string Name { get; private set; }
    public string Desc { get; private set; }
    public eAttackType AttackType { get; private set; }
    public eCreatureType CreatureType { get; private set; }
    public string ShowAttackType
    {
        get
        {
            if (Position == eCreaturePosition.front)
                return "tanker_"+AttackType;
            return AttackType.ToString();
        }
    }

    public List<string> CreatureTags = new List<string>();
    CreatureStatPreset StatPreset;
    public StatInfo Stat { get; private set; }
    public StatInfo StatIncrease { get; private set; }

    public List<SkillInfo> Skills { get; private set; }
    public SkillInfo TeamSkill { get; private set; }

    public bool HasSkill { get { return Skills.Count > 1; } }

    public List<string> Skins = new List<string>();

    public eCreaturePosition Position { get; private set; }

    override public void Load(XmlNode node)
    {
        base.Load(node);

        Name = node.Attributes["name"].Value;
        XmlAttribute desc_attr = node.Attributes["description"];
        if (desc_attr != null)
            Desc = desc_attr.Value.Replace("\\n", "\n");

        AttackType = (eAttackType)Enum.Parse(typeof(eAttackType), node.Attributes["attack_type"].Value);
        CreatureType = (eCreatureType)Enum.Parse(typeof(eCreatureType), node.Attributes["creature_type"].Value);
        CreatureTags.Add(AttackType.ToString());
        CreatureTags.Add(CreatureType.ToString());

        XmlAttribute position_attr = node.Attributes["position"];
        if (position_attr != null)
            Position = (eCreaturePosition)Enum.Parse(typeof(eCreaturePosition), position_attr.Value);
        else
            Position = eCreaturePosition.front;

        CreatureTags.Add(Position.ToString());

        XmlAttribute tag_attr = node.Attributes["tag"];
        if (tag_attr != null)
        {
            string split = tag_attr.Value;
            Array.ForEach(split.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), tag => CreatureTags.Add(tag));
        }

        XmlAttribute preset_attr = node.Attributes["preset_type"];
        if(preset_attr != null && CreatureInfoManager.Instance.StatPresets != null)
        {
            string preset_type = preset_attr.Value;
            StatPreset = CreatureInfoManager.Instance.StatPresets.Find(e => e.type == preset_type);
            Stat = StatPreset.Stat(AttackType);
            StatIncrease = StatPreset.StatIncrease(AttackType);
        }
        else
        {
            //throw new System.Exception("StatPreset is not exist.");
            Stat = CreatureStatPreset.GetStatInfo(node.SelectSingleNode("Stat"), AttackType);
            StatIncrease = CreatureStatPreset.GetStatInfo(node.SelectSingleNode("IncreaseStatPerLevel"), AttackType);
        }

        Skills = new List<SkillInfo>();
        foreach (XmlNode skill_node in node.SelectNodes("Skill"))
        {
            if (skill_node.NodeType == XmlNodeType.Comment)
                continue;

            Skills.Add(SkillInfoManager.Instance.GetInfoByID(skill_node.Attributes["id"].Value));
        }

        XmlNode teamSkillNode = node.SelectSingleNode("TeamSkill");
        if (teamSkillNode != null)
        {
            if (teamSkillNode.NodeType != XmlNodeType.Comment)
            {
                TeamSkill = SkillInfoManager.Instance.GetInfoByID(teamSkillNode.Attributes["id"].Value);
            }
        }

        XmlNode equipNode = node.SelectSingleNode("Equip");
        EquipWeaponCategory = EquipInfoManager.Instance.GetCategory(equipNode.Attributes["weapon"].Value);
        EquipArmorCategory = EquipInfoManager.Instance.GetCategory(equipNode.Attributes["armor"].Value);

        Skins.Add("default");
    }

    public string GetSkinName(short index)
    {
        return Skins[index];
    }

    public string GetPositionString()
    {
        return Localization.Get("CreaturePosition_" + Position);
    }

    public string GetTooltip()
    {
        string tooltip = Localization.Format("CreatureTooltip", Name, Localization.Get(ShowAttackType), GetPositionString());
        if (TeamSkill != null)
            tooltip += "\n\n" + Localization.Format("Tooltip_LeaderSkill", TeamSkill.Name);
        return tooltip;
    }

    public bool ContainsTag(string tag)
    {
        if (CreatureTags.Exists(s => s.Equals(tag)))
            return true;
        return false;
    }

    public bool ContainsTags(List<string> tags)
    {
        foreach(var tag in tags)
        {
            if (ContainsTag(tag) == true) return true;
        }
        return false;
    }
}
