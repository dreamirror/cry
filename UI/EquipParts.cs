using UnityEngine;
using System.Collections;

public class EquipParts : MonoBehaviour {
    public UIToggleSprite[] m_ToggleStuff;
    public UIToggle m_ToggleFull;
    public UIToggleSprite m_SpriteFull;

    public void Init(Equip equip)
    {
        m_ToggleFull.value = equip.EnchantLevel >= 5;

        if (m_ToggleFull.value == true)
            m_SpriteFull.SetSpriteActive(equip.AvailableUpgrade());
        else
        {
            for (int i = 0; i < 3 && i < equip.Stuffs.Count; ++i)
            {
                m_ToggleStuff[i].SetSpriteActive(equip.Stuffs[i].Count > 0);
            }
        }
    }
}
