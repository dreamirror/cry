using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using System;

namespace C2G
{
    public class Connect
    {
        public bool request_info;
        public bool request_data;
    }

    public class CommonAck
    {
        public object packet;
        public List<object> additionals;
    }

    public class p_InfoFile
    {
        public p_InfoFile() { }
        public p_InfoFile(string filename, string data) { this.filename = filename; this.data = data; }
        public string filename;
        public string data;
    }

    [NetworkCore.Packet(Caching = false)]
    public class ConnectAck
    {
        public int daily_index;
        public int weekly_index;

        // player info
        public pd_PlayerData player_info;

        // config values
        public List<pd_GameConfigValue> config_values;
        public List<pd_EventHottime> events;

        // details
        public pd_PlayerDetailData detail_data;

        // infos
        public int info_version;
        public List<p_InfoFile> info_files;

        // friends
        public List<pd_FriendsDetail> friends;

        // guild_info
        public pd_GuildInfo guild;

        public DateTime server_time;
    }

    public class LootCreature
    {
        public string loot_id;
        public bool is_free;
    }

    public class LootCreatureAck
    {
        public pd_CreatureLootData creature_loot_data;
    }


    public class LootCreature10
    {
        public string loot_id;
    }

    public class LootCreature10Ack
    {
        public List<pd_CreatureLootData> loots;
    }

    public class SetLeader
    {
        public pd_LeaderCreatureInfo leader_creature;
    }

    public class CreatureSummon
    {
        public string item_id;
    }
    public class CreatureSummonAck
    {
        public short use_count;
        public pd_CreatureLootData creature_loot_data;
    }

    public class EquipEnchant
    {
        public long equip_idx;
        public long creature_idx;
        public string equip_id;
        public short enchant_level;
    }

    public class EquipEnchantAck
    {
        public pd_EquipData equip;
        public List<pd_ItemData> item;
        public pd_GoodsData use_gold;
    }

    public class EquipUpgrade
    {
        public long equip_idx;
        public string equip_id;
        public short equip_grade;
        public long creature_idx;
    }

    public class EquipUpgradeAck
    {
        public pd_ItemData item;
        public pd_EquipData equip;
        public pd_GoodsData use_gold;
    }

    public class EnterBattle
    {
        public string map_id;
        public string stage_id;
        public pe_Difficulty difficulty;

        public pd_TeamData team_data;
        public List<string> creature_ids;
    }

    public class EnterBattleAck
    {

    }

    public class EndBattle
    {
        public pe_Battle battle_type;
        public pe_EndBattle end_type;

        public string map_id;
        public string stage_id;
        public pe_Difficulty difficulty;
        public bool is_new_clear;

        public List<pd_BattleEndCreatureInfo> creatures;
        public pd_TeamData team_data;
    }

    public class EndBattleAck
    {
        public bool set_new_map;
        public pd_PlayerExpAddInfo player_exp_add_info;
        public List<pd_CreatureExpAddInfo> creature_exp_add_infos;
        public List<pd_GoodsData> add_goods;
        public List<pd_ItemLootInfo> loot_items;
        public List<pd_RuneData> loot_runes;
        public List<pd_CreatureData> loot_creatures;
        public List<pd_EquipData> loot_creatures_equip;
        public List<long> maxlevel_reward_mail_idx;
    }

    public class GetWorldBossInfo
    {
    }

    public class GetWorldBossInfoAck
    {
        public pd_WorldBoss info;
    }

    public class EndWorldBoss
    {
        public pe_EndBattle end_type;

        public string map_id;

        public int score;
        public pd_TeamData team_data;
    }

    public class EndWorldBossAck
    {
        public pd_WorldBoss info;
    }

    public class GetWorldBossRanking
    {

    }

    public class GetWorldBossRankingAck
    {
        public List<pd_WorldBossPlayerInfo> players;
    }

    public class SkillEnchant
    {
        public long creature_idx;
        public string creature_id;

        public string skill_id;
        public short skill_index;
        public short skill_level;
    }
    public class SkillEnchantLevel : SkillEnchant
    {
        public short add_level;
    }
    public class SkillEnchantAllMax
    {
        public long creature_idx;
        public string creature_id;
        public short creature_level;

        public List<short> skill_level;
    }

    public class SkillEnchantAck
    {
        public pd_GoodsData use_gold;
    }

    public class ItemSale
    {
        public int item_idn;
        public short item_count;
    }

    public class ItemSaleAck
    {
        public pd_GoodsData add_gold;
    }

    public class StuffMake
    {
        public int item_idn;
    }

    public class StuffMakeAck
    {
    }

    public class StoreLootInfoGet
    {
        public string store_id;
    }

    public class StoreLootInfoGetAck
    {
        public List<pd_StoreLootInfo> infos;
    }

    public class StoreLimitInfoGet
    {
        public string store_id;
    }
    public class StoreLimitInfoGetAck
    {
        public List<pd_StoreLimitInfo> infos;
    }


    public class StoreItemsGet
    {
        public string clear_map_id;
        public string store_id;

        public List<string> exclude_ids;

    }


    public class StoreItemsGetAck
    {
        public string store_id;
        public List<pd_StoreItem> store_items;
        public pd_GoodsData use_goods;
    }

    public class StoreGoodsBuy
    {
        public string store_id;
        public string item_id;
    }

    public class StoreGoodsBuyAck
    {

    }

    public class StoreItemBuy
    {
        public string store_id;
        public short store_idx;

        public string rune_id;
        public pd_GoodsData goods;
        public string creature_id;
        public short creature_grade;
    }

    public class StoreItemBuyAck
    {
        public pd_RuneData loot_rune;
        public pd_CreatureLootData loot_creature;
    }

    public class StoreItemsRefresh
    {
        public string clear_map_id;
        public string store_id;

        public List<string> exclude_ids;
    }

    public class StoreLootItem
    {
        public string loot_id;
        public bool is_free;
    }

    public class StoreLootItemAck
    {
        public pd_ItemLootInfo loot_item;
        public pd_RuneData loot_rune;
    }
    public class StoreLootItem10
    {
        public string loot_id;
    }
    public class StoreLootItem10Ack
    {
        public List<pd_ItemLootInfo> loot_items;
        public List<pd_RuneData> loot_runes;
    }

    public class Cheat
    {
        public string category;
        public string command;
        public string param;
    }

    public class CheatAck
    {
        public string error;
        public pd_PlayerData player_info;
        public pd_PlayerDetailData detail_data;
    }

    public class DailyIndex
    {
        public int daily_index;
        public int weekly_index;
    }

    public class ReconnectInfo
    {
        public int reconnect_index;
        public List<pd_GameConfigValue> game_configs;
        public List<pd_EventHottime> events;
        public List<long> event_idx;
    }

    public class QuestProgress
    {
        public List<pd_QuestDataUpdate> updates;
    }

    public class QuestReward
    {
        public string quest_id;
    }

    public class QuestRewardAck
    {
        public pd_PlayerExpAddInfo player_add_exp_info;
        public Reward3Ack reward_ack;
    }

    public class UnreadMail
    {
        public pe_UnreadMailState unread_type;
    }

    public class NotifyMenu
    {
        public bool is_pvp_rank_changed;
        public bool is_friends_requested;
        public bool is_store_free_loot;
    }

    public class CreatureLevelup
    {
        public long creature_idx;
        public short grade;
        public short level;
        public int exp;
        public short add_level;
    }

    public class CreatureLevelupAck
    {
        public pd_CreatureExpAddInfo creature_exp_add_info;
        public pd_GoodsData use_goods;
        public long maxlevel_reward_mail_idx;
    }

    public class NicknameSet
    {
        public string nickname;
    }
    public class NicknameSetAck
    {
        public pe_NicknameResult result;
    }

    public class MapClearReward
    {
        public string map_id;
        public pe_Difficulty difficulty;
        public short index;
    }

    sealed public class Reward3Ack
    {
        public List<pd_GoodsData> add_goods;
        public List<pd_ItemLootInfo> loot_items;
        public List<pd_RuneData> loot_runes;
        public List<pd_CreatureData> loot_creatures;
        public List<pd_EquipData> loot_creatures_equip;
        public List<pd_RewardLootInfo> loots;
    }

    public class MapClearRewardAck
    {
        public Reward3Ack reward_ack;
    }

    public class FriendsAckBase
    {
        public pe_FriendsResult result;
    }
    public class FriendsInfoGet
    {
        public eFriendsState state;
    }

    [NetworkCore.Packet(Caching = false)]
    public class FriendsInfoGetAck
    {
        public List<pd_FriendsInfo> friends;
    }

    public class FriendsRequestCancel
    {
        public long account_idx;
        public bool is_all;
    }

    public class FriendsCandidateList
    {

    }

    [NetworkCore.Packet(Caching = false)]
    public class FriendsCandidateListAck : FriendsAckBase
    {
        public List<pd_PlayerInfo> players;
    }

    public class FriendsRequest
    {
        public List<long> account_idx;
    }
    public class FriendsRequestAck : FriendsAckBase
    {
        public int request_count;
    }
    public class FriendsRefuse
    {
        public long account_idx;
        public bool is_all;
    }
    public class FriendsApprove
    {
        public long account_idx;
        public bool is_all;
    }

    public class FriendsSend
    {
        public long account_idx;
        public bool is_all;
    }
    public class FriendsGiftGet
    {
        public long account_idx;
        public bool is_all;
    }
    public class FriendsGiftGetAck : FriendsAckBase
    {
        public long token_friends;
    }
    public class FriendsDelete
    {
        public long account_idx;
    }
    public class FriendsRequestWithNickname
    {
        public string nickname;
    }

    public class MailGet
    {
    }

    [NetworkCore.Packet(Caching = false)]
    public class MailGetAck
    {
        public List<pd_MailInfo> info;
    }

    public class MailRead
    {
        public long mail_idx;
        public bool is_read;
        public bool is_exist_reward;
    }

    public class MailReadAck
    {
        public pd_MailDetailInfo detail_info;
    }

    public class MailReward
    {
        public long mail_idx;

        public List<pd_MailRewardInfo> rewards;
    }

    public class MailRewardAck
    {
        public Reward3Ack reward_ack;
    }

    public class PVPRegistDefense
    {
        public string message;

        public pd_LeaderCreatureInfo leader_creature;
        public int team_power;
        public pd_TeamData team_data;
    }

    public class MailRewardDirect
    {
        public long mail_idx;
    }
    public class MailRewardDirectAck
    {
        public pd_MailDetailInfo result_mail;
    }

    public class PvpUpdateDefense
    {
        public pd_LeaderCreatureInfo leader_creature;
        public int team_power;
        public pd_TeamData team_data;
    }

    public class PVPRegistDefenseAck
    {
    }

    public class PVPGetInfo
    {
        public int defense_team_power;
    }

    [NetworkCore.Packet(Caching = false)]
    public class PVPGetInfoAck
    {
        public DateTime last_offense_at;
        public List<pd_PvpPlayerInfo> pvp_player_infos;
    }

    public class PvpGetBattleInfo
    {
        public long enemy_account_idx;
    }

    [NetworkCore.Packet(Caching = false)]
    public class PvpGetBattleInfoAck
    {
        public List<pd_CreatureData> creatures;
        public List<pd_EquipData> equips;
        public List<pd_RuneData> runes;
        public pd_TeamData team_data;
    }

    public class PvpEnterBattle
    {
        public long enemy_account_idx;
        public pd_TeamData team_data;
    }

    public class PvpEnterBattleAck
    {

    }

    public class PvpBuyBattleTime
    {
    }
    public class PvpBuyBattleTimeAck
    {
        public pd_GoodsData use_goods;
    }

    public class PvpBuyBattleCount
    {
    }
    public class PvpBuyBattleCountAck
    {
        public pd_GoodsData use_goods;
    }

    public class PvpRankInfoGet
    {

    }
    public class PvpRankInfoGetAck
    {
        public List<pd_PvpPlayerInfo> pvp_rankers;
    }
    public class PvpEnd
    {
        public long enemy_account_idx;
        public int enemy_rank;
        public bool is_win;
        //public List<short> hp_percent;
        //public List<short> enemy_hp_percent;
    }
    public class PvpEndAck
    {
        public int rank;
        public int rank_up;
    }

    public class PvpMessageUpdate
    {
        public string message;
    }
    public class PvpMessageUpdateAck
    {

    }

    public class PvpListGet
    {
    }
    public class PvpListGetAck
    {

    }

    public class PvpResultGet
    {
    }
    public class PvpResultGetAck
    {
    }

    public class CreatureEnchant
    {
        public long creature_idx;
        public short creature_grade;
        public List<pd_CreatureEnchantInfo> materials;
    }

    public class CreatureEnchantAck
    {
        public short creature_enchant;
        public short creature_enchant_point;
        public int use_gold;
    }

    public class CreatureOverEnchant
    {
        public long creature_idx;
        public short creature_grade;
        public short creature_enchant;

        public long material_idx;
        public short material_grade;
        public short material_enchant;
    }

    public class CreatureOverEnchantAck
    {
        public short creature_enchant;
        public int use_gold;
    }

    public class CreatureMix
    {
        public long creature_idx;
        public short creature_grade;
        public long material_creature_idx;
    }

    public class CreatureMixAck
    {
        public pd_CreatureLootData creature_loot_data;
        public int use_gold;
    }

    public class CreatureEvolve
    {
        public long creature_idx;
        public short creature_grade;
        public long material_creature_idx;
    }

    public class CreatureEvolveAck
    {
        public pd_CreatureData creature_data;
        public int use_gold;
    }

    public class SlotBuy
    {
        public pe_SlotBuy buy_type;
        public short count_buy_count;
    }

    public class SlotBuyAck
    {
        public short count_max;
        public short count_buy_count;
        public int use_gem;
    }

    public class AttendInfoRequest
    {
    }
    public class AttendInfoRequestAck
    {
        public List<pd_AttendInfo> attends;
    }

    public class AttendRewardGet
    {
        public int attend_idn;
        public short take_count;
        public bool is_additional;
    }

    public class AttendRewardGetAck
    {
        public short take_count;
    }

    public class RuneEquip
    {
        public long rune_idx;
        public string rune_id;
        public long creature_idx;
        public string creature_id;
    }

    public class RuneUnequip
    {
        public long rune_idx;
        public short rune_grade;
    }

    public class RuneUnequipAck
    {
        public pd_GoodsData use_goods;
    }

    public class RuneEnchant
    {
        public long rune_idx;
        public short rune_grade;
        public bool is_premium;
        public short rune_level;
    }

    public class RuneEnchantAck
    {
        public bool is_success;
        public short rune_level;
        public pd_GoodsData use_goods;
    }

    public class CreatureSales
    {
        public List<long> creature_idxes;
        public List<long> creature_grades;
    }

    public class CreatureSalesAck
    {
        public pd_GoodsData add_goods;
    }

    public class StuffPurchase
    {
        public int stuff_idn;
    }

    public class StuffPurchaseAck
    {
    }

    public class RunesSale
    {
        public List<long> rune_idxes;
        public List<long> rune_grades;
    }

    public class RunesSaleAck
    {
        public pd_GoodsData add_goods;
    }

    public class RuneUpgrade
    {
        public List<long> material_idxes;
        public short material_grade;
    }

    public class RuneUpgradeAck
    {
        public pd_GoodsData use_goods;
        public pd_RuneData rune_info;
    }

    public class NotifyMailGet
    {
    }

    public class NotifyMailGetAck
    {
        public List<pd_MailDetailInfo> mail_info;
    }

    public class CreatureBook
    {
    }

    public class CreatureBookAck
    {
        public List<pd_CreatureBook> book_info;
    }
    public class CreatureLock
    {
        public long creature_idx;
        public bool is_lock;
    }

    public class CreatureEvalInitInfo
    {
        public string creature_id;
    }

    public class CreatureEvalInitInfoAck
    {
        public List<pd_CreatureEvalBoard> board_info;
        public double avg_score;
        public int my_score;
        public string first_obtainer_nickname;
    }

    public class CreatureEvalMoreBoard
    {
        public string creature_id;
        public long smallest_board_idx;
    }

    public class CreatureEvalMoreBoardAck
    {
        public List<pd_CreatureEvalBoard> board_info;
    }

    public class CreatureEvalBoardWrite
    {
        public string creature_id;
        public string message;
    }

    public class CreatureEvalBoardWriteAck
    {
        public long board_idx;
        public string message;
        public bool is_success;
    }

    public class CreatureEvalBoardDelete
    {
        public string creature_id;
        public long board_idx;
    }

    public class CreatureEvalBoardDeleteAck
    {
    }

    public class CreatureEvalScoreUpdate
    {
        public string creature_id;
        public int score;
    }

    public class CreatureEvalScoreUpdateAck
    {
        public double avg_score;
    }

    public class CreatureEvalStateUpdate
    {
        public long board_idx;
        public string creature_id;
        public pe_EvalState eval_state;
    }

    public class CreatureEvalStateUpdateAck
    {
    }

    public class AdventureInfoDetail
    {

    }
    public class AdventureInfoDetailAck
    {
        public List<pd_AdventureInfo> adventure_infos;
    }

    public class AdventureBegin
    {
        public string map_id;
        public pd_TeamData team_data;
    }
    public class AdventureBeginAck
    {
        public pd_AdventureInfo adventure_info;
    }
    public class AdventureGetReward
    {
        public string map_id;
        public DateTime end_time;
        public List<pd_MailRewardInfo> rewards;
    }
    public class AdventureGetRewardAck
    {
        public pd_GoodsData use_goods;
        public Reward3Ack reward_ack;
    }


    public class GuildCreate
    {
        public string guild_name;
        public bool is_auto;
        public short guild_limit_level;
        public string guild_emblem;
        public string guild_intro;
        public string guild_notification;
    }

    public class GuildUpdate
    {
        public long guild_idx;
    }

    public class GuildAttend
    {
        public long guild_idx;
    }
    public class GuildGoldGive
    {
        public long guild_idx;
        public int give_gold;
    }
    public class GuildEmblemChange
    {
        public long guild_idx;

    }
    public class GuildEmblemChangeAck
    {
        public pd_GoodsData use_goods;
    }

    public class GuildJoin
    {
        public long guild_idx;
        public long member_account_idx;
        public bool refuse;
    }

    public class GuildLeave
    {
        public long guild_idx;
        public long member_account_idx;
    }

    public class GuildListForJoin
    {
        public short player_level;
    }
    public class GuildListForJoinAck
    {
        public List<pd_GuildJoinInfo> guild_join_info;
        public List<pd_GuildRequestInfo> request_guilds;
    }
    public class GuildListForRequest
    {
    }
    public class GuildListRank
    {
        public int page;
    }
    public class GuildListRankAck
    {
        public List<pd_GuildInfo> guild_infos;
        public int total;
    }
    public class GuildRequest
    {
        public long guild_idx;
    }
    public class GuildSearch
    {
        public string guild_name;
    }
    public class GuildSetting
    {
        public long guild_idx;
        public string guild_notify;
        public string guild_intro;
        public short guild_limit_level;
        public bool is_auto;
    }
    public class GuildStateChange
    {
        public long guild_idx;
        public long member_account_idx;
        public short member_state;
    }

    public class GuildInfoMaster
    {
        public long guild_idx;
    }
    public class GuildInfoMasterAck
    {
        public string guild_master;
    }

    public class GuildMemberGet
    {
        public long guild_idx;
    }

    public class GuildMemberDetail
    {
        public long player_idx;
    }
    public class GuildMemberDetailAck
    {
        public pd_TeamData team_data;
        public List<pd_CreatureData> creatures;
    }
    public class GuildRequestGet
    {
        public long guild_idx;
    }
    public class GuildRequestGetAck
    {
        public List<pd_GuildRequestedInfo> requested_infos;
    }

    public class GuildRequestedDetail
    {
        public long player_idx;
    }
    public class GuildRequestedDetailAck
    {
        public pd_GuildRequestedInfoDetail detail;
    }
    public class GuildRefuseAll
    {
        public long guild_idx;
    }
    public class GuildAck
    {
        public pe_GuildResult result;
        public pd_GuildInfo guild_info;
        public pd_GoodsData use_goods;
        public List<pd_GuildMemberPlayInfo> guild_members;
        public List<pd_GuildRequestInfo> guild_request;
    }

    public class KingsGiftRefresh
    {
        public string last_map_id;
    }

    public class KingsGiftRefreshAck
    {

        public pd_GoodsData got_info;
        public pd_KingsGiftInfo next_info;
    }

    public class WorldBossGetBattleInfo
    {
        public long ranker_account_idx;
    }

    [NetworkCore.Packet(Caching = false)]
    public class WorldBossGetBattleInfoAck
    {
        public List<pd_CreatureData> creatures;
        public List<pd_EquipData> equips;
        public List<pd_RuneData> runes;
        public pd_TeamData team_data;
    }


    //////////////////////////////////////////////////////////
    //for tutorial
    public class TutorialState
    {
        public short tutorial_state;
        public short next_tutorial_state;

        public EnterBattle enter_battle;
        public LootCreature loot_creature;
        public EndBattle end_battle;
        public SkillEnchantLevel skill_enchant;
        public SkillEnchantAllMax skill_enchant_all_max;
        public EquipEnchant equip_enchant;
        public CreatureMix creature_mix;
        public CreatureEnchant creature_enchant;
        public RuneEquip rune_equip;
    }

    public class TutorialStateAck
    {
        public LootCreatureAck loot_creature;
        public EndBattleAck end_battle;
        public SkillEnchantAck skill_enchant;
        public EquipEnchantAck equip_enchant;
        public CreatureMixAck creature_mix;
        public CreatureEnchantAck creature_enchant;
        public Reward3Ack rewards_ack;
    }

    public class Reconnect
    {
    }

    public class ReconnectAck
    {
    }
}


