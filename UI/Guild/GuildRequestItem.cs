using UnityEngine;
using System.Collections;
using PacketInfo;

public class GuildRequestItem : MonoBehaviour
{
    public UIToggle m_ToggleSelected;
    public UISprite m_SpriteProfile;
    public UILabel m_LabelName;
    public UILabel m_LabelLevel;
    public UILabel m_LabelTime;

    pd_GuildRequestedInfo RequestInfo;
    System.Action<pd_GuildRequestedInfo, bool> OnSelected = null;
    pd_GuildRequestedInfoDetail Detail = null;
    public void Init(pd_GuildRequestedInfo request_info, System.Action<pd_GuildRequestedInfo, bool> OnSelectDelegate)
    {
        RequestInfo = request_info;
        OnSelected = OnSelectDelegate;

        m_SpriteProfile.spriteName = RequestInfo.leader_creature.GetProfileName();
        m_LabelLevel.text = RequestInfo.player_level.ToString();
        m_LabelName.text = RequestInfo.nickname;

        m_LabelTime.text = RequestInfo.created_at.ToString(Localization.Get("GuildRequestTimeFormat"));
        m_ToggleSelected.value = false;
    }

    public void OnClickMember()
    {
        if(Detail != null)
        {
            Popup.Instance.Show(ePopupMode.GuildRequestedInfo, RequestInfo, Detail);
            return;
        }
        C2G.GuildRequestedDetail packet = new C2G.GuildRequestedDetail();
        packet.player_idx = RequestInfo.account_idx;
        Network.GameServer.JsonAsync<C2G.GuildRequestedDetail, C2G.GuildRequestedDetailAck>(packet, OnGuildRequestedDetail);
    }

    public void OnClickSelect()
    {
        m_ToggleSelected.value = !m_ToggleSelected.value;
        if (OnSelected != null)
            OnSelected(RequestInfo, m_ToggleSelected.value);
    }

    void OnGuildRequestedDetail(C2G.GuildRequestedDetail packet, C2G.GuildRequestedDetailAck ack)
    {
        Detail = ack.detail;
        Popup.Instance.Show(ePopupMode.GuildRequestedInfo, RequestInfo, Detail);
    }
}
