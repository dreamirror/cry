using System;

namespace SharedData
{
    public enum eEquipType : short
    {
        weapon,
        armor,
    }

    public enum eAttackType : short
    {
        physic,
        magic,
        heal,
    }

    public enum eCreatureType : short
    {
        hero,
        monster,
    }

    public enum eCreaturePosition :short
    {
        front,
        middle,
        rear,
    }

    public enum eQuestType : short
    {
        AchievementBase,
        Achievement,
        Daily,
        Weekly,
    }

    public enum eQuestCondition : short
    {
        progress,
        time,
    }

    public enum eQuestTrigger : short
    {
        none,
        map_try,                    // param : map_type, map_id, stage_index
        map_clear,                  // param : map_type, map_id, stage_index
        map_boss_clear,                  // param : map_id

        loot_item,
        loot_hero,

        skill_enchant,

        hero_mix,
        hero_evolve,
        hero_enchant,

        player_level,

        monster_kill,
        pvp_battle,
        boss_battle,
        player_level_up,

        get_hero,
        get_hero_3,
        get_hero_4,
        get_hero_5,
        get_hero_6,

        equip_enchant,
        equip_upgrade,

        get_stuff,
        get_rune,
        rune_enchant,

        friends_gift,

        achievement_monster_kill = 1000,
        achievement_pvp_battle,
        achievement_boss_battle,
        achievement_player_level_up,
        achievement_get_hero,
        achievement_get_hero_3,
        achievement_get_hero_4,
        achievement_get_hero_5,
        achievement_get_hero_6,
        achievement_skill_enchant,
        achievement_hero_mix,
        achievement_hero_evolve,
        achievement_equip_enchant,
        achievement_equip_upgrade,
        achievement_get_stuff,
        achievement_get_rune,
        achievement_hero_enchant,
        achievement_rune_enchant,
        achievement_friends_gift,
    }

    public enum eQuestMove
    {
        Invalid,
        Menu,
        Popup,
    }

    public enum eRuneEquipType
    {
        physic,
        magic,
        heal,
        all,
    }
    
    public enum eItemActionType
    {
        Loot,
    }
}