using NetworkCore;
using System;
using System.Collections.Generic;
using PacketEnums;

namespace PacketInfo
{
    public enum pe_GoodsType : short
    {
        invalid,
        token_gold,
        token_gem,
        token_energy,
        token_mileage,
        token_cash,
        token_arena,
        token_boss,
        token_raid,
        token_exp_powder,
        token_friends,
    }

    public enum pe_StoreItemType : short
    {
        Item,
        Stuff,
        SoulStone,
        Creature,
        Rune,
        Token,
    }

    public enum pe_StoreType : short
    {
        Loot,
        Goods,
        Items,
        Mileage,
        Boss,
        Arena,
    }

    public enum pe_SlotBuy : int
    {
        Creature,
        Rune,
    }

    public class pd_GoodsData
    {
        public pe_GoodsType goods_type;
        public long goods_value;

        public pd_GoodsData() { }
        public pd_GoodsData(pe_GoodsType goods_type, long goods_value)
        {
            this.goods_type = goods_type;
            this.goods_value = goods_value;
        }
    }

    public class pd_LeaderCreatureInfo
    {
        public int leader_creature_idn;
        public short leader_creature_skin_index;
    }

    public class pd_PlayerData
    {
        public string nickname;
        public short player_level;
        public int player_exp;
        public pd_LeaderCreatureInfo leader_creature;
        public DateTime energy_time;
        public short energy_max;
        public short additional_energy;
        public DateTime last_login_at;
        public DateTime created_at;
        public bool can_cheat;
        public short friends_delete_limit;
        public int friends_delete_daily_index;
        public short tutorial_state;
        public short creature_count_max;
        public short creature_count_buy_count;
        public short rune_count_max;
        public short rune_count_buy_count;

        public pd_KingsGiftInfo kings_gift;

        public pe_UnreadMailState unread_mail;

        public List<pd_GoodsData> goods;
        public List<pd_QuestData> quests;

        public List<long> event_idx;
    }

    public class pd_PlayerDetailData
    {
        public List<pd_CreatureData> creatures;
        public List<pd_EquipData> equips;
        public List<pd_RuneData> runes;
        public List<pd_ItemData> items;
        public List<pd_MapClearData> maps;
        public List<pd_MapClearReward> map_rewards;
        public List<pd_TeamData> teams;
        public List<pd_CreatureBook> books;
    }

    public class pd_CreatureData //生物的属性
    {
        public long creature_idx;
        public int creature_idn;
        public short skin_index;
        public short creature_grade;
        public short creature_enchant;
        public short creature_enchant_point;
        public short creature_level;
        public int creature_exp;
        [ProcedureParamList(1, 6, "skill_level", (short)0)] //特性 param： int startIndex, int count, string keyName, object invalidKey
        public List<short> skill_level;
        [ProcedureParamList(1, 2, "equip_idx", (long)0)]
        public List<long> equip_idx;
        public bool is_lock;
    }

    public class pd_EquipData
    {
        public long equip_idx;
        public long creature_idx;
        public int equip_idn;
        public short equip_level;
        public short equip_enchant;
    }

    public class pd_RuneData
    {
        public long rune_idx;
        public long creature_idx;
        public int rune_idn;
        public short rune_level;
        public DateTime equipped_at;
    }

    public class pd_CreatureLootData
    {
        public pd_CreatureData creature;
        public List<pd_EquipData> equip;
    }

    public class pd_ItemData
    {
        public int item_idn;
        public short item_count;
        public short item_piece_count;
    }

    public class pd_MapClearData
    {
        public int map_idn;
        public short stage_index;
        public pe_Difficulty difficulty;
        public int try_count;
        public int clear_count;
        public short clear_rate;
        public short daily_clear_count;
        public int daily_index;
        public DateTime updated_at;

        public short GetDailyClearCount(int daily_index)
        {
            if (daily_index != this.daily_index)
                return 0;
            return daily_clear_count;
        }
    }

    public class pd_MapClearReward
    {
        public int map_idn;
        public pe_Difficulty difficulty;
        public bool reward_1;
        public bool reward_2;
        public bool reward_3;
        public bool GetAt(int idx)
        {
            switch (idx)
            {
                case 0: return reward_1;
                case 1: return reward_2;
                case 2: return reward_3;
                default:
                    throw new Exception("out of index");
            }
        }
        public void SetAt(int idx, bool value)
        {
            switch (idx)
            {
                case 0: reward_1 = value; break;
                case 1: reward_2 = value; break;
                case 2: reward_3 = value; break;
                default:
                    throw new Exception("out of index");
            }
        }
    }

    public class pd_TeamCreature
    {
        public long team_creature_idx;
        public short auto_skill_index;

        public pd_TeamCreature() { }
        public pd_TeamCreature(long idx)
        {
            team_creature_idx = idx;
            auto_skill_index = 0;
        }
    }

    public class pd_TeamData
    {
        public pe_Team team_type;
        public bool is_auto;
        public bool is_fast;
        public pe_UseLeaderSkillType use_leader_skill_type;

        public long leader_creature_idx;

        [ProcedureParamList(1, 5, "team_creature_idx", (long)0)]
        public List<pd_TeamCreature> creature_infos;

        public bool no_duplicate;
    }

    public class pd_GameConfigValue
    {
        public pd_GameConfigValue()
        {

        }

        public pd_GameConfigValue(string id, string value_type, string value)
        {
            this.id = id;
            this.value_type = value_type;
            this.value = value;
        }

        public string id;
        public string value_type;
        public string value;
    }

    public class pd_BattleEndCreatureInfo
    {
        public long creature_idx;
        public bool is_dead;
        public pd_BattleEndCreatureInfo(long idx, bool isDead)
        {
            creature_idx = idx;
            is_dead = isDead;
        }
    }

    public class pd_ItemLootInfo
    {
        public int item_idn;
        public short add_piece_count;

        public pd_ItemLootInfo() { }
        public pd_ItemLootInfo(int item_idn, short add_piece_count)
        {
            this.item_idn = item_idn;
            this.add_piece_count = add_piece_count;
        }
    }

    public class pd_StoreLootInfo
    {
        public string loot_id;
        public short available_count;
        public DateTime available_time;
        public int daily_index;
        public int weekly_index;
    }

    public class pd_StoreLimitInfo
    {
        public string store_id;
        public string item_id;
        public short available_count;
        public int daily_index;
        public int weekly_index;
    }

    public class pd_StoreItem
    {
        public short store_idx;
        public pe_StoreItemType item_type;
        public int item_idn;
        public short item_count;
        public short item_piece_count_max;
        public pd_GoodsData price;
        public short buying_state;
    }


    public class _sp_StoreItem
    {
        public pe_StoreItemType item_type;
        public int item_idn;
        public short item_count;
        public short item_count_max;
        public int item_piece_count_max;
        public pe_GoodsType goods_type;
        public int goods_value;
    }

    public class pd_QuestData
    {
        public int quest_idn;
        public long quest_progress;
        public bool rewarded;
        public int daily_index;
        public int weekly_index;
    }

    public class pd_QuestDataUpdate
    {
        public pd_QuestDataUpdate() { }
        public pd_QuestDataUpdate(int quest_idn, long quest_progress) { this.quest_idn = quest_idn; this.quest_progress = quest_progress; }
        public int quest_idn;
        public long quest_progress;
    }

    public enum eFriendsState : short
    {
        Friends,
        Request,
        Requested,
        Deleted,
        Candidate,
    }

    public class pd_PlayerInfo
    {
        public long account_idx;
        public string nickname;
        public short player_level;
        public pd_LeaderCreatureInfo leader_creature;
        public DateTime last_login_at;
    }
    public class pd_FriendsInfo : pd_PlayerInfo
    {
        public pd_FriendsStateInfo state_info;
        public bool is_connected;
        public pd_FriendsInfo()
        {
            //state_info = new pd_FriendsStateInfo();
            //state_info.state = eFriendsState.Candidate;
        }
        public pd_FriendsInfo(pd_PlayerInfo info)
        {
            account_idx = info.account_idx;
            nickname = info.nickname;
            player_level = info.player_level;
            leader_creature = info.leader_creature;
            last_login_at = info.last_login_at;

            state_info = new pd_FriendsStateInfo();
            state_info.available_gift = false;
            state_info.give_daily_index = 0;
            state_info.state = eFriendsState.Request;
        }
    }
    public class pd_FriendsStateInfo
    {
        public eFriendsState state;
        public int give_daily_index;
        public bool available_gift;
    }
    public class pd_FriendsDetail : pd_FriendsInfo
    {
        public bool connected;
    }
    public class pd_MailInfo
    {
        public long mail_idx;
        public string title;
        public string sender_nickname;
        public pe_MailType mail_type;
        public bool exists_reward;
        public bool is_read;
        public bool is_rewarded;
        public bool open_direct;
        public DateTime created_at;
    }

    public class pd_MailTableInfo
    {
        public long mail_idx;
        public long mail_detail_idx;
        public string title;
        public string param_base;
        public bool is_rewarded;
        [ProcedureParamList(1, 3, "reward_idn", (int)0)]
        public List<pd_MailRewardInfo> rewards;
    }

    public class pd_MailDetailInfo
    {
        public long mail_idx;
        public string title;
        public string body_message;
        public bool used_reward;
        public List<pd_MailRewardInfo> rewards;
    }

    public class pd_MailState
    {
        public bool exists_reward;
        public bool is_read;
        public bool is_rewarded;
    }

    public class pd_MailRewardInfo
    {
        public pd_MailRewardInfo() { }
        public pd_MailRewardInfo(int reward_idn, int reward_value)
        {
            this.reward_idn = reward_idn;
            this.reward_value = reward_value;
        }

        public int reward_idn;
        public int reward_value;
    }

    public class pd_UsedEventInfo
    {
        public long send_idx;
        public DateTime updated_at;
    }

    public class pd_PvpPlayerInfo
    {
        public long account_idx;
        public string nickname;
        public pd_LeaderCreatureInfo leader_creature;
        public string message;
        public int rank;
        public short player_level;
        public int team_power;
    }

    public class pd_PvpResultInfo
    {
        public bool is_win;
        public pd_PvpPlayerInfo player_info;
        [ProcedureParamList(1, 5, "hp_percent", (short)-1)]
        public List<short> hp_percent;
        public DateTime battle_time;
    }

    public class pd_PlayerExpInfo
    {
        public int player_exp;
    }

    public class pd_PlayerExpAddInfo
    {
        public short player_level;
        public int player_exp;
        public int add_player_exp;
        public short energy_bonus;
    }

    public class pd_CreatureExpAddInfo
    {
        public long creature_idx;
        public short creature_level;
        public int creature_exp;
        public int add_creature_exp;
    }

    public class pd_CreatureEnchantInfo
    {
        public long creature_idx;
        public short creature_grade;
    }

    public class pd_AttendInfo
    {
        public pd_AttendInfo() { }
        public pd_AttendInfo(int attend_idn, short take_count, short take_count_max, int last_daily_index)
        {
            this.attend_idn = attend_idn;
            this.take_count = take_count;
            this.take_count_max = take_count_max;
            this.last_daily_index = last_daily_index;
        }
        public int attend_idn;
        public short take_count;
        public short take_count_max;
        public int last_daily_index;
    }

    public enum pe_RewardLootType
    {
        Token,
        Item,
        Rune,
        Hero,
    }

    public class pd_RewardLootInfo
    {
        public pe_RewardLootType type;
        public int index;
        public pd_RewardLootInfo(pe_RewardLootType type, short index)
        {
            this.type = type;
            this.index = index;
        }
    }

    public class pd_CreatureBook
    {
        public int creature_idn;
        public int take_count;
    }

    public class ChatMessage : HubUserInfo
    {
        public pe_MsgType msg_type;
        public string group_name;
        public string msg;
        public DateTime received_at;
    }

    public class pd_CreatureEvalBoard
    {
        public long board_idx;
        public int good;
        public int bad;
        public string message;
        public pd_ThumbInfo thumb_info;

        public pe_EvalState my_eval_state;
        public bool is_best;
    }

    public class pd_ThumbInfo
    {
        public long account_idx;
        public pd_LeaderCreatureInfo leader_creature;
        public string nickname;
        public short player_level;
    }

    public enum pe_EvalState : byte
    {
        None = 0,
        Good = 1,
        Bad = 2,
    }

    public class pd_AdventureInfo
    {
        public int map_idn;
        public bool is_begin;
        public DateTime end_at;
        public bool is_rewarded;
        [ProcedureParamList(1, 3, "reward_idn", (int)0)]
        public List<pd_MailRewardInfo> rewards;
    }

    public class pd_GuildInfo
    {
        public long guild_idx;
        public string guild_name;
        public short guild_level;
        public int guild_exp;
        public string guild_intro;
        public string guild_notify;
        public bool is_auto;
        public short guild_limit_level;
        public short member_count;
        public short guild_limit_member;
        public string guild_emblem;
        public DateTime guild_levelup_at;
        public DateTime created_at;
        public int rank;
    }
    public class pd_GuildRequestInfo
    {
        public long guild_idx;
        public long account_idx;
        public DateTime created_at;
        public string nickname;
        public DateTime last_login_at;
    }
    public class pd_GuildInfoMaster
    {
        public string guild_master;
    }
    public class pd_GuildInfoDetail
    {
        public pd_GuildInfo info;
        public string guild_master;
        public pd_GuildInfoDetail() { }
        public pd_GuildInfoDetail(pd_GuildInfo _info) { info = _info; }
    }
    public class pd_GuildJoinInfo
    {
        public pd_GuildInfo guild_info;
        public int request_count;
    }
    public class pd_GuildRequestedInfo
    {
        public long account_idx;
        public short player_level;
        public pd_LeaderCreatureInfo leader_creature;
        public string nickname;
        public DateTime last_login_at;
        public DateTime created_at;
        public bool is_connected;
    }
    public class pd_GuildRequestedInfoDetail
    {
        public pd_TeamData team_data;
        public List<pd_CreatureData> creatures;

    }
    public class pd_GuildMemberInfo
    {
        public long account_idx;
        public int give;
        public int attend_daily_index;
        public short member_state;
        public DateTime updated_at;
        public DateTime created_at;
        public string nickname;
        public DateTime last_login_at;

        public pd_GuildMemberInfo() { }
        public pd_GuildMemberInfo(pd_GuildMemberInfo info)
        {
            account_idx = info.account_idx;
            give = info.give;
            attend_daily_index = info.attend_daily_index;
            member_state = info.member_state;
            updated_at = info.updated_at;
            created_at = info.created_at;
            nickname = info.nickname;
            last_login_at = info.last_login_at;
        }
    }
    public class pd_GuildMemberPlayInfo : pd_GuildMemberInfo
    {
        public short player_level;
        public pd_LeaderCreatureInfo leader_creature;
        public bool is_connected;
        public pd_GuildMemberPlayInfo() { }
        public pd_GuildMemberPlayInfo(pd_GuildMemberInfo info) : base(info) { }
        public pd_GuildMemberPlayInfo(pd_GuildMemberPlayInfo info) : base(info as pd_GuildMemberInfo)
        {
            player_level = info.player_level;
            leader_creature = info.leader_creature;
            is_connected = info.is_connected;
        }
    }
    public class pd_GuildMemberInfoDetail : pd_GuildMemberPlayInfo
    {
        public pd_TeamData team_data;
        public List<pd_CreatureData> creatures;
        public pd_GuildMemberInfoDetail() { }
        public pd_GuildMemberInfoDetail(pd_GuildMemberPlayInfo info) : base(info) { }
    }
    public class pd_KingsGiftInfo
    {
        public int kings_gift_idn;
        public pd_GoodsData reward_data;
        public DateTime takeable_at;
    }
    public class pd_EventHottimeData
    {
        public TimeSpan start;
        public TimeSpan end;
    }

    public enum pe_EventHottimeState
    {
        End,
        Waiting,
        Ing = 10,
        WaitingHottime,
        Hottime,
    }

    public enum pe_EventHottimeShow
    {
        Default,
        Show,
        Always,
        Hidden
    }

    public class pd_EventHottime
    {
        public long idx;
        public string event_id;
        public string title;
        public bool show_state;
        public pe_EventHottimeState state;
        public DateTime end_date;
        public List<int> ValueData;

        [Newtonsoft.Json.JsonIgnore]
        public int Value { get { return ValueData[0]; } }
        [Newtonsoft.Json.JsonIgnore]
        public float Percent { get { return Value * 0.0001f; } }
        [Newtonsoft.Json.JsonIgnore]
        public bool OnGoing { get { return state == pe_EventHottimeState.Ing || state == pe_EventHottimeState.Hottime; } }
    }

    public class pd_WorldBoss
    {
        public int rank;
        public int score;
    }

    public class pd_WorldBossPlayerInfo
    {
        public long account_idx;
        public string nickname;
        public pd_LeaderCreatureInfo leader_creature;
        public int rank;
        public short player_level;
        public int score;
    }
}
