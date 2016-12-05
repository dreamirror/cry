using MNS;
using PacketInfo;
using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class RuneInfoManager : InfoManager<RuneInfo, ItemInfoBase, RuneInfoManager>
{
    public SlotInfo Slot { get; private set; }
    public List<RuneGrade> Grades { get; private set; }

    static public bool IsRune(long idn)
    {
        return 50000 <= idn && idn < 60000;
    }

    override protected void PreLoadData(XmlNode node)
    {
        base.PreLoadData(node);

        Slot = new SlotInfo(node.SelectSingleNode("SlotInfo"));

        Grades = new List<RuneGrade>();
        Grades.Add(null);
        foreach (XmlNode child in node.SelectSingleNode("RuneGrade").ChildNodes)
        {
            Grades.Add(new RuneGrade(child));
        }
    }

    public RuneInfo GetRuneInfoByIdn(long key)
    {
        return GetInfoByIdn(key) as RuneInfo;
    }
}

public class RuneGrade
{
    public short Grade { get; private set; }
    public short MaxLevel { get; private set; }
    public int SalePrice { get; private set; }
    public pe_GoodsType UnequipPriceType { get; private set; }
    public int UnequipPrice { get; private set; }
    public int UpgradeCost { get; private set; }
    public short EnchantCostRate { get; private set; }

    public RuneGrade(XmlNode node)
    {
        Grade = short.Parse(node.Attributes["grade"].Value);
        MaxLevel = short.Parse(node.Attributes["max_level"].Value);
        SalePrice = int.Parse(node.Attributes["sale_price"].Value);
        UnequipPriceType = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), node.Attributes["unequip_price_type"].Value);
        UnequipPrice = int.Parse(node.Attributes["unequip_price"].Value);
        UpgradeCost = int.Parse(node.Attributes["upgrade_cost"].Value);
        EnchantCostRate = short.Parse(node.Attributes["enchant_cost_rate"].Value);
    }
}

public class RuneInfo : ItemInfoGradeBase
{
    public SkillInfo Skill { get; private set; }
    public RuneGrade GradeInfo { get; private set; }
    public eRuneEquipType EquipType { get; private set; }

    public override eItemType ItemType { get { return eItemType.Rune; } }

    override public void Load(XmlNode node)
    {
        base.Load(node);

        string skill_id = node.Attributes["skill_id"].Value;
        Skill = SkillInfoManager.Instance.GetInfoByID(skill_id);

        short grade = short.Parse(node.Attributes["grade"].Value);
        GradeInfo = RuneInfoManager.Instance.Grades[grade];

        EquipType = (eRuneEquipType)Enum.Parse(typeof(eRuneEquipType), node.Attributes["equip_type"].Value);
    }

    public override string GetTooltip()
    {
        return Localization.Format("RuneInfoTooltip", Name, GradeInfo.MaxLevel) + "\n\n[99ff99]" + Skill.Desc;
    }

    public bool CheckEquipType(eAttackType attack_type)
    {
        if (EquipType == eRuneEquipType.all)
            return true;

        return (short)EquipType == (short)attack_type;
    }
}

public class RuneTable : CSVData<short>
{
    public short Level;
    public float RateEnchantNormal, RateEnchantPremium;
    public int CostEnchantNormal, CostEnchantPremium;

    static bool check_index = false;
    static int rate_enchant_normal_index, rate_enchant_premium_index, cost_enchant_normal_index, cost_enchant_premium_index;

    public short Key { get { return Level; } }
    public void Load(CSVReader reader, CSVReader.RowData row)
    {
        CheckIndex(reader);

        Level = short.Parse(row.GetData(0));
        RateEnchantNormal = float.Parse(row.GetData(rate_enchant_normal_index));
        RateEnchantPremium = float.Parse(row.GetData(rate_enchant_premium_index));
        CostEnchantNormal = int.Parse(row.GetData(cost_enchant_normal_index));
        CostEnchantPremium = int.Parse(row.GetData(cost_enchant_premium_index));
    }
    //level,enchant_normal_rate,enchant_premium_rate,enchant_normal_cost,enchant_premium_cost
    void CheckIndex(CSVReader reader)
    {
        if (check_index == true)
            return;

        rate_enchant_normal_index = reader.GetFieldIndex("enchant_normal_rate");
        rate_enchant_premium_index = reader.GetFieldIndex("enchant_premium_rate");
        cost_enchant_normal_index = reader.GetFieldIndex("enchant_normal_cost");
        cost_enchant_premium_index = reader.GetFieldIndex("enchant_premium_cost");

        check_index = true;
    }
}
public class RuneTableManager : InfoManagerCSV<RuneTableManager, short, RuneTable>
{
    public float GetEnchantPercent(short rune_level, bool is_premium)
    {
        if (is_premium)
            return m_Datas[rune_level].RateEnchantPremium * 100;
        else
            return m_Datas[rune_level].RateEnchantNormal * 100;

    }

    public int GetCostValueEnchant(short rune_grade, short rune_level, bool is_premium)
    {
        int grade_rate = RuneInfoManager.Instance.Grades[rune_grade].EnchantCostRate;

        if (is_premium)
            return m_Datas[rune_level].CostEnchantPremium * grade_rate;
        else
            return m_Datas[rune_level].CostEnchantNormal * grade_rate;
    }
}
