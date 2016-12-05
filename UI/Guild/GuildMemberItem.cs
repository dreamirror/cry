using UnityEngine;
using System.Collections;
using PacketInfo;

public class GuildMemberItem : MonoBehaviour
{
    public UIToggle m_ToggleSelected;
    public UISprite m_SpriteProfile;
    public UILabel m_LabelName;
    public UILabel m_LabelLevel;
    public UILabel m_LabelTime;
    public UILabel m_LabelMemberState;
    public UILabel m_LabelExp;

    public pd_GuildMemberInfoDetail MemberInfo;
    public void Init(pd_GuildMemberInfoDetail memeber_info)
    {
        MemberInfo = memeber_info;

        m_SpriteProfile.spriteName = MemberInfo.leader_creature.GetProfileName();
        m_LabelLevel.text = MemberInfo.player_level.ToString();
        m_LabelName.text = MemberInfo.nickname;

        if (memeber_info.is_connected)
            m_LabelTime.text = Localization.Get("UserConnected");
        else
            m_LabelTime.text = Network.GetConnectedTimeString(MemberInfo.last_login_at);

        m_LabelMemberState.text = Localization.Get(string.Format("GuildMemberState{0}", MemberInfo.member_state));
        m_LabelExp.text = Localization.Format("GoodsFormat", MemberInfo.give);

        m_ToggleSelected.Set(MemberInfo.account_idx == SHSavedData.AccountIdx);
    }

    public void OnClickMember()
    {
        if(MemberInfo.creatures == null)
        {
            C2G.GuildMemberDetail packet = new C2G.GuildMemberDetail();
            packet.player_idx = MemberInfo.account_idx;
            Network.GameServer.JsonAsync<C2G.GuildMemberDetail, C2G.GuildMemberDetailAck>(packet, OnGuildMemberDetail);
        }
        else
            Popup.Instance.Show(ePopupMode.GuildMemberInfo, MemberInfo);
    }

    void OnGuildMemberDetail(C2G.GuildMemberDetail packet, C2G.GuildMemberDetailAck ack)
    {
        MemberInfo.creatures = ack.creatures;
        MemberInfo.team_data = ack.team_data;
        Popup.Instance.Show(ePopupMode.GuildMemberInfo, MemberInfo);
    }
}
