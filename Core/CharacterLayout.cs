using UnityEngine;
using System.Collections;

public class CharacterLayout : MonoBehaviour {

    public CharacterContainer[] m_Characters;
    public CharacterContainer Center;

    public void Init(BattleLayout layout)
    {
        m_Characters = GetComponentsInChildren<CharacterContainer>();

//         foreach (var container in m_Characters)
//             container.Init(new CreatureDummy(container, container.GetComponentInChildren<Character>()));
    }
    public void Batch(BattleLayout layout)
    {
        if (Center != null)
        {
            Vector3 vCenter = Vector3.zero;
            vCenter.x = (m_Characters.Length - 1) * -layout.horizontal * 0.5f;
            Center.transform.localPosition = vCenter;
        }

        Vector3 pos = Vector3.zero;
        pos.z = layout.TopFirst ? layout.vertical : -layout.vertical;

        for (int i = 0; i < m_Characters.Length; ++i)
        {
            pos.x = i * -layout.horizontal;
            m_Characters[i].Batch(pos);
            pos.z *= -1f;
        }
    }
}
