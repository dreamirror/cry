using System;
using System.Collections.Generic;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR;
using UnityEngine;
using BestHTTP.SignalR.Authentication;
using BestHTTP;
using PacketEnums;
using PacketInfo;
using Newtonsoft.Json;
using BestHTTP.SignalR.Messages;
using System.Collections;
using H2C;
using NetworkCore;

public delegate void OnHubConnectedDelegate(H2C.ConnectedToHub packet);
public delegate void OnHubGroupConnectedDelegate(string group_name);
public delegate void OnHubPreMsgDelegate(List<ChatMessage> line);
public delegate void OnHubMsgInsertLabel(ChatLine line);
public delegate void OnHubDisconnected(bool need_reconnect, bool is_maintenance);

public class ChatClient : HubClient
{
    public bool IsListenChannel { get; set; }
    public bool isListenWhisper { get; set; }
    public bool IsListenYell { get; set; }
    public bool IsListenGuild { get; set; }

    public bool PreMessageLoaded { get; private set; }

    public DateTime BannedAt { get; private set; }

    void Init()
    {
        AddHandler<H2C.ConnectedToHub>(OnConnected);
        AddHandler<H2C.SendMessage>(OnSendMessage);
        AddHandler<H2C.PreHubMessageList>(OnPreHubMessageList);
        AddHandler<H2C.DisconnectToHub>(OnDisconnected);        
        AddHandler<C2H.WhisperMessageSendAck>(WhisperMessageSendAck);
        AddHandler<H2C.RecvWhisperMessage>(RecvWhisperMessage);
        AddHandler<H2C.NotifyLootCreature>(OnNotifyLootCreatureMessage);
        AddHandler<H2C.NotifyLootRune>(OnNotifyLootRuneMessage);

        AddHandler<C2H.ChannelConnectAck>(ConnectChannelAckHandler);

        AddHandler<H2C.GuildAttendNotification>(GuildAttendNotificationHandler);
        AddHandler<H2C.GuildGoldNotification>(GuildGoldNotificationHandler);
        AddHandler<H2C.GuildJoinNotification>(GuildJoinNotificationHandler);
        
        AddHandler<C2H.GuildConnectAck>(ConnectGuildHandler);

        AddHandler<H2C.GuildLeaveNotification>(GuildLeaveNotificationHandler);
        AddHandler<H2C.FriendsRequest>(FriendsRequestHandler);

        IsListenChannel = true;
        IsListenYell = true;
        IsListenGuild = true;
        isListenWhisper = true;
    }

    public ChatClient()
    {
        Init();
    }

    void ConnectGuildHandler(C2H.GuildConnectAck packet)
    {

    }

    void FriendsRequestHandler(H2C.FriendsRequest packet)
    {
        if (packet.target_user.account_idx == SHSavedData.AccountIdx)
        {
            ChatLine line = new ChatLine(packet);
            ChatLineManager.Instance.AddLine(line);
            m_ChatLabelCallback(line);

            Network.Instance.NotifyMenu.is_friends_requested = true;
        }
    }

    void GuildAttendNotificationHandler(H2C.GuildAttendNotification packet)
    {
        if (GuildManager.Instance.IsGuildJoined == false) return;
        if (GuildManager.Instance.GuildInfo.info.guild_idx != packet.guild_idx) return;
        GuildManager.Instance.UpdateGuildMembers(packet.user_info.account_idx, packet.exp, true);

        if (IsListenGuild == true)
        {
            ChatLine line = new ChatLine(packet);
            ChatLineManager.Instance.AddLine(line);
            m_ChatLabelCallback(line);
        }

        GuildManager.Instance.SetGuildInfo(packet.guild_info);
        if (GameMain.Instance.CurrentGameMenu == GameMenu.Guild)
        {
            GameMain.Instance.GetCurrentMenu().UpdateMenu();
        }
    }
    void GuildGoldNotificationHandler(H2C.GuildGoldNotification packet)
    {
        if (GuildManager.Instance.IsGuildJoined == false) return;
        if (GuildManager.Instance.GuildInfo.info.guild_idx != packet.guild_idx) return;
        GuildManager.Instance.UpdateGuildMembers(packet.user_info.account_idx, GuildInfoManager.Config.GetGivePoint(packet.gold));

        if (IsListenGuild == true)
        {
            ChatLine line = new ChatLine(packet);
            ChatLineManager.Instance.AddLine(line);
            m_ChatLabelCallback(line);
        }

        GuildManager.Instance.SetGuildInfo(packet.guild_info);
        if (GameMain.Instance.CurrentGameMenu == GameMenu.Guild)
        {
            GameMain.Instance.GetCurrentMenu().UpdateMenu();
        }
    }

    void GuildLeaveNotificationHandler(H2C.GuildLeaveNotification packet)
    {
        if (GuildManager.Instance.IsGuildJoined == false) return;
        if (GuildManager.Instance.GuildInfo.info.guild_idx != packet.guild_idx) return;

        if (packet.user_info.account_idx == SHSavedData.AccountIdx)
            return;

        if (IsListenGuild == true)
        {
            ChatLine line = new ChatLine(packet);
            ChatLineManager.Instance.AddLine(line);
            m_ChatLabelCallback(line);
        }


        GuildManager.Instance.RemoveMember(packet.user_info.account_idx);

        if (GameMain.Instance.CurrentGameMenu == GameMenu.Guild)
        {
            GameMain.Instance.GetCurrentMenu().UpdateMenu();
        }
    }
    void GuildJoinNotificationHandler(GuildJoinNotification packet)
    {
        if (packet.is_refuse == false)
        {
            GuildManager.Instance.SetGuildInfo(packet.guild_info);
            if (packet.user_info.account_idx == SHSavedData.AccountIdx)
            {
                JoinGuildChannel();
            }
            if (GameMain.Instance.CurrentGameMenu == GameMenu.Guild)
            {
                GameMain.Instance.GetCurrentMenu().UpdateMenu();
            }
            else
                Network.Instance.SendGuildUpdate();
        }

        if (IsListenGuild == true && packet.is_refuse == false)
        { 
            ChatLine line = new ChatLine(packet);
            ChatLineManager.Instance.AddLine(line);
            m_ChatLabelCallback(line);
        }
    }

    void ConnectChannelAckHandler(C2H.ChannelConnectAck packet)
    {
        if (PreMessageLoaded == false)
        {   
            C2H.PreHubMessageRequest _PreHubMessageRequest = new C2H.PreHubMessageRequest();
            _PreHubMessageRequest.group_name = packet.connected_channel;
            _PreHubMessageRequest.guild_info = GuildManager.Instance.GuildInfo.info;
            SendHubPacket(_PreHubMessageRequest);
        }
        GroupName = packet.connected_channel;
        m_HubGroupConnectedCallback(packet.connected_channel);
    }

    void RecvWhisperMessage(H2C.RecvWhisperMessage packet)
    {
        if (IsConnected == false)
            return;
        
        ChatLine line = new ChatLine(packet);
        ChatLineManager.Instance.AddLine(line);

        m_ChatLabelCallback(line);
    }

    void WhisperMessageSendAck(C2H.WhisperMessageSendAck packet)
    {
        if (IsConnected == false)
            return;
        
        ChatLine line = new ChatLine(packet);
        ChatLineManager.Instance.AddLine(line);
        m_ChatLabelCallback(line);
    }

    public void RequestPreMessage(string group_name)
    {
        SendHubPacket(new C2H.PreHubMessageRequest { group_name = group_name, guild_info = GuildManager.Instance.GuildInfo.info });
    }

    public void SendChat(string message, pe_MsgType msg_type)
    {
        C2H.GroupMessageSend _GroupMessageSend = new C2H.GroupMessageSend();
        _GroupMessageSend.account_idx = SHSavedData.AccountIdx;
        _GroupMessageSend.group_name = msg_type == pe_MsgType.Guild ? GuildManager.Instance.GuildChannelName : GroupName;
        _GroupMessageSend.message = message;
        _GroupMessageSend.nickname = Network.PlayerInfo.nickname;
        _GroupMessageSend.level = Network.PlayerInfo.player_level;
        _GroupMessageSend.msg_type = msg_type;
        _GroupMessageSend.guild_info = GuildManager.Instance.GuildInfo.info;
        SendHubPacket(_GroupMessageSend);
    }

    public void SendWhisper(string message, string target_nickname)
    {
        C2H.WhisperMessageSend _WhisperMessageSend = new C2H.WhisperMessageSend();
        _WhisperMessageSend.account_idx = SHSavedData.AccountIdx;
        _WhisperMessageSend.target_nickname = target_nickname;
        _WhisperMessageSend.message = message;
        _WhisperMessageSend.nickname = Network.PlayerInfo.nickname;
        _WhisperMessageSend.level = Network.PlayerInfo.player_level;
        SendHubPacket(_WhisperMessageSend);
    }

    public void SendYell(string message)
    {
        C2H.GroupMessageSend _GroupMessageSend = new C2H.GroupMessageSend();
        _GroupMessageSend.account_idx = SHSavedData.AccountIdx;
        _GroupMessageSend.group_name = GroupName;
        _GroupMessageSend.message = message;
        _GroupMessageSend.nickname = Network.PlayerInfo.nickname;
        _GroupMessageSend.level = Network.PlayerInfo.player_level;
        _GroupMessageSend.msg_type = pe_MsgType.Yell;

        SendHubPacket(_GroupMessageSend);
    }

    public void ConnectGuild(pd_GuildInfo info, bool is_join = false)
    {
        C2H.GuildConnect packet = new C2H.GuildConnect();
        packet.guild_info = info;
        packet.is_join_guild = is_join;

        SendHubPacket(packet);
    }

    void OnSendMessage(H2C.SendMessage packet)
    {
        if (IsConnected == false)
            return;

        if (packet.msg_type == pe_MsgType.Normal && IsListenChannel == false)
            return;
        if (packet.msg_type == pe_MsgType.Guild && IsListenGuild == false)
            return;
        if (packet.msg_type == pe_MsgType.RecvWhisper && isListenWhisper == false)
            return;
        if (packet.msg_type == pe_MsgType.Yell && IsListenYell == false)
            return;
        
        ChatLine line = new ChatLine(packet);        
        ChatLineManager.Instance.AddLine(line);
        
        m_ChatLabelCallback(line);
    }

    void OnNotifyLootCreatureMessage(H2C.NotifyLootCreature packet)
    {
        ChatLine line = new ChatLine(packet);
        ChatLineManager.Instance.AddLine(line);
        m_ChatLabelCallback(line);
    }

    void OnNotifyLootRuneMessage(H2C.NotifyLootRune packet)
    {
        ChatLine line = new ChatLine(packet);
        ChatLineManager.Instance.AddLine(line);
        m_ChatLabelCallback(line);
    }

    void OnPreHubMessageList(H2C.PreHubMessageList packet)
    {   
        ChatLineManager.Instance.PreMessageLoad(packet.list);
        if(m_ChatPreMsgCallback != null)
            m_ChatPreMsgCallback(packet.list);

        PreMessageLoaded = true;
    }

    void OnDisconnected(H2C.DisconnectToHub packet)
    {
        switch (packet.error_code)
        {
            case pe_HubErrorCode.Normal:            
            case pe_HubErrorCode.InvalidHeader:
                if(m_HubDisconnectedCallback != null)
                    m_HubDisconnectedCallback(true, false);
                break;
            case pe_HubErrorCode.Maintenance:
                if (m_HubDisconnectedCallback != null)
                    m_HubDisconnectedCallback(false, true);
                break;
        }
    }

    void OnConnected(ConnectedToHub packet)
    {
        ChannelCount = packet.group_count;
        BannedAt = packet.banned_at;

        if (m_HubConnectedCallback != null)
            m_HubConnectedCallback(packet);
    }

    public event OnHubConnectedDelegate OnHubConnectedCallback { add { m_HubConnectedCallback += value; } remove { m_HubConnectedCallback -= value; } }
    public event OnHubGroupConnectedDelegate OnGroupConnectedCallback { add { m_HubGroupConnectedCallback += value; } remove { m_HubGroupConnectedCallback -= value; } }
    public event OnHubMsgInsertLabel OnChatMsgInsertLabel { add { m_ChatLabelCallback += value; } remove { m_ChatLabelCallback -= value; } }
    public event OnHubPreMsgDelegate OnChatPreMsgCallback { add { m_ChatPreMsgCallback += value; } remove { m_ChatPreMsgCallback -= value; } }
    public event OnHubDisconnected OnHubDisconnectedCallback { add { m_HubDisconnectedCallback += value; } remove { m_HubDisconnectedCallback -= value; } }

    private OnHubGroupConnectedDelegate m_HubGroupConnectedCallback;
    private OnHubMsgInsertLabel m_ChatLabelCallback;
    private OnHubPreMsgDelegate m_ChatPreMsgCallback;
}

public class HubClient
{
    Dictionary<string, IHubHandler> m_handlers = new Dictionary<string, IHubHandler>();

    protected OnHubConnectedDelegate m_HubConnectedCallback;
    protected OnHubDisconnected m_HubDisconnectedCallback;

    public int ChannelCount { get; set; }
    public bool IsConnected { get { return State == HubState.connected; } }

    public string HubName { get; private set; }
    public pe_HubType HubType { get; private set; }
    
    Hub hub { get { return m_connection.Hubs[0]; } }
    public string GroupName { get; set; }
    

    public HubState State;    
    public bool WaitReconnect { get; private set; }

    bool m_disconnected = true;
    Connection m_connection;
    string m_uri;
    long m_token;

    public void Start(pe_HubType hub_type, string uri, string group, long token, int channel_count)
    {
        State = HubState.idle;
        HubType = hub_type;
        GroupName = group;
        m_uri = uri;
        HubName = hub_type.ToString();
        m_token = token;

        ChannelCount = channel_count;
        
        Connect();
    }

    public void Connect()
    {   
        if (State != HubState.idle || string.IsNullOrEmpty(m_uri) )
        {
            Close();
            return;
        }
        if (IsConnected == true)
        {
            return;
        }

        C2H.HubConnectHeader header = new C2H.HubConnectHeader();
        header.access_token = m_token;
        header.nickname = Network.PlayerInfo.nickname;
        header.account_idx = SHSavedData.AccountIdx;

        m_connection = new Connection(new Uri(m_uri), HubName);
        m_connection.AuthenticationProvider = new HeaderAuthentication(header);

        m_connection.OnConnected += OnConnected;
        m_connection.OnClosed += OnClosed;
        m_connection.OnError += OnError;
        m_connection.OnStateChanged += OnStateChange;
        m_connection.Open();

        
        State = HubState.connecting;
    }

    private void OnStateChange(Connection connection, ConnectionStates oldState, ConnectionStates newState)
    {
        Debug.LogFormat("[{0}] [{1}] => [{2}]", hub.Name, oldState, newState);

        
        if (oldState == ConnectionStates.Initial && newState == ConnectionStates.Negotiating)
        {
            if (State == HubState.negotiating)
            {
                Close();
                return;
            }
            State = HubState.negotiating;
        }
    }

    private void OnError(Connection connection, string error)
    {
        connection.Close();

        Debug.LogFormat("OnError : [{0}] [{1}] [{2}]", hub.Name, connection.State, error);
        if (m_HubDisconnectedCallback != null && m_disconnected == true)
            m_HubDisconnectedCallback(false, false);
    }

    void OnClosed(Connection connection)
    {
        State = HubState.idle;

        Debug.LogFormat("OnClosed : [{0}] [{1}]", hub.Name, connection.State);
        m_disconnected = false;
    }

    public void Close()
    {
        if (m_connection == null) 
            return;
        State = HubState.idle;
        m_connection.Close();
    }

    void OnConnected(Connection connection)
    {   
        State = HubState.connected;

        hub.On("H2C_SendPacket", OnReceivePacket);

        m_disconnected = true;
    }

    public void ChangeGroup(string group)
    {
        C2H.ChannelConnect packet = new C2H.ChannelConnect();
        packet.choose_channel = group;
        packet.before_channel = GroupName;
        packet.my_nickname = Network.PlayerInfo.nickname;
        SendHubPacket(packet);
    }

    public void ChangeNickname(string new_nickname)
    {
        C2H.NicknameChange _NicknameSet = new C2H.NicknameChange();
        _NicknameSet.new_nickname = new_nickname;
        SendHubPacket(_NicknameSet);
    }

    protected void AddHandler<PT>(Action<PT> callback)
    {
        m_handlers.Add(typeof(PT).Name, new HubHandler<PT>(callback));
    }

    protected void SendHubPacket<PT>(PT packet) 
        where PT : class
    {
        PacketCore _PacketHub = new PacketCore();
        _PacketHub.Name = typeof(PT).Name;
        _PacketHub.Data = JsonConvert.SerializeObject(packet);
        Debug.LogFormat("(Send) [{0}] [{1}] : [{2}]", hub.Name, _PacketHub.Name, _PacketHub.Data);        
        hub.Call("SendPacket", _PacketHub.Name, _PacketHub.Data);
    }

    void OnReceivePacket(Hub hub, MethodCallMessage message)
    {
        PacketCore packet = new PacketCore();
        packet.Data = ((IDictionary)message.Arguments[0])["Data"].ToString();
        packet.Name = ((IDictionary)message.Arguments[0])["Name"].ToString();

        Debug.LogFormat("(Recv) [{0}] [{1}] : [{2}]", hub.Name, packet.Name, packet.Data);

        IHubHandler handler;
        if (m_handlers.TryGetValue(packet.Name, out handler) == true)
        {
            handler.Handler(packet);
        }
    }

    public void LeaveGuildChannel()
    {
        C2H.GuildLeave packet = new C2H.GuildLeave();
        
        packet.guild_info = GuildManager.Instance.GuildInfo.info;
        packet.leave_user = new HubUserInfo();
        packet.leave_user.nickname = Network.PlayerInfo.nickname;
        SendHubPacket(packet);
    }

    public void JoinGuildChannel()
    {
        C2H.GuildConnect packet = new C2H.GuildConnect();
        packet.guild_info = GuildManager.Instance.GuildInfo.info;
        packet.nickname = Network.PlayerInfo.nickname;
        SendHubPacket(packet);
    }
}


public class HubHandler<RecvT> : IHubHandler
{
    Action<RecvT> callback;
    public string Name { get { return typeof(RecvT).Name; } }
    public HubHandler(Action<RecvT> callback)
    {
        this.callback = callback;
    }

    public void Handler(PacketCore packet)
    {
        callback(JsonConvert.DeserializeObject<RecvT>(packet.Data));
    }
}

public interface IHubHandler
{
    void Handler(PacketCore packet);
    string Name { get; }
}

public class HeaderAuthentication : IAuthenticationProvider
{
    private C2H.HubConnectHeader header;
    public HeaderAuthentication(C2H.HubConnectHeader connect_header)
    {
        header = connect_header;
    }
    public bool IsPreAuthRequired
    {
        get
        {
            return false;
        }
    }

    public event OnAuthenticationFailedDelegate OnAuthenticationFailed;
    public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

    public void PrepareRequest(HTTPRequest request, RequestTypes type)
    {
        request.SetHeader("header", JsonConvert.SerializeObject(header));
    }

    public void StartAuthentication()
    {

    }
}

public enum HubState
{   
    idle = 0,
    negotiating = 1,
    connecting = 2,
    connected = 3,
}