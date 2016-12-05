using MNS;
using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

// condition
class QuestConditionFactory : AbstractFactory<eQuestCondition, QuestConditionBase, QuestConditionFactory>
{
    public QuestConditionFactory()
    {
        Add(eQuestCondition.progress, typeof(QuestConditionProgress));
        Add(eQuestCondition.time, typeof(QuestConditionTime));
    }
}

abstract public class QuestConditionBase
{
    abstract public eQuestCondition ConditionType { get; }
    virtual public long ProgressMax { get { return 0; } }
    virtual public void Load(XmlNode node) { }
}

public class QuestConditionProgress : QuestConditionBase
{
    override public eQuestCondition ConditionType { get { return eQuestCondition.progress; } }

    override public void Load(XmlNode node)
    {
        progress_max = long.Parse(node.Attributes["progress_max"].Value);
    }

    public long progress_max { get; private set; }
    override public long ProgressMax { get { return progress_max; } }
}

public class QuestConditionTime : QuestConditionBase
{
    override public eQuestCondition ConditionType { get { return eQuestCondition.time; } }

    override public void Load(XmlNode node)
    {
        time_begin = TimeSpan.Parse(node.Attributes["time_begin"].Value);
        time_end = TimeSpan.Parse(node.Attributes["time_end"].Value);
    }

    public TimeSpan time_begin { get; private set; }
    public TimeSpan time_end { get; private set; }
}

// trigger
class QuestTriggerFactory : AbstractFactory<eQuestTrigger, QuestTriggerBase, QuestTriggerFactory>
{
    public QuestTriggerFactory()
    {
        Add(eQuestTrigger.map_clear, typeof(QuestTriggerMapClear));
        Add(eQuestTrigger.map_try, typeof(QuestTriggerMapTry));
        Add(eQuestTrigger.loot_hero, typeof(QuestTriggerLootHero));
        Add(eQuestTrigger.skill_enchant, typeof(QuestTriggerSkillEnchant));

        Add(eQuestTrigger.map_boss_clear, typeof(QuestTriggerMapBossClear));
        Add(eQuestTrigger.monster_kill, typeof(QuestTriggerMonsterKill));
        Add(eQuestTrigger.achievement_monster_kill, typeof(AchievementTriggerMonsterKill));

        Add(eQuestTrigger.achievement_pvp_battle, typeof(AchievementTriggerPVPBattle));
        Add(eQuestTrigger.pvp_battle, typeof(QuestTriggerPVPBattle));

        Add(eQuestTrigger.achievement_boss_battle, typeof(AchievementTriggerBossBattle));
        Add(eQuestTrigger.boss_battle, typeof(QuestTriggerBossBattle));

        Add(eQuestTrigger.achievement_player_level_up, typeof(AchievementTriggerPlayerLevelUP));
        Add(eQuestTrigger.player_level_up, typeof(QuestTriggerPlayerLevelUP));

        Add(eQuestTrigger.achievement_get_hero, typeof(AchievementTriggerGetHero));
        Add(eQuestTrigger.achievement_get_hero_3, typeof(AchievementTriggerGetHero_3));
        Add(eQuestTrigger.achievement_get_hero_4, typeof(AchievementTriggerGetHero_4));
        Add(eQuestTrigger.achievement_get_hero_5, typeof(AchievementTriggerGetHero_5));
        Add(eQuestTrigger.achievement_get_hero_6, typeof(AchievementTriggerGetHero_6));

        Add(eQuestTrigger.get_hero_3, typeof(QuestTriggerGetHero3));
        Add(eQuestTrigger.get_hero_4, typeof(QuestTriggerGetHero4));
        Add(eQuestTrigger.get_hero_5, typeof(QuestTriggerGetHero5));
        Add(eQuestTrigger.get_hero_6, typeof(QuestTriggerGetHero6));

        Add(eQuestTrigger.achievement_skill_enchant, typeof(AchievementTriggerSkillEnchant));

        Add(eQuestTrigger.achievement_hero_evolve, typeof(AchievementTriggerHeroEvolve));
        Add(eQuestTrigger.hero_evolve, typeof(QuestTriggerHeroEvolve));
        Add(eQuestTrigger.achievement_hero_mix, typeof(AchievementTriggerHeroMix));
        Add(eQuestTrigger.hero_mix, typeof(QuestTriggerHeroMix));

        Add(eQuestTrigger.achievement_equip_enchant, typeof(AchievementTriggerEquipEnchant));
        Add(eQuestTrigger.equip_enchant, typeof(QuestTriggerEquipEnchant));
        Add(eQuestTrigger.achievement_equip_upgrade, typeof(AchievementTriggerEquipUpgrade));
        Add(eQuestTrigger.equip_upgrade, typeof(QuestTriggerEquipUpgrade));
        Add(eQuestTrigger.achievement_get_stuff, typeof(AchievementTriggerGetStuff));
        Add(eQuestTrigger.get_stuff, typeof(QuestTriggerGetStuff));
        Add(eQuestTrigger.achievement_get_rune, typeof(AchievementTriggerGetRune));
        Add(eQuestTrigger.get_rune, typeof(QuestTriggerGetRune));
        Add(eQuestTrigger.achievement_hero_enchant, typeof(AchievementTriggerHeroEnchant));
        Add(eQuestTrigger.hero_enchant, typeof(QuestTriggerHeroEnchant));
        Add(eQuestTrigger.achievement_rune_enchant, typeof(AchievementTriggerRuneEnchant));
        Add(eQuestTrigger.rune_enchant, typeof(QuestTriggerRuneEnchant));
        Add(eQuestTrigger.achievement_friends_gift, typeof(AchievementTriggerFriendsGift));
        Add(eQuestTrigger.friends_gift, typeof(QuestTriggerFriendsGift));

    }
}

abstract public class QuestTriggerBase
{
    abstract public eQuestTrigger TriggerType { get; }
    virtual public void Load(XmlNode node) { }
}

abstract public class QuestTriggerMapBase : QuestTriggerBase
{
    override public void Load(XmlNode node)
    {
        map_type = node.Attributes["map_type"].Value;
    }

    public string map_type { get; private set; }
}

public class QuestTriggerMapClear : QuestTriggerMapBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.map_clear; } }
}
public class QuestTriggerMapBossClear : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.map_boss_clear; } }
    public string map_id;
    public eDifficult difficulty;
    public override void Load(XmlNode node)
    {
        base.Load(node);
        map_id = node.Attributes["map_id"].Value;
        difficulty = (eDifficult)Enum.Parse(typeof(eDifficult), node.Attributes["difficulty"].Value);
    }
}
public class AchievementTriggerMonsterKill : QuestTriggerMapBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_monster_kill; } }
}
public class QuestTriggerMonsterKill : QuestTriggerMapBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.monster_kill; } }
}
public class QuestTriggerMapTry : QuestTriggerMapBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.map_try; } }
}

public class QuestTriggerLootItem : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.loot_item; } }
}

public class QuestTriggerLootHero : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.loot_hero; } }
}

public class QuestTriggerSkillEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.skill_enchant; } }
}
public class AchievementTriggerPVPBattle : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_pvp_battle; } }
}
public class QuestTriggerPVPBattle : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.pvp_battle; } }
}
public class AchievementTriggerBossBattle : QuestTriggerMapTry
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_boss_battle; } }
}
public class QuestTriggerBossBattle : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.boss_battle; } }
}

public class AchievementTriggerPlayerLevelUP : QuestTriggerMapTry
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_player_level_up; } }
}
public class QuestTriggerPlayerLevelUP : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.player_level_up; } }
}

abstract public class QuestTriggerGetHeroBase : QuestTriggerBase
{
    abstract public int least_grade { get; }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_hero; } }
}
public class AchievementTriggerGetHero : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_hero; } }
}
public class AchievementTriggerGetHero_3 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 3; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_hero_3; } }
}
public class AchievementTriggerGetHero_4 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 4; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_hero_4; } }
}
public class AchievementTriggerGetHero_5 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 5; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_hero_5; } }
}
public class AchievementTriggerGetHero_6 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 6; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_hero_6; } }
}

public class QuestTriggerGetHero3 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 3; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_hero_3; } }
}
public class QuestTriggerGetHero4 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 4; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_hero_4; } }
}
public class QuestTriggerGetHero5 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 5; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_hero_5; } }
}
public class QuestTriggerGetHero6 : QuestTriggerGetHeroBase
{
    override public int least_grade { get { return 6; } }
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_hero_6; } }
}
public class AchievementTriggerSkillEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_skill_enchant; } }
}
public class AchievementTriggerHeroEvolve : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_hero_evolve; } }
}
public class QuestTriggerHeroEvolve : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.hero_evolve; } }
}
public class AchievementTriggerHeroMix : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_hero_mix; } }
}
public class QuestTriggerHeroMix : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.hero_mix; } }
}
public class AchievementTriggerEquipEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_equip_enchant; } }
}
public class QuestTriggerEquipEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.equip_enchant; } }
}
public class AchievementTriggerEquipUpgrade : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_equip_upgrade; } }
}
public class QuestTriggerEquipUpgrade : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.equip_upgrade; } }
}
public class AchievementTriggerGetStuff : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_stuff; } }
}
public class QuestTriggerGetStuff : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_stuff; } }
}
public class AchievementTriggerGetRune : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_get_rune; } }
}
public class QuestTriggerGetRune : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.get_rune; } }
}
public class AchievementTriggerHeroEnchant: QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_hero_enchant; } }
}
public class QuestTriggerHeroEnchant: QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.hero_enchant; } }
}
public class AchievementTriggerRuneEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_rune_enchant; } }
}
public class QuestTriggerRuneEnchant : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.rune_enchant; } }
}
public class AchievementTriggerFriendsGift : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.achievement_friends_gift; } }
}
public class QuestTriggerFriendsGift : QuestTriggerBase
{
    override public eQuestTrigger TriggerType { get { return eQuestTrigger.friends_gift; } }
}

// move
class QuestMoveFactory : AbstractFactory<eQuestMove, QuestMoveBase, QuestMoveFactory>
{
    public QuestMoveFactory()
    {
        Add(eQuestMove.Menu, typeof(QuestMoveMenu));
        Add(eQuestMove.Popup, typeof(QuestMovePopup));
    }
}

public class QuestMoveBase
{
    public eQuestMove MoveType { get; protected set; }
    virtual public void Load(XmlNode node)
    {
        MoveType = (eQuestMove)Enum.Parse(typeof(eQuestMove), node.Attributes["type"].Value);
    }
    public string value;
    public string value2;
    public eDifficult difficulty = eDifficult.Normal;
}

public class QuestMoveMenu : QuestMoveBase
{
    override public void Load(XmlNode node)
    {
        base.Load(node);
        try
        {
            menu = (GameMenu)Enum.Parse(typeof(GameMenu), node.Attributes["value"].Value);

            XmlAttribute value2_attr = node.Attributes["value2"];
            if (value2_attr != null)
                value2 = value2_attr.Value;
            XmlAttribute difficulty_attr = node.Attributes["difficulty"];
            if(difficulty_attr != null)
                difficulty = (eDifficult)Enum.Parse(typeof(eDifficult), difficulty_attr.Value);
        }
        catch (ArgumentException)
        {
            MoveType = eQuestMove.Invalid;
        }
    }

    public GameMenu menu;
}

public class QuestMovePopup : QuestMoveBase
{
    override public void Load(XmlNode node)
    {
        base.Load(node);
        try
        {
            popup = (ePopupMode)Enum.Parse(typeof(ePopupMode), node.Attributes["value"].Value);
        }
        catch (ArgumentException)
        {
            MoveType = eQuestMove.Invalid;
        }
    }

    public ePopupMode popup;
}
public class QuestAvailableCondition
{
    public string after_clear;
    public QuestAvailableCondition(XmlNode node)
    {
        after_clear = node.Attributes["after_clear"].Value;
    }
}

// questinfo
public class QuestInfo : InfoBaseString
{
    public eQuestType Type { get; private set; }
    public QuestConditionBase Condition { get; private set; }
    public QuestTriggerBase Trigger { get; private set; }
    public QuestMoveBase Move { get; private set; }
    public string Title { get; private set; }
    string _Description;
    public string Description { get { return string.Format(_Description, Condition.ProgressMax); } }
    public string IconID { get; private set; }
    public int RewardExp { get; private set; }
    public List<RewardBase> Rewards = new List<RewardBase>();
    public eQuestTrigger FireTriggerType;
    public QuestInfo PrevQuestInfo;
    public QuestAvailableCondition AvailableCondition { get; private set; }
    public bool ProgressShow { get; private set; }
    public override void Load(XmlNode node)
    {
        base.Load(node);
        Type = (eQuestType)Enum.Parse(typeof(eQuestType), node.Attributes["type"].Value);
        eQuestCondition condition = (eQuestCondition)Enum.Parse(typeof(eQuestCondition), node.Attributes["condition"].Value);
        Condition = QuestConditionFactory.Instance.Create(condition);
        Condition.Load(node);
        Title = node.Attributes["title"].Value;
        _Description = node.Attributes["description"].Value;
        IconID = node.Attributes["icon_id"].Value;

        if (condition == eQuestCondition.progress)
        {
            XmlNode triggerNode = node.SelectSingleNode("Trigger");
            if (triggerNode != null)
            {
                eQuestTrigger trigger = (eQuestTrigger)Enum.Parse(typeof(eQuestTrigger), triggerNode.Attributes["type"].Value);
                Trigger = QuestTriggerFactory.Instance.Create(trigger);
                Trigger.Load(triggerNode);
            }
        }

        XmlAttribute reward_exp_attr = node.Attributes["reward_exp"];
        if (reward_exp_attr != null)
            RewardExp = int.Parse(reward_exp_attr.Value);
        foreach (XmlNode reward_node in node.SelectNodes("Reward"))
        {
            RewardBase quest_reward = new RewardBase(reward_node);
            Rewards.Add(quest_reward);
        }

        XmlNode available_condition_node = node.SelectSingleNode("Condition");
        if (available_condition_node != null)
        {
            AvailableCondition = new QuestAvailableCondition(available_condition_node);
        }

        XmlNode moveNode = node.SelectSingleNode("Move");
        if (moveNode != null)
        {
            try
            {
                eQuestMove move = (eQuestMove)Enum.Parse(typeof(eQuestMove), moveNode.Attributes["type"].Value);
                Move = QuestMoveFactory.Instance.Create(move);
                Move.Load(moveNode);
            }
            catch (ArgumentException)
            { }
        }

        XmlAttribute fire_type_attr = node.Attributes["fire_type"];
        if (fire_type_attr != null)
            FireTriggerType = (eQuestTrigger)Enum.Parse(typeof(eQuestTrigger), fire_type_attr.Value);

        XmlAttribute progress_show_attr = node.Attributes["progress_show"];
        if (progress_show_attr != null)
            ProgressShow = bool.Parse(progress_show_attr.Value);
        else
            ProgressShow = false;

    }

    public bool CheckProgress(long progress, int daily_index, int weekly_index, int data_daily_index, int data_weekly_index)
    {
        if (Condition.ConditionType == eQuestCondition.time)
            return false;

        bool can_progress = progress < Condition.ProgressMax;

        switch (Type)
        {
            case eQuestType.Daily:
                if (daily_index != data_daily_index)
                    return true;
                break;

            case eQuestType.Weekly:
                if (weekly_index != data_weekly_index)
                    return true;
                break;
        }
        return can_progress;
    }
}

public class QuestInfoManager : InfoManager<QuestInfo, QuestInfo, QuestInfoManager>
{
    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);
        m_Infos.Where(e => e.AvailableCondition != null).ToList().ForEach(c => c.PrevQuestInfo = GetInfoByID(c.AvailableCondition.after_clear));
    }
}
