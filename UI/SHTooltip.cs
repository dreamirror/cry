using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void OnShowTooltipDelegate();

[AddComponentMenu("SmallHeroes/UI/SHTooltip")]
[RequireComponent(typeof(BoxCollider2D))]
public class SHTooltip : MonoBehaviour
{
    public List<EventDelegate> OnShowTooltip = new List<EventDelegate>();
    public float span_press_time = 0.2f;

    public bool Showed { get; private set; }
    public bool Pressed { get { return longPressed; } }
    public BoxCollider2D Collider { get { return m_Collider; } }

    public string LocalizeKey;

    bool bDisableTooltip = false;

    BoxCollider2D m_Collider = null;
    float pressedTime;
    bool longPressed;

    void Start()
    {
        m_Collider = gameObject.GetComponent<BoxCollider2D>();

        pressedTime = 0;
        longPressed = false;
        Showed = false;

    }

    void Update()
    {
        if (bDisableTooltip) return;
        if ((OnShowTooltip.Count == 0 && string.IsNullOrEmpty(LocalizeKey) == true)|| pressedTime == 0f) return;

        if(Showed == false && longPressed == false && Time.realtimeSinceStartup > pressedTime + span_press_time)
        {
            longPressed = true;
            Show();
        }

    }

    void Show()
    {
        if (bDisableTooltip) return;
        Showed = true;
        OnShowTooltip.ForEach(e => e.Execute());
        if (string.IsNullOrEmpty(LocalizeKey) == false)
            Tooltip.Instance.ShowTarget(Localization.Get(LocalizeKey), this);
    }

    public void CancelShow()
    {
        Showed = false;
    }

    void OnPress(bool isPressed)
    {
        if (bDisableTooltip) return;
        longPressed = false;
        if (isPressed)
        {
            Showed = false;

            //var button = gameObject.GetComponent<UIButton>();
            //if (button == null || button.enabled == false || button.onClick.Count == 0)
            //{
            //    longPressed = true;
            //    Show();
            //}
            //else
                pressedTime = Time.realtimeSinceStartup;
        }
        else
            pressedTime = 0f;
    }

    public void SetDisableTooltip(bool is_disable)
    {
        bDisableTooltip = is_disable;
    }

}
