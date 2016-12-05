using UnityEngine;
using System.Collections;
using PacketInfo;
using System;

public class HottimeEventIcon : MonoBehaviour
{
    public UISprite m_SpriteIcon;
    public UILabel m_LabelRemainTime;
    pd_EventHottime Info;
    Action DisableDelegate;
    void Update()
    {
        if (Info == null) return;
        var remain = Info.end_date - Network.Instance.ServerTime;
        if(remain.TotalSeconds < 0)
        {
            gameObject.SetActive(false);
            if (DisableDelegate != null)
                DisableDelegate();
            return;
        }
        if (remain.TotalSeconds < 60)
            m_LabelRemainTime.text = Localization.Format("HottimeEventIconRemainSeconds", remain.TotalSeconds);
        else if (remain.TotalMinutes < 60)
            m_LabelRemainTime.text = Localization.Format("HottimeEventIconRemainMinutes", remain.Minutes, remain.Seconds);
        else 
            m_LabelRemainTime.text = Localization.Format("HottimeEventIconRemainHours", remain.Hours, remain.Minutes);
    }
    public void Init(pd_EventHottime info, Action _disableDelegate = null)
    {
        Info = info;

        DisableDelegate = _disableDelegate;

        m_SpriteIcon.spriteName = HottimeEventInfoManager.Instance.GetInfoByID(Info.event_id).IconID;
    }
    public void OnShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(Info.title, tooltip);
    }
}
