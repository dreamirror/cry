using UnityEngine;
using System.Collections;

public class GuildTab : MonoBehaviour
{
    public UIToggle m_Toggle;
    public UILabel m_LabelTitle;

    eGuildTabMode m_Mode;
    System.Action<eGuildTabMode> m_Callback = null;
    public void Init(string title, eGuildTabMode mode, System.Action<eGuildTabMode> callback)
    {
        m_LabelTitle.text = title;
        m_Mode = mode;
        m_Toggle.Set(false);
        m_Callback = callback;
    }

    public void OnTabClick()
    {
        switch(m_Mode)
        {
            case eGuildTabMode.GuildCreate:
            case eGuildTabMode.RequestList:
                if (Network.PlayerInfo.player_level < GuildInfoManager.Config.AtLeastPlayerLevel)
                {
                    Tooltip.Instance.ShowMessageKeyFormat("GuildCreateAtLeast", GuildInfoManager.Config.AtLeastPlayerLevel);
                    return;
                }
                break;
        }
        m_Toggle.value = true;
        if (m_Callback != null)
            m_Callback(m_Mode);
    }

}
