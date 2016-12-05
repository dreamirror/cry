using UnityEngine;
using System.Collections;

public class PlayerProfile : MonoBehaviour {
    
    public UISprite m_SpriteProfile;
    public UILabel m_LabelNickname;
    public UILabel m_LabelPlayerlevel;

    EventDelegate.Callback m_ClickCallback = null;
    public void UpdateProfile(PacketInfo.pd_LeaderCreatureInfo info, string nickname, int player_level, EventDelegate.Callback _callback = null)
    {
        m_LabelNickname.text = nickname;
        m_LabelPlayerlevel.text = player_level.ToString();
        m_SpriteProfile.spriteName = info.GetProfileName();
        m_ClickCallback = _callback;
    }

    public void OnClickProfile()
    {
        if (m_ClickCallback != null)
            m_ClickCallback();
    }
}
