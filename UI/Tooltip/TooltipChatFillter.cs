using UnityEngine;
using System.Collections;

public class TooltipChatFillter : TooltipBase {

    public GameObject m_Contents;
    public UIToggle m_ChannelFillter;
    public UIToggle m_WhisperFillter;
    public UIToggle m_GuildFillter;
    public UIToggle m_YellFillter;


    System.Action OnClickChannel;
    System.Action OnClickWhisper;
    System.Action OnClickGuild;
    System.Action OnClickYell;

    public override void Init(params object[] parms)
    {
        OnClickChannel = parms[0] as System.Action;
        OnClickWhisper = parms[1] as System.Action;
        OnClickGuild = parms[2] as System.Action;
        OnClickYell = parms[3] as System.Action;

        m_ChannelFillter.value = Network.ChatServer.IsListenChannel;
        m_WhisperFillter.value = Network.ChatServer.isListenWhisper;
        m_GuildFillter.value = Network.ChatServer.IsListenGuild;
        m_YellFillter.value = Network.ChatServer.IsListenYell;

        m_Contents.transform.localPosition = new Vector3(-455, 0);
    }

    public void OnChannelMark(UIToggle toggle)
    {
        Network.ChatServer.IsListenChannel = toggle.value;
    }
    public void OnChannelClick()
    {
        OnClickChannel();
        Close();
    }

    public void OnWhisperMark(UIToggle toggle)
    {
        Network.ChatServer.isListenWhisper = toggle.value;
    }

    public void OnWhisperClick()
    {
        OnClickWhisper();
        Close();
    }

    public void OnGuildMark(UIToggle toggle)
    {   
        Network.ChatServer.IsListenGuild = toggle.value;
    }

    public void OnGuildClick()
    {
        OnClickGuild();
        Close();
    }

    public void OnYellMark(UIToggle toggle)
    {
        Network.ChatServer.IsListenYell = toggle.value;
    }

    public void OnYellClick()
    {
        OnClickYell();
        Close();
    }

    public void Close()
    {
        OnFinished();
    }
}
