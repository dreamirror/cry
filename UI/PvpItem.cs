using UnityEngine;
using System.Collections;
using PacketInfo;
using System;

public class PvpItem : MonoBehaviour
{
    public GameObject PrefabProfile;
    public GameObject ProfileIndicator;

    public UIToggle m_ToggleContents;
    public UILabel m_LabelRank;
    public UILabel m_LabelPower;

    PlayerProfile m_Profile;
    pd_PvpPlayerInfo m_Info;
    Action<pd_PvpPlayerInfo> m_OnStartBattleClick = null;
    public void Init(pd_PvpPlayerInfo info, Action<pd_PvpPlayerInfo> OnCallback = null)
    {
        if(info == null)
        {
            m_ToggleContents.value = false;
            return;
        }
        m_ToggleContents.value = true;

        m_Info = info;

        m_LabelPower.text = Localization.Format("PowerValue", info.team_power);

        m_LabelRank.text = Localization.Format("PVPRank", info.rank);

        if(m_Profile == null)
        {
            m_Profile = NGUITools.AddChild(ProfileIndicator, PrefabProfile).GetComponent<PlayerProfile>();
        }
        m_Profile.UpdateProfile(info.leader_creature, info.nickname, info.player_level);

        m_OnStartBattleClick = OnCallback;
    }

    public void OnClickBattle()
    {
        if (m_OnStartBattleClick != null)
            m_OnStartBattleClick(m_Info);
    }
}
