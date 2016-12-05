using UnityEngine;
using System.Collections;

public class TooltipChatChannel : TooltipBase {

    public PrefabManager m_ChannelSelectBtnPrefabManager;
    public UIGrid m_BtnGrid;
    public UILabel m_CurrentPageLabel;

    public GameObject m_Contents;

    
    int m_max_page;
    int m_now_page = 1;
    const int PAGE_PER_BTN_COUNT = 20;
    System.Action<string> ChannelChangeCallback;

    public override void Init(params object[] parms)
    {
        ChannelChangeCallback = parms[0] as System.Action<string>;
        m_max_page = (Network.ChatServer.ChannelCount / PAGE_PER_BTN_COUNT) + 1;

        m_Contents.transform.localPosition = new Vector3(-40, 60);

        ChannelBtnDraw();
    }

    public override void Play()
    {
        gameObject.SetActive(true);
    }

    public void OnChannelListRightClick()
    {
        if (m_max_page <= m_now_page)
            return;

        m_CurrentPageLabel.text = (++m_now_page).ToString();
        ChannelBtnDraw();
    }

    public void OnChannelListLeftClick()
    {
        if (1 >= m_now_page)
            return;

        m_CurrentPageLabel.text = (--m_now_page).ToString();

        ChannelBtnDraw();
    }

    public void Close()
    {
        m_ChannelSelectBtnPrefabManager.Destroy();
        OnFinished();
    }

    void ChannelBtnDraw()
    {
        m_ChannelSelectBtnPrefabManager.Destroy();

        int btn_start = m_now_page <= 1 ? 1 : ((m_now_page - 1) * PAGE_PER_BTN_COUNT) + 1;

        for (int i = btn_start; i < btn_start + PAGE_PER_BTN_COUNT; i++)
        {
            if (Network.ChatServer.ChannelCount < i)
                continue;
            var btn = m_ChannelSelectBtnPrefabManager.GetNewObject<PopupChatChannelBtn>(m_BtnGrid.transform, Vector3.zero);
            btn.Init(i, ChannelChangeCallback, Close);
        }

        m_BtnGrid.Reposition();
    }

}
