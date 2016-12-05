using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipScreenLock : TooltipBase
{
    public UITweener m_SubTween;
    public UISprite m_Unlocking, m_UnlockingProgress;
    public UILabel m_Desc3dTouch;

    public override void Init(params object[] parms)
    {
        Show();
    }

    void Show()
    {
        m_Desc3dTouch.gameObject.SetActive(Input.touchPressureSupported);
        m_Unlocking.gameObject.SetActive(false);
        m_SubTween.ResetToBeginning();
        m_SubTween.PlayForward();
    }

    public void OnClickBlock()
    {
        Show();
    }

    void Update()
    {
        if (unlock_time != 0f)
        {
            double passed_time = Time.unscaledTime - unlock_time;
            float progress = (float)passed_time / 2f;
            m_UnlockingProgress.fillAmount = progress;
            if (passed_time > 2f)
            {
                Unlock();
            }
        }
    }

    void Unlock()
    {
        unlock_time = 0f;
        m_Unlocking.gameObject.SetActive(false);
        Tooltip.Instance.ShowMessageKey("ScreenLockUnlocked");
        OnFinished();
    }

    float unlock_time = 0f;
    public void OnPressUnlock()
    {
        unlock_time = Time.unscaledTime;
        m_Unlocking.gameObject.SetActive(true);
    }

    public void OnReleaseUnlock()
    {
        if (unlock_time != 0f && Time.unscaledTime - unlock_time < 0.2f)
            Show();

        m_Unlocking.gameObject.SetActive(false);
        unlock_time = 0f;
    }

    public void OnDeepTouchUnlock()
    {
        Unlock();
    }
}
