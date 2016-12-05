using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketInfo;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
//这个是所有角色的类
public enum eCreatureSort
{
    Grade, //
    Level,//等级
    Enchant,
    Power,
    IDN,
    HP, //血量
    Attack,
    Defense,
}

public class CreatureManager : SaveDataSingleton<List<pd_CreatureData>, CreatureManager> //角色的管理类 SaveDataSingleton 是一个单例类
{
    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////
    override public void Init(List<pd_CreatureData> datas, List<pd_CreatureData> file_datas)
    {
        Creatures = new List<Creature>();
        if (datas == null) return;
        foreach (pd_CreatureData data in datas)
        {
            Creature creature = new Creature(data); //根据传进来的数据创建Creaure
            Creatures.Add(creature);
        }
        Instance.Sort();

        SetUpdateNotify();
    }

    override protected List<pd_CreatureData> CreateSaveData()
    {
        return Creatures.Select(c => c.CreateSaveData()).ToList();
    }

    ////////////////////////////////////////////////////////////////
    public bool IsUpdateNotify { get; private set; } //是不是更新修改
    public bool IsNotify { get; private set; } //是不是 修改

    public List<Creature> Creatures { get; private set; }

    bool IsSorted = false;
    public Creature Add(pd_CreatureData data) //通过data的idx查找是不是有creature 如果没有 就创建 如果有就根据data 的值改变他的属性
    {
        Creature creature = GetInfoByIdx(data.creature_idx);
        if (creature == null)
        {
            creature = new Creature(data);
            Creatures.Add(creature);
        }
        else
            creature.Set(data);
        IsSorted = false;

        SetUpdateNotify();
        Save();
        CreatureBookManager.Instance.CreatureInfoChanged(data.creature_idn);
        return creature;
    }

    public void AddTutorialCard(Creature creature)
    {
        Creatures.Add(creature);        
    }

    public void Update(pd_CreatureData data)
    {
        GetInfoByIdx(data.creature_idx).Set(data);
        IsSorted = false;

        SetUpdateNotify();
        CreatureManager.Instance.Save();
        EquipManager.Instance.Save();
        RuneManager.Instance.Save();
    }

    public void Remove(long creature_idx)
    {
        EquipManager.Instance.RemoveByCreatureIdx(creature_idx);
        RuneManager.Instance.RemoveRune(creature_idx);
        Creatures.RemoveAll(c => c.Idx == creature_idx);

        SetUpdateNotify();

        TeamDataManager.Instance.RemoveCreature(creature_idx);
        TeamDataManager.Instance.SetUpdateNotify();
        Save();
    }

    public Creature GetInfoByIdx(long idx) //通过Id返回相应的Creature
    {
        return Creatures.Find(c => c.Idx == idx);
    }

    public bool Contains(Creature creature)
    {
        return Creatures.Contains(creature);
    }

    public void SetUpdateNotify()
    {
        IsUpdateNotify = true;
    }

    public void UpdateNotify()
    {
        if (IsUpdateNotify == false || Creatures == null)
            return;

        IsUpdateNotify = false;

        Creatures.ForEach(c => c.CheckNotify());
        IsNotify = Creatures.Any(c => c.Grade > 0 && c.IsNotify);

        TeamDataManager.Instance.SetUpdateNotify();
    }

    public void SkillLevelUP(Creature creature, int skill_index, short add_level)
    {
        creature.Skills[skill_index].LevelUP(add_level);
        SetUpdateNotify();
        IsSorted = false;
        Save();
    }

    public void UpdateEquip(Creature creature, pd_EquipData equip_data)
    {
        Equip equip = null;
        if (creature.Weapon.EquipIdx == equip_data.equip_idx)
            equip = creature.Weapon;
        else if (creature.Armor.EquipIdx == equip_data.equip_idx)
            equip = creature.Armor;
        else
            throw new Exception("invalid equip");

        equip.Set(equip_data);

        creature.CalculateStat();
        IsSorted = false;

        SetUpdateNotify();
        Save();
        EquipManager.Instance.Save();
    }

    public List<Creature> GetFilteredList(System.Func<Creature, bool> func)
    {
        return Creatures.Where(func).ToList();
    }

    public List<Creature> GetSortedList(eCreatureSort sort, bool is_ascending = false, List<Creature> creatures = null)
    {
        if (creatures == null)
            creatures = Creatures;

        IOrderedEnumerable<Creature> list = null;

        if (is_ascending == true)
        {
            switch (sort)
            {
                case eCreatureSort.Grade: list = creatures.OrderBy(c => c.Grade); break;
                case eCreatureSort.Level: list = creatures.OrderBy(c => c.Level); break;
                case eCreatureSort.Enchant: list = creatures.OrderBy(c => c.Enchant); break;
                case eCreatureSort.IDN: list = creatures.OrderBy(c => c.Info.IDN); break;
                case eCreatureSort.Power: list = creatures.OrderBy(c => c.Power); break;
                case eCreatureSort.HP: list = creatures.OrderBy(c => c.StatTotal.MaxHP); break;
                case eCreatureSort.Attack: list = creatures.OrderBy(c => c.StatTotal.GetAttack()); break;
                case eCreatureSort.Defense: list = creatures.OrderBy(c => c.StatTotal.GetDefense()); break;
            }
        }
        else
        {
            switch (sort)
            {
                case eCreatureSort.Grade: list = creatures.OrderByDescending(c => c.Grade); break;
                case eCreatureSort.Level: list = creatures.OrderByDescending(c => c.Level); break;
                case eCreatureSort.Enchant: list = creatures.OrderByDescending(c => c.Enchant); break;
                case eCreatureSort.IDN: list = creatures.OrderByDescending(c => c.Info.IDN); break;
                case eCreatureSort.Power: list = creatures.OrderByDescending(c => c.Power); break;
                case eCreatureSort.HP: list = creatures.OrderByDescending(c => c.StatTotal.MaxHP); break;
                case eCreatureSort.Attack: list = creatures.OrderByDescending(c => c.StatTotal.GetAttack()); break;
                case eCreatureSort.Defense: list = creatures.OrderByDescending(c => c.StatTotal.GetDefense()); break;
            }
        }
        if (list == null)
            return creatures;

        if (is_ascending == true)
            return list.ThenBy(c => c.Grade).ThenBy(c => c.Level).ThenBy(c => c.Enchant).ThenBy(c => c.Power).ThenBy(c => c.Info.IDN).ToList();
        return list.ThenByDescending(c => c.Grade).ThenByDescending(c => c.Level).ThenByDescending(c => c.Enchant).ThenByDescending(c => c.Power).ThenByDescending(c => c.Info.IDN).ToList();
    }

    public void Sort()
    {
        if (IsSorted) return;
        Creatures = Creatures.OrderByDescending(c => c.Grade).ThenByDescending(c => c.Level).ThenByDescending(c => c.Enchant).ThenByDescending(c => c.Power).ThenByDescending(c => c.Info.IDN).ToList();
        IsSorted = true;
    }

    public void SetSort()
    {
        IsSorted = false;
    }
}

public class Creature : ICreature
{
    public bool IsNotify { get; private set; }

    public StatInfo StatBase { get; private set; }
    public StatInfo StatGrade { get; private set; }
    public StatInfo StatAdd { get; private set; }
    public StatInfo StatTotal { get; private set; }

    public List<Skill> Skills { get; private set; }
    public Skill TeamSkill { get; private set; }

    public List<Skill> GetSkillsByType(eSkillType type)
    {
        return Skills.Where(s => s.Info.Type == type && s.Info.ActionName != "attack").ToList();
    }

    public Equip Weapon { get; private set; }
    public Equip Armor { get; private set; }
    public List<Rune> Runes { get; private set; }

    public short Grade { get; private set; }
    public short Enchant { get; private set; }
    public short EnchantPoint { get; private set; }
    public short Level { get; private set; }
    public int Exp { get; private set; }
    public bool IsLock { get; set; }

    public int SalePrice { get { return CreatureInfoManager.Instance.Grades[Grade].sale_price; } }

    public int RuneSlotCount
    {
        get
        {
            int rune_slot_count = Grade - 1;
            if (Grade == 6 && Enchant > 5)
                rune_slot_count = Math.Min(10, rune_slot_count + Enchant - 5);
            return rune_slot_count;
        }
    }

    public short CalculateEnchantPoint(Creature target)
    {
        int grade_gap = Math.Max(0, target.Grade - Grade);
        return CreatureInfoManager.Instance.EnchantInfos[grade_gap].enchant_point;
    }

    public string GetLevelText(bool use_limit = true)
    {
        if (use_limit && IsLevelLimit)
            return Localization.Format("HeroLevel", Level)+"[sup] MAX[/sup]";
        return Localization.Format("HeroLevel", Level);
    }

    public string GetEnchantText()
    {
        if (Enchant == 0)
            return "";
        return Localization.Format("HeroEnchant", Enchant);
    }

    public bool IsLevelLimit { get { return Level >= LevelLimit; } }
    public short LevelLimit
    {
        get
        {
            return CreatureInfoManager.Instance.Grades[Grade].level_max;
        }
    }
    public bool AvailableLevelup
    {
        get
        {
            int exp_max = LevelInfoManager.Instance.GetCharacterExpMax(Level);
            if (exp_max == 0)
                return false;

            if (Level >= LevelLimit)
                return false;

            return (exp_max - Exp) <= Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_exp_powder);
        }
    }

    public float GradePercent { get { return CreatureInfoManager.Instance.Grades[Grade].enchants[Enchant].stat_percent; } }
    public int EnchantGold { get { return CreatureInfoManager.Instance.EnchantInfos[Grade].enchant_gold; } }
    public int MixGold { get { return CreatureInfoManager.Instance.EnchantInfos[Grade].mix_gold; } }
    public int EvolveGold { get { return CreatureInfoManager.Instance.EnchantInfos[Grade].evolve_gold; } }

    public int Power
    {
        get
        {
            return CalculatePower(StatTotal, Skills.Sum(e => e.Level), GradePercent);
        }
    }

    static public int CalculatePower(StatInfo stat, int total_skill_level, float grade_percent)
    {
        return stat.MaxHP / 10 + stat.PhysicAttack + stat.PhysicDefense
            + stat.MagicAttack + stat.MagicDefense + stat.Heal + (int)(total_skill_level * 10 * grade_percent);
    }

    public Creature(pd_CreatureData data, pd_EquipData weapon = null, pd_EquipData armor = null, List<Rune> runes = null)
    {
        SetInternal(data, weapon, armor, runes);
    }

    public Creature(long creature_idx, int creature_idn, short skin_index, short grade, short enchant, short level)
    {
        Info = CreatureInfoManager.Instance.GetInfoByIdn(creature_idn);
        SkinName = Info.GetSkinName(skin_index);
        Grade = grade;
        Enchant = enchant;
        Level = level;
        Idx = creature_idx;

        EquipInfo equip_info = EquipInfoManager.Instance.GetInfoByIdn(Info.EquipWeaponCategory.Equips.First().IDN);
        Weapon = new Equip(equip_info);
        equip_info = EquipInfoManager.Instance.GetInfoByIdn(Info.EquipArmorCategory.Equips.First().IDN);
        Armor = new Equip(equip_info);

        Skills = new List<Skill>();
        for (short i = 0; i < Info.Skills.Count; ++i)
        {
            SkillInfo skillInfo = Info.Skills[i];
            short skill_level = 1;
            Skills.Add(new Skill(i, this, skillInfo, skill_level));
        }
        if (Info.TeamSkill != null)
            TeamSkill = new Skill(-1, this, Info.TeamSkill, Level);

        Runes = new List<Rune>();

        CalculateStat();
    }

    public Creature(Creature c)
    {
        Idx = c.Idx;
        Info = c.Info;
        SkinName = c.SkinName;

        Weapon = c.Weapon;
        Armor = c.Armor;

        Level = c.Level;
        Grade = (short)(c.Grade + 1);
        Enchant = c.Enchant;
        EnchantPoint = c.EnchantPoint;

        Skills = c.Skills;

        CalculateStat();
    }

    public void Set(pd_CreatureData data)
    {
        SetInternal(data, null, null, null);
    }

    void SetInternal(pd_CreatureData data, pd_EquipData weapon, pd_EquipData armor, List<Rune> runes) //设置Creature对象的信息
    {
        this.Info = CreatureInfoManager.Instance.GetInfoByIdn(data.creature_idn);

        Idx = data.creature_idx;
        SkinName = Info.GetSkinName(data.skin_index);
        Grade = data.creature_grade;
        Level = data.creature_level;
        Enchant = data.creature_enchant;
        EnchantPoint = data.creature_enchant_point;
        Exp = data.creature_exp;
        IsLock = data.is_lock;

        Skills = new List<Skill>();
        for (short i = 0; i < Info.Skills.Count; ++i)
        {
            SkillInfo skillInfo = Info.Skills[i];
            short skill_level = Level;
            if (i > 0)
            {
                if (i - 1 < data.skill_level.Count)
                    skill_level = data.skill_level[i - 1];
                else
                    skill_level = 1;
            }
            Skills.Add(new Skill(i, this, skillInfo, skill_level));
        }
        if(Info.TeamSkill != null)
            TeamSkill = new Skill(-1, this, Info.TeamSkill, Level);

        if (weapon == null)
            Weapon = EquipManager.Instance.GetEquipByIdx(data.equip_idx[0]);
        else
            Weapon = new Equip(weapon);
        if (armor == null)
            Armor = EquipManager.Instance.GetEquipByIdx(data.equip_idx[1]);
        else
            Armor = new Equip(armor);

        if (runes != null)
            Runes = runes;
        else
            Runes = RuneManager.Instance.GetRunesByCreatureIdx(this.Idx);

        CalculateStat();
    }

    public void Loot()
    {
        Grade = 1;
        CalculateStat();
        CreatureManager.Instance.SetUpdateNotify();
    }

    public void CalculateStat()
    {
        StatBase = new StatInfo(Info.Stat);
        StatBase.AddRange(Info.StatIncrease, Level);

        StatGrade = new StatInfo(StatBase);
        StatGrade.Multiply(GradePercent);

        StatAdd = new StatInfo();
        if(Weapon != null)
            Weapon.AddStats(StatAdd);
        if(Armor != null)
            Armor.AddStats(StatAdd);

        foreach(var skill in Skills.Where(s => s.Info.Type == eSkillType.passive || s.Info.Type == eSkillType.passive_etc))
        {
            skill.AddStats(StatAdd, StatGrade, GradePercent);
        }

        foreach (var rune in Runes)
        {
            rune.Info.Skill.AddStats(StatAdd, StatGrade, Info.AttackType, 1f, rune.StatLevel);
        }

        StatTotal = new StatInfo(StatGrade);
        StatTotal.AddRange(StatAdd);
    }

    public StatInfo CalculateBattleStat(float grade_percent)
    {
        StatInfo stat_grade = new StatInfo(StatBase);
        stat_grade.Multiply(grade_percent);

        StatInfo battle_stat = new StatInfo(stat_grade);
        Weapon.AddStats(battle_stat);
        Armor.AddStats(battle_stat);

        foreach (var skill in Skills.Where(s => s.Info.Type == eSkillType.passive || s.Info.Type == eSkillType.passive_etc))
        {
            skill.AddStats(battle_stat, stat_grade, grade_percent);
        }

        foreach (var rune in Runes)
        {
            rune.Info.Skill.AddStats(battle_stat, stat_grade, Info.AttackType, 1f, rune.StatLevel);
        }

        return battle_stat;
    }

    public pd_CreatureData CreateSaveData()
    {
        pd_CreatureData data = new pd_CreatureData();
        data.creature_idx = Idx;
        data.creature_idn = Info.IDN;

        data.creature_grade = Grade;
        data.creature_enchant = Enchant;
        data.creature_enchant_point = EnchantPoint;
        data.creature_level = Level;
        data.creature_exp = Exp;

        data.skill_level = new List<short>();
        foreach (var skill in Skills.GetRange(1, Skills.Count - 1))
        {
            data.skill_level.Add(skill.Level);
        }
        data.equip_idx = new List<long>();
        data.equip_idx.Add(Weapon.EquipIdx);
        data.equip_idx.Add(Armor.EquipIdx);
        data.is_lock = IsLock;

        return data;
    }


    public BattleEndCreature UpdateExp(pd_CreatureExpAddInfo exp_add_info)
    {
        BattleEndCreature param = new BattleEndCreature(this);
        if (exp_add_info == null)
            return param;

        param.AddExp = exp_add_info.add_creature_exp;

        if (exp_add_info.creature_level > Level)
            param.IsLevelUp = true;

        // apply
        Level = exp_add_info.creature_level;
        Exp = exp_add_info.creature_exp;

        if (param.IsLevelUp)
        {
            CalculateStat();
            CreatureManager.Instance.SetSort();
        }

        return param;
    }

    public void CheckNotify()
    {
        if (Level > 1)
        {
            Weapon.CheckNotify(Level);
            Armor.CheckNotify(Level);
            IsNotify = AvailableSkillEnchant || Weapon.IsNotify || Armor.IsNotify;
        }
        else
            IsNotify = false;
    }

    public bool AvailableSkillEnchant
    {
        get
        {
            return Skills.Exists(s => s.Info.ActionName != "attack" && s.Level < this.Level);
        }
    }
    public void SkillEnchantAllMax()
    {
        Skills.Where(e=>e.Info.ActionName != "attack").ToList().ForEach(s => s.LevelUP((short)(Level - s.Level)));
        CreatureManager.Instance.SetUpdateNotify();
        CreatureManager.Instance.UpdateNotify();
        CreatureManager.Instance.Save();
    }
    public int AllSkillEnchantCost()
    {
        int res = 0;

        int gold_base = GameConfig.Get<int>("skill_enchant_gold_base");
        int gold_per_level = GameConfig.Get<int>("skill_enchant_increase_gold_per_level");

        var enchanting_skills = Skills.Where(e => e.Info.ActionName != "attack");
        foreach(var skill in enchanting_skills)
        {
            if (Level == skill.Level) continue;
            // end level total gold - current level total gold
            res += (gold_base * (Level - skill.Level)) + gold_per_level * ((Level - 2) * (Level - 1) - (skill.Level - 2) * (skill.Level - 1)) / 2;
        }

        var event_info = EventHottimeManager.Instance.GetInfoByID("hero_skill_enchant_discount");
        if (event_info != null)
        {
            res = (int)(res * event_info.Percent);
        }
        return res;
    }
    public string GetTooltip()
    {
        return string.Format("{0}", Info.Name);
    }

    public void SetEnchant(short enchant, short enchant_point)
    {
        Enchant = enchant;
        EnchantPoint = enchant_point;
        CalculateStat();
    }

    public string GetStatString(bool default_value)
    {
        var stat_string = "";
        foreach (eStatType type in Enum.GetValues(typeof(eStatType)))
        {
            if ((int)type >= 100)
                continue;

            if (default_value != StatInfo.IsDefaultValue(Info.AttackType, type))
                continue;

            int value = StatTotal.GetValue(type);
            if (value == 0) continue;

            if (string.IsNullOrEmpty(stat_string) == false)
                stat_string += "\n";

            if (StatInfo.IsPercentValue(type) == true)
            {
                stat_string += string.Format("{0} : [c][7D0000]{1}%[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value / 100f);
            }
            else
            {
                stat_string += string.Format("{0} : [c][7D0000]{1}[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value);
            }
        }

        return stat_string;
    }

    public void AddRune(Rune rune)
    {
        Runes.Add(rune);
        CalculateStat();
    }

    public void EnchantRune(Rune rune)
    {
        int rune_node = Runes.FindIndex(r => r.RuneIdx == rune.RuneIdx);
        if (rune_node < 0)
            return;
        Runes[rune_node] = rune;
        CalculateStat();
        CreatureManager.Instance.Save();
        RuneManager.Instance.Save();
    }

    public void RemoveRune(Rune rune)
    {
        Runes.Remove(rune);
        CalculateStat();
        CreatureManager.Instance.Save();
        RuneManager.Instance.Save();
    }
}
