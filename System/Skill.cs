using UnityEngine;
using System.Collections;

public class Skill
{
    public Creature Creature { get; private set; }
    public SkillInfo Info { get; private set; }
    public short Index { get; private set; }
    public short Level { get; private set; }

    public Skill(short index, Creature creature, SkillInfo info, short level)
    {
        Index = index;
        Creature = creature;
        Info = info;
        Level = level;
    }

    public void LevelUP(short add_level)
    {
        Level+= add_level;
        Creature.CalculateStat();
    }

    public string GetTooltip()
    {
        if (Info.Type == eSkillType.leader_active)
            return (Info.GetTooltip() + "\n\n" + Info.DescTotal(Creature.GradePercent, Creature.Level)).Trim();
        return (string.Format("{0} {1}\n\n{2}",Localization.Format("Level",Level),Info.GetTooltip(),Info.DescTotal(Creature.GradePercent, Level))).Trim();
    }

    public string GetLevelupTooltip(int add_level)
    {
        string res = Info.DescPerLevel(Creature.GradePercent, Level, Level+add_level);
        return res.Trim();
    }

    public void AddStats(StatInfo info, StatInfo base_stat, float grade_percent)
    {
        Info.AddStats(info, base_stat, Creature.Info.AttackType, grade_percent, Level);
    }
}
