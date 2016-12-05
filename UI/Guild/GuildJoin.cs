using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using System;

public class GuildJoin : GuildContentsBase
{
    public PrefabManager GuildInfoPrefabManager;

    public GameObject m_BtnJoin;
    public GameObject m_BtnCancel;


    public UILabel m_LabelRequestCount;
    public PrefabManager GuildInfoItemPrefabManager;
    public UIScrollView m_ScrollGuild;
    public UIGrid m_GridGuild;

    public GameObject m_PageIndicator;
    public UILabel m_LabelPage;
    public GameObject m_PanelFilter;
    public UILabel m_LabelFilter;

    int m_Page = 1;
    int m_Total = 0;
    GuildInfoItem m_SelectedGuild = null;
    GuildInfoDetail m_GuildInfoDetail = null;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        GuildInfoPrefabManager.Clear();
        m_GuildInfoDetail = GuildInfoPrefabManager.GetNewObject<GuildInfoDetail>(GuildInfoPrefabManager.transform, Vector3.zero);

        m_SelectedGuild = null;
        OnClickFilterJoinable();
    }
    override public void Uninit()
    {
        m_SelectedGuild = null;
        GuildInfoItemPrefabManager.Clear();
        GuildInfoPrefabManager.Clear();
        base.Uninit();
    }
    public void OnClickJoin()
    {
        if (Network.PlayerInfo.player_level < GuildInfoManager.Config.AtLeastPlayerLevel)
        {
            Tooltip.Instance.ShowMessageKeyFormat("GuildCreateAtLeast", GuildInfoManager.Config.AtLeastPlayerLevel);
            return;
        }

        if (RequestGuilds.Count >= GuildInfoManager.Config.RequestCount)
        {
            Tooltip.Instance.ShowMessageKey("GuildRequestErrorRequestCount");
            return;
        }
        if (Network.PlayerInfo.player_level < m_SelectedGuild.GuildInfo.info.guild_limit_level)
        {
            Tooltip.Instance.ShowMessageKey("GuildRequestErrorLevelLimit");
            return;
        }
        if (m_SelectedGuild.GuildInfo.info.member_count >= m_SelectedGuild.GuildInfo.info.guild_limit_member)
        {
            Tooltip.Instance.ShowMessageKey("GuildRequestErrorMemberLimit");
            return;
        }
        if (m_SelectedGuild.GuildInfo.info.is_auto)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(SendRequest), "GuildJoinInstantConfirm");
            return;
        }
        SendRequest();
    }

    private void SendRequest(bool confirm = true)
    {
        if (confirm == false) return;
        C2G.GuildRequest packet = new C2G.GuildRequest();
        packet.guild_idx = m_SelectedGuild.GuildInfo.info.guild_idx;
        Network.GameServer.JsonAsync<C2G.GuildRequest, C2G.GuildAck>(packet, OnGuildRequest);
    }

    public void OnClickJoinCancel()
    {
        C2G.GuildJoin packet = new C2G.GuildJoin();
        packet.guild_idx = m_SelectedGuild.GuildInfo.info.guild_idx;
        packet.member_account_idx = SHSavedData.AccountIdx;
        packet.refuse = true;
        Network.GameServer.JsonAsync<C2G.GuildJoin, C2G.GuildAck>(packet, OnGuildJoinCancel);
    }
    public void OnClickFilter()
    {
        m_PanelFilter.SetActive(true);
    }


    public void OnClickFilterAll()
    {
        m_PageIndicator.SetActive(true);

        m_PanelFilter.SetActive(false);
        m_LabelFilter.text = Localization.Get("GuildAll");

        GetGuildListForRank();
    }

    private void GetGuildListForRank()
    {
        C2G.GuildListRank packet = new C2G.GuildListRank();
        packet.page = m_Page;
        Network.GameServer.JsonAsync<C2G.GuildListRank, C2G.GuildListRankAck>(packet, OnGuildListRank);
    }

    public void OnClickFilterJoinable()
    {
        m_PageIndicator.SetActive(false);
        m_PanelFilter.SetActive(false);
        m_LabelFilter.text = Localization.Get("GuildJoinable");
        
        C2G.GuildListForJoin packet = new C2G.GuildListForJoin();
        packet.player_level = Network.PlayerInfo.player_level;
        Network.GameServer.JsonAsync<C2G.GuildListForJoin, C2G.GuildListForJoinAck>(packet, OnGuildListForJoin);
    }
    public void OnClickSearch()
    {
        Popup.Instance.Show(ePopupMode.GuildSearch, new Action<pd_GuildInfo>(OnSearchGuild));
    }
    public void OnClickLeft()
    {
        if (m_Page == 1) return;
        m_Page--;
        GetGuildListForRank();
    }
    public void OnClickRight()
    {
        if (m_Page >= (m_Total - 1) / GuildInfoManager.Config.GuildCountPerPage + 1) return;
        m_Page++;
        GetGuildListForRank();
    }

    //////////////////////////////////////////////////////////////////////////
    void SetGuildInfo(pd_GuildInfoDetail info)
    {
        m_GuildInfoDetail.Init(info);

        UpdateBtns();

        m_LabelPage.text = Localization.Format("GuildMemberCountFormat", m_Page, (m_Total - 1) / GuildInfoManager.Config.GuildCountPerPage + 1);
    }
    //////////////////////////////////////////////////////////////////////////
    void OnSelectedGuild(GuildInfoItem info_item)
    {
        m_SelectedGuild = info_item;
        SetGuildInfo(info_item.GuildInfo);
    }
    void AddRequest(pd_GuildInfo guild_info)
    {
        if (RequestGuilds == null)
            RequestGuilds = new List<pd_GuildRequestInfo>();

        var request_info = new pd_GuildRequestInfo();
        request_info.guild_idx = guild_info.guild_idx;
        request_info.account_idx = SHSavedData.AccountIdx;
        RequestGuilds.Add(request_info);
        UpdateRequestCount();
    }

    private void UpdateRequestCount()
    {
        m_LabelRequestCount.text = Localization.Format("GuildMemberCountFormat", RequestGuilds == null ? 0 : RequestGuilds.Count, GuildInfoManager.Config.RequestCount);
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
    //////////////////////////////////////////////////////////////////////////
    List<pd_GuildRequestInfo> RequestGuilds = null;
    void OnGuildListForJoin(C2G.GuildListForJoin packet, C2G.GuildListForJoinAck ack)
    {
        if(ack.guild_join_info != null && ack.guild_join_info.Count > 0)
        {
            GuildInfoItemPrefabManager.Clear();
            GuildInfoItem first = null;
            foreach (var guild_info in ack.guild_join_info)
            {
                var item = GuildInfoItemPrefabManager.GetNewObject<GuildInfoItem>(m_GridGuild.transform, Vector3.zero);
                item.Init(new pd_GuildInfoDetail(guild_info.guild_info), OnSelectedGuild);
                if (first == null)
                    first = item;
            }
            m_GridGuild.Reposition();
            m_ScrollGuild.ResetPosition();
            first.OnClickGuild();
        }
        else
        {
            Tooltip.Instance.ShowMessageKey("NotExistJoinableGuild");
            OnClickFilterAll();
        }
        RequestGuilds = ack.request_guilds;
        UpdateRequestCount();
    }

    void OnGuildListRank(C2G.GuildListRank packet, C2G.GuildListRankAck ack)
    {
        m_Total = ack.total;
        if (ack.guild_infos != null && ack.guild_infos.Count > 0)
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
        UpdateRequestCount();
    }

    void OnGuildRequest(C2G.GuildRequest packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                if (ack.guild_info != null)
                {
                    GuildManager.Instance.SetGuildInfo(ack.guild_info);
                    Network.ChatServer.JoinGuildChannel();
                    GameMain.Instance.ChangeMenu(GameMenu.Guild);
                    return;
                }
                m_SelectedGuild.SetRequeted(true);
                AddRequest(m_SelectedGuild.GuildInfo.info);
                UpdateBtns();
                break;
            case pe_GuildResult.GuildRequestCountMax:
                Tooltip.Instance.ShowMessageKey("GuildRequestErrorGuildRequestCount");
                break;
            case pe_GuildResult.RequestCountMax:
                Tooltip.Instance.ShowMessageKey("GuildRequestErrorRequestCount");
                break;
            case pe_GuildResult.GuildJoinTimeDelay:
                Tooltip.Instance.ShowMessageKey("GuildJoinTimeDelay");
                break;
            case pe_GuildResult.LimitLevel:
                Tooltip.Instance.ShowMessageKeyFormat("GuildCreateAtLeast", GuildInfoManager.Config.AtLeastPlayerLevel);
                break;
        }
    }

    private void UpdateBtns()
    {
        m_BtnCancel.gameObject.SetActive(RequestGuilds != null && RequestGuilds.Exists(e=>e.guild_idx == m_SelectedGuild.GuildInfo.info.guild_idx));
        m_BtnJoin.gameObject.SetActive(RequestGuilds == null || RequestGuilds.Exists(e => e.guild_idx == m_SelectedGuild.GuildInfo.info.guild_idx) == false);
    }

    void OnGuildJoinCancel(C2G.GuildJoin packet, C2G.GuildAck ack)
    {
        switch (ack.result)
        {
            case pe_GuildResult.Success:
                RequestGuilds.RemoveAll(e => e.guild_idx == packet.guild_idx);
                m_SelectedGuild.SetRequeted(false);
                UpdateRequestCount();
                UpdateBtns();
                break;
        }
    }
}
