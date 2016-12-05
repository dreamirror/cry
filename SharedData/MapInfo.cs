using MNS;
using PacketEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

public enum eDifficult
{
    Normal,
    Hard,
}

public class MapInfoManager : InfoManager<MapInfo, MapInfo, MapInfoManager>
{
    Dictionary<string, MapStageInfo> m_Stages;

    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);

        m_Stages = new Dictionary<string, MapStageInfo>();
        foreach (var info in m_Infos)
        {
            foreach (var stage_info in info.Stages)
            {
                m_Stages.Add(stage_info.ID, stage_info);
            }
        }

        foreach (var info in m_Infos)
        {
            foreach (var condition in info.Conditions)
            {
                if (condition != null)
                    condition.SetConditionText();
            }

            foreach (var stage_info in info.Stages)
            {
                foreach (var condition in stage_info.Conditions)
                {
                    if (condition != null)
                        condition.SetConditionText();
                }
            }
        }
    }

    public MapStageDifficulty GetStageInfoByID(string id, pe_Difficulty difficulty)
    {
        MapStageInfo stage_info;
        if (m_Stages.TryGetValue(id, out stage_info) == false)
        {
            throw new System.Exception(string.Format("Not exists stage : {0}", id));
        }
        return stage_info.Difficulty[(int)difficulty];
    }

    public int GetMainMapCount()
    {
        return m_Infos.Count(e => e.MapType == "main");
    }

    public void CheckOpenContents(ref List<ContentsOpenInfo> opens, eMapCondition condition_type, string value, string value2)
    {
        foreach (var info in m_Infos)
        {
            if (info.IDN <= GameConfig.Get<int>("contents_open_main_map"))
            {
                foreach (var condition in info.Conditions)
                {
                    if (condition != null)
                        condition.CheckOpenContents(ref opens, condition_type, value, value2);
                }

                foreach (var stage_info in info.Stages)
                {
                    foreach (var condition in stage_info.Conditions)
                    {
                        if (condition != null)
                            condition.CheckOpenContents(ref opens, condition_type, value, value2);
                    }
                }
            }
        }
    }

    public List<MapInfo> GetWeeklyDungeons()
    {
        return m_Infos.Where(e => e.MapType == "weekly").OrderBy(o => o.CheckCondition() != null).ToList();
    }

    public MapStageDifficulty GetNextStageInfo(MapStageDifficulty stage_info)
    {
        var map_info = stage_info.MapInfo;
        if (map_info.MapType == "boss")
            return null;

        MapStageDifficulty next_stage_info = null;
        if (map_info.MapType == "weekly")
        {
            if ((int)stage_info.Difficulty < stage_info.StageInfo.Difficulty.Count - 1)
            {
                next_stage_info = stage_info.StageInfo.Difficulty[(int)stage_info.Difficulty + 1];
            }
        }
        else
        {
            if (stage_info.StageInfo.StageIndex >= map_info.Stages.Count - 1)
            {
                if (map_info.MapType == "main")
                {
                    // next map
                    if (ContainsIdn(map_info.IDN + 1))
                    {
                        var next_map_info = GetInfoByIdn(map_info.IDN + 1);
                        next_stage_info = next_map_info.Stages[0].Difficulty[(int)stage_info.Difficulty];
                    }
                }
            }
            else
            {
                next_stage_info = map_info.Stages[stage_info.StageInfo.StageIndex + 1].Difficulty[(int)stage_info.Difficulty];
            }
        }
        return next_stage_info;
    }
}


public class MapRewardInfo
{
    public int IDN { get { return Reward.IDN; } }
    public bool IsShow { get; private set; }
    public short Percent { get; private set; }
    public int Value { get; private set; }
    public pe_Difficulty Difficulty { get; private set; }

    public ItemInfoBase Reward { get; private set; }

    public MapRewardInfo(XmlNode node, pe_Difficulty difficulty = pe_Difficulty.Normal)
    {
        string id = node.Attributes["id"].Value;
        Reward = ItemInfoManager.Instance.GetInfoByID(id);
        Difficulty = difficulty;
        var showAttr = node.Attributes["show"];
        if (showAttr != null)
            IsShow = bool.Parse(showAttr.Value);
        switch (Reward.ItemType)
        {
            case eItemType.Stuff:
            case eItemType.Item:
                Percent = short.Parse(node.Attributes["percent"].Value);
                break;

            default:
                Value = int.Parse(node.Attributes["value"].Value);
                break;
        }
        if (Reward == null)
            throw new System.Exception(string.Format("not exist item id in MapRewardInfo : {0}", id));
    }
}

public class MapInfo : InfoBaseString
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<MapStageInfo> Stages { get; set; }

    public string BG_ID { get; private set; }
    public int TryLimit { get; private set; }
    public string MapType { get; private set; }

    public short CheckCreature { get; private set; }
    public short CheckRune { get; private set; }

    public List<MapRewardInfo> Rewards { get; private set; }

    public MapCondition[] Conditions { get; private set; }
    public List<string> AvailableTags { get; private set; }

    public string GetShowName(pe_Difficulty difficulty)
    {
        switch(MapType)
        {
            case "main":
                return Localization.Format("DungeonName", IDN, Name, Localization.Get("MapDifficulty_" + difficulty));

            default:
                return Name;
        }
    }

    public MapCondition CheckCondition(pe_Difficulty difficulty = pe_Difficulty.Normal)
    {
        if (Conditions[(int)difficulty] == null)
            return null;

        return Conditions[(int)difficulty].CheckCondition();
    }

    override public void Load(XmlNode node)
    {
        base.Load(node);
        Name = node.Attributes["name"].Value;

        XmlAttribute descAttr = node.Attributes["description"];
        if (descAttr != null)
            Description = descAttr.Value;
        else
            Description = "";

        var bg_id_attr = node.Attributes["bg_id"];
        if (bg_id_attr != null)
            BG_ID = bg_id_attr.Value;
        else
            BG_ID = ID;

        var map_type_attr = node.Attributes["map_type"];
        if (map_type_attr != null)
            MapType = map_type_attr.Value;

        var try_limit_attr = node.Attributes["try_limit"];
        if (try_limit_attr != null)
            TryLimit = short.Parse(try_limit_attr.Value);
        else
            TryLimit = -1;

        CheckCreature = short.Parse(node.Attributes["check_creature"].Value);
        CheckRune = short.Parse(node.Attributes["check_rune"].Value);

        Conditions = new MapCondition[5];
        foreach (XmlNode condition_node in node.SelectNodes("Condition"))
        {
            MapCondition condition = new MapCondition(condition_node);
            Conditions[(int)condition.difficulty] = condition;

            condition.ContentsOpen = new ContentsOpenInfo();
            condition.ContentsOpen.icon_id = "mapicon_" + ID;
            switch (MapType)
            {
                case "main":
                    condition.ContentsOpen.title = Localization.Get("OpenContentsDungeonMain");
                    condition.ContentsOpen.message = GetShowName(condition.difficulty);
                    break;

                case "event":
                case "weekly":
                    condition.ContentsOpen.title = Localization.Get("OpenContentsDungeonEvent");
                    condition.ContentsOpen.message = GetShowName(condition.difficulty);
                    break;

                default:
                    condition.ContentsOpen.title = "invalid";
                    condition.ContentsOpen.message = "invalid";
                    break;
            }
        }

        Stages = new List<MapStageInfo>();
        foreach (XmlNode stageNode in node.SelectNodes("Stage"))
        {
            Stages.Add(new MapStageInfo(this, stageNode));
        }

        AvailableTags = new List<string>();
        XmlAttribute available_tag_attr = node.Attributes["available_tag"];
        if(available_tag_attr != null)
        {
            string tags = available_tag_attr.Value;
            Array.ForEach(tags.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), tag => AvailableTags.Add(tag));
        }
        //Rewards = new List<MapRewardInfo>();
        //foreach (XmlNode difficultyNode in node.SelectNodes("RewardDifficulty"))
        //{
        //    pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), difficultyNode.Attributes["type"].Value);
        //    foreach (XmlNode stageNode in difficultyNode.SelectNodes("Reward"))
        //    {
        //        Rewards.Add(new MapRewardInfo(stageNode, difficulty));
        //    }
        //}
    }
}


public enum eMapCondition
{
    MapClear,
    MapStageClear,
    Weekly,
    Period,
    Block,
}

public class MapCondition
{
    public ContentsOpenInfo ContentsOpen { get; set; }
    public string Condition { get; private set; }

    public pe_Difficulty difficulty;
    public eMapCondition type;
    public string value, value2;

    public MapCondition(XmlNode node)
    {
        type = (eMapCondition)Enum.Parse(typeof(eMapCondition), node.Attributes["type"].Value);

        XmlAttribute difficultyAttr = node.Attributes["difficulty"];
        if (difficultyAttr != null)
            difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), difficultyAttr.Value);
        else
            difficulty = pe_Difficulty.Normal;

        value = node.Attributes["value"].Value;

        switch (type)
        {
            case eMapCondition.MapClear:
            case eMapCondition.MapStageClear:
                {
                    XmlAttribute value2Attr = node.Attributes["value2"];
                    if (value2Attr != null)
                    {
                        value2 = value2Attr.Value;
                    }
                    else
                        value2 = "Normal";
                }
                break;
            case eMapCondition.Period:
                value2 = node.Attributes["value2"].Value;
                break;
        }
    }

    public MapCondition CheckCondition()
    {
        switch(type)
        {
            case eMapCondition.MapClear:
                {
                    MapInfo map_info = MapInfoManager.Instance.GetInfoByID(value);
                    MapStageInfo stage_info = map_info.Stages.Last();

                    pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), value2);

                    var clear_data = MapClearDataManager.Instance.GetData(stage_info, difficulty);
                    if (clear_data == null || clear_data.clear_rate == 0)
                    {
                        return this;
                    }
                }
                break;

            case eMapCondition.MapStageClear:
                {
                    pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), value2);

                    MapStageDifficulty stage_info = MapInfoManager.Instance.GetStageInfoByID(value, difficulty);


                    var clear_data = MapClearDataManager.Instance.GetData(stage_info);
                    if (clear_data == null || clear_data.clear_rate == 0)
                    {
                        return this;
                    }
                }
                break;
            case eMapCondition.Weekly:
                DayOfWeek cur_dow = Network.Instance.ServerTime.DayOfWeek;
                DayOfWeek set_dow = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), value);
                if (cur_dow != set_dow)
                    return this;               

                break;
            case eMapCondition.Period:
                DateTime begin_time = DateTime.Parse(value);
                DateTime end_time = DateTime.Parse(value2);
                if (begin_time > Network.Instance.ServerTime || end_time < Network.Instance.ServerTime)
                    return this;
                break;
            case eMapCondition.Block:
                return this;
        }
        return null;
    }

    public void CheckOpenContents(ref List<ContentsOpenInfo> opens, eMapCondition condition_type, string value, string value2)
    {
        if (ContentsOpen != null && condition_type == type && value == this.value && value2 == this.value2)
            opens.Add(ContentsOpen);
    }

    public void SetConditionText()
    {
        switch (type)
        {
            case eMapCondition.MapClear:
            case eMapCondition.MapStageClear:
                {
                    switch (type)
                    {
                        case eMapCondition.MapClear:
                            {
                                MapInfo map_info = MapInfoManager.Instance.GetInfoByID(value);
                                pe_Difficulty show_difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), value2);

                                Condition = Localization.Format("StageConditionMapClear", map_info.GetShowName(show_difficulty));
                            }
                            break;

                        case eMapCondition.MapStageClear:
                            {
                                pe_Difficulty show_difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), value2);
                                MapStageDifficulty stage_info = MapInfoManager.Instance.GetStageInfoByID(value, show_difficulty);

                                Condition = Localization.Format("StageConditionMapStageClear", stage_info.ShowName);
                            }
                            break;
                    }
                }
                break;

            case eMapCondition.Block:
                {
                    Condition = Localization.Get(value);
                }
                break;
        }
    }

}

public class MapStageDifficulty
{
    public pe_Difficulty Difficulty { get; private set; }
    public int TryLimit { get; private set; }
    public List<CreatureInfo> Recommends { get; private set; }
    public List<MapWaveInfo> Waves { get; private set; }
    public short Energy { get; private set; }
    public int RewardGold { get; private set; }
    public int RewardExp { get; private set; }
    public MapCondition Condition { get; private set; }

    public MapStageInfo StageInfo { get; private set; }
    public MapInfo MapInfo { get { return StageInfo.MapInfo; } }
    public short StageIndex { get { return StageInfo.StageIndex; } }
    public Vector3 MapPos { get { return StageInfo.MapPos; } }
    public string ID { get { return StageInfo.ID; } }
    public string BG_ID { get { return StageInfo.BG_ID; } }
    public eStageType StageType { get { return StageInfo.StageType; } }
    public pe_Team TeamID { get { return StageInfo.TeamID; } }
    public string Name { get { return StageInfo.Name; } }
    public string Description { get { return StageInfo.Description; } }
    public string ShowName { get { return StageInfo.GetShowName(Difficulty); } }
    public MapCondition CheckCondition { get { return StageInfo.CheckCondition(Difficulty); } }
    public List<RewardLootInfo> DropInfo { get; private set; }
    public List<RewardLoot> DropItems { get { return DropInfo.Count == 0 ? StageInfo.DropItems() : DropItems2(); } }

    public int Power
    {
        get
        {
            return Waves.Sum(w => w.Creatures.Where(c => c.CreatureInfo != null).Sum(c => c.Power));
        }
    }

    public MapStageDifficulty(MapStageInfo stage_info, XmlNode node)
    {
        StageInfo = stage_info;
        
        XmlAttribute difficultyAttr = node.Attributes["type"];
        if (difficultyAttr != null)
            Difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), difficultyAttr.Value);
        else
            Difficulty = pe_Difficulty.Normal;

        Condition = StageInfo.Conditions[(int)Difficulty];

        XmlAttribute tryLimitAttr = node.Attributes["try_limit"];
        if (tryLimitAttr != null)
            TryLimit = int.Parse(tryLimitAttr.Value);
        else
            TryLimit = StageInfo.MapInfo.TryLimit;

        if (MapInfo.MapType != "weekly")
        {
            Energy = short.Parse(node.Attributes["energy"].Value);
            RewardExp = int.Parse(node.Attributes["reward_exp"].Value);
            RewardGold = int.Parse(node.Attributes["reward_gold"].Value);
        }
        Waves = new List<MapWaveInfo>();
        foreach (XmlNode waveNode in node.SelectNodes("Wave"))
        {
            Waves.Add(new MapWaveInfo(waveNode));
        }

        //Rewards = new List<MapRewardInfo>();
        //foreach (XmlNode rewardNode in node.SelectNodes("Reward"))
        //{
        //    var reward = new MapRewardInfo(rewardNode);
        //    Rewards.Add(reward);

        //    //if (MapInfo.MapType == "main" && (reward.Reward as StuffInfo) != null)
        //    //    (reward.Reward as StuffInfo).AddDropInfo(this);
        //}

        Recommends = new List<CreatureInfo>();
        foreach (XmlNode recommendNode in node.SelectNodes("Recommend"))
        {
            Recommends.Add(CreatureInfoManager.Instance.GetInfoByID(recommendNode.Attributes["id"].Value));
        }

        DropInfo = new List<RewardLootInfo>();
        foreach (XmlNode LootNode in node.SelectNodes("Loot"))
        {
            DropInfo.Add(new RewardLootInfo(LootNode, Difficulty));
        }
    }
    List<RewardLoot> DropItems2()
    {
        return DropInfo.SelectMany(d => d.groups.SelectMany(g => g.rewards)).Reverse().ToList();
    }

}
public class MapStageInfo
{
    public MapInfo MapInfo { get; private set; }

    public eStageType StageType { get; private set; }
    public string ID { get; private set; }
    public string BG_ID { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Vector3 MapPos { get; private set; }
    public short StageIndex { get { return (short)MapInfo.Stages.FindIndex(st => st.ID == ID); } }

    public List<MapStageDifficulty> Difficulty;
    public MapCondition[] Conditions { get; private set; }
    public List<RewardLootInfo> DropInfo { get; private set; }

    public pe_Team TeamID
    {
        get
        {
            switch (MapInfo.MapType)
            {
                case "event":
                    return (pe_Team)MapInfo.IDN;

                case "worldboss":
                    return (pe_Team)MapInfo.IDN;

                case "boss":
                    return (pe_Team)(MapInfo.IDN+StageIndex);

                case "weekly":
                    return (pe_Team)MapInfo.IDN;

                default:
                    return pe_Team.Main;
            }
        }
    }

    public MapCondition CheckCondition(pe_Difficulty difficulty)
    {
        var condition = MapInfo.CheckCondition(difficulty);
        if (condition != null)
            return condition;

        if (Conditions == null || Conditions[(int)difficulty] == null)
            return null;

        return Conditions[(int)difficulty].CheckCondition();
    }

    public MapStageInfo(MapInfo mapInfo, XmlNode node)
    {
        MapInfo = mapInfo;

        ID = node.Attributes["id"].Value;

        var bg_id_attr = node.Attributes["bg_id"];
        if (bg_id_attr != null)
            BG_ID = bg_id_attr.Value;
        else
            BG_ID = MapInfo.BG_ID;

        if (MapInfo.MapType != "weekly")
        {
            Name = node.Attributes["name"].Value;
            Description = node.Attributes["description"].Value.Replace("\\n", "\n");

            Vector3 map_pos = new Vector3();
            map_pos.x = float.Parse(node.Attributes["map_x"].Value);
            map_pos.y = float.Parse(node.Attributes["map_y"].Value);
            MapPos = map_pos;
        }

        var stage_type_attr = node.Attributes["stage_type"];
        if (stage_type_attr != null)
            StageType = (eStageType)Enum.Parse(typeof(eStageType), stage_type_attr.Value);
        
        Conditions = new MapCondition[5];
        foreach (XmlNode condition_node in node.SelectNodes("Condition"))
        {
            MapCondition condition = new MapCondition(condition_node);
            Conditions[(int)condition.difficulty] = condition;

            condition.ContentsOpen = new ContentsOpenInfo();
            condition.ContentsOpen.icon_id = "mapicon_" + ID;
            switch (MapInfo.MapType)
            {
                case "boss":
                    condition.ContentsOpen.title = Localization.Get("OpenContentsDungeonBoss");
                    condition.ContentsOpen.message = GetShowName(condition.difficulty);
                    break;

                default:
                    condition.ContentsOpen.title = "invalid";
                    condition.ContentsOpen.message = "invalid";
                    break;
            }
        }

        Difficulty = new List<MapStageDifficulty>();
        if (MapInfo.MapType == "main" || MapInfo.MapType == "weekly")
        {
            foreach (XmlNode difficultyNode in node.SelectNodes("Difficulty"))
            {
                Difficulty.Add(new MapStageDifficulty(this, difficultyNode));
            }
        }
        else
        {
            Difficulty.Add(new MapStageDifficulty(this, node));
        }

        DropInfo = new List<RewardLootInfo>();
        foreach (XmlNode LootNode in node.SelectNodes("Loot"))
        {
            DropInfo.Add(new RewardLootInfo(LootNode, pe_Difficulty.Normal));
        }
    }

    public string GetShowName(pe_Difficulty difficulty)
    {
        switch (MapInfo.MapType)
        {
            case "main":
                return Localization.Format("StageMainShowName", MapInfo.IDN, StageIndex + 1, Name, Localization.Get("MapDifficulty_"+ difficulty));

            case "event":
                return Localization.Format("StageEventShowName", MapInfo.Name, Name);

            case "weekly":
                return Localization.Format("StageWeeklyShowName", MapInfo.Name, Name, Localization.Get("MapDifficulty_" + difficulty));

            default:
                return Name;
        }
    }

    public List<RewardLoot> DropItems()
    {
        return DropInfo.SelectMany(d => d.groups.SelectMany(g => g.rewards)).Reverse().ToList();
    }
}

public class MapWaveInfo
{
    public List<MapCreatureInfo> Creatures { get; private set; }

    public MapWaveInfo(XmlNode node)
    {
        Creatures = new List<MapCreatureInfo>();
        foreach (XmlNode creatureNode in node.SelectNodes("Creature"))
        {
            Creatures.Add(new MapCreatureInfo(creatureNode));
        }
    }
}

public enum eStageType
{
    Normal,
    Boss,
    WorldBoss,
}

public enum eMapCreatureType
{
    Normal,
    Elite,
    Boss,
    WorldBoss,
}

public class MapPassiveInfo
{
    public SkillInfo SkillInfo { get; private set; }
    public MapPassiveInfo(XmlNode node)
    {
        SkillInfo = SkillInfoManager.Instance.GetInfoByID(node.Attributes["id"].Value);
    }
}

public class MapCreatureInfo
{
    public CreatureInfo CreatureInfo { get; private set; }
    public string SkinName { get; private set; }
    public short Level { get; private set; }
    public short Grade { get; private set; }
    public short Enchant { get; private set; }
    public eMapCreatureType CreatureType { get; private set; }
    public bool IsShow { get; private set; }
    public short AutoSkillIndex { get; private set; }
    public float GradePercent { get { return CreatureInfoManager.Instance.Grades[Grade].enchants[Enchant].stat_percent; } }
    public pe_UseLeaderSkillType UseLeaderSkillType = pe_UseLeaderSkillType.Manual;

    public List<MapPassiveInfo> PassiveInfos { get; private set; }
    public int Power
    {
        get
        {
            float grade_percent = GradePercent;
            return Creature.CalculatePower(GetStat(Level, grade_percent, Enchant), CreatureInfo.Skills.Count * Level, grade_percent);
        }
    }

    public StatInfo GetStat(short level, float grade_percent, short enchant)
    {
        StatInfo base_stat = new StatInfo(CreatureInfo.Stat);
        base_stat.AddRange(CreatureInfo.StatIncrease, level);
        base_stat.Multiply(grade_percent);

        StatInfo stat = new StatInfo(base_stat);
        int equip_grade = 0;
        for(int i=1; i< CreatureInfoManager.Instance.Grades.Count; ++i)
        {
            if (level <= CreatureInfoManager.Instance.Grades[i].level_max)
            {
                equip_grade = i - 1;
                break;
            }
        }
        EquipInfoManager.Instance.AddStats(CreatureInfo.EquipWeaponCategory.Equips[equip_grade], enchant, stat);
        EquipInfoManager.Instance.AddStats(CreatureInfo.EquipArmorCategory.Equips[equip_grade], enchant, stat);

        foreach (var skill_info in CreatureInfo.Skills.Where(s => s.Type == eSkillType.passive || s.Type == eSkillType.passive_etc))
        {
            skill_info.AddStats(stat, base_stat, CreatureInfo.AttackType, grade_percent, level);
        }

        return stat;
    }

    public MapCreatureInfo(XmlNode node)
    {
        string id = node.Attributes["id"].Value;
        if (id == "dummy")
            return;

        CreatureInfo = CreatureInfoManager.Instance.GetInfoByID(node.Attributes["id"].Value);
        SkinName = "default";
        Level = short.Parse(node.Attributes["level"].Value);
        Grade = short.Parse(node.Attributes["grade"].Value);
        Enchant = short.Parse(node.Attributes["enchant"].Value);

        XmlAttribute typeAttr = node.Attributes["type"];
        if (typeAttr != null) CreatureType = (eMapCreatureType)Enum.Parse(typeof(eMapCreatureType), typeAttr.Value);

        XmlAttribute showAttr = node.Attributes["show"];
        if (showAttr != null)
            IsShow = bool.Parse(showAttr.Value);

        XmlAttribute autoSkillIndexAttr = node.Attributes["auto_skill_index"];
        if (autoSkillIndexAttr != null)
            AutoSkillIndex = short.Parse(autoSkillIndexAttr.Value);

        XmlAttribute useLeaderSkillTypeAttr = node.Attributes["use_leader_skill_type"];
        if (useLeaderSkillTypeAttr != null)
            UseLeaderSkillType = (pe_UseLeaderSkillType)Enum.Parse(typeof(pe_UseLeaderSkillType), useLeaderSkillTypeAttr.Value);

        PassiveInfos = new List<MapPassiveInfo>();
        foreach (XmlNode childNode in node.SelectNodes("Passive"))
        {
            PassiveInfos.Add(new MapPassiveInfo(childNode));
        }
    }
}

