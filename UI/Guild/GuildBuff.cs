using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;

public class GuildBuff : GuildContentsBase
{
    public PrefabManager GuildInfoPrefabManager;

    public PrefabManager GuildBuffItemPrefabManager;
    public UIScrollView m_ScrollGuild;
    public UIGrid m_GridGuild;
    public GameObject m_GuildSettingDisable;

    GuildInfoDetail m_GuildInfoDetail = null;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);
        GuildInfoPrefabManager.Clear();
        m_GuildInfoDetail = GuildInfoPrefabManager.GetNewObject<GuildInfoDetail>(GuildInfoPrefabManager.transform, Vector3.zero);

        UpdateInfo();
        InitGuildBuff();
    }
    override public void Uninit()
    {
        GuildBuffItemPrefabManager.Clear();
        GuildInfoPrefabManager.Clear();
        base.Uninit();
    }
    public override void UpdateInfo()
    {
        m_GuildInfoDetail.Init(GuildManager.Instance.GuildInfo, false);
        m_GuildSettingDisable.SetActive(GuildManager.Instance.AvailableGuildManagement == false);
    }
    public void OnClickAttend()
    {
        UIGuildInfo.GuildAttend();
    }
    public void OnClickGiveGold()
    {
        Popup.Instance.Show(ePopupMode.GuildGoldGive);
    }
    public void OnClickSetting()
    {
        if (GuildManager.Instance.AvailableGuildManagement == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableGuildManagement");
            return;
        }
        Popup.Instance.Show(ePopupMode.GuildSetting);
    }



    //////////////////////////////////////////////////////////////////////////
    private void InitGuildBuff()
    {
        for(int i=GuildManager.Instance.GuildInfo.info.guild_level; i<=GuildInfoManager.Config.GuildLevelMax; ++i)
        {
            var item = GuildBuffItemPrefabManager.GetNewObject<GuildBuffItem>(m_GridGuild.transform, Vector3.zero);
            item.Init((short)i, GuildManager.Instance.GuildInfo.info.guild_level == i);
        }
        m_GridGuild.Reposition();
        m_ScrollGuild.ResetPosition();
    }
    //////////////////////////////////////////////////////////////////////////
}
