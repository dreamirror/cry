using PacketEnums;
using PacketInfo;
using System;
using System.Collections.Generic;

public class HubUserInfo
{
    public long account_idx;
    public int level;
    public string nickname;
}

public class GuildNotification
{
    public HubUserInfo user_info;
    public long guild_idx;
}


namespace C2H
{
    public class HubConnectHeader
    {
        public long account_idx;
        public long access_token;
        public string nickname;
    }

    public class ChannelConnect
    {   
        public string before_channel;
        public string choose_channel;
        public string my_nickname;
    }
    public class ChannelConnectAck
    {
        public string connected_channel;
    }

    public class GuildConnect
    {
        public pd_GuildInfo guild_info;
        public string nickname;
        public bool is_join_guild;
    }

    public class GuildConnectAck
    {
        public List<long> member_account_idx;
    }

    public class GuildLeave
    {
        public bool is_kick;
        public HubUserInfo leave_user;
        public pd_GuildInfo guild_info;
    }

    public class GroupMessageSend : HubUserInfo
    {
        public pe_MsgType msg_type;
        public pd_GuildInfo guild_info;
        public string group_name;        
        public string message;
    }

    public class WhisperMessageSend : HubUserInfo
    {
        public string target_nickname;
        public string message;
    }

    public class WhisperMessageSendAck : HubUserInfo
    {
        public bool no_exist_nickname;
        public bool not_found;
        public string message;
    }

    public class PreHubMessageRequest
    {
        public string group_name;
        public pd_GuildInfo guild_info;
    }

    public class NicknameChange
    {   
        public string new_nickname;
    }
}

namespace H2C
{
    public class ConnectedToHub
    {
        public int group_count;
        public DateTime banned_at;
    }
    public class SendMessage : HubUserInfo
    {
        public pe_MsgType msg_type;
        public string message;
        public DateTime recv_at;
    }
    public class PreHubMessageList
    {
        public List<ChatMessage> list;
    }

    public class RecvWhisperMessage : HubUserInfo
    {
        public long recv_account_idx;
        public string message;
    }

    public class DisconnectToHub
    {
        public pe_HubErrorCode error_code;
    }

    public class NotifyLootCreature : HubUserInfo
    {
        public pe_TakeWhere take_where;
        public int creature_idn;
        public short skin_index;
        public short creature_grade;
    }

    public class NotifyLootRune : HubUserInfo
    {
        public pe_TakeWhere take_where;
        public int item_idn;
        public short item_grade;
    }

    public class GuildAttendNotification : GuildNotification
    {
        public int exp;
        public pd_GuildInfo guild_info;
    }
    public class GuildGoldNotification : GuildNotification
    {
        public int gold;
        public pd_GuildInfo guild_info;
    }

    public class GuildLeaveNotification : GuildNotification
    {
        public bool is_expulsion;
    }

    public class GuildJoinNotification : GuildNotification
    {
        public pd_GuildInfo guild_info;
        public bool is_refuse;
    }

    public class FriendsRequest
    {
        public HubUserInfo send_user;
        public HubUserInfo target_user;
    }

    public class LocalizationMessage
    {
        public pe_MsgType msg_type;
        public string key;
        public object[] values;
    }
}


