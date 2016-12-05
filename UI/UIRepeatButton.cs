using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIRepeatButton : UIButton
{
    float interval = 0.05f;
    float first_interval = 0.25f;

    bool m_IsPressed = false;
    double m_NextClick = 0f;
    override protected void OnPress(bool isPressed) { m_IsPressed = isPressed; m_NextClick = Time.realtimeSinceStartup + first_interval; if (_OnPressed != null && isPressed) _OnPressed(); }
    public Action _OnRepeat = null;
    public Action _OnPressed = null;

    void Update()
    {
        Single time = Time.realtimeSinceStartup;
        if (m_IsPressed && time > m_NextClick)
        {
            m_NextClick = time + interval;

            ProcessRepeat();
        }
    }

    void ProcessRepeat()
    {
        if (_OnRepeat != null)
            _OnRepeat();
    }

    public void SetPressed(bool isPressed)
    {
        m_IsPressed = isPressed;
    }
}
