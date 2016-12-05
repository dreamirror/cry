using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SharedData;

public class EquipCategory
{
    public string Name { get; private set; }
    public List<EquipInfo> Equips { get; private set; }

    public EquipCategory(string name, List<EquipInfo> equips)
    {
        Name = name;
        Equips = equips;
    }
}

public class EquipInfoManager : InfoManager<EquipInfo, EquipInfo, EquipInfoManager>
{
    public Dictionary<string, EquipCategory> Categories { get; private set; }
    public Dictionary<string, CategoryInfo> CategoryInfos { get; private set; }
    public EquipStatInfo StatInfo { get; private set; }
    protected override void PreLoadData(XmlNode node)
    {
        XmlNode category_list = node.SelectSingleNode("CategoryList");
        CategoryInfos = new Dictionary<string, CategoryInfo>();
        foreach(XmlNode category_node in category_list.ChildNodes)
        {
            CategoryInfo category = new CategoryInfo(category_node);
            CategoryInfos.Add(category.Name, category);
        }
        XmlNode equip_stat_info_node = node.SelectSingleNode("EquipStatInfo");
        StatInfo = new EquipStatInfo(equip_stat_info_node);
    }

    protected override void PostLoadData(XmlNode node)
    {
        Categories = new Dictionary<string, EquipCategory>();
        foreach (var category in m_Infos.GroupBy(e => e.Category))
        {
            Categories.Add(category.Key, new EquipCategory(category.Key, category.ToList()));
        }
    }

    public EquipCategory GetCategory(string category_name)
    {
        EquipCategory category;
        if (Categories.TryGetValue(category_name, out category) == false)
            throw new System.Exception(string.Format("GetCategory Error : {0}", category_name));
        return category;
    }

    public CategoryInfo GetCategoryInfo(string category_name)
    {
        CategoryInfo category_info;
        if (CategoryInfos.TryGetValue(category_name, out category_info) == false)
            throw new System.Exception(string.Format("GetCategoryInfo Error : {0}", category_name));
        return category_info;
    }

    public StatInfo AddStats(EquipInfo equipInfo, int enchant, StatInfo stat_info)
    {
        CategoryInfo categoryInfo = GetCategoryInfo(equipInfo.Category);
        return AddStats(categoryInfo.EquipType, categoryInfo.AttackType, equipInfo.Grade, enchant, stat_info);
    }

    public StatInfo AddStats(eEquipType type, eAttackType attack_type, int equip_grade, int enchant, StatInfo stat_info)
    {
        EquipStat stat = null;
        switch(type)
        {
            case eEquipType.weapon:
                stat = StatInfo.Weapons.Find(e => e.Grade == equip_grade && e.Enchant == enchant);
                if(stat == null)
                    throw new System.Exception(string.Format("Can't Find Weapon Stat Info : grade({0}), enchant({1})", equip_grade, enchant));
                stat_info.AddValue(eStatType.PhysicAttack, stat.Value);
                stat_info.AddValue(eStatType.MagicAttack, stat.Value);
                stat_info.AddValue(eStatType.Heal, stat.Value);
                break;

            case eEquipType.armor:
                stat = StatInfo.Armors.Find(e => e.Grade == equip_grade && e.Enchant == enchant);
                if (stat == null)
                    throw new System.Exception(string.Format("Can't Find Armor Stat Info : grade({0}), enchant({1})", equip_grade, enchant));
                switch (attack_type)
                {
                    case eAttackType.physic:
                        stat_info.AddValue(eStatType.PhysicDefense, stat.Value);
                        stat_info.AddValue(eStatType.MagicDefense, (int)(stat.Value * stat.DefenseRate));
                        break;
                    case eAttackType.magic:
                        stat_info.AddValue(eStatType.MagicDefense, stat.Value);
                        stat_info.AddValue(eStatType.PhysicDefense, (int)(stat.Value * stat.DefenseRate));
                        break;
                    case eAttackType.heal:
                        stat_info.AddValue(eStatType.PhysicDefense, (int)(stat.Value * stat.DefenseRate));
                        stat_info.AddValue(eStatType.MagicDefense, (int)(stat.Value * stat.DefenseRate));
                        break;
                }
                break;
        }

        return stat_info;
    }
}

public class CategoryInfo
{
    public string Name { get; private set; }
    public eAttackType AttackType { get; private set; }
    public eEquipType EquipType { get; private set; }
    public CategoryInfo(XmlNode node)
    {
        Name = node.Attributes["category"].Value;
        AttackType = (eAttackType)Enum.Parse(typeof(eAttackType), node.Attributes["type"].Value);
        EquipType = (eEquipType)Enum.Parse(typeof(eEquipType), node.Attributes["equip_type"].Value);
    }
}


public class EquipStat
{
    public short Grade { get; private set; }
    public short Enchant { get; private set; }
    public int Value { get; private set; }
    public double DefenseRate { get; private set; }
    public int EnchantCost { get; private set; }

    public EquipStat(XmlNode node)
    {
        Grade = short.Parse(node.Attributes["grade"].Value);
        Enchant = short.Parse(node.Attributes["enchant"].Value);
        Value = int.Parse(node.Attributes["value"].Value);
        EnchantCost = int.Parse(node.Attributes["cost"].Value);

        XmlAttribute rate_attr = node.Attributes["rate"];
        if (rate_attr != null)
            DefenseRate = double.Parse(rate_attr.Value);
        else
            DefenseRate = 0f;
    }
}

public class EquipStatInfo
{
    public List<EquipStat> Weapons { get; private set; }
    public List<EquipStat> Armors { get; private set; }
    public EquipStatInfo(XmlNode node)
    {
        Weapons = new List<EquipStat>();
        Armors = new List<EquipStat>();
        foreach(XmlNode equip_type_node in node.ChildNodes)
        {
            eEquipType equip_type = (eEquipType)Enum.Parse(typeof(eEquipType), equip_type_node.Attributes["type"].Value);

            List<EquipStat> current = null;
            if(equip_type == eEquipType.weapon)
                current = Weapons;
            else if (equip_type == eEquipType.armor)
                current = Armors;

            foreach (XmlNode data in equip_type_node.ChildNodes)
            {
                current.Add(new EquipStat(data));
            }
        }

    }
}
public class EquipEnchantInfo
{
    public short Enchant { get; private set; }
    public List<ItemInfoBase> Stuffs { get; private set; }

    public EquipEnchantInfo(XmlNode node, short grade)
    {
        Enchant = short.Parse(node.Attributes["enchant"].Value);

        Stuffs = new List<ItemInfoBase>();
        if (Enchant < 5)
        {
            Stuffs.Add(StuffInfoManager.Instance.GetInfoByID(node.Attributes["stuff_1"].Value));
            //Stuffs.Add(StuffInfoManager.Instance.GetInfoByID(node.Attributes["stuff_2"].Value));
            //Stuffs.Add(StuffInfoManager.Instance.GetInfoByID(node.Attributes["stuff_3"].Value));
        }
        else if(grade < 6)
            Stuffs.Add(StuffInfoManager.Instance.GetInfoByID(node.Attributes["stuff_1"].Value));
    }
}

public class EquipInfo : InfoBaseString
{
    public string Name { get; private set; }
    public string Category { get; private set; }
    public short Grade { get; private set; }
    public List<EquipEnchantInfo> Enchants { get; private set; }
    public CategoryInfo CategoryInfo { get; private set; }
    public List<EquipStat> Stats { get; private set; }
    public string IconID { get; private set; }

    public string NextEquipID { get; private set; }
    override public void Load(XmlNode node)
    {
        base.Load(node);
        Name = node.Attributes["name"].Value;
        Grade = short.Parse(node.Attributes["grade"].Value);
        Category = node.Attributes["category"].Value;
        CategoryInfo = EquipInfoManager.Instance.GetCategoryInfo(Category);

        if (CategoryInfo.EquipType == eEquipType.weapon)
            Stats = EquipInfoManager.Instance.StatInfo.Weapons.FindAll(e => e.Grade == Grade);
        else
            Stats = EquipInfoManager.Instance.StatInfo.Armors.FindAll(e => e.Grade == Grade);

        XmlAttribute iconIDAttr = node.Attributes["icon_id"];
        if (iconIDAttr != null)
            IconID = iconIDAttr.Value;
        else
            IconID = ID;

        XmlNodeList enchantNodes = node.SelectNodes("Enchant");
        //if (enchantNodes.Count != 6)
        //    throw new System.Exception(string.Format("EquipInfo({0}) : EnchantCount({1}) failed", ID, enchantNodes.Count));

        Enchants = new List<EquipEnchantInfo>();
        int enchant_level = 0;
        foreach (XmlNode enchantNode in enchantNodes)
        {
            if (enchantNode.NodeType == XmlNodeType.Comment)
                continue;

            EquipEnchantInfo enchant_info = new EquipEnchantInfo(enchantNode, Grade);
            if (enchant_info.Enchant != enchant_level)
                throw new System.Exception(string.Format("EquipInfo({0}) : EnchantLevel ({1}) failed", ID, enchant_level));
            Enchants.Add(enchant_info);

            ++enchant_level;
        }

        XmlAttribute next_idn_attr = node.Attributes["next_equip_id"];
        if (next_idn_attr != null)
            NextEquipID = next_idn_attr.Value;
        else
            NextEquipID = null;
    }

    public short NextEquipLevel
    {
        get
        {
            if (NextEquipID != null)
            {
                EquipInfo info = EquipInfoManager.Instance.GetInfoByID(NextEquipID);
                if (info != null)
                    return info.Grade;
            }
            return short.MaxValue;
        }
    }

    public int EnchantCost(int grade)
    {
        EquipStat stat = Stats.Find(e => e.Enchant == grade);
        if (stat == null)
            throw new System.Exception(string.Format("can't find enchant cost info :  grade {0}", grade));
        return stat.EnchantCost;
    }
    public string Tooltip(int enchant)
    {
        StatInfo stat = new StatInfo();
        EquipInfoManager.Instance.AddStats(this, enchant, stat);
        
        return stat.Tooltip(CategoryInfo.AttackType);
    }
    public string GetName()
    {
        return Localization.Format("EquipTitle", Grade, Name);
    }

}
