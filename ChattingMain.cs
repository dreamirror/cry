using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketEnums;
using System;
using PacketInfo;

class LineData
{   
    public Color color;    
    public string message;
    public DateTime received_at;
}

public class ChattingMain : MonoBehaviour
{
    static ChattingMain m_Instance = null;

    static public ChattingMain Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = ((GameObject)Instantiate(Resources.Load("Prefab/Chatting"))).GetComponent<ChattingMain>();
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    static public bool IsInstance { get { return m_Instance != null; } }
    static public void Clear() { Destroy(m_Instance.gameObject); m_Instance = null; }
    public GameObject contents;
    public bool is_notify_icon;

    ChattingLabel m_Label;
    List<LineData> m_label_data;

    bool is_chat_connect;
    bool is_need_chatinfo;

    bool is_maintenance;
    bool need_disconnect;
    
    bool show_popup;

    bool m_init = false;

    public void Init()
    {
        if (m_init == false)
        {
            m_label_data = new List<LineData>();
            m_Label = Load();

            Network.ChatServer.OnHubConnectedCallback += OnChatConnected;
            Network.ChatServer.OnGroupConnectedCallback += OnChatGroupConnected;
            Network.ChatServer.OnHubDisconnectedCallback += OnChatDisconnect;
            Network.ChatServer.OnChatMsgInsertLabel += OnChatMsgInsertLabel;
            Network.ChatServer.OnChatPreMsgCallback += OnChatPreMsgHandler;
            
        }
        if (string.IsNullOrEmpty(Network.PlayerInfo.nickname) == false)
        {
            RequestChatInfo();
        }
        m_init = true;
    }

    private void OnChatConnected(H2C.ConnectedToHub connect)
    {
        is_maintenance = false;
        Network.ChatServer.ChangeGroup(string.Empty);

        if(GuildManager.Instance.IsGuildJoined == true)
            Network.ChatServer.ConnectGuild(GuildManager.Instance.GuildInfo.info);
    }

    private void OnChatHubCloseHandler(bool need_reconnect)
    {
        if (is_chat_connect == false)
            is_chat_connect = need_reconnect;
    }

    private void OnChatPreMsgHandler(List<ChatMessage> line)
    {   
    }

    private void OnChatMsgInsertLabel(ChatLine line)
    {
        if (line.AccountIdx != SHSavedData.AccountIdx)
        {
            InsertNewLabelMessage(line.GetColor(), line.Msg);

            if (contents != null && contents.activeSelf == true)
            {
                if (is_notify_icon == true)
                    return;
                else
                {
                    is_notify_icon = true;
                    GameMain.Instance.UpdateNotify(true);
                }
            }
        }
    }

    public void RequestChatInfo()
    {
        if (Network.ChatServer.IsConnected == false)
        {
            Network.ShowIndicator();
            Network.ChatServer.Start(pe_HubType.SmallHeroChat, Network.Instance.GetChattingUri, string.IsNullOrEmpty(Network.ChatServer.GroupName) ? string.Empty : Network.ChatServer.GroupName, 0, 25);
        }
    }

    void OnChatGroupConnected(string group_name)
    {
        Network.HideIndicator();
        ChatLineManager.Instance.AddLine(new ChatLine(Localization.Format("ConnectChannel", group_name)));

        if (show_popup == true)
            ShowPopup();
        show_popup = false;
    }

    void OnChatDisconnect(bool need_reconnect, bool is_maintenance)
    {
        
        if (is_maintenance == true)
        {   
            need_disconnect = true;
            is_chat_connect = false;
            this.is_maintenance = is_maintenance;
        }
        else if (need_reconnect == true)
        {
            is_need_chatinfo = true;
            is_chat_connect = true;
        }
        
    }
    
    void Update()
    {
        CheckHub();
        CheckMainLabel();
    }
        
    public void ShowChattingPopup()
    {
        //if (GameConfig.Get<bool>("contents_chatting_maintenance"))
        //{
        //    Tooltip.Instance.ShowMessage(Localization.Get("ChatMaintenance"));
        //    return;
        //}
        if (Network.ChatServer.IsConnected == false)
        {
            RequestChatInfo();
            show_popup = true;
        }
        else
            ShowPopup();
    }
    void ShowPopup()
    {
        Popup.Instance.Show(ePopupMode.Chat);
    }
    
    public void InsertNewLabelMessage(Color color, string message)
    {
        if (m_label_data.Count == 0 && m_Label.CheckLabel())
            m_Label.SetLabel(color, message);
        else
            m_label_data.Add(new LineData { color = color, message = message, received_at = DateTime.Now });
    }

    public void ActiveChatLabel(bool is_active)
    {
        if (is_active == false)
        {
            if (is_notify_icon == true)
            {
                is_notify_icon = false;
                GameMain.Instance.UpdateNotify(true);
            }            
        }
        contents.SetActive(is_active);
    }
    
    ChattingLabel Load()
    {
        ChattingLabel result = null;

        ChattingLabel obj = Resources.Load("Prefab/ChattingLabel", typeof(ChattingLabel)) as ChattingLabel;

        result = Instantiate(obj);
        result.transform.parent = contents.transform;
        result.transform.localPosition = Vector3.zero;
        result.transform.localScale = Vector3.one;
        result.gameObject.SetActive(false);
        return result;
    }

    void CheckMainLabel()
    {
        if (m_Label == null || GameConfig.Get<bool>("contents_chatting_maintenance") == true)
            return;

        if (m_Label.CheckLabel() && m_label_data.Count > 0)
        {
            m_Label.SetLabel(m_label_data[0].color, m_label_data[0].message);
            m_label_data.Remove(m_label_data[0]);
        }
    }

    void OnDestroy()
    {
        Close();
    }

    void OnApplicationPause(bool pause_state)
    {
        if (is_maintenance == true)
            return;

        if (pause_state)
        {
            Close();
        }
        else
        {
            if (Network.ChatServer.State == HubState.idle)
                is_chat_connect = true;
        }
    }

    public void ConnectCheck()
    {
        if (Network.ChatServer.IsConnected == false)
        {   
            is_chat_connect = true;
            is_need_chatinfo = true;
        }
    }

    public void Close()
    {
        if (Network.ChatServer.IsConnected == true)
        {
            if (is_maintenance == true)
                Tooltip.Instance.ShowMessageKey("ChatMaintenance");
            else
                ChatLineManager.Instance.AddLine(new ChatLine(Localization.Get("DisconnectChatServer")));

            Network.ChatServer.Close();            
            need_disconnect = false;
        }
    }

    void CheckHub()
    {
        if (is_chat_connect == true)
        {
            is_chat_connect = false;
            if (is_need_chatinfo == true)
            {
                Network.ChatServer.Close();
                RequestChatInfo();
                is_need_chatinfo = false;
            }
            else
                Network.ChatServer.Connect();
        }
        if (need_disconnect == true)
            Close();
    }
}
