using UnityEngine;
using System.Collections;
using PacketInfo;
using LinqTools;

public class PopupHottimeEvent : PopupBase
{
    public PrefabManager EventItemPrefab;
    public UniWebView m_WebView;

    //void Update()
    //{
    //    //if(Input.GetKeyDown(KeyCode.Escape))
    //    //{
    //    //    OnClose();
    //    //}
    //}
    //void OnReceivedKeyCode(UniWebView webview, int keycode)
    //{//This event only fired on Android.
    //    if ((KeyCode)keycode == KeyCode.Escape)
    //    {
    //        OnClose();
    //    }

    //}
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_WebView.gameObject.SetActive(true);
        m_WebView.backButtonEnable = false;
        //m_WebView.OnReceivedKeyCode += OnReceivedKeyCode;
        m_WebView.SetShowSpinnerWhenLoading(true);
        //716,546
        m_WebView.insets.top = (720 - 546) / 2;
        m_WebView.insets.bottom = (720 - 546) / 2;
        m_WebView.insets.right = 114;
        m_WebView.insets.left = 1280 - 716 - m_WebView.insets.right;

        EventItemPrefab.Clear();
        HottimeEventItem first = null;
        foreach (var item in EventHottimeManager.Instance.GetShowEvents())
        {
            var event_item = EventItemPrefab.GetNewObject<HottimeEventItem>(EventItemPrefab.transform, Vector3.zero);
            event_item.Init(item, OnSelectHottimeEvent);
            if (first == null)
                first = event_item;
        }

        if (first != null)
            first.OnClickItem();
        ResetPosition();
    }

    private void ResetPosition()
    {
        EventItemPrefab.GetComponent<UIGrid>().Reposition();
        var scroll = EventItemPrefab.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
    }

    public override void OnFinishedHide()
    {
        m_WebView.Hide();
        EventItemPrefab.Clear();
        base.OnFinishedHide();
    }
    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        ResetPosition();
    }
    public override void OnClose()
    {
        base.OnClose();
    }

    void OnSelectHottimeEvent(pd_EventHottime info)
    {
        m_WebView.Show();
        string url = "http://naver.com/";
        if (m_WebView.currentUrl != url)
            m_WebView.Load(url);
    }
}
