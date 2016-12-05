using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;
using System;
using PacketEnums;

public class ChatLineManager : MNS.Singleton<ChatLineManager>
{
    public bool IsInit { get; private set; }
    public int ChattingMaxLine = GameConfig.Get<int>("popup_max_line");    

    List<ChatLine> m_list;

    public ChatLineManager()
    {
        m_list = new List<ChatLine>();
    }

    public void Init()
    {
        IsInit = true;
        
    }

    public void PreMessageLoad(List<ChatMessage> pre_msg)
    {
        foreach (ChatMessage line in pre_msg)
            AddLine(new ChatLine( new H2C.SendMessage { account_idx = line.account_idx,
                                                        level = line.level,
                                                        nickname = line.nickname,
                                                        message = line.msg,
                                                        msg_type = line.msg_type,
                                                        recv_at = line.received_at} ));

        OrderChatList();
    }
    ////////////////////////////////////////////////////////////////
    
    public List<ChatLine> GetChatList()
    {
        OrderChatList();
        if (m_list.Count > GameConfig.Get<int>("popup_max_line"))
            m_list.RemoveRange(0, m_list.Count - GameConfig.Get<int>("popup_max_line"));
        return m_list;
    }

    public void AddLine(ChatLine message)
    {   
        m_list.Add(message);
    }

    void OrderChatList()
    {
        m_list = m_list.OrderBy(line => line.RecvAt).ToList();
    }
}

public class ChatLine
{
    public pe_HubType HubType { get; private set; }
    public pe_MsgType LineType { get; private set; }
    public long AccountIdx { get; private set; }
    
    public string Nickname { get; private set; }
    public int Level { get; private set; }
    
    public string Msg { get; private set; }
    public string ItemMsg { get; private set; }

    public H2C.NotifyLootCreature LootCreature { get; private set; }
    public H2C.NotifyLootRune LootRune { get; private set; }

    public DateTime RecvAt { get; private set; }

    public ChatLine(H2C.FriendsRequest packet)
    {
        LineType = pe_MsgType.System;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Nickname = packet.send_user.nickname;
        Msg = Localization.Format("FriendsRequestMessage", Nickname);
    }
    public ChatLine(H2C.SendMessage message)
    {
        Msg = MakeMessage(message.msg_type, message.nickname, message.message);
        
        LineType = message.msg_type;
        AccountIdx = message.account_idx;        
        Nickname = message.nickname;
        Level = message.level;
        RecvAt = message.recv_at;
        HubType = pe_HubType.SmallHeroChat;
    }
    
    public ChatLine(string system_message)
    {
        Msg = MakeMessage(pe_MsgType.System, string.Empty, system_message);
        LineType = pe_MsgType.System;
        AccountIdx = 0;
        Nickname = string.Empty;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
    }

    public ChatLine(H2C.RecvWhisperMessage whisper_message)
    {
        Msg = MakeMessage(pe_MsgType.RecvWhisper, whisper_message.nickname, whisper_message.message);
        LineType = pe_MsgType.RecvWhisper;
        AccountIdx = whisper_message.account_idx;
        Nickname = whisper_message.nickname;        
        Level = whisper_message.level;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;

    }

    public ChatLine(C2H.WhisperMessageSendAck whisper_message)
    {
        LineType = pe_MsgType.SendWhisper;
        Nickname = whisper_message.nickname;
        if (whisper_message.not_found == false && whisper_message.no_exist_nickname == false)
        {
            Msg = MakeMessage(pe_MsgType.SendWhisper, whisper_message.nickname, whisper_message.message);
            Level = whisper_message.level;
        }
        else if (whisper_message.not_found == true)
        {
            Msg = "[ " + Nickname + " ]" + Localization.Get("ChatNotFound");
        }
        else if (whisper_message.no_exist_nickname == true)
        {
            Msg = "[ " + Nickname + " ]" + Localization.Get("ChatNoExistNickname");
        }

        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        
    }

    public ChatLine(H2C.NotifyLootCreature loot_creature)
    {
        Msg = Localization.Format(string.Format("{0}{1}", "TakeWhere", loot_creature.take_where), loot_creature.nickname, loot_creature.creature_grade, CreatureInfoManager.Instance.GetInfoByIdn(loot_creature.creature_idn).Name);

        LineType = pe_MsgType.Item;
        AccountIdx = loot_creature.account_idx;
        
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Level = loot_creature.level;
        Nickname = loot_creature.nickname;

        LootCreature = loot_creature;
    }

    public ChatLine(H2C.NotifyLootRune loot_rune)
    {
        Msg = Localization.Format(string.Format("{0}{1}", "TakeWhere", loot_rune.take_where), loot_rune.nickname, ItemInfoManager.Instance.GetInfoByIdn(loot_rune.item_idn).Name);

        LineType = pe_MsgType.Item;
        AccountIdx = loot_rune.account_idx;

        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Level = loot_rune.level;
        Nickname = loot_rune.nickname;

        LootRune = loot_rune;
    }

    public ChatLine(H2C.GuildAttendNotification packet)
    {
        LineType = pe_MsgType.Guild;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Nickname = packet.user_info.nickname;
        Msg = Localization.Format("GuildChatAttendFormat", Nickname, packet.exp);
    }
    public ChatLine(H2C.GuildGoldNotification packet)
    {
        LineType = pe_MsgType.Guild;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Nickname = packet.user_info.nickname;
        Msg = Localization.Format("GuildChatGoldGiveFormat", Nickname, packet.gold);
    }
    public ChatLine(H2C.GuildLeaveNotification packet)
    {
        LineType = pe_MsgType.Guild;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Nickname = packet.user_info.nickname;
        if (packet.is_expulsion)
            Msg = Localization.Format("GuildChatExpulsionMember", Nickname);
        else
            Msg = Localization.Format("GuildChatLeaveMember", Nickname);
    }
    public ChatLine(H2C.GuildJoinNotification packet)
    {
        LineType = pe_MsgType.Guild;
        RecvAt = Network.Instance.ServerTime;
        HubType = pe_HubType.SmallHeroChat;
        Nickname = packet.user_info.nickname;
        Msg = Localization.Format("GuildChatJoinMember", Nickname);
    }

    
    public Color GetColor()
    {
        switch (LineType)
        {
            case pe_MsgType.Normal:
                if (AccountIdx == SHSavedData.AccountIdx)
                    return new Color(0.975f, 0.9f, 0.9f);      // normal
                else
                    return new Color(1f, 1f, 1f);      // normal
            case pe_MsgType.Guild:
                return new Color(1f, 1f, 0.5f);
            case pe_MsgType.SendWhisper:
                return new Color(0.7f, 0.7f, 1f);  // whisper
            case pe_MsgType.RecvWhisper:
                return new Color(0.5f, 0.5f, 1f);  // whisper
            case pe_MsgType.System:
                return new Color(0.5f, 1f, 0.5f);
            case pe_MsgType.Notify:
                return new Color(0.5f, 1f, 1f);   // notify
            case pe_MsgType.Yell:
                return new Color(1f, 0.6f, 0f);   // yell
            case pe_MsgType.Emergency:
                return new Color(1f, 0.5f, 0.5f);   // error
            case pe_MsgType.Item:
                return new Color(0.85f, 0.75f, 0.65f);
            default:
                return Color.black;
        }
    }

    string MakeMessage(pe_MsgType type, string nickname, string body_message)
    {
        string msg = string.Empty;
        switch (type)
        {
            case pe_MsgType.Normal:
            case pe_MsgType.SendWhisper:
            case pe_MsgType.RecvWhisper:
            case pe_MsgType.Guild:
            case pe_MsgType.Yell:
                msg += string.Format("[url=Profile][\t{0}][/url]: ", nickname);
                break;
            default:
                {
                    msg += string.Format("[{0}] ",GetHeaderMsgType(type));
                }
                break;
        }
        msg += body_message;
        return msg;
    }

    string GetHeaderMsgType(pe_MsgType type)
    {
        switch (type)
        {
            case pe_MsgType.Normal:
                return Localization.Get("Normal");
            case pe_MsgType.Guild:
                return Localization.Get("Guild");
            case pe_MsgType.RecvWhisper:
            case pe_MsgType.SendWhisper:
                return Localization.Get("Whisper");
            case pe_MsgType.System:
            case pe_MsgType.Item:
                return Localization.Get("System");
            case pe_MsgType.Yell:
                return Localization.Get("Yell");
            case pe_MsgType.Emergency:
            case pe_MsgType.Notify:
            case pe_MsgType.Push:
                return Localization.Get("Operator");
            default:
                return "";
        }
    }
}
