using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;

public class GuildInfoDetail : MonoBehaviour
{
    public UISprite m_SpriteEmblem;
    public UILabel m_LabelName;
    public UILabel m_LabelMaster;
    public UILabel m_LabelRank;
    public UILabel m_LabelLevel;
    public UILabel m_LabelExp;
    public UILabel m_LabelExpValue;
    public UILabel m_LabelIntro;
    public UILabel m_LabelIntroValue;
    public UILabel m_LabelBuff1;
    public UILabel m_LabelBuff2;


    public void Init(pd_GuildInfoDetail info, bool bIntro = true)
    {
        SetGuildInfo(info,bIntro);
    }
    
    //////////////////////////////////////////////////////////////////////////
    void SetGuildInfo(pd_GuildInfoDetail info, bool bIntro)
    {
        m_SpriteEmblem.spriteName = info.info.guild_emblem;
        m_LabelName.text = info.info.guild_name;
        m_LabelLevel.text = info.info.guild_level.ToString();

        m_LabelMaster.text = info.guild_master;
        m_LabelRank.text = info.info.rank.ToString();

        m_LabelBuff1.text = GuildInfoManager.Config.GuildBuffString(1, info.info.guild_level);
        m_LabelBuff2.text = GuildInfoManager.Config.GuildBuffString(2, info.info.guild_level);

        if (bIntro == false)//(Network.GuildInfo != null && Network.GuildInfo.guild_idx == info.guild_idx)
        {            
            m_LabelIntro.text = Localization.Get("GuildNotification");
            m_LabelIntroValue.text = info.info.guild_notify;
            m_LabelExp.text = Localization.Get("GuildExp");
            m_LabelExpValue.text = Localization.Format("GuildExpFormat", info.info.guild_exp, GuildInfoManager.Config.GetExpPercent(info.info.guild_level, info.info.guild_exp));
        }
        else
        {
            m_LabelIntro.text = Localization.Get("GuildIntro");
            m_LabelIntroValue.text = info.info.guild_intro;
            m_LabelExp.text = Localization.Get("GuildMember");
            m_LabelExpValue.text = Localization.Format("GuildMemberCountFormat", info.info.member_count, GuildInfoManager.Config.GetLimitMemberCount(info.info.guild_level));
        }
    }    
}
