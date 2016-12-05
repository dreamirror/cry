using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketEnums;

public class PopupGuildSetting : PopupBase
{
    public UILabel m_LabelLimitLevel;
    public GameObject m_LevelLimitPanel;
    public UILabel[] m_LabelFilters;

    public UIToggle m_ToggleAuto;

    public UIInput m_InputGuildIntro;
    public UIInput m_InputGuildNotification;

    short m_JoinLevelLimit = 10;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        SetLimitLevel(GuildManager.Instance.GuildInfo.info.guild_limit_level);

        int limit_level = 10;
        foreach (var label in m_LabelFilters)
        {
            label.text = Localization.Format("GuildJoinLimitLevelFormat", limit_level);
            limit_level += 10;
        }

        m_ToggleAuto.value = GuildManager.Instance.GuildInfo.info.is_auto;

        m_InputGuildIntro.label.text = GuildManager.Instance.GuildInfo.info.guild_intro;
        m_InputGuildNotification.label.text = GuildManager.Instance.GuildInfo.info.guild_notify;
    }
    void SetLimitLevel(short level)
    {
        m_JoinLevelLimit = level;
        m_LabelLimitLevel.text = Localization.Format("GuildJoinLimitLevelFormat", level);
        m_LevelLimitPanel.SetActive(false);
    }
    public void OnClickLevel10() { SetLimitLevel(10); }
    public void OnClickLevel20() { SetLimitLevel(20); }
    public void OnClickLevel30() { SetLimitLevel(30); }
    public void OnClickLevel40() { SetLimitLevel(40); }
    public void OnClickLevel50() { SetLimitLevel(50); }
    public void OnClickLevel60() { SetLimitLevel(60); }
    public void OnClickLevel70() { SetLimitLevel(70); }
    public void OnClickLevel80() { SetLimitLevel(80); }

    public void OnClickAuto()
    {
        m_ToggleAuto.value = true;
    }
    public void OnClickNoAuto()
    {
        m_ToggleAuto.value = false;
    }
    public void OnClickIntro()
    {
        m_InputGuildIntro.isSelected = true;
    }
    public void OnClickNotification()
    {
        m_InputGuildNotification.isSelected = true;
    }
    public void OnClickLevelLimit()
    {
        m_LevelLimitPanel.SetActive(true);
    }
    bool CheckModify()
    {
        return m_ToggleAuto.value != GuildManager.Instance.GuildInfo.info.is_auto
            || m_JoinLevelLimit != GuildManager.Instance.GuildInfo.info.guild_limit_level
            || GuildManager.Instance.GuildInfo.info.guild_notify != m_InputGuildNotification.label.text
            || GuildManager.Instance.GuildInfo.info.guild_intro != m_InputGuildIntro.label.text
            ;
    }
    public void OnClickModify()
    {
        if(CheckModify() == false)
        {
            base.OnClose();
            return;
        }
        C2G.GuildSetting packet = new C2G.GuildSetting();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        packet.guild_intro = m_InputGuildIntro.label.text;
        packet.guild_notify= m_InputGuildNotification.label.text;
        packet.guild_limit_level = m_JoinLevelLimit;
        packet.is_auto = m_ToggleAuto.value;
        Network.GameServer.JsonAsync<C2G.GuildSetting, C2G.GuildAck>(packet, OnGuildSetting);
    }

    void OnGuildSetting(C2G.GuildSetting packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                GuildManager.Instance.SetGuildInfo(ack.guild_info);
                GameMain.Instance.UpdateMenu();
                base.OnClose();
                break;
        }
    }
}
