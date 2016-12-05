using BestHTTP;
using CodeStage.AntiCheat.ObscuredTypes;
using MNS;
using PacketEnums;
using PacketInfo;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum eConnectState
{
    idle,
    connecting,
    connected,
}

public class Network : Singleton<Network> {

    static public int ReconnectIndex { get; private set; }
    static public int DailyIndex { get; private set; }
    static public int WeeklyIndex { get; private set; }

    static public bool IsAgree { get; set; }

    CS_Client game_server = new CS_Client();

    ChatClient chat_server = new ChatClient();

    static public ChatClient ChatServer { get { return Instance.chat_server; } }

    static public ClientPlayerData PlayerInfo { get; private set; }


    static public CS_Client GameServer { get { return Instance.game_server; } }
    //static public bool GameServerConnected { get { return GameServerState == eConnectState.connected; } }

    static public eConnectState ConnectState { get; set; }
    static public MapStageDifficulty NewStageInfo { get; set; }
    static public MapStageDifficulty LastOpenContentsStageInfo { get; set; }
    static public MapStageDifficulty BattleStageInfo { get; set; }
    static public ItemInfoBase TargetItemInfo { get; set; }
    static public PVPBattleInfo PVPBattleInfo { get; set; }

    TimeSpan ServerTimeSpan = new TimeSpan(0);
    public DateTime ServerTime { get { return DateTime.Now.Add(ServerTimeSpan); } }
    
    public pe_UnreadMailState UnreadMailState { get; private set; }
    public bool UpdateMail { get; set; }
    public bool UnreadNotifyMail { get; private set; }

    public C2G.NotifyMenu NotifyMenu { get; set; }

    public void SetUnreadMail(pe_UnreadMailState new_mail_state)
    {
        if (UnreadMailState != new_mail_state)
            UpdateMail = true;
        UnreadMailState = new_mail_state;
    }
    
    //    string server_uri = "http://sh_dev.monsmile.com:4120";

#if UNITY_EDITOR || UNITY_STANDALONE
  //  string server_uri = "http://localhost:3020";
   // public string GetChattingUri = "http://localhost:3010/chat";
	string server_uri = "http://sh_dev.monsmile.com:4120";
	public string GetChattingUri = "http://sh_dev.monsmile.com:4110/chat";
#elif SH_DEV
    string server_uri = "http://sh_dev.monsmile.com:4120";
    public string GetChattingUri = "http://sh_dev.monsmile.com:4110/chat";
#else
    string server_uri = "http://sh_dev.monsmile.com:4220";
    public string GetChattingUri = "http://sh_dev.monsmile.com:4210/chat";
#endif

    public bool IsInit { get; private set; }
    public void Init()
    {
        if (IsInit == true)
            return;

        IsInit = true;
        ObscuredPrefs.preservePlayerPrefs = true;
        game_server.InitServer(SHSavedData.AccountIdx, server_uri, "C2L", OnError);

        game_server.AddCommonHandler<NetworkCore.PacketError>(OnPacketError);
        game_server.AddPreHandler<C2G.DailyIndex>(OnDailyIndex);
        game_server.AddPreHandler<C2G.ReconnectInfo>(OnReconnectInfo);
        game_server.AddPostHandler<C2G.QuestProgress>(OnQuestProgress);
        game_server.AddPostHandler<C2G.UnreadMail>(OnUnreadMail);
        game_server.AddPostHandler<C2G.NotifyMenu>(OnNotifyMenu);

        game_server.PostCallback = Networking.Instance.OnPostCallback;
        
        NotifyMenu = new C2G.NotifyMenu();
        ResetConnectingState();
    }
    static public void Uninit()
    {
        ConnectState = eConnectState.idle;
    }

    public void RequestAssetBundleVersion(System.Action<C2L.RequestAssetBundleVersion, C2L.RequestAssetBundleVersionAck> callback)
    {
        var packet = new C2L.RequestAssetBundleVersion();
        packet.app_info = GetAppInfo();
        game_server.JsonAsync<C2L.RequestAssetBundleVersion, C2L.RequestAssetBundleVersionAck>(packet, callback);
    }

    public void StartConnect()
    {
        //#if UNITY_EDITOR
        //        SHSavedData.Instance.InfoVersion = 0;
        //        SHSavedData.AccessToken = 1;
        //#endif
        
        game_server.InitPacketNamespace("C2L");

        long account_idx = SHSavedData.AccountIdx;
        if (account_idx != -1)
        {
            ProcessLoginAuto();
        }
        else
        {
            ProcessPlatformCheck();
        }
    }

    void ProcessLoginAuto()
    {
        ConnectState = eConnectState.connecting;
        C2L.LoginAuto _LoginAuto = new C2L.LoginAuto();
        _LoginAuto.account_idx = SHSavedData.AccountIdx;
        _LoginAuto.login_token = SHSavedData.LoginToken;
        _LoginAuto.access_token = SHSavedData.AccessToken;

        _LoginAuto.info_version = SHSavedData.Instance.InfoVersion;

        _LoginAuto.push_id = PushManager.Instance.PushID;
        _LoginAuto.push_token = PushManager.Instance.PushToken;

        _LoginAuto.login_platform = SHSavedData.LoginPlatform;
        _LoginAuto.app_info = GetAppInfo();

        _LoginAuto.agree = IsAgree;

        game_server.JsonAsync<C2L.LoginAuto, C2L.LoginAutoAck>(_LoginAuto, OnLoginAuto);
    }

    void OnError(CS_Client server, string name, HTTPRequest request, HTTPResponse response)
    {
        HideIndicator();
        if (ConnectState == eConnectState.connecting)
            ConnectState = eConnectState.idle;

        if (request.State != HTTPRequestStates.Finished)
        {
            switch(request.State)
            {
                case HTTPRequestStates.ConnectionTimedOut:
                case HTTPRequestStates.TimedOut:
                    Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action<CS_Client, HTTPRequest>(CallbackRetry), new object[] { server, request }, "Retry"), "NetworkErrorTimeOut");
                    break;

                case HTTPRequestStates.Error:
                    Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "NetworkErrorQuit", request.Exception.Message);
                    break;

                default:
                    Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "NetworkErrorQuit", request.State);
                    break;
            }
        }
        else
        {
            switch (response.StatusCode)
            {
//                 case 500:
//                     Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action<CS_Client, HTTPRequest>(CallbackRetry), new object[] { server, request }, "Retry"), "NetworkErrorTimeOut");
//                     break;
// 
                default:
                    Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "NetworkErrorQuit", string.Format("[{0}]\n{1}", response.StatusCode, response.Message));
                    break;
            }
        }
    }

    static public bool IsRetry { get; private set; }

    void CallbackRetry(CS_Client server, HTTPRequest request)
    {
//        ConnectState = eConnectState.connecting;

        server.ResendPacket(request);
        IsRetry = false;
    }

    void CallbackQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    void CallbackRestart()
    {
        if (GameMain.Instance)
            GameMain.Instance.Logout(false);
    }

    void ResetConnectingState()
    {
        ConnectState = eConnectState.idle;
        PlayerInfo = null;
#if !FINAL_BUILD
        CheatCommandHandlers.Uninitialize();
#endif
    }

    void ProcessPlatformCheck()
    {
        C2L.PlatformCheck _PlatformCheck = new C2L.PlatformCheck();
        _PlatformCheck.login_platform = SHSavedData.LoginPlatform;

        switch (SHSavedData.LoginPlatform)
        {
            case LoginPlatform.Facebook:
                _PlatformCheck.login_id = Facebook.Unity.AccessToken.CurrentAccessToken.UserId;
                break;

            case LoginPlatform.GameCenter:
            case LoginPlatform.GooglePlay:
                _PlatformCheck.login_id = UM_GameServiceManager.Instance.Player.PlayerId;
                break;

            case LoginPlatform.Guest:
                ProcessGuestLogin();
                return;

            case LoginPlatform.Betakey:
                ProcessBetakeyLogin();
                return;
        }
        game_server.JsonAsync<C2L.PlatformCheck, C2L.PlatformCheckAck>(_PlatformCheck, OnPlatformCheck);
    }

    void OnPlatformCheck(C2L.PlatformCheck packet, C2L.PlatformCheckAck ack)
    {
        C2L.PlatformLogin _PlatformLogin = new C2L.PlatformLogin();
        _PlatformLogin.login_platform = packet.login_platform;
        _PlatformLogin.login_id = packet.login_id;
        _PlatformLogin.bundle_identifier = GetAppInfo().bundle_identifier;
        _PlatformLogin.is_new = !ack.is_exist;
        SendPlatformLogin(_PlatformLogin);
    }

    void SendPlatformLogin(C2L.PlatformLogin _PlatformLogin)
    {
        game_server.JsonAsync<C2L.PlatformLogin, C2L.PlatformLoginAck>(_PlatformLogin, OnPlatformLogin);
    }

    void OnPlatformLogin(C2L.PlatformLogin packet , C2L.PlatformLoginAck ack)
    {
        if (ack.result == C2L.eLoginResult.Successed)
        {
            SHSavedData.AccountIdx = ack.account_idx;
            SHSavedData.LoginToken = ack.login_token;
            SHSavedData.LoginPlatform = packet.login_platform;

            game_server.InitAccountIdx(SHSavedData.AccountIdx);

            ProcessLoginAuto();
        }
        else
        {
            ConnectState = eConnectState.idle;
            Popup.Instance.ShowMessageKey("LoginResult_" + ack.result);
        }
    }

    void OnInputBetaKey(string key)
    {
        ConnectState = eConnectState.connecting;

        C2L.BetakeyLogin packet = new C2L.BetakeyLogin();
        packet.bundle_identifier = GetAppInfo().bundle_identifier;
        packet.betakey = key;

        game_server.JsonAsync<C2L.BetakeyLogin, C2L.BetakeyLoginAck>(packet, OnBetakeyLogin);
    }

    void OnBetakeyLogin(C2L.BetakeyLogin packet, C2L.BetakeyLoginAck ack)
    {
        if (ack.result == C2L.eLoginResult.Successed)
        {
            SHSavedData.AccountIdx = ack.account_idx;
            SHSavedData.LoginToken = ack.login_token;
            SHSavedData.LoginPlatform = LoginPlatform.Betakey;

            game_server.InitAccountIdx(SHSavedData.AccountIdx);

            ProcessLoginAuto();
        }
        else
        {
            ConnectState = eConnectState.idle;
            Popup.Instance.ShowMessageKey("LoginResult_"+ack.result);
        }
    }

    void OnGuestLogin(C2L.GuestLogin packet, C2L.GuestLoginAck ack)
    {
        if (ack.result == C2L.eLoginResult.Successed)
        {
            SHSavedData.AccountIdx = ack.account_idx;
            SHSavedData.LoginToken = ack.login_token;
            SHSavedData.LoginPlatform = LoginPlatform.Guest;

            game_server.InitAccountIdx(SHSavedData.AccountIdx);

            ProcessLoginAuto();
        }
        else
        {
            ConnectState = eConnectState.idle;
            Popup.Instance.ShowMessageKey("LoginResult_" + ack.result);
        }
    }


    void ProcessBetakeyLogin()
    {
        ConnectState = eConnectState.idle;
        Popup.Instance.Show(ePopupMode.Input, Localization.Get("InputBetakey"), (System.Action<string>)OnInputBetaKey);
    }

    void ProcessGuestLogin()
    {
        ConnectState = eConnectState.idle;
        C2L.GuestLogin _GuestLogin = new C2L.GuestLogin();
        _GuestLogin.bundle_identifier = GetAppInfo().bundle_identifier;
        game_server.JsonAsync<C2L.GuestLogin, C2L.GuestLoginAck>(_GuestLogin, OnGuestLogin);
    }

    void OnLoginAuto(C2L.LoginAuto packet, C2L.LoginAutoAck ack)
    {
        if (ack.result == C2L.eLoginResult.SessionExpired)
        {
            SHSavedData.AccountIdx = -1;
            if (SHSavedData.LoginPlatform == LoginPlatform.Facebook && Facebook.Unity.FB.IsLoggedIn)
                Facebook.Unity.FB.LogOut();
            if (SHSavedData.LoginPlatform == LoginPlatform.GooglePlay && UM_GameServiceManager.Instance.IsConnected)
                UM_GameServiceManager.Instance.Disconnect();
            SHSavedData.LoginPlatform = PacketEnums.LoginPlatform.Invalid;
            ConnectState = eConnectState.idle;
            return;
        }

        if (ack.result != C2L.eLoginResult.Successed)
        {
            if (ack.result == C2L.eLoginResult.NeedAgree)
            {
                Popup.Instance.Show(ePopupMode.Policy, new System.Action(ProcessLoginAuto));
                return;
            }
            //Debug.LogError(ack.result);

            ConnectState = eConnectState.idle;

            if (Localization.Exists("LoginResult_" + ack.result) == true)
                Popup.Instance.ShowMessageKey("LoginResult_" + ack.result);
            else
                Popup.Instance.ShowMessage(ack.result.ToString());

            return;
        }

        ConnectState = eConnectState.connecting;

        SHSavedData.AccessToken = ack.access_token;
        //         if (ack.request_info == false)
        //             SHSavedData.Instance.LoadData(true);
        //         if (ack.request_data == false)
        //             SaveDataManger.Instance.InitFromFile();

        game_server.InitPacketNamespace("C2G");
        game_server.InitSession(SHSavedData.AccountIdx, ack.access_token, ack.reconnect_index);

        if (ExceptionHandler.Instance != null)
            ExceptionHandler.Instance.SendLastReport();

        ack.request_data = true;
        ack.request_info = true;

        C2G.Connect _Connect = new C2G.Connect();
        _Connect.request_info = ack.request_info;
        _Connect.request_data = ack.request_data;
        game_server.JsonAsync<C2G.Connect, C2G.ConnectAck>(_Connect, OnConnect);
    }

    void OnConnect(C2G.Connect packet, C2G.ConnectAck ack)
    {
        Debug.Log("OnConnect : ");
        DailyIndex = ack.daily_index;
        WeeklyIndex = ack.weekly_index;
        ServerTimeSpan = ack.server_time - DateTime.Now;

        GameConfig.Instance.Init(ack.config_values);
        EventHottimeManager.Instance.Init(ack.events);
        
        if (ack.info_files != null)
        {
            SHSavedData.Instance.SaveGameInfo(ack.info_files, ack.info_version);
        }
        SHSavedData.Instance.LoadData();

        PlayerInfo = new ClientPlayerData(ack.player_info);
        
        GuildManager.Instance.SetGuildInfo(ack.guild);
#if !FINAL_BUILD
        CheatCommandHandlers.Uninitialize();
        if (PlayerInfo.can_cheat == true)
            CheatCommandHandlers.Initialize();
#endif

        QuestManager.Instance.Init(ack.player_info.quests);

        if (ack.detail_data != null)
            SaveDataManger.Instance.InitFromData(ack.detail_data);
        else
        {
            if (SaveDataManger.Instance.InitFromFile() == false)
            {
                C2G.Connect _Connect = new C2G.Connect();
                _Connect.request_info = true;
                _Connect.request_data = true;
                game_server.JsonAsync<C2G.Connect, C2G.ConnectAck>(_Connect, OnConnect);
                return;
            }
        }

        switch (ack.player_info.unread_mail)
        {
            case pe_UnreadMailState.MainMenuOpen:
                SetUnreadMail(pe_UnreadMailState.MainMenuOpen);
                break;
            case pe_UnreadMailState.UnreadMail:
                SetUnreadMail(pe_UnreadMailState.UnreadMail);
                break;
        }

        if (string.IsNullOrEmpty(PlayerInfo.nickname) == true)
        {
            Popup.Instance.Show(ePopupMode.Nickname, false, new EventDelegate.Callback(OnNicknameSetCallback));
        }
        else
        {
            ConnectState = eConnectState.connected;

            ChattingMain.Instance.Init();
        }
    }

    void OnNicknameSetCallback()
    {
        ConnectState = eConnectState.connected;
    }
    void OnPacketError(CS_Client server, HTTPRequest request, object send_packet, NetworkCore.PacketError packet)
    {
        HideIndicator();
        Debug.LogWarningFormat("OnPacketError({0}, {1}) : {2}", packet.type, send_packet.GetType().FullName, packet.message);

        if (ConnectState == eConnectState.connecting)
            ConnectState = eConnectState.idle;

        switch (packet.type)
        {
            case NetworkCore.PacketErrorType.Title:
                Popup.Instance.ShowCallback(new PopupCallback.Callback(new Action(CallbackRestart), null), packet.message);
                break;

            case NetworkCore.PacketErrorType.SessionExpired:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackRestart), null), "LoginResult_SessionExpired");
                break;

            case NetworkCore.PacketErrorType.NeedToUpdate:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new System.Action<string>(ApplicationUpdate), new object[] { packet.message }), "NeedToUpdate");
                break;

            case NetworkCore.PacketErrorType.Retry:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action<CS_Client, HTTPRequest>(CallbackRetry), new object[] { server, request }, "Retry"), "ServerError", packet.message);
                IsRetry = true;
                break;

            case NetworkCore.PacketErrorType.Maintenance:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "LoginResult_Maintenance", packet.message);
                break;

            case NetworkCore.PacketErrorType.Timeout:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action<CS_Client, HTTPRequest>(CallbackRetry), new object[] { server, request }, "Retry"), "NetworkErrorTimeOut");
                IsRetry = true;
                break;

            case NetworkCore.PacketErrorType.UserLimit:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "LoginResult_UserLimit", packet.message);
                break;

            case NetworkCore.PacketErrorType.Quit:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "ServerErrorQuit", packet.message);
                break;

            case NetworkCore.PacketErrorType.Ignore:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(null, null), "ServerError", packet.message);
                break;

            case NetworkCore.PacketErrorType.ServerForward:
                game_server.InitUri(packet.message);
                CallbackRetry(server, request);
                break;

            case NetworkCore.PacketErrorType.Message:
                Popup.Instance.ShowMessage(packet.message);
                break;

            case NetworkCore.PacketErrorType.MessageKey:
                Popup.Instance.ShowMessageKey(packet.message);
                break;

            case NetworkCore.PacketErrorType.Reconnect:
                CallbackRetry(server, request);
                break;

            default:
                Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(CallbackQuit), null), "ServerErrorQuit", packet.message);
                break;

        }
    }

    public void SetLeader(pd_LeaderCreatureInfo info)
    {
        PlayerInfo.leader_creature = info;

        C2G.SetLeader packet = new C2G.SetLeader();
        packet.leader_creature = info;
        game_server.JsonAsync<C2G.SetLeader, NetworkCore.AckDefault>(packet, null);
    }

    void OnDailyIndex(CS_Client server, HTTPRequest request, object send_packet, C2G.DailyIndex packet)
    {
        DailyIndex = packet.daily_index;
        WeeklyIndex = packet.weekly_index;
    }

    void OnReconnectInfo(CS_Client server, HTTPRequest request, object send_packet, C2G.ReconnectInfo packet)
    {
        game_server.InitReconnectIndex(packet.reconnect_index);
        GameConfig.Instance.Update(packet.game_configs);
        EventHottimeManager.Instance.Update(packet.events, packet.event_idx);
    }

    void OnQuestProgress(CS_Client server, HTTPRequest request, object send_packet, C2G.QuestProgress packet)
    {
        QuestManager.Instance.UpdateData(packet.updates);
    }

    void OnUnreadMail(CS_Client server, HTTPRequest request, object send_packet, C2G.UnreadMail packet)
    {
        if (PlayerInfo == null)
            return;
        SetUnreadMail(packet.unread_type);
    }

    void OnNotifyMenu(CS_Client server, HTTPRequest request, object send_packet, C2G.NotifyMenu packet)
    {
        NotifyMenu = packet;
    }

    public bool CheckGoods(pe_GoodsType goods_type, long price)
    {
        long value = Network.PlayerInfo.GetGoodsValue(goods_type);
        if (price > value)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, goods_type);
            return false;
        }
        return true;
    }

    public bool CheckCreatureSlotCount(int count, bool slot_buy, bool show_message, PopupSlotBuy.OnOkDeleage callback)
    {
        if (count == 0)
            return true;

        if (CreatureManager.Instance.Creatures.Count + count > PlayerInfo.creature_count_max)
        {
            if (show_message)
            {
                if (slot_buy)
                    Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Creature, callback);
                else
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnCreatureSlotCallback), "CreatureSlotMax", "Purchase");
            }

            return false;
        }
        return true;
    }

    void OnCreatureSlotCallback(bool is_confirm)
    {
        if (is_confirm)
            Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Creature, null);
    }

    public bool CheckRuneSlotCount(int count, bool slot_buy, bool show_message, PopupSlotBuy.OnOkDeleage callback)
    {
        if (count == 0)
            return true;

        if (RuneManager.Instance.Runes.Count(r => r.CreatureIdx == 0) + count > PlayerInfo.rune_count_max)
        {
            if (show_message)
            {
                if (slot_buy)
                    Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Rune, callback);
                else
                    Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnRuneSlotCallback), "RuneSlotMax", "Purchase");
            }
            return false;
        }
        return true;
    }

    void OnRuneSlotCallback(bool is_confirm)
    {
        if (is_confirm)
            Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Rune, null);
    }

    C2L.AppInfo m_AppInfo = null;
    public C2L.AppInfo GetAppInfo()
    {
        if (m_AppInfo == null)
        {
            m_AppInfo = new C2L.AppInfo();
            m_AppInfo.bundle_identifier = Application.bundleIdentifier;
            m_AppInfo.bundle_version = Application.version;
            m_AppInfo.platform = Application.platform.ToString();
            m_AppInfo.device_info = string.Format("deviceModel : {0}", SystemInfo.deviceModel);
            m_AppInfo.device_info += string.Format(", os : {0}", SystemInfo.operatingSystem);
            m_AppInfo.device_info += string.Format(", deviceName : {0}", SystemInfo.deviceName);
            m_AppInfo.device_info += string.Format(", systemMemorySize : {0}", SystemInfo.systemMemorySize);
            m_AppInfo.device_info += string.Format(", graphicsDeviceName : {0}", SystemInfo.graphicsDeviceName);
            m_AppInfo.device_info += string.Format(", graphicsMemorySize : {0}", SystemInfo.graphicsMemorySize);
            m_AppInfo.device_info += string.Format(", graphicsShaderLevel : {0}", SystemInfo.graphicsShaderLevel);
            m_AppInfo.device_info += string.Format(", maxTextureSize : {0}", SystemInfo.maxTextureSize);
            m_AppInfo.device_info += string.Format(", npotSupport : {0}", SystemInfo.npotSupport);
        }
        return m_AppInfo;
    }

    public void ProcessReward3Ack(C2G.Reward3Ack ack)
    {
        ack.add_goods.ForEach(g => Network.PlayerInfo.AddGoods(g));
        ack.loot_items.ForEach(i => ItemManager.Instance.Add(i));
        ack.loot_runes.ForEach(i => RuneManager.Instance.Add(i));
        Network.Instance.LootCreatures(ack.loot_creatures, ack.loot_creatures_equip);

        GameMain.Instance.UpdatePlayerInfo();
    }

    public void LootCreature(pd_CreatureLootData data)
    {
        if (data == null)
            return;

        EquipManager.Instance.Add(data.equip[0]);
        EquipManager.Instance.Add(data.equip[1]);
        CreatureManager.Instance.Add(data.creature);
        
        SaveDataManger.Instance.Save();
    }

    public void LootCreatures(List<pd_CreatureData> creatures, List<pd_EquipData> creatures_equip)
    {
        if (creatures_equip != null)
            creatures_equip.ForEach(e => EquipManager.Instance.Add(e));

        if (creatures != null)
            creatures.ForEach(c => CreatureManager.Instance.Add(c));

        SaveDataManger.Instance.Save();
    }

    static public void ShowIndicator()
    {
#if UNITY_IOS
        Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.WhiteLarge);
#elif UNITY_ANDROID
        Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Large);
#endif
        Handheld.StartActivityIndicator();

//        MNP.ShowPreloader("HeroCry", "Loading...");
    }

    static public void HideIndicator()
    {
        Handheld.StopActivityIndicator();

        //MNP.HidePreloader();
    }

    static public string GetConnectedTimeString(DateTime last_login_at)
    {
        string res;
        var login_span = Network.Instance.ServerTime - last_login_at;
        if (login_span.TotalDays >= 1)
            res = Localization.Format("LastConnectedFormatDay", login_span.Days);
        else if (login_span.Hours >= 1)
            res = Localization.Format("LastConnectedFormatHour", login_span.Hours, login_span.Minutes);
        else
            res = Localization.Format("LastConnectedFormatMinute", login_span.Minutes);
        return res;
    }

    public void SendGuildUpdate()
    {
        C2G.GuildUpdate packet = new C2G.GuildUpdate();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        GameServer.JsonAsync<C2G.GuildUpdate, NetworkCore.AckDefault>(packet, null);
    }

    void ApplicationUpdate(string update_url)
    {
        if (string.IsNullOrEmpty(update_url) == false)
            Application.OpenURL(update_url);
        Application.Quit();
    }

}

