using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;

public class GuildRequestList : GuildContentsBase
{
    public PrefabManager GuildInfoPrefabManager;

    public UILabel m_LabelRequestCount;
    public PrefabManager GuildInfoItemPrefabManager;
    public UIScrollView m_ScrollGuild;
    public UIGrid m_GridGuild;

    GuildInfoDetail m_GuildInfoDetail = null;
    GuildInfoItem m_SelectedGuild = null;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        m_SelectedGuild = null;
        GuildInfoPrefabManager.Clear();
        m_GuildInfoDetail = GuildInfoPrefabManager.GetNewObject<GuildInfoDetail>(GuildInfoPrefabManager.transform, Vector3.zero);

        C2G.GuildListForRequest packet = new C2G.GuildListForRequest();
        Network.GameServer.JsonAsync<C2G.GuildListForRequest, C2G.GuildListForJoinAck>(packet, OnGuildListForRequest);
    }
    override public void Uninit()
    {
        m_SelectedGuild = null;
        GuildInfoItemPrefabManager.Clear();
        GuildInfoPrefabManager.Clear();
        base.Uninit();
    }

    public void OnClickJoinCancel()
    {
        C2G.GuildJoin packet = new C2G.GuildJoin();
        packet.guild_idx = m_SelectedGuild.GuildInfo.info.guild_idx;
        packet.member_account_idx = SHSavedData.AccountIdx;
        packet.refuse = true;
        Network.GameServer.JsonAsync<C2G.GuildJoin, C2G.GuildAck>(packet, OnGuildJoinCancel);
    }

    //////////////////////////////////////////////////////////////////////////
    void OnClickGuild(GuildInfoItem info_item)
    {
        m_SelectedGuild = info_item;
        m_GuildInfoDetail.Init(info_item.GuildInfo);
    }

    //////////////////////////////////////////////////////////////////////////
    void OnGuildJoinCancel(C2G.GuildJoin packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                m_RequestGuilds.RemoveAll(e => e.guild_info.guild_idx == packet.guild_idx);
                InitGuildList();
                break;
        }
    }
    List<pd_GuildJoinInfo> m_RequestGuilds = null;
    void OnGuildListForRequest(C2G.GuildListForRequest packet, C2G.GuildListForJoinAck ack)
    {
        m_RequestGuilds = ack.guild_join_info;
        InitGuildList();
    }

    private void InitGuildList()
    {
        if (m_RequestGuilds != null && m_RequestGuilds.Count > 0)
        {
            GuildInfoItemPrefabManager.Clear();
            GuildInfoItem first = null;
            foreach (var guild_info in m_RequestGuilds)
            {
                var item = GuildInfoItemPrefabManager.GetNewObject<GuildInfoItem>(m_GridGuild.transform, Vector3.zero);
                item.Init(new pd_GuildInfoDetail(guild_info.guild_info), OnClickGuild);
                if (first == null)
                    first = item;
            }
            m_GridGuild.Reposition();
            m_ScrollGuild.ResetPosition();
            first.OnClickGuild();
            m_LabelRequestCount.text = Localization.Format("GuildMemberCountFormat", m_RequestGuilds.Count, GuildInfoManager.Config.RequestCount);
        }
        else
        {
            Tooltip.Instance.ShowMessageKey("NotExistRequestGuild");
            var guild_menu = GameMain.Instance.GetCurrentMenu().GetComponent<Guild>();
            if (guild_menu != null)
                guild_menu.SetTab(eGuildTabMode.GuildJoin);
        }
    }
}
