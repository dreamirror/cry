using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using System;

public class GuildManagement : GuildContentsBase
{
    public PrefabManager GuildMemberPrefabManager;
    public UILabel m_LabelMemberCount;
    public PrefabManager GuildRequestPrefabManager;
    public UILabel m_LabelRequestCount;

    List<pd_GuildRequestedInfo> Requested;
    List<pd_GuildRequestedInfo> Selected = new List<pd_GuildRequestedInfo>();

    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        InitGuildMember();
        C2G.GuildRequestGet packet = new C2G.GuildRequestGet();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        Network.GameServer.JsonAsync<C2G.GuildRequestGet, C2G.GuildRequestGetAck>(packet, OnGuildRequestGet);
    }
    override public void Uninit()
    {
        GuildRequestPrefabManager.Clear();
        GuildMemberPrefabManager.Clear();
        base.Uninit();
    }

    public override void UpdateInfo()
    {
        base.UpdateInfo();
        InitGuildMember();
    }

    public void OnClickAllRefuse()
    {
        if (Requested == null || Requested.Count == 0) return;
        C2G.GuildRefuseAll packet = new C2G.GuildRefuseAll();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        Network.GameServer.JsonAsync<C2G.GuildRefuseAll, C2G.GuildAck>(packet, OnGuildRefuseAll);
    }
    public void OnClickRefuse()
    {
        if(Selected.Count == 0)
        {
            Tooltip.Instance.ShowMessageKey("GuildRequestRefuseSelect");
            return;
        }
        C2G.GuildJoin packet = new C2G.GuildJoin();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        packet.member_account_idx = Selected[0].account_idx;
        packet.refuse = true;
        Network.GameServer.JsonAsync<C2G.GuildJoin, C2G.GuildAck>(packet, OnGuildRefuse);
    }
    public void OnClickAllow()
    {
        if (GuildManager.Instance.GuildMemberCount == GuildManager.Instance.GuildMemberMax)
        {
            Tooltip.Instance.ShowMessageKey("GuildErrorMemberLimit");
            return;
        }
        if (GuildManager.Instance.GuildMemberCount + Selected.Count > GuildManager.Instance.GuildMemberMax)
        {
            Tooltip.Instance.ShowMessageKey("GuildErrorMemberLimitExceed");
            return;
        }
        if (Selected.Count == 0)
        {
            Tooltip.Instance.ShowMessageKey("GuildRequestAllowSelect");
            return;
        }
        C2G.GuildJoin packet = new C2G.GuildJoin();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        packet.member_account_idx = Selected[0].account_idx;
        packet.refuse = false;
        Network.GameServer.JsonAsync<C2G.GuildJoin, C2G.GuildAck>(packet, OnGuildAllow);
    }
    //////////////////////////////////////////////////////////////////////////
    private void InitGuildMember()
    {
        GuildMemberPrefabManager.Clear();
        foreach (var member in GuildManager.Instance.GuildMembers)
        {
            var item = GuildMemberPrefabManager.GetNewObject<GuildMemberItem>(GuildMemberPrefabManager.transform, Vector3.zero);
            item.Init(member);
        }
        GuildMemberPrefabManager.GetComponent<UIGrid>().Reposition();
        GuildMemberPrefabManager.GetComponentInParent<UIScrollView>().ResetPosition();
        m_LabelMemberCount.text = Localization.Format("GuildMemberFormat", GuildManager.Instance.GuildMembers.Count, GuildManager.Instance.GuildMemberMax);
    }
    private void InitGuildRequest()
    {
        GuildRequestPrefabManager.Clear();
        foreach (var request in Requested)
        {
            var item = GuildRequestPrefabManager.GetNewObject<GuildRequestItem>(GuildRequestPrefabManager.transform, Vector3.zero);
            item.Init(request, OnSelect);
        }
        GuildRequestPrefabManager.GetComponent<UIGrid>().Reposition();
        GuildRequestPrefabManager.GetComponentInParent<UIScrollView>().ResetPosition();

        m_LabelRequestCount.text = Localization.Format("GuildMemberFormat", Requested.Count, GuildInfoManager.Config.GuildRequestCount);
    }
    
    void OnSelect(pd_GuildRequestedInfo request_info, bool is_selected)
    {
        if (is_selected)
            Selected.Add(request_info);
        else
            Selected.Remove(request_info);
    }
    //////////////////////////////////////////////////////////////////////////
    void OnGuildRequestGet(C2G.GuildRequestGet packet, C2G.GuildRequestGetAck ack)
    {
        Requested = ack.requested_infos;
        InitGuildRequest();
    }

    void OnGuildRefuseAll(C2G.GuildRefuseAll packet, C2G.GuildAck ack)
    {
        Selected.Clear();
        Requested.Clear();
        InitGuildRequest();
    }

    void OnGuildRefuse(C2G.GuildJoin packet, C2G.GuildAck ack)
    {
        Selected.RemoveAll(e => e.account_idx == packet.member_account_idx);
        Requested.RemoveAll(e => e.account_idx == packet.member_account_idx);
        if (Selected.Count > 0)
            OnClickRefuse();
        else
            InitGuildRequest();
    }
    void OnGuildAllow(C2G.GuildJoin packet, C2G.GuildAck ack)
    {
        var requested_info = Selected.Find(e => e.account_idx == packet.member_account_idx);
        Selected.RemoveAll(e => e.account_idx == packet.member_account_idx);
        Requested.RemoveAll(e => e.account_idx == packet.member_account_idx);
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                {
                    pd_GuildMemberInfoDetail member = new pd_GuildMemberInfoDetail();
                    member.account_idx = requested_info.account_idx;
                    member.last_login_at = requested_info.last_login_at;
                    member.leader_creature = requested_info.leader_creature;
                    member.nickname = requested_info.nickname;
                    member.player_level = requested_info.player_level;
                    member.member_state = 2;
                    member.give = 0;
                    member.attend_daily_index = 0;
                    member.created_at = Network.Instance.ServerTime;
                    member.updated_at = Network.Instance.ServerTime;
                    
                    GuildManager.Instance.AddGuildMember(member);
                }
                break;
            case pe_GuildResult.JoinAnotherGuild:
                Tooltip.Instance.ShowMessageKey("ERRORJoinAnotherGuild");
                break;
            case pe_GuildResult.GuildMemberFull:
                Tooltip.Instance.ShowMessageKey("GuildErrorMemberLimit");
                break;
            case pe_GuildResult.GuildJoinTimeDelay:
                Tooltip.Instance.ShowMessageKey("ERRORJoinAnotherGuild");
                break;
        }
        if (Selected.Count > 0)
            OnClickAllow();
        else
        {
            InitGuildRequest();
            InitGuildMember();
        }
    }
}
