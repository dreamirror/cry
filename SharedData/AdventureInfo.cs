using LinqTools;
using MNS;
using PacketEnums;
using PacketInfo;
using System;
using System.Collections.Generic;
using System.Xml;

public class AdventureInfoManager : InfoManager<AdventureInfo, AdventureInfo, AdventureInfoManager>
{
    public string Title, Desc;
    public int InstantCompletePeriod;
    public pd_GoodsData Price;

    List<pd_AdventureInfo> m_AdventureInfoDetail;

    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);

        XmlNode info_node = node.SelectSingleNode("Info");
        Title = info_node.Attributes["title"].Value;
        Desc = info_node.Attributes["desc"].Value;

        InstantCompletePeriod = int.Parse(info_node.Attributes["instant_complete_period"].Value);
        Price = new pd_GoodsData();
        Price.goods_type = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), info_node.Attributes["price_type"].Value);
        Price.goods_value = int.Parse(info_node.Attributes["price_value"].Value);

        foreach (var info in m_Infos)
        {
            foreach (var condition in info.Conditions)
            {
                if (condition != null)
                    condition.SetConditionText();
            }
        }
    }

    public void CheckOpenContents(ref List<ContentsOpenInfo> opens, eMapCondition condition_type, string value, string value2)
    {
        foreach (var info in m_Infos)
        {
            foreach (var condition in info.Conditions)
            {
                if (condition != null)
                    condition.CheckOpenContents(ref opens, condition_type, value, value2);
            }
        }
    }

    public List<AdventureInfo> GetList()
    {
        return m_Infos;
    }

    public void SetInfoDetails(List<pd_AdventureInfo> info_detail)
    {
        m_AdventureInfoDetail = info_detail;
    }

    public void SetInfoDetail(pd_AdventureInfo info)
    {
        m_AdventureInfoDetail.RemoveAll(e => e.map_idn == info.map_idn);
        m_AdventureInfoDetail.Add(info);
    }

    public pd_AdventureInfo GetInfo(int idn)
    {
        return m_AdventureInfoDetail.Find(e => e.map_idn == idn);
    }
}

public class AdventureInfo : InfoBaseString
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public int Period { get; private set; }
    public int NeedCreature { get; private set; }
    public int MinGrade { get; private set; }

    public RewardLootInfo Reward { get; private set; }

    public MapCondition[] Conditions { get; private set; }
    public List<string> AvailableTags { get; private set; }

    public string ShowName { get { return Name; } }
    public string ShowCondition { get { return Localization.Format("AdventureConditionFormat", MinGrade, GetTagString(), NeedCreature); } }

    public List<RewardLootInfo> DropInfo { get; private set; }

    string GetTagString()
    {
        if (AvailableTags == null || AvailableTags.Count == 0) return "";
        string res = string.Format("[url={0}]{1}[/url]", "Tag_"+AvailableTags[0], Localization.Get("Tag_"+AvailableTags[0]) );
        for(int i=1; i<AvailableTags.Count; ++i)
        {
            res += " " + string.Format("[url={0}]{1}[/url]", "Tag_" + AvailableTags[i], Localization.Get("Tag_" + AvailableTags[i]));
        }
        return res;
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

        Period = int.Parse(node.Attributes["period"].Value);
        NeedCreature = int.Parse(node.Attributes["need_creature"].Value);
        MinGrade = int.Parse(node.Attributes["min_grade"].Value);

        Conditions = new MapCondition[5];
        foreach (XmlNode condition_node in node.SelectNodes("Condition"))
        {
            MapCondition condition = new MapCondition(condition_node);
            Conditions[(int)condition.difficulty] = condition;

            condition.ContentsOpen = new ContentsOpenInfo();
            condition.ContentsOpen.icon_id = "mapicon_" + ID;
            condition.ContentsOpen.title = Localization.Get("OpenContentsDungeonMain");
            //condition.ContentsOpen.message = GetShowName(condition.difficulty);
        }

        AvailableTags = new List<string>();
        XmlAttribute available_tag_attr = node.Attributes["available_tag"];
        if(available_tag_attr != null)
        {
            string tags = available_tag_attr.Value;
            Array.ForEach(tags.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), tag => AvailableTags.Add(tag));
        }

        DropInfo = new List<RewardLootInfo>();
        foreach (XmlNode LootNode in node.SelectNodes("Loot"))
        {
            DropInfo.Add(new RewardLootInfo(LootNode, pe_Difficulty.Normal));
        }
    }

    public List<RewardLoot> DropItems()
    {
        return DropInfo.SelectMany(d => d.groups.SelectMany(g => g.rewards)).Reverse().ToList();
    }
}