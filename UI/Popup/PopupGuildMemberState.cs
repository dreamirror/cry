using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketEnums;
using PacketInfo;

public class PopupGuildMemberState : PopupBase
{
    public GameObject m_Master;
    public GameObject m_PartMaster;
    public GameObject m_Member;

    public GameObject m_BtnExpulsion;
    public UIGrid m_Grid;

    short m_State;
    short m_ChangedState;
    Action<short> OnStateChanged = null;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_State = (short)parms[0];
        OnStateChanged = parms[1] as Action<short>;

        m_ChangedState = m_State;
        UpdateState();

        m_BtnExpulsion.SetActive(false);
        m_Grid.Reposition();
        //m_BtnExpulsion.SetActive(m_State != 0);
    }

    void SendCallback(short state)
    {
        if (OnStateChanged != null)
            OnStateChanged(state);
    }
    void UpdateState()
    {
        m_Master.SetActive(m_ChangedState == 0);
        m_PartMaster.SetActive(m_ChangedState == 1);
        m_Member.SetActive(m_ChangedState == 2);
    }
    public void OnClickMaster()
    {
        if(GuildManager.Instance.State.member_state == 0)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnConfirmMaster), "GuildMasterChangeConfirm");
            return;
        }
        //m_ChangedState = 0;
        //UpdateState();
    }
    void OnConfirmMaster(bool is_confirmed)
    {
        if(is_confirmed)
        {
            m_ChangedState = 0;
            UpdateState();
        }
    }
    public void OnClickPartMaster()
    {
        m_ChangedState = 1;
        UpdateState();
    }
    public void OnClickMemeber()
    {
        m_ChangedState = 2;
        UpdateState();
    }
    public void OnClickGuildExpulsion()
    {
        SendCallback(3);
    }
    public void OnClickConfirm()
    {
        base.OnClose();
        if (m_State != m_ChangedState)
            SendCallback(m_ChangedState);
    }

}
