using MNS;
using PacketEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

public class MapClearRewardInfoManager : InfoManager<MapClearRewardInfo, MapClearRewardInfo, MapClearRewardInfoManager>
{
}

public class MapClearRewardInfo : InfoBaseString
{
    public int Total;
    List<RewardCondition> _conditions;
    public List<RewardCondition> conditions(pe_Difficulty difficulty)
    {
        return _conditions.Where(e => e.difficulty == difficulty).ToList();
    }

    List<RewardLootInfo> _loot_infos;
    override public void Load(XmlNode node)
    {
        base.Load(node);
        MapInfo map_info = MapInfoManager.Instance.GetInfoByIdn(IDN);
        Total = map_info.Stages.Count * 3;

        _conditions = new List<RewardCondition>();
        _loot_infos = new List<RewardLootInfo>();
        foreach (XmlNode difficultyNode in node.ChildNodes)
        {
            pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), difficultyNode.Attributes["type"].Value);
            foreach (XmlNode child in difficultyNode.SelectNodes("Condition"))
            {
                _conditions.Add(new RewardCondition(child, difficulty));
            }

            _loot_infos.Add(new RewardLootInfo((XmlNode)difficultyNode.SelectSingleNode("Loot"), difficulty));

            if (conditions(difficulty).Count < 3)
                throw new System.Exception("MapClearRewardInfo is not valid.");
        }

    }
    public bool CheckCondition(int idx, int value, pe_Difficulty difficulty)
    {
        return conditions(difficulty)[idx].condition <= value;
    }
}

public class RewardCondition
{
    public pe_Difficulty difficulty;
    public short condition;
    public List<RewardBase> rewards;
    public RewardCondition() { }
    public RewardCondition(XmlNode node, pe_Difficulty difficulty)
    {
        this.difficulty = difficulty;
        condition = short.Parse(node.Attributes["clear_rate"].Value);
        rewards = new List<RewardBase>();
        foreach (XmlNode child in node.ChildNodes)
        {
            rewards.Add(new RewardBase(child));
        }
    }
}

public class RewardLootInfo
{
    public int loot_count_min;
    public int loot_count_max;
    public List<RewardLootGroup> groups;
    public pe_Difficulty difficulty;
    public RewardLootInfo(XmlNode node, pe_Difficulty difficulty)
    {
        this.difficulty = difficulty;
        loot_count_min = int.Parse(node.Attributes["loot_count_min"].Value);
        loot_count_max = int.Parse(node.Attributes["loot_count_max"].Value);
        groups = new List<RewardLootGroup>();
        foreach (XmlNode child in node.ChildNodes)
        {
            groups.Add(new RewardLootGroup(child));
        }
    }
}

public class RewardLootGroup
{
    public int loot_count_max;
    public List<RewardLoot> rewards;
    public int _chance;
    public int Chance { get { return _chance; } }
    public string show_id { get; private set; }
    public int show_value { get; private set; }
    public RewardLootGroup(XmlNode node)
    {
        XmlAttribute countMaxAttr = node.Attributes["loot_count_max"];
        if (countMaxAttr != null)
            loot_count_max = int.Parse(node.Attributes["loot_count_max"].Value);
        else
            loot_count_max = int.MaxValue;
        _chance = int.Parse(node.Attributes["chance"].Value);
        rewards = new List<RewardLoot>();
        foreach (XmlNode child in node.ChildNodes)
        {
            rewards.Add(new RewardLoot(child));
        }

        XmlAttribute show_attr = node.Attributes["show_id"];
        if (show_attr != null)
        {
            show_id = show_attr.Value;
            show_value = int.Parse(node.Attributes["show_value"].Value);
        }

    }
}

public class RewardLoot : RewardBase
{
    public string ID;
    public int _chance;
    public int Chance { get { return _chance; } }
    public bool IsShow;

    public RewardLoot(XmlNode node)
    {
        ID = node.Attributes["id"].Value;
        if (ItemInfoManager.Instance.ContainsKey(ID))
            ItemInfo = ItemInfoManager.Instance.GetInfoByID(ID);
        else if (CreatureInfoManager.Instance.ContainsKey(ID) == true)
            CreatureInfo = CreatureInfoManager.Instance.GetInfoByID(ID);
//         if (ItemInfo == null && CreatureInfo == null)
//             throw new System.Exception(string.Format("invalid reward id : {0}", id));
        XmlAttribute valueAttr = node.Attributes["value"];
        if (valueAttr != null)
            Value = int.Parse(valueAttr.Value);
        else
            Value = 1;

        _chance = int.Parse(node.Attributes["chance"].Value);

        XmlAttribute showAttr = node.Attributes["show"];
        if (showAttr != null)
            IsShow = bool.Parse(showAttr.Value);
    }
}

public class RewardBase
{
    public CreatureInfo CreatureInfo = null;
    public ItemInfoBase ItemInfo = null;
    public int Value;
    public int Value2;
    public int Value3;

    public RewardBase() { }

    public int GetIdn()
    {
        if (ItemInfo != null)
            return ItemInfo.IDN;
        return CreatureInfo.IDN;
    }

    public string GetName()
    {
        if (ItemInfo != null)
            return ItemInfo.Name;
        return CreatureInfo.Name;
    }

    public RewardBase(int reward_idn, int value, int value2 = 0, int value3 = 0)
    {
        if (ItemInfoManager.Instance.ContainsIdn(reward_idn))
            ItemInfo = ItemInfoManager.Instance.GetInfoByIdn(reward_idn);
        else if (CreatureInfoManager.Instance.ContainsIdn(reward_idn) == true)
            CreatureInfo = CreatureInfoManager.Instance.GetInfoByIdn(reward_idn);

        if (ItemInfo == null && CreatureInfo == null)
            throw new System.Exception(string.Format("invalid reward idn : {0}", reward_idn));
        this.Value = value;
        Value2 = value2;
        Value3 = value3;
    }

    public RewardBase(XmlNode node)
    {
        string id = node.Attributes["id"].Value;
        if (ItemInfoManager.Instance.ContainsKey(id))
            ItemInfo = ItemInfoManager.Instance.GetInfoByID(id);
        else if (CreatureInfoManager.Instance.ContainsKey(id) == true)
            CreatureInfo = CreatureInfoManager.Instance.GetInfoByID(id);
        if (ItemInfo == null && CreatureInfo == null)
            throw new System.Exception(string.Format("invalid reward id : {0}", id));
        Value = int.Parse(node.Attributes["value"].Value);

        XmlAttribute value2_attr = node.Attributes["value2"];
        if (value2_attr != null)
            Value2 = int.Parse(value2_attr.Value);
        else Value2 = 0;

        XmlAttribute value3_attr = node.Attributes["value3"];
        if (value3_attr != null)
            Value3 = int.Parse(value3_attr.Value);
        else Value3 = 0;
    }
}
