using UnityEngine;
using System.Collections;
using PacketInfo;
using System;
using PacketEnums;

public class FriendsItem : MonoBehaviour
{
    public UISprite m_SpriteUser;
    public UILabel m_LabelNickname;
    public UILabel m_LabelLevel;
    public UILabel m_LabelLoginTime;
    public GameObject[] m_Rights;
    public UIButton m_BtnSend;
    public UIButton m_BtnGet;

    Action<pd_PlayerInfo> OnCallback;
    pd_PlayerInfo m_Info;
    pd_FriendsStateInfo StateInfo { get { return (m_Info as pd_FriendsInfo).state_info; } }
    //---------------------------------------------------------------------------
    public void Init(pd_PlayerInfo info, eFriendsTabMode state, Action<pd_PlayerInfo> callback)
    {
        m_Info = info;
        OnCallback = callback;
        gameObject.SetActive(true);
        string profile_id = info.leader_creature.GetProfileName();
        m_SpriteUser.spriteName = profile_id;
        m_LabelLevel.text = info.player_level.ToString();// Localization.Format("Level", info.player_level);
        m_LabelNickname.text = info.nickname;

        var friend_info = m_Info as pd_FriendsInfo;
        if (friend_info != null && friend_info.is_connected)
            m_LabelLoginTime.text = Localization.Get("UserConnected");
        else
            m_LabelLoginTime.text = Network.GetConnectedTimeString(info.last_login_at);

        Array.ForEach(m_Rights, e => e.SetActive(false));
        switch (state)
        {
            case eFriendsTabMode.FriendsList:
                m_Rights[0].SetActive(true);
                break;
            case eFriendsTabMode.FriendsAdd:
                m_Rights[1].SetActive(true);
                break;
            case eFriendsTabMode.FriendsRequestList:
                m_Rights[2].SetActive(true);
                break;
            case eFriendsTabMode.FriendsApproveList:
                m_Rights[3].SetActive(true);
                break;
        }

        UpdateButton();
    }

    private void UpdateButton()
    {
        pd_FriendsInfo friends_info = m_Info as pd_FriendsInfo;
        if (friends_info != null)
        {
            if (StateInfo.give_daily_index != Network.DailyIndex)
            {
                m_BtnSend.GetComponent<BoxCollider2D>().enabled = true;
                m_BtnSend.state = UIButtonColor.State.Normal;
            }
            else
            {
                m_BtnSend.GetComponent<BoxCollider2D>().enabled = false;
                m_BtnSend.state = UIButtonColor.State.Disabled;
            }

            if (StateInfo.available_gift == true)
            {
                m_BtnGet.GetComponent<BoxCollider2D>().enabled = true;
                m_BtnGet.state = UIButtonColor.State.Normal;
            }
            else
            {
                m_BtnGet.GetComponent<BoxCollider2D>().enabled = false;
                m_BtnGet.state = UIButtonColor.State.Disabled;
            }
        }
    }

    //---------------------------------------------------------------------------

    public void OnClickSend()
    {
        C2G.FriendsSend packet = new C2G.FriendsSend();
        packet.account_idx = m_Info.account_idx;
        packet.is_all = false;
        Network.GameServer.JsonAsync<C2G.FriendsSend, C2G.FriendsAckBase>(packet, OnFriendsSendHandler);
    }
    void OnFriendsSendHandler(C2G.FriendsSend packet, C2G.FriendsAckBase ack)
    {
        switch(ack.result)
        {
            case pe_FriendsResult.Success:
                Tooltip.Instance.ShowMessageKey("FriendsSendGiftSuccess");
                StateInfo.give_daily_index = Network.DailyIndex;
                UpdateButton();
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }
    public void OnClickGet()
    {
        C2G.FriendsGiftGet packet = new C2G.FriendsGiftGet();
        packet.account_idx = m_Info.account_idx;
        packet.is_all = false;
        Network.GameServer.JsonAsync<C2G.FriendsGiftGet, C2G.FriendsGiftGetAck>(packet, OnFriendsGiftGetHandler);
    }
    void OnFriendsGiftGetHandler(C2G.FriendsGiftGet packet, C2G.FriendsGiftGetAck ack)
    {
        //pd_FriendsInfo friends_info = m_Info as pd_FriendsInfo;
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                Tooltip.Instance.ShowMessageKey("FriendsSendGiftSuccess");
                StateInfo.available_gift = false;
                Network.PlayerInfo.SetGoodsValue(pe_GoodsType.token_friends, ack.token_friends);
                UpdateButton();
                if (OnCallback != null)
                    OnCallback(m_Info);
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }
    public void OnClickDelete()
    {
        Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnDeleteConfirm), "FriendsDeleteConfirm");
    }
    void OnDeleteConfirm(bool is_confirm)
    {
        if(is_confirm)
        {
            C2G.FriendsDelete packet = new C2G.FriendsDelete();
            packet.account_idx = m_Info.account_idx;
            Network.GameServer.JsonAsync<C2G.FriendsDelete, C2G.FriendsAckBase>(packet, OnFriendsDeleteHandler);
        }
    }

    void OnFriendsDeleteHandler(C2G.FriendsDelete packet, C2G.FriendsAckBase ack)
    {
        switch(ack.result)
        {
            case pe_FriendsResult.Success:
                StateInfo.state = eFriendsState.Deleted;
                gameObject.SetActive(false);
                if (OnCallback != null)
                    OnCallback(m_Info);
                break;
            case pe_FriendsResult.LimitDeleteFriends:
                Tooltip.Instance.ShowMessageKey("LimitDeleteFriends");
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }
    public void OnClickReqeust()
    {
        C2G.FriendsRequest packet = new C2G.FriendsRequest();
        packet.account_idx = new System.Collections.Generic.List<long>();
        packet.account_idx.Add(m_Info.account_idx);
        Network.GameServer.JsonAsync<C2G.FriendsRequest, C2G.FriendsRequestAck>(packet, OnFriendsRequestHandler);
    }
    void OnFriendsRequestHandler(C2G.FriendsRequest packet, C2G.FriendsRequestAck ack)
    {
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                gameObject.SetActive(false);                
                if (OnCallback != null)
                    OnCallback(new pd_FriendsInfo(m_Info));
                break;
            case pe_FriendsResult.AlreadyRequest:
            case pe_FriendsResult.AlreadyRequested:
            case pe_FriendsResult.TargetFriendsCountMax:
                gameObject.SetActive(false);
                if (OnCallback != null)
                    OnCallback(m_Info);
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }
    public void OnClickRequestCancel()
    {
        C2G.FriendsRequestCancel packet = new C2G.FriendsRequestCancel();
        packet.account_idx = m_Info.account_idx;
        packet.is_all = false;
        Network.GameServer.JsonAsync<C2G.FriendsRequestCancel, C2G.FriendsAckBase>(packet, OnFriendsRequestCancelHandler);
    }
    void OnFriendsRequestCancelHandler(C2G.FriendsRequestCancel packet, C2G.FriendsAckBase ack)
    {
        gameObject.SetActive(false);
        pd_FriendsInfo friends_info = m_Info as pd_FriendsInfo;
        friends_info.state_info.state = eFriendsState.Deleted;

        if (OnCallback != null)
            OnCallback(m_Info);
    }

    public void OnClickRefuse()
    {
        C2G.FriendsRefuse packet = new C2G.FriendsRefuse();
        packet.account_idx = m_Info.account_idx;
        packet.is_all = false;
        Network.GameServer.JsonAsync<C2G.FriendsRefuse, C2G.FriendsAckBase>(packet, OnFriendsRefuseHandler);
    }
    void OnFriendsRefuseHandler(C2G.FriendsRefuse packet, C2G.FriendsAckBase ack)
    {
        StateInfo.state = eFriendsState.Deleted;
        gameObject.SetActive(false);
        if (OnCallback != null)
            OnCallback(m_Info);
    }
    public void OnClickApprove()
    {
        C2G.FriendsApprove packet = new C2G.FriendsApprove();
        packet.account_idx = m_Info.account_idx;
        packet.is_all = false;
        Network.GameServer.JsonAsync<C2G.FriendsApprove, C2G.FriendsAckBase>(packet, OnFriendsApproveHandler);
    }
    void OnFriendsApproveHandler(C2G.FriendsApprove packet, C2G.FriendsAckBase ack)
    {
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                StateInfo.state = eFriendsState.Friends;
                gameObject.SetActive(false);
                if (OnCallback != null)
                    OnCallback(m_Info);
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }

    }
}
