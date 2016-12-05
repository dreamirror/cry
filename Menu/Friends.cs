using UnityEngine;
using System.Collections.Generic;
using PacketInfo;
using System;
using PacketEnums;
using LinqTools;

public enum eFriendsTabMode
{
    FriendsList,
    FriendsRequestList,
    FriendsApproveList,
    FriendsAdd,
    FriendsSNS,
}

public class Friends : MenuBase
{

    public GameObject FriendsItemPrefab;
    public UIGrid m_Grid;
    public UILabel m_LabelEmpty;

    public UILabel m_LabelFriendsCount_List;
    public UILabel m_LabelFriendsDeleteCount;
    public UILabel m_LabelFriendsGift;
    public UILabel m_LabelFriendsCount_Request;
    public UILabel m_LabelFriendsCount_Approve;

    public UIButton m_BtnSendAll;
    public UIButton m_BtnGetAll;

    public GameObject m_NotifyRequested;
    public GameObject[] m_BottomMenus;
    eFriendsTabMode m_CurrentTab = eFriendsTabMode.FriendsList;
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        m_CurrentTab = eFriendsTabMode.FriendsList;
        //GetFriendsInfo();

        UpdateNotify();
        return true;
    }

    override public void UpdateMenu()
    {
        InitItem();
        UpdateNotify();
    }
    void UpdateNotify()
    {
        m_NotifyRequested.SetActive(Network.Instance.NotifyMenu.is_friends_requested);
    }
    ////////////////////////////////////////////////////////////////
    List<pd_FriendsInfo> m_FriendsList = new List<pd_FriendsInfo>();
    List<pd_FriendsInfo> m_Friends;
    List<pd_PlayerInfo> m_Players;
    void GetFriendsInfo()
    {
        if (m_Friends != null) m_Friends.Clear();
        if (m_Players != null) m_Players.Clear();
        switch (m_CurrentTab)
        {
            case eFriendsTabMode.FriendsSNS:
                return;
            case eFriendsTabMode.FriendsAdd:
                {
                    C2G.FriendsCandidateList _packet = new C2G.FriendsCandidateList();
                    Network.GameServer.JsonAsync<C2G.FriendsCandidateList, C2G.FriendsCandidateListAck>(_packet, OnFriendsCandidateListHandler);
                    return;
                }
        }
        C2G.FriendsInfoGet packet = new C2G.FriendsInfoGet();
        packet.state = (eFriendsState)m_CurrentTab;
        Network.GameServer.JsonAsync<C2G.FriendsInfoGet, C2G.FriendsInfoGetAck>(packet, OnFriendsInfoGetHandler);
    }

    //C2G.FriendsInfoGetAck ack;
    void OnFriendsInfoGetHandler(C2G.FriendsInfoGet packet, C2G.FriendsInfoGetAck ack)
    {
        //this.ack = ack;
        m_Friends = ack.friends.FindAll(e=>e.state_info.state == (eFriendsState)m_CurrentTab);
        m_Players = null;
        if (m_CurrentTab == eFriendsTabMode.FriendsList)
        {
            m_FriendsList.Clear();
            m_FriendsList.AddRange(ack.friends);
            UpdateSendGetButton();
        }
        if (m_CurrentTab == eFriendsTabMode.FriendsApproveList && m_Friends.Count > 0)
        {
            Network.Instance.NotifyMenu.is_friends_requested = true;
            UpdateNotify();
        }
        InitItem();
    }
    void OnFriendsCandidateListHandler(C2G.FriendsCandidateList packet, C2G.FriendsCandidateListAck ack)
    {
        m_Friends = null;
        m_Players = ack.players;
        InitItem();
    }
    int FriendsCountMax = 0;
    int FriendsGiftMax = 0;
    int FriendsGiftValue = 0;
    void Start()
    {
        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();
        FriendsCountMax = GameConfig.Get<int>("friends_count_max");
        FriendsGiftMax = GameConfig.Get<int>("friends_gift_max");
        FriendsGiftValue = GameConfig.Get<int>("friends_gift_value");
    }

    List<FriendsItem> m_ListItem = new List<FriendsItem>();
    void InitItem()
    {
        if (m_Grid.gameObject.activeInHierarchy == false) return;

        m_Grid.GetChildList().ForEach(o => o.gameObject.SetActive(false));
        if (m_Friends != null)
        {
            for (int i = 0; i < m_Friends.Count; ++i)
            {
                FriendsItem item;
                if (m_ListItem.Count > i)
                    item = m_ListItem[i];
                else
                {
                    item = NGUITools.AddChild(m_Grid.gameObject, FriendsItemPrefab).GetComponent<FriendsItem>();
                    m_ListItem.Add(item);
                }
                item.Init(m_Friends[i], m_CurrentTab, OnClickCallback);
            }
        }
        else if (m_Players != null)
        {
            for (int i = 0; i < m_Players.Count; ++i)
            {
                FriendsItem item;
                if (m_ListItem.Count > i)
                    item = m_ListItem[i];
                else
                {
                    item = NGUITools.AddChild(m_Grid.gameObject, FriendsItemPrefab).GetComponent<FriendsItem>();
                    m_ListItem.Add(item);
                }
                item.Init(m_Players[i], m_CurrentTab, OnClickCallback);
            }
        }
        m_LabelEmpty.gameObject.SetActive(m_Grid.GetChildList().Count == 0);

        UpdateText();

        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
    }

    private void UpdateText()
    {
        if (m_Friends != null)
        {
            m_LabelFriendsGift.text = Localization.Format("GiftCount", Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_friends), FriendsGiftMax);
            m_LabelFriendsCount_Request.text = Localization.Format("FriendsRequestCount", m_Friends.Count, FriendsCountMax);
        }

        switch (m_CurrentTab)
        {
            case eFriendsTabMode.FriendsList:
                m_LabelEmpty.text = Localization.Get("NotExistFriends");
                break;
            case eFriendsTabMode.FriendsAdd:
                m_LabelEmpty.text = Localization.Get("NotExistRecommandFriends");
                break;
            case eFriendsTabMode.FriendsRequestList:
                m_LabelEmpty.text = Localization.Get("NotExistRequestFriends");
                break;
            case eFriendsTabMode.FriendsApproveList:
                m_LabelEmpty.text = Localization.Get("NotExistApproveFriends");
                break;
            case eFriendsTabMode.FriendsSNS:
                m_LabelEmpty.text = Localization.Get("");
                break;
        }

        m_LabelFriendsCount_Approve.text = Localization.Format("FriendsCount", m_FriendsList.Count(f=>f.state_info.state == eFriendsState.Requested), FriendsCountMax); 
        m_LabelFriendsCount_List.text = Localization.Format("FriendsCount", m_FriendsList.Count(f=>f.state_info.state == eFriendsState.Friends), FriendsCountMax);
        m_LabelFriendsDeleteCount.gameObject.SetActive(SHSavedData.FriendDeleteCount > 0);
        m_LabelFriendsDeleteCount.text = Localization.Format("FriendsDeleteCount", SHSavedData.FriendDeleteCount, Network.PlayerInfo.friends_delete_limit);
    }

    public void OnValueChanged(UIToggle toggle)
    {
        if (toggle.value == false) return;

        Array.ForEach(m_BottomMenus, e => e.SetActive(false));
        switch(toggle.name)
        {
            case "toggleMenu_1":
                m_CurrentTab = eFriendsTabMode.FriendsList;
                break;
            case "toggleMenu_2":
                m_CurrentTab = eFriendsTabMode.FriendsAdd;
                break;
            case "toggleMenu_3":
                m_CurrentTab = eFriendsTabMode.FriendsRequestList;
                break;
            case "toggleMenu_4":
                m_CurrentTab = eFriendsTabMode.FriendsApproveList;
                break;
            case "toggleMenu_5":
                m_CurrentTab = eFriendsTabMode.FriendsSNS;
                Tooltip.Instance.ShowMessageKey("NotImplement");
                return;
        }
        m_BottomMenus[(int)m_CurrentTab].SetActive(true);

        GetFriendsInfo();
    }

    void OnClickCallback(pd_PlayerInfo info)
    {
        Vector3 pos = m_Grid.transform.localPosition;
        m_Grid.Reposition();
        m_Grid.transform.localPosition = pos;
        pd_FriendsInfo friends_info = info as pd_FriendsInfo;
        if (friends_info != null)
        {
            if(friends_info.state_info.state == eFriendsState.Friends && m_FriendsList.Exists(e => e.account_idx == info.account_idx) == false)
                m_FriendsList.Add(friends_info);
            else if(friends_info.state_info.state == eFriendsState.Deleted)
            {
                m_FriendsList.Remove(friends_info);
                SHSavedData.FriendDeleteCount = SHSavedData.FriendDeleteCount + 1;
            }
            else if(friends_info.state_info.state == eFriendsState.Request && m_Players != null)
            {
                m_Players.RemoveAll(f => f.account_idx == info.account_idx);
                if(m_Players.Count == 0)
                    InitItem();
            }
        }
        UpdateText();

        if (m_CurrentTab == eFriendsTabMode.FriendsApproveList)
        {
            m_Friends.RemoveAll(e => e.account_idx == info.account_idx);
            Network.Instance.NotifyMenu.is_friends_requested = m_Friends.Count > 0;
            UpdateNotify();
        }
        if (m_CurrentTab == eFriendsTabMode.FriendsList)
            UpdateSendGetButton();
    }

    public void OnClickSendAll()
    {
        if (m_Friends.Exists(e => e.state_info.give_daily_index != Network.DailyIndex) == false)
        {
            Tooltip.Instance.ShowMessageKey("NotExistsFriendsGift");
            return;
        }

        C2G.FriendsSend packet = new C2G.FriendsSend();
        packet.account_idx = 0;
        packet.is_all = true;
        Network.GameServer.JsonAsync<C2G.FriendsSend, C2G.FriendsAckBase>(packet, OnFriendsSendHandler);
    }
    public void OnClickGetAll()
    {
        if (m_Friends.Exists(e => e.state_info.available_gift == true) == false)
        {
            Tooltip.Instance.ShowMessageKey("NotExistsFriendsGift");
            return;
        }

        C2G.FriendsGiftGet _packet = new C2G.FriendsGiftGet();
        _packet.account_idx = 0;
        _packet.is_all = true;
        Network.GameServer.JsonAsync<C2G.FriendsGiftGet, C2G.FriendsGiftGetAck>(_packet, OnFriendsGiftGetHandler);
    }
    void UpdateSendGetButton()
    {
        if (m_Friends.Exists(e => e.state_info.give_daily_index != Network.DailyIndex))
        {
            m_BtnSendAll.gameObject.SetActive(true);
            m_BtnGetAll.gameObject.SetActive(false);
        }
        else if (m_Friends.Exists(e => e.state_info.available_gift == true))
        {
            m_BtnSendAll.gameObject.SetActive(false);
            m_BtnGetAll.gameObject.SetActive(true);
        }
        else 
        {
            m_BtnSendAll.gameObject.SetActive(false);
            m_BtnGetAll.gameObject.SetActive(false);
        }
    }
    void OnFriendsSendHandler(C2G.FriendsSend packet, C2G.FriendsAckBase ack)
    {
        if (ack.result != pe_FriendsResult.Success) return;

        int count = m_Friends.Count(e => e.state_info.give_daily_index != Network.DailyIndex);
        Tooltip.Instance.ShowMessageKeyFormat("FriendsGiftSendAllCompleted", count);
        m_Friends.ForEach(e => e.state_info.give_daily_index = Network.DailyIndex);
        //if(m_FriendsList.FindAll(e=>e.state_info.available_gift == true).Count * FriendsGiftValue + Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_friends) > FriendsGiftMax)
        //{
        //    Tooltip.Instance.ShowMessageKey("NoMoreGetFriendsGift");
        //    return;
        //}

        InitItem();
        UpdateSendGetButton();
    }
    void OnFriendsGiftGetHandler(C2G.FriendsGiftGet packet, C2G.FriendsGiftGetAck ack)
    {
        m_Friends.ForEach(e => e.state_info.available_gift = false);
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                Network.PlayerInfo.SetGoodsValue(pe_GoodsType.token_friends, ack.token_friends);
                Tooltip.Instance.ShowMessageKeyFormat("FriendsGiftGetAllCompleted", ack.token_friends);
                break;
            default:
                ShowFriendsErrorTooltip(ack.result);
                break;
        }
        InitItem();
        UpdateSendGetButton();
    }
    public void OnClickMoveShop()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }
    public void OnClickFind()
    {
        Popup.Instance.Show(ePopupMode.FriendsRequest, new Action<string>(OnFindNicknameCallback));
    }
    void OnFindNicknameCallback(string nickname)
    {
        pd_PlayerInfo info = m_Players.Find(p => p.nickname == nickname);
        if(info != null)
        {
            m_Players.Remove(info);
            InitItem();
        }
    }
    public void OnClickRefresh()
    {
        GetFriendsInfo();
    }
    public void OnClickRequestAll()
    {
        if (m_Players.Count == 0) return;
        C2G.FriendsRequest packet = new C2G.FriendsRequest();
        packet.account_idx = m_Players.Select(e=>e.account_idx).ToList();
        Network.GameServer.JsonAsync<C2G.FriendsRequest, C2G.FriendsRequestAck>(packet, OnFriendsRequestHandler);
    }
    void OnFriendsRequestHandler(C2G.FriendsRequest packet, C2G.FriendsRequestAck ack)
    {
        switch(ack.result)
        {
            case pe_FriendsResult.Success:
                m_Players.Clear();
                InitItem();
                break;
            default:
                ShowFriendsErrorTooltip(ack.result);
                if (ack.request_count > 0)
                {
                    m_Players.Clear();
                    InitItem();
                }
                break;
        }
    }
    public void OnClickCancelAll()
    {
        if (m_Friends.Count == 0) return;
        C2G.FriendsRequestCancel packet = new C2G.FriendsRequestCancel();
        packet.account_idx = 0;
        packet.is_all = true;
        Network.GameServer.JsonAsync<C2G.FriendsRequestCancel, C2G.FriendsAckBase>(packet, OnFriendsRequestCancelHandler);
    }
    void OnFriendsRequestCancelHandler(C2G.FriendsRequestCancel packet, C2G.FriendsAckBase ack)
    {
        m_Friends.Clear();
        InitItem();
    }

    public void OnClickRefuseAll()
    {
        if (m_Friends.Count == 0) return;
        C2G.FriendsRefuse packet = new C2G.FriendsRefuse();
        packet.account_idx = 0;
        packet.is_all = true;
        Network.GameServer.JsonAsync<C2G.FriendsRefuse, C2G.FriendsAckBase>(packet, OnFriendsRefuseHandler);
    }
    void OnFriendsRefuseHandler(C2G.FriendsRefuse packet, C2G.FriendsAckBase ack)
    {
        m_Friends.Clear();
        InitItem();
        Network.Instance.NotifyMenu.is_friends_requested = false;
        UpdateNotify();
    }
    public void OnClickApproveAll()
    {
        if (m_Friends.Count == 0) return;
        C2G.FriendsApprove packet = new C2G.FriendsApprove();
        packet.account_idx = 0;
        packet.is_all = true;
        Network.GameServer.JsonAsync<C2G.FriendsApprove, C2G.FriendsAckBase>(packet, OnFriendsApproveHandler);
    }
    void OnFriendsApproveHandler(C2G.FriendsApprove packet, C2G.FriendsAckBase ack)
    {
        switch(ack.result)
        {
            case pe_FriendsResult.Success:
                m_Friends.ForEach(e => e.state_info.state = eFriendsState.Friends);
                m_FriendsList.AddRange(m_Friends);
                m_Friends.Clear();
                InitItem();
                Network.Instance.NotifyMenu.is_friends_requested = false;
                UpdateNotify();

                break;
            default:
                ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }

    static public void ShowFriendsErrorTooltip(pe_FriendsResult result)
    {
        switch (result)
        {
            case pe_FriendsResult.AlreadyFriends:
                Tooltip.Instance.ShowMessageKey("AlreadyFriends");
                break;
            case pe_FriendsResult.AlreadyRequest:
                Tooltip.Instance.ShowMessageKey("AlreadyRequest");
                break;
            case pe_FriendsResult.AlreadyRequested:
                Tooltip.Instance.ShowMessageKey("AlreadyRequested");
                break;
            case pe_FriendsResult.InvalidRequest:
                Tooltip.Instance.ShowMessageKey("InvalidRequest");
                break;
            case pe_FriendsResult.FriendsCountMax:
                Tooltip.Instance.ShowMessageKey("FullFriendsList");
                break;
            case pe_FriendsResult.FriendsRequestCountMax:
                Tooltip.Instance.ShowMessageKey("FullFriendsRequestList");
                break;
            case pe_FriendsResult.TargetFriendsCountMax:
                Tooltip.Instance.ShowMessageKey("FullTargetFriendsList");
                break;
            case pe_FriendsResult.LimitGiftMax:
                Tooltip.Instance.ShowMessageKey("NoMoreGetFriendsGift");
                break;
            case pe_FriendsResult.NotExistsNickname:                
                Tooltip.Instance.ShowMessageKey("NotFoundFriends");
                break;
            default:
                break;
        }
    }
}
