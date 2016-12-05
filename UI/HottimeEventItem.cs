using UnityEngine;
using System.Collections;
using PacketInfo;
using System;

public class HottimeEventItem : MonoBehaviour
{
    public GameObject m_EventProgress;
    public UILabel m_LabelTitle;

    pd_EventHottime Info;
    Action<pd_EventHottime> _OnSelected = null;
    public void Init(pd_EventHottime info, Action<pd_EventHottime> OnSelectedDelegate)
    {
        Info = info;
        _OnSelected = OnSelectedDelegate;

        m_LabelTitle.text = Info.title;
        m_EventProgress.SetActive(Info.OnGoing);
    }

    public void OnClickItem()
    {
        if (_OnSelected != null)
            _OnSelected(Info);
    }
}
