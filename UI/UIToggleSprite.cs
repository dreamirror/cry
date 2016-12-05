using UnityEngine;
using System.Collections;

public class UIToggleSprite : UISprite {
    [SerializeField]
    string m_NormalName = null;
    [SerializeField]
    string m_ActiveName = null;

    public bool ActiveSprite { get { return spriteName == m_ActiveName; } }

    public void SetSpriteActive(bool bActive)
    {
        spriteName = bActive ? m_ActiveName : m_NormalName;
    }

    public void OnValueChanged(UIToggle toggle)
    {
        SetSpriteActive(toggle.value);
    }
}
