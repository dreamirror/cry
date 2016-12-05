using UnityEngine;
using System.Collections;
using PacketInfo;

public class GuildEmblemItem : MonoBehaviour
{
    public UIToggle m_ToggleSelected;
    public UISprite m_SpriteEmblem;

    System.Action<string> OnClickDelegate = null;
    public void Init(string emblem, System.Action<string> _del)
    {
        OnClickDelegate = _del;

        m_ToggleSelected.Set(false);

        m_SpriteEmblem.spriteName = emblem;
    }

    public void OnClickGuild()
    {
        m_ToggleSelected.value = true;
        if (OnClickDelegate != null)
            OnClickDelegate(m_SpriteEmblem.spriteName);
    }
}
