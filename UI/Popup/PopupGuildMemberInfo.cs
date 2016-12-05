using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketEnums;
using PacketInfo;

public class PopupGuildMemberInfo : PopupBase
{
    public UISprite m_SpriteLeader;
    public UILabel m_LabelNickname;
    public UILabel m_LabelLevel;
    public UILabel m_LabelJoinAt;
    public UILabel m_LabelLoginAt;
    public UILabel m_LabelState;
    public UILabel m_LabelGivePoint;

    public PrefabManager HeroPrefabManager;

    public UIGrid m_GridBtns;
    public GameObject m_BtnChangeState;
    public GameObject m_BtnLeave;
    public GameObject m_BtnExpulsion;
    pd_GuildMemberInfoDetail m_Info;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_Info = parms[0] as pd_GuildMemberInfoDetail;
        Init();
    }
    public override void OnClose()
    {
        HeroPrefabManager.Clear();
        base.OnClose();
    }
    public override void OnFinishedHide()
    {
        HeroPrefabManager.Clear();
        base.OnFinishedHide();
    }
    private void Init()
    {
        m_SpriteLeader.spriteName = m_Info.leader_creature.GetProfileName();

        m_LabelLevel.text = m_Info.player_level.ToString();
        m_LabelNickname.text = m_Info.nickname;

        m_LabelJoinAt.text = Localization.Format("GuildJoinTime", (Network.Instance.ServerTime - m_Info.created_at).TotalDays);
        if (m_Info.is_connected)
            m_LabelLoginAt.text = Localization.Get("UserConnected");
        else
            m_LabelLoginAt.text = Network.GetConnectedTimeString(m_Info.last_login_at);

        m_LabelState.text = Localization.Get(string.Format("GuildMemberState{0}", m_Info.member_state));
        m_LabelGivePoint.text = Localization.Format("GoodsFormat", m_Info.give);

        HeroPrefabManager.Clear();
        foreach (var creature in m_Info.creatures)
        {
            var item = HeroPrefabManager.GetNewObject<EnchantHero>(HeroPrefabManager.transform, Vector3.zero);
            item.Init(new Creature(creature));
            item.m_label_in_team.text = "";
        }
        HeroPrefabManager.GetComponent<UIGrid>().Reposition();

        m_BtnChangeState.SetActive(GuildManager.Instance.AvailableGuildManagement == true && GuildManager.Instance.GuildInfo.info.member_count > 1
            && m_Info.member_state != 0);
        m_BtnLeave.SetActive(SHSavedData.AccountIdx == m_Info.account_idx
            && (m_Info.member_state == 0 ? GuildManager.Instance.GuildInfo.info.member_count == 1 : true));
        m_BtnExpulsion.SetActive(SHSavedData.AccountIdx != m_Info.account_idx
            && (m_Info.member_state == 0 ? (Network.Instance.ServerTime - m_Info.last_login_at).TotalSeconds >= 60 * 60 * 24 * 7 : GuildManager.Instance.AvailableGuildManagement == true));
        m_GridBtns.Reposition();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        m_GridBtns.Reposition();
        HeroPrefabManager.GetComponent<UIGrid>().Reposition();
    }

    public void OnClickChangeState()
    {
        Popup.Instance.Show(ePopupMode.GuildMemberState, m_Info.member_state, new Action<short>(OnChangeState));
    }
    void OnChangeState(short state)
    {
        if(m_Info.member_state != state)
        {
            if(state == 3)
            {
                C2G.GuildLeave packet = new C2G.GuildLeave();
                packet.guild_idx = GuildManager.Instance.GuildIdx;
                packet.member_account_idx = m_Info.account_idx;
                Network.GameServer.JsonAsync<C2G.GuildLeave, C2G.GuildAck>(packet, OnGuildLeave);
            }
            else
            {
                C2G.GuildStateChange packet = new C2G.GuildStateChange();
                packet.guild_idx = GuildManager.Instance.GuildIdx;
                packet.member_account_idx = m_Info.account_idx;
                packet.member_state = state;
                Network.GameServer.JsonAsync<C2G.GuildStateChange, C2G.GuildAck>(packet, OnGuildStateChange);
            }
        }
    }
    public void OnClickLeaveGuild()
    {
        Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnLeaveGuild), "GuildLeaveConfirm");
    }
    public void OnClickExpulsion()
    {
        if (m_Info.member_state == 0)
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnGuildMasterExpulstion), "GuildMasterExpulsionConfirm");
        else
            OnLeaveGuild(true);
    }
    void OnGuildMasterExpulstion(bool is_confirm)
    {
        if(is_confirm)
        {
            OnChangeState(3);
        }
    }
    public void OnClickConfirm()
    {
        OnClose();
    }

    void OnLeaveGuild(bool is_confirm)
    {
        if(is_confirm)
        {
            C2G.GuildLeave packet = new C2G.GuildLeave();
            packet.guild_idx = GuildManager.Instance.GuildIdx;
            packet.member_account_idx = m_Info.account_idx;
            Network.GameServer.JsonAsync<C2G.GuildLeave, C2G.GuildAck>(packet, OnGuildLeave);
        }
    }

    void OnGuildLeave(C2G.GuildLeave packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                HeroPrefabManager.Clear();
                parent.Close(true, true);

                if (packet.member_account_idx == SHSavedData.AccountIdx)
                {
                    GuildManager.Instance.LeaveGuild();
                    GameMain.Instance.BackMenu();
                }
                else
                {
                    if(m_Info.member_state == 0)
                    {
                        GuildManager.Instance.State.member_state = 0;
                    }
                    GuildManager.Instance.RemoveMember(packet.member_account_idx);
                    //GuildManager.Instance.SetGuildInfo(ack.guild_info);
                    //GuildManager.Instance.SetGuildMembers(ack.guild_members);
                    GameMain.Instance.UpdateMenu();
                }
                break;
        }
    }

    void OnGuildStateChange(C2G.GuildStateChange packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                m_Info.member_state = packet.member_state;
                if(m_Info.member_state == 0)
                {
                    GuildManager.Instance.SetGuildMaster(m_Info.nickname);
                    if (GuildManager.Instance.State.member_state == 0)
                        GuildManager.Instance.State.member_state = 1;
                }
                Init();
                GameMain.Instance.UpdateMenu();
                break;
        }
    }
}
