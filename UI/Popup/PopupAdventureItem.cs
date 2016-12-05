using UnityEngine;
using System.Collections;
using System;

public class PopupAdventureItem : MonoBehaviour {

    public UILabel m_labelTitle;
    public UIToggle m_Toggle;
    public UISprite m_SpriteLock;
    public UIToggleSprite m_ToggleSprite;
    //public UILabel m_LabelState;
    public GameObject m_Progress;
    public GameObject m_Complete;

    System.Action<AdventureInfo> callback;

    AdventureInfo m_AdvantureInfo;
    public AdventureInfo Info { get { return m_AdvantureInfo; } }
    DateTime m_EndTime = DateTime.MinValue;
    public bool IsLock { get { return m_SpriteLock.gameObject.activeSelf; } }

    void Update()
    {
        if (m_EndTime != DateTime.MinValue)
        {
            if (m_EndTime < Network.Instance.ServerTime)
            {
                m_EndTime = DateTime.MinValue;
                m_Progress.SetActive(false);
                m_Complete.SetActive(true);
            }
        }
    }
    public void Init(AdventureInfo map_info, System.Action<AdventureInfo> callback)
    {
        m_AdvantureInfo = map_info;

        var detail = AdventureInfoManager.Instance.GetInfo(map_info.IDN);
        m_Progress.SetActive(false);
        m_Complete.SetActive(false);
        
        if (detail != null && detail.is_rewarded == false)
        {
            if (Network.Instance.ServerTime > detail.end_at)
                m_Complete.SetActive(true);            
            else if (detail.is_begin  && Network.Instance.ServerTime < detail.end_at)
            {
                m_Progress.SetActive(true);
                m_EndTime = detail.end_at;
            }
        }

        m_labelTitle.text = m_AdvantureInfo.Name;
        m_ToggleSprite.SetSpriteActive(false);
        m_Toggle.value = false;
        m_Toggle.Set(false);

        this.callback = callback;

        m_SpriteLock.gameObject.SetActive(m_AdvantureInfo.CheckCondition() != null);
    }

    public void Select()
    {
        m_Toggle.value = true;
    }

    public void OnValueChanged()
    {
        if (m_Toggle.value == true)
            this.callback(m_AdvantureInfo);
    }
}
