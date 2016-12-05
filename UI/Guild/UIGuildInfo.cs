using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using LinqTools;

public class UIGuildInfo : GuildContentsBase
{
    public PrefabManager GuildInfoPrefabManager;


    public UILabel m_LabelMemberCount;
    public PrefabManager GuildMemberItemPrefabManager;
    public UIScrollView m_ScrollGuild;
    public UIGrid m_GridGuild;

    public GameObject m_GuildSettingDisable;

    GuildInfoDetail m_GuildInfoDetail = null;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        GuildInfoPrefabManager.Clear();
        m_GuildInfoDetail = GuildInfoPrefabManager.GetNewObject<GuildInfoDetail>(GuildInfoPrefabManager.transform, Vector3.zero);

        if (GuildManager.Instance.GuildMembers == null)
        {
            C2G.GuildMemberGet packet = new C2G.GuildMemberGet();
            packet.guild_idx = GuildManager.Instance.GuildIdx;
            Network.GameServer.JsonAsync<C2G.GuildMemberGet, C2G.GuildAck>(packet, OnGuildMemberGet);
        }
        else
        {
            UpdateGuildInfo(GuildManager.Instance.GuildInfo);
            InitGuildMembers();
        }
    }
    override public void Uninit()
    {
        GuildMemberItemPrefabManager.Clear();
        GuildInfoPrefabManager.Clear();
        base.Uninit();
    }
    public override void UpdateInfo()
    {
       UpdateGuildInfo(GuildManager.Instance.GuildInfo);
       InitGuildMembers();
    }
    static public void GuildAttend()
    {
        if(GuildManager.Instance.IsAttendance == true)
        {
            Tooltip.Instance.ShowMessageKey("AlreadyGuildAttend");
            return;
        }
        C2G.GuildAttend packet = new C2G.GuildAttend();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        Network.GameServer.JsonAsync<C2G.GuildAttend, C2G.GuildAck>(packet, OnGuildAttend);
    }
    public void OnClickAttend()
    {
        GuildAttend();
    }
    public void OnClickGiveGold()
    {
        Popup.Instance.Show(ePopupMode.GuildGoldGive);
    }
    public void OnClickSetting()
    {
        if(GuildManager.Instance.AvailableGuildManagement == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableGuildManagement");
            return;
        }
        Popup.Instance.Show(ePopupMode.GuildSetting);
    }


    //////////////////////////////////////////////////////////////////////////
    void UpdateGuildInfo(pd_GuildInfoDetail info)
    {
        if (info == null) return;
        m_GuildInfoDetail.Init(info, false);

        m_LabelMemberCount.text = Localization.Format("GuildMemberCountFormat", GuildManager.Instance.GuildMembers.Count, GuildInfoManager.Config.GetLimitMemberCount(info.info.guild_level));
        m_GuildSettingDisable.SetActive(GuildManager.Instance.AvailableGuildManagement == false);
    }

    //////////////////////////////////////////////////////////////////////////
    void OnGuildMemberGet(C2G.GuildMemberGet packet, C2G.GuildAck ack)
    {
        GuildManager.Instance.SetGuildInfo(ack.guild_info);
        GuildManager.Instance.SetGuildMembers(ack.guild_members.Select(e=>new pd_GuildMemberInfoDetail(e)).ToList());
        InitGuildMembers();

        UpdateGuildInfo(GuildManager.Instance.GuildInfo);

        parent.AddManagementTab();
    }

    private void InitGuildMembers()
    {
        GuildMemberItemPrefabManager.Clear();
        foreach (var member in GuildManager.Instance.GuildMembers)
        {
            var item = GuildMemberItemPrefabManager.GetNewObject<GuildMemberItem>(m_GridGuild.transform, Vector3.zero);
            item.Init(member);
        }
        m_GridGuild.Reposition();
        m_ScrollGuild.ResetPosition();
    }

    static void OnGuildAttend(C2G.GuildAttend packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                GuildManager.Instance.Attend();
                GuildManager.Instance.SetGuildInfo(ack.guild_info);
                GameMain.Instance.UpdateMenu();
                Tooltip.Instance.ShowMessageKey("GuildAttendComplete");
                break;
        }
    }
}
