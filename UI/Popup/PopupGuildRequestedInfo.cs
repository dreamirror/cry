using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketEnums;
using PacketInfo;

public class PopupGuildRequestedInfo : PopupBase
{
    public UISprite m_SpriteLeader;
    public UILabel m_LabelNickname;
    public UILabel m_LabelLevel;
    public UILabel m_LabelLoginAt;

    public PrefabManager HeroPrefabManager;

    public UIGrid m_GridBtns;
    public GameObject m_BtnChangeState;
    public GameObject m_BtnLeave;
    pd_GuildRequestedInfo m_Info;
    pd_GuildRequestedInfoDetail m_Detail;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_Info = parms[0] as pd_GuildRequestedInfo;
        m_Detail = parms[1] as pd_GuildRequestedInfoDetail;

        m_SpriteLeader.spriteName = m_Info.leader_creature.GetProfileName();

        m_LabelLevel.text = m_Info.player_level.ToString();
        m_LabelNickname.text = m_Info.nickname;

        if (m_Info.is_connected)
            m_LabelLoginAt.text = Localization.Get("UserConnected");
        else
            m_LabelLoginAt.text = Network.GetConnectedTimeString(m_Info.last_login_at);

        foreach (var creature in m_Detail.creatures)
        {
            var item = HeroPrefabManager.GetNewObject<EnchantHero>(HeroPrefabManager.transform, Vector3.zero);
            item.Init(new Creature(creature));
            item.m_label_in_team.text = "";
        }
        HeroPrefabManager.GetComponent<UIGrid>().Reposition();
    }
    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        HeroPrefabManager.GetComponent<UIGrid>().Reposition();
    }

    public override void OnFinishedHide()
    {
        HeroPrefabManager.Clear();
        base.OnFinishedHide();
    }
    public void OnClickConfirm()
    {
        base.OnClose();
    }
}
