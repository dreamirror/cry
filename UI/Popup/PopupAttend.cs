using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System;
using System.Linq;

public class PopupAttend : PopupBase {

    public UILabel TitleLabel;
    public UIGrid AttendItemGrid;
    public PrefabManager AttendItemPrefabManager;
    public UILabel PrincessMentLabel;

    Attend m_Attend;

    List<Attend> activated_list;
    List<PopupAttendItem> m_RewardItems;

    OnPopupCloseDelegate _OnPopupCloseDelegate = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        bool show_all = (bool)parms[0];
        if (show_all)
            activated_list = AttendManager.Instance.Attends.ToList();
        else
            activated_list = AttendManager.Instance.Attends.Where(a => a.IsNewReward).ToList();

        if (parms.Length > 1)
            _OnPopupCloseDelegate = parms[1] as OnPopupCloseDelegate;
        else
            _OnPopupCloseDelegate = null;

        Init();    
    }

    void Init()
    {   
        DrawAttend();
    }

    void DrawAttend()
    {
        if (activated_list == null || activated_list.Count == 0)
        {
            OnClose();
            return;
        }

        m_RewardItems = new List<PopupAttendItem>();

        m_Attend = activated_list[0];
        activated_list.RemoveAt(0);

        for (short day_index = 0; day_index < m_Attend.Info.rewards.Count; ++day_index)
        {
            var item = AttendItemPrefabManager.GetNewObject<PopupAttendItem>(AttendItemGrid.transform, Vector3.zero);
            item.Init(day_index, day_index < m_Attend.Data.take_count_max, day_index < m_Attend.Data.take_count, m_Attend.Info.rewards[day_index]);

            m_RewardItems.Add(item);
        }

        PrincessMentLabel.text = m_Attend.Info.description;

        AttendItemGrid.Reposition();

    }
    
    public void OnSendBtnClick()
    {
        if (m_Attend.Data.take_count < m_Attend.Data.take_count_max)
        {
            foreach (var item in m_RewardItems)
            {
                if (item.IsRewarded == false && item.IsEnabled == true)
                {
                    C2G.AttendRewardGet _AttendRewardGet = new C2G.AttendRewardGet();
                    _AttendRewardGet.attend_idn = m_Attend.Data.attend_idn;
                    _AttendRewardGet.take_count = m_Attend.Data.take_count;
                    _AttendRewardGet.is_additional = m_Attend.Data.last_daily_index == Network.DailyIndex;
                    Network.GameServer.JsonAsync<C2G.AttendRewardGet, C2G.AttendRewardGetAck>(_AttendRewardGet, OnAttendRewardGetAckHandler);
                    return;
                }
            }
        }
        Tooltip.Instance.ShowMessageKey("AttendNoRewards");
    }

    void OnAttendRewardGetAckHandler(C2G.AttendRewardGet packet, C2G.AttendRewardGetAck ack)
    {
        m_Attend.SetReward(ack.take_count);
        m_RewardItems[packet.take_count].SetReward();
        if(Network.Instance.UnreadMailState == PacketEnums.pe_UnreadMailState.None)
            Network.Instance.SetUnreadMail(PacketEnums.pe_UnreadMailState.UnreadMail);
        Tooltip.Instance.ShowMessageKey("AttendRewarded");
    }

    public override void OnClose()
    {
        if (activated_list != null && activated_list.Count > 0)
            DrawAttend();
        else
            base.OnClose();
    }

    public override void OnFinishedHide()
    {
        if (_OnPopupCloseDelegate != null)
            _OnPopupCloseDelegate();
        base.OnFinishedHide();
    }
}
