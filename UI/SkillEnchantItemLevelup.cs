using UnityEngine;
using System.Collections;

public class SkillEnchantItemLevelup : MonoBehaviour {

    System.Action<GameObject> OnFinish;
    public UILabel m_Label;

    public void Init(float delay, string text, System.Action<GameObject> OnFinish)
    {
        m_Label.text = text;
        this.OnFinish = OnFinish;
        foreach (var tween in m_Label.gameObject.GetComponents<UITweener>())
        {
            tween.delay = delay;
        }
        m_Label.gameObject.GetComponent<UIPlayTween>().Play(true);
    }

    public void OnFinished()
    {
        OnFinish(gameObject);
    }
}
