using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PacketEnums;
using System;

public class TooltipProfile : TooltipBase
{
    public GameObject ProfileIndicater;
    public GameObject ProfilePrefab;
    PlayerProfile m_PlayerProfile;

    Action<PopupChatLine> OnClickWhisperCallback = null;
    PopupChatLine m_Chat = null;
    public override void Init(params object[] parms)
    {
        m_Chat = parms[0] as PopupChatLine;
        if (m_PlayerProfile == null)
        {
            m_PlayerProfile = NGUITools.AddChild(ProfileIndicater, ProfilePrefab).GetComponent<PlayerProfile>();
            m_PlayerProfile.GetComponent<BoxCollider2D>().enabled = false;
        }

        OnClickWhisperCallback = parms[1] as Action<PopupChatLine>;

//        m_PlayerProfile.UpdateProfile(m_Chat.Line.ThumbIdx, m_Chat.Line.Nickname, m_Chat.Line.Level);
    }

    public void OnClickFriendsReuqest()
    {
        if (m_Chat.Line.AccountIdx == SHSavedData.AccountIdx)
            return;

        C2G.FriendsRequest packet = new C2G.FriendsRequest();
        packet.account_idx = new List<long>();
        packet.account_idx.Add(m_Chat.Line.AccountIdx);
        Network.GameServer.JsonAsync<C2G.FriendsRequest, C2G.FriendsRequestAck>(packet, OnFriendsRequestHandler);
    }
    void OnFriendsRequestHandler(C2G.FriendsRequest send, C2G.FriendsRequestAck ack)
    {
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                Tooltip.Instance.ShowMessageKey("RequestedFriends");
                Close();
                break;
            case pe_FriendsResult.AlreadyRequest:
            case pe_FriendsResult.AlreadyRequested:
            case pe_FriendsResult.TargetFriendsCountMax:
                gameObject.SetActive(false);
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }

    public void OnClickWhisper()
    {
        if(OnClickWhisperCallback != null)
        {
            OnClickWhisperCallback(m_Chat);
        }
        Close();
    }
    public void Close()
    {
        OnFinished();
    }
}
