using UnityEngine;
using PacketEnums;
using System;

public class PopupChat : PopupBase
{
    //Text Grid
    public UIScrollView ChatLineScrollView;
    public UIGrid ChatLineGrid;
    public PrefabManager ChatLinePrefabManager;

    //Input
    public UIInput ChattingBarInput;

    //Fillter Groups
    public UILabel FillterLabel;

    //Whisper
    public UIInput WhisperTargetInput;

    //Change Channel Groups
    public UILabel CurrentChannelLabel;
    public GameObject CurrentChannelBtn;

    pe_MsgType m_selectedType = pe_MsgType.Normal;
    
    float m_start_scrollview_transform_y;
    bool DraggedScrollView { get { return (ChatLineScrollView.transform.localPosition.y - m_start_scrollview_transform_y) > 1; } }

    bool need_refresh;

    Color[] m_GradientBottomColor = new Color[4];
    
    public delegate void OnNicknameClickDelegate(PopupChatLine line);
    public delegate void OnItemClickDelegate(PopupChatLine line);

    void OnEnable()
    {
        ChattingMain.Instance.ActiveChatLabel(false);        
    }

    void OnDisable()
    {
        ChattingMain.Instance.ActiveChatLabel(true);
    }
    
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        Init();

        m_GradientBottomColor[0] = new Color(1f, 0.85f, 0.5f);
        m_GradientBottomColor[1] = new Color(0.5f, 0.5f, 1f);
        m_GradientBottomColor[2] = new Color(1f, 1f, 0.5f);
        m_GradientBottomColor[3] = new Color(1f, 0.6f, 0f);
        
        Network.ChatServer.OnChatMsgInsertLabel += OnRecvChatMessage;
        Network.ChatServer.OnGroupConnectedCallback += OnChannelConnected;
        Network.ChatServer.OnHubDisconnectedCallback += OnHubDisconnected;

        if (Application.platform == RuntimePlatform.Android)
        {
            ChattingBarInput.hideInput = true;
            WhisperTargetInput.hideInput = true;
        }
    }

    void OnChannelConnected(string group_name)
    {
        ChatLine chat = new ChatLine(Localization.Format("ConnectChannel", group_name));

        var line = ChatLinePrefabManager.GetNewObject<PopupChatLine>(ChatLineGrid.transform, Vector3.zero);

        line.Init(chat, null, null);

        if (ChatLineGrid.GetChildList().Count > ChatLineManager.Instance.ChattingMaxLine)
            ChatLinePrefabManager.Free(ChatLineGrid.GetChild(ChatLineGrid.GetChildList().Count - 1).gameObject);

        OnDragFinished(true);
    }

    void OnHubDisconnected(bool need_reconnect, bool is_maintenance)
    {
        if (is_maintenance == true)
        {
            OnClose();            
            return;
        }
        MessageDraw();
    }

    void OnRecvChatMessage(ChatLine line)
    {
        if (DraggedScrollView == true)
        {
            need_refresh = true;
            return;
        }

        var line_obj = ChatLinePrefabManager.GetNewObject<PopupChatLine>(ChatLineGrid.transform, Vector3.zero);
        line_obj.Init(line, new OnNicknameClickDelegate(OnClickNickname), new OnItemClickDelegate(OnClickItem));

        float v = (ChatLineScrollView.transform.localPosition.y - m_start_scrollview_transform_y);
        if (v < 1)
            ChatLineScrollView.ResetPosition();

        ChatLineGrid.Reposition();

        if (ChatLineGrid.GetChildList().Count > ChatLineManager.Instance.ChattingMaxLine)
            ChatLinePrefabManager.Free(ChatLineGrid.GetChild(ChatLineGrid.GetChildList().Count - 1).gameObject);
    }

    public override void OnFinishedHide()
    {   

        Network.ChatServer.OnChatMsgInsertLabel -= OnRecvChatMessage;
        Network.ChatServer.OnGroupConnectedCallback -= OnChannelConnected;
        Network.ChatServer.OnHubDisconnectedCallback -= OnHubDisconnected;
        base.OnFinishedHide();
    }

    public override void OnClose()
    {

#if UNITY_ANDROID
        if (AndroidKeyboard.TouchScreenKeyboard.instance != null)
            AndroidKeyboard.TouchScreenKeyboard.instance.active = false;
#endif

        //ChannelGroupObject.SetActive(false);
        ChatLinePrefabManager.Destroy();
        parent.Close();
    }

    public override void OnFinishedShow()
    {
        MessageDraw();
    }

    void Init()
    {
        ChatLineScrollView.onDragFinished = OnDragFinished;

        FillterLabel.text = Localization.Get(m_selectedType.ToString());
        CurrentChannelLabel.text = string.Format("{0}{1}", Localization.Get("Channel"), Network.ChatServer.GroupName);
        ChattingBarInput.defaultText = Localization.Get("InputMessage");
        ChattingBarInput.characterLimit = GameConfig.Get<int>("line_max_char");
        WhisperTargetInput.characterLimit = GameConfig.Get<int>("nickname_max");
    }

    void MessageDraw()
    {
        ChatLinePrefabManager.Destroy();

        foreach (var line in ChatLineManager.Instance.GetChatList())
        {
            var chat_line = ChatLinePrefabManager.GetNewObject<PopupChatLine>(ChatLineGrid.transform, Vector3.zero);
            chat_line.Init(line, new OnNicknameClickDelegate(OnClickNickname), new OnItemClickDelegate(OnClickItem));
        }
        ChatLineScrollView.ResetPosition();
        m_start_scrollview_transform_y = ChatLineScrollView.gameObject.transform.localPosition.y;

        ChatLineGrid.Reposition();
    }

    public void OnInputSubmit()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
            ChattingBarInput.isSelected = true;
        OnClickSendMessage();
    }

    public void OnClickSendMessage()
    {
        string text = NGUIText.StripSymbols(ChattingBarInput.value);

        if (string.IsNullOrEmpty(text) && !Localization.Get("InputMessage").Equals(text))
            return;

        ChattingMain.Instance.ConnectCheck();

        if (text.Length > GameConfig.Get<int>("line_max_char"))
            text = text.Remove(GameConfig.Get<int>("line_max_char"));

        switch (m_selectedType)
        {
            case pe_MsgType.Normal:
            case pe_MsgType.Guild:
                switch(text)
                {
                    case "@crash":
                        throw new System.Exception("CrashTest");

                    case "@cheat":
                        if (Network.PlayerInfo.can_cheat == true)
                            Popup.Instance.Show(ePopupMode.Cheat);
                        return;
                }
                if (m_selectedType == pe_MsgType.Guild && GuildManager.Instance.GuildInfo == null)
                {
                    return;
                }
                Network.ChatServer.SendChat(text,m_selectedType);
                break;
            case pe_MsgType.SendWhisper:
                Network.ChatServer.SendWhisper(text, WhisperTargetInput.value);
                break;
            case pe_MsgType.Yell:
                Network.ChatServer.SendYell(text);
                break;
        }

        ChattingBarInput.value = string.Empty;

        OnDragFinished(DraggedScrollView);
    }

    public void OnChannelList()
    {
        Tooltip.Instance.ShowTooltip(eTooltipMode.ChatChannel, new Action<string>(ChannelChangeHandler));
    }

    void OnDragFinished()
    {
        OnDragFinished(false);
    }
    void OnDragFinished(bool is_force)
    {
        if (is_force == true || (need_refresh == true && DraggedScrollView == false))
        {
            Array.ForEach(ChatLineGrid.GetComponentsInChildren<PopupChatLine>(), line => ChatLinePrefabManager.Free(line.gameObject));
            MessageDraw();
            need_refresh = false;
        }
    }

    void OnPreMessageReceive(H2C.PreHubMessageList list)
    {
        ChatLinePrefabManager.Destroy();
        MessageDraw();
    }

    public void OnClickChattingInput()
    {
#if UNITY_ANDROID
        AndroidKeyboard.AdditionalOptions.keepKeyboardOn = true;
#endif
        ChattingBarInput.keepKeyboardOn = true;
    }

    public void OnClickWhisperInput()
    {
#if UNITY_ANDROID
        AndroidKeyboard.AdditionalOptions.keepKeyboardOn = false;
#endif
        ChattingBarInput.keepKeyboardOn = false;
    }

    /////////////////////////// Fillter ////////////////////////////
    public void OnFillterClick()
    {
        Tooltip.Instance.ShowTooltip(eTooltipMode.ChatFillter, new Action(OnChannelClick), new Action(OnWhisperClick), new Action(OnGuildClick), new Action(OnYellClick));
    }

    public void OnChannelClick()
    {
        m_selectedType = pe_MsgType.Normal;
        FillterLabel.gradientBottom = m_GradientBottomColor[0];
        FillterLabel.gradientTop = new Color(1f, 1f, 1f);
        FillterLabel.text = Localization.Get(m_selectedType.ToString());

        CurrentChannelBtn.SetActive(true);
        WhisperTargetInput.gameObject.SetActive(false);
    }

    public void OnWhisperClick()
    {
        m_selectedType = pe_MsgType.SendWhisper;
        FillterLabel.gradientBottom = m_GradientBottomColor[1];
        FillterLabel.gradientTop = new Color(1f, 1f, 1f);
        FillterLabel.text = Localization.Get(m_selectedType.ToString());

        CurrentChannelBtn.SetActive(false);
        WhisperTargetInput.gameObject.SetActive(true);
    }

    public void OnGuildClick()
    {
        //TODO : Check exist guild
        if (GuildManager.Instance.GuildInfo == null || GuildManager.Instance.GuildInfo.info == null)
        {
            Tooltip.Instance.ShowMessageKey("NeedGuild");
            return;
        }

        m_selectedType = pe_MsgType.Guild;
        FillterLabel.gradientBottom = m_GradientBottomColor[2];
        FillterLabel.gradientTop = new Color(1f, 1f, 1f);
        FillterLabel.text = Localization.Get(m_selectedType.ToString());

        CurrentChannelBtn.SetActive(true);
        WhisperTargetInput.gameObject.SetActive(false);
    }
    
    public void OnYellClick()
    {
        m_selectedType = pe_MsgType.Yell;
        FillterLabel.gradientBottom = m_GradientBottomColor[3];
        FillterLabel.gradientTop = new Color(1f, 1f, 1f);
        FillterLabel.text = Localization.Get(m_selectedType.ToString());

        CurrentChannelBtn.SetActive(true);
        WhisperTargetInput.gameObject.SetActive(false);
    }

    public void OnClickFriendsWhisper(PopupChatLine chat)
    {
        m_selectedType = pe_MsgType.SendWhisper;
        FillterLabel.gradientBottom = m_GradientBottomColor[1];
        FillterLabel.gradientTop = new Color(1f, 1f, 1f);
        FillterLabel.text = Localization.Get(m_selectedType.ToString());

        CurrentChannelBtn.SetActive(false);
        WhisperTargetInput.gameObject.SetActive(true);

        WhisperTargetInput.value = chat.Line.Nickname;
    }

    public void OnClickNickname(PopupChatLine chat)
    {
        if (chat.Line.AccountIdx == 0 || chat.Line.AccountIdx == SHSavedData.AccountIdx)
            return;
        Tooltip.Instance.ShowTooltip(eTooltipMode.Profile, chat, new Action<PopupChatLine>(OnClickFriendsWhisper));
    }

    public void OnClickItem(PopupChatLine chat)
    {
        if (chat.Line.LootCreature == null)
            return;

        if(GameMain.Instance.CurrentGameMenu == GameMenu.Battle)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableShowCharacterInBattle");
            return;
        }
        Tooltip.Instance.ShowTooltip(eTooltipMode.Character, chat);
    }

    public void ChannelChangeHandler(string channel_name)
    {
        CurrentChannelLabel.text = channel_name;
    }

    public void OnClick()
    {
        ChattingMain.Instance.ConnectCheck();
    }
}
