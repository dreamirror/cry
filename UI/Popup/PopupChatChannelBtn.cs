using UnityEngine;
using System.Collections;


public class PopupChatChannelBtn : MonoBehaviour {

    public UILabel text;
    int m_channel_number;
    System.Action<string> ChannelChangeCallback;
    System.Action CloseTooltip;

    public void Init(int channel_number, System.Action<string> ChannelChangeCallback, System.Action CloseTooltip)
    {
        text.text = string.Format("{0}{1}", Localization.Get("Channel"), channel_number);        
        m_channel_number = channel_number;
        this.ChannelChangeCallback = ChannelChangeCallback;
        this.CloseTooltip = CloseTooltip;
    }

    public void OnChannelChange()
    {
        if (m_channel_number == int.Parse(Network.ChatServer.GroupName))  
            return;
        if (Network.ChatServer.IsConnected == false)
        {
            ChattingMain.Instance.ConnectCheck();
            return;
        }

        Network.ChatServer.ChangeGroup(m_channel_number.ToString());
        ChannelChangeCallback(string.Format("{0}{1}", Localization.Get("Channel"), m_channel_number.ToString()));
        CloseTooltip();
    }
}
