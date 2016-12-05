using UnityEngine;
using System.Collections;
using SharedData;

public class MissionRewardItemExp : MonoBehaviour
{
    public UILabel m_Text;

    public void Init(int exp)
    {
        m_Text.text = string.Format("x{0}", exp);
    }
}
