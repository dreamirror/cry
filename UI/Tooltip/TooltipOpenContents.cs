using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContentsOpenInfo
{
    public string icon_id;
    public string title;
    public string message;
}

public class TooltipOpenContents : TooltipBase
{
    public UITweener m_SubTween;

    public UILabel m_LabelTitle, m_LabelMessage;
    public UISprite m_Icon;

    List<ContentsOpenInfo> m_Infos;
    public override void Init(params object[] parms)
    {
        m_Infos = parms[0] as List<ContentsOpenInfo>;

        CheckInfo();
    }

    bool CheckInfo()
    {
        if (m_Infos.Count == 0)
            return false;

        Init(m_Infos[0]);
        m_Infos.RemoveAt(0);
        return true;
    }

    void Init(ContentsOpenInfo info)
    {
        m_Icon.spriteName = info.icon_id;
        m_LabelTitle.text = info.title;
        m_LabelMessage.text = info.message;

        m_SubTween.ResetToBeginning();
        m_SubTween.PlayForward();
    }

    public override void Play()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (CheckInfo() == true)
            return;

        if (finished != null)
            m_Tween.onFinished.Remove(finished);
        Destroy(gameObject);
    }
}
