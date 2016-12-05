using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using System;

public class GuildRank : GuildContentsBase
{
    public PrefabManager GuildInfoPrefabManager;

    public PrefabManager GuildInfoItemPrefabManager;
    public UIScrollView m_ScrollGuild;
    public UIGrid m_GridGuild;
    public UILabel m_LabelPage;

    int m_Page = 1;
    GuildInfoItem m_SelectedGuild = null;
    GuildInfoDetail m_GuildInfoDetail = null;
    int m_Total = 0;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        GuildInfoPrefabManager.Clear();
        m_GuildInfoDetail = GuildInfoPrefabManager.GetNewObject<GuildInfoDetail>(GuildInfoPrefabManager.transform, Vector3.zero);

        m_SelectedGuild = null;
        GetGuildListForRank();
    }
    override public void Uninit()
    {
        m_SelectedGuild = null;
        GuildInfoItemPrefabManager.Clear();
        GuildInfoPrefabManager.Clear();
        base.Uninit();
    }
    
    void GetGuildListForRank()
    {
        C2G.GuildListRank packet = new C2G.GuildListRank();
        packet.page = m_Page;
        Network.GameServer.JsonAsync<C2G.GuildListRank, C2G.GuildListRankAck>(packet, OnGuildListRank);
    }
    public void OnClickSearch()
    {
        Popup.Instance.Show(ePopupMode.GuildSearch, new Action<pd_GuildInfo>(OnSearchGuild));
    }
    void OnSearchGuild(pd_GuildInfo info)
    {
        GuildInfoItemPrefabManager.Clear();
        var item = GuildInfoItemPrefabManager.GetNewObject<GuildInfoItem>(m_GridGuild.transform, Vector3.zero);
        item.Init(new pd_GuildInfoDetail(info), OnSelectedGuild);
        item.OnClickGuild();
        m_GridGuild.Reposition();
        m_ScrollGuild.ResetPosition();
    }
    public void OnClickLeft()
    {
        if (m_Page == 1) return;
        m_Page--;
        GetGuildListForRank();
    }
    public void OnClickRight()
    {
        if(m_Page >= (m_Total - 1) / GuildInfoManager.Config.GuildCountPerPage +1) return;
        m_Page++;
        GetGuildListForRank();
    }

    //////////////////////////////////////////////////////////////////////////
    void SetGuildInfo()
    {
        m_GuildInfoDetail.Init(m_SelectedGuild.GuildInfo);
        m_LabelPage.text = Localization.Format("GuildMemberCountFormat", m_Page, (m_Total - 1) / GuildInfoManager.Config.GuildCountPerPage + 1);
    }
    //////////////////////////////////////////////////////////////////////////
    void OnSelectedGuild(GuildInfoItem info_item)
    {
        m_SelectedGuild = info_item;
        SetGuildInfo();
    }

    //////////////////////////////////////////////////////////////////////////
    void OnGuildListRank(C2G.GuildListRank packet, C2G.GuildListRankAck ack)
    {
        m_Total = ack.total;
        if(ack.guild_infos != null && ack.guild_infos.Count > 0)
        {
            GuildInfoItemPrefabManager.Clear();
            GuildInfoItem first = null;
            foreach (var guild_info in ack.guild_infos)
            {
                var item = GuildInfoItemPrefabManager.GetNewObject<GuildInfoItem>(m_GridGuild.transform, Vector3.zero);
                item.Init(new pd_GuildInfoDetail(guild_info), OnSelectedGuild);
                if (first == null)
                    first = item;
            }
            m_GridGuild.Reposition();
            m_ScrollGuild.ResetPosition();

            first.OnClickGuild();
        }
    }
}
