using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using PacketEnums;

public class EventParamLevelUp
{
    public short old_level, new_level;
    public int old_exp, new_exp;
    public int add_exp;
    public short old_energy, new_energy;
}

public class EventParamBattleEnd
{
    public pe_EndBattle end_type;
    public EventParamLevelUp player_levelup;
    public List<BattleEndCreature> creatures;
    public List<pd_GoodsData> add_goods;
    public List<pd_ItemLootInfo> loot_items;
    public List<pd_RuneData> loot_runes;
    public short clear_rate;
    public List<long> loot_creatures;
    public List<long> maxlevel_reward_mail_idxs;
    public bool is_boss;
    
}

public class EventParamPVPBattleEnd
{
    public pe_EndBattle end_type;
    public int rank;
    public int rank_up;
}

public class EventParamWorldBossBattleEnd
{
    public int score;
    public int score_up;
    public int rank;
    public int rank_up;
    public bool is_first;
}

public class EventParamItemMade
{
    public EventParamItemMade(Item item, short item_count)
    {
        this.item = item;
        this.item_count = item_count;
    }
    public Item item;
    public int item_count;
}

public class EventParamBattleSweep
{
    public int sweep_count;
    public EventParamLevelUp player_levelup;
    public List<pd_GoodsData> add_goods;
    public List<pd_ItemLootInfo> loot_items;
    public pd_CreatureLootData loot_creature;
}

