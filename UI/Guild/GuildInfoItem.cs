using UnityEngine;
using System.Collections;
using PacketInfo;

public class GuildInfoItem : MonoBehaviour
{
    public GameObject m_Request;
    public UIToggle m_ToggleSelected;
    public UISprite m_SpriteEmblem;
    public UILabel m_LabelName;
    public UILabel m_LabelLevel;
    public UILabel m_LabelLimitLevel;
    public UILabel m_LabelMember;
    public UILabel m_LabelState;

    public pd_GuildInfoDetail GuildInfo;
    System.Action<GuildInfoItem> OnClickDelegate = null;
    public void Init(pd_GuildInfoDetail guild_info, System.Action<GuildInfoItem> _del)
    {
        GuildInfo = guild_info;
        OnClickDelegate = _del;

        m_ToggleSelected.value = false;


        //Network.PlayerInfo
        m_Request.SetActive(false);

        m_SpriteEmblem.spriteName = guild_info.info.guild_emblem;
        m_LabelName.text = guild_info.info.guild_name;
        m_LabelLevel.text = guild_info.info.guild_level.ToString();
        m_LabelLimitLevel.text = guild_info.info.guild_limit_level.ToString();
        m_LabelMember.text = Localization.Format("GuildMemberFormat", guild_info.info.member_count, guild_info.info.guild_limit_member);
        m_LabelState.text = Localization.Get(guild_info.info.is_auto ? "GuildStateAuto" : "GuildStateNotAuto");
    }

    public void SetRequeted(bool bReqested)
    {
        m_Request.SetActive(bReqested);
    }
    public void OnClickGuild()
    {
        m_ToggleSelected.value = true;
        C2G.GuildInfoMaster packet = new C2G.GuildInfoMaster();
        packet.guild_idx = GuildInfo.info.guild_idx;
        Network.GameServer.JsonAsync<C2G.GuildInfoMaster, C2G.GuildInfoMasterAck>(packet, OnGuildInfoDetail);

    }

    void OnGuildInfoDetail(C2G.GuildInfoMaster packet, C2G.GuildInfoMasterAck ack)
    {
        GuildInfo.guild_master = ack.guild_master;
        if (OnClickDelegate != null)
            OnClickDelegate(this);
    }
}
