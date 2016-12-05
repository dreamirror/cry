using UnityEngine;
using System.Collections.Generic;
using System;

public class PopupRune : MonoBehaviour
{
    public GameObject runeItemPrefab;
    public Transform m_RuneIndicator;

    public UIToggle m_EquippedToggle;
    public UILabel m_title, m_desc;

    public TweenPosition m_tween;

    public GameObject m_EventRuneUnequip;
    public GameObject m_EventRuneEnchant;

    public delegate void OnRuneClickDelegate(Rune rune, string name);
    OnRuneClickDelegate m_OnClickCallback;

    public Rune Rune { get; private set;}
    RuneItem m_RuneItem;

    public void Init(Rune rune, bool equipped, OnRuneClickDelegate del)
    {
        m_OnClickCallback = del;
        Rune = rune;
        if (m_RuneItem == null)
            m_RuneItem = NGUITools.AddChild(m_RuneIndicator.gameObject, runeItemPrefab).GetComponent<RuneItem>();
        m_EquippedToggle.value = equipped;
        m_RuneItem.Init(Rune, false, null);
        m_title.text = Rune.GetName();
        m_desc.text = Rune.GetDesc();

        m_tween.ResetToBeginning();
        m_tween.PlayForward();

        m_EventRuneUnequip.SetActive(EventHottimeManager.Instance.IsRuneUnequipEvent);
        m_EventRuneEnchant.SetActive(EventHottimeManager.Instance.IsRuneEnchantEvent);
    }

    public void OnClick(GameObject btn)
    {
        m_OnClickCallback(Rune, btn.name);
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }
}
