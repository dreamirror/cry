using UnityEngine;
using System.Collections;

public class PopupWeeklyItem : MonoBehaviour {

    public UILabel m_labelTitle;
    public UIToggle m_Toggle;
    public UISprite m_SpriteLock;
    public UIToggleSprite m_ToggleSprite;

    System.Action<MapInfo> callback;

    MapInfo m_MapInfo;

    public bool IsLock { get { return m_SpriteLock.gameObject.activeSelf; } }

    public void Init(MapInfo map_info, System.Action<MapInfo> callback)
    {
        m_MapInfo = map_info;
        m_labelTitle.text = m_MapInfo.Name;
        //m_ToggleSprite.SetSpriteActive(false);
        m_Toggle.value = false;
        m_Toggle.Set(false);

        this.callback = callback;

        m_SpriteLock.gameObject.SetActive(m_MapInfo.CheckCondition() != null);
    }

    public void Select()
    {
        m_Toggle.value = true;
    }

    public void OnValueChanged()
    {
        if (m_Toggle.value == true)
            this.callback(m_MapInfo);
    }
}
