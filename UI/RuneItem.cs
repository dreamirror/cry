using UnityEngine;
using System.Collections;

public class RuneItem : MonoBehaviour
{
    public UIToggle m_toggle, m_lock_toggle;
    public delegate void OnRuneClickDelegate(RuneItem rune);

    public UISprite m_SpriteIcon;
    public UISprite m_SellCheckMark;
    
    public UILabel m_LabelGrade, m_LabelLevel;

    public UISprite m_SpriteBlock;
    public SHTooltip m_Tooltip;

    OnRuneClickDelegate OnRuneClick = null;
    public Rune Rune { get; private set; }

    int slot_number;
    //---------------------------------------------------------------------------
    public void Init(Rune rune, bool is_lock, OnRuneClickDelegate _del, int slot_index = 0)
    {
        gameObject.SetActive(true);
        //GetComponent<BoxCollider2D>().enabled = (_del != null && rune != null) || is_lock == true;
        if (rune == null)
            GetComponent<UIButtonScale>().enabled = false;

        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);

        m_toggle.Set(is_lock || rune == null);
        
        slot_number = slot_number != 0 ? slot_number : slot_index;
        
        if (rune != null)
        {
            m_LabelGrade.text = rune.Info.Grade.ToString();
            m_LabelLevel.text = Localization.Format("HeroLevel", rune.Level);
            m_SpriteIcon.spriteName = rune.Info.IconID;
        }
        m_lock_toggle.Set(is_lock);

        Rune = rune;
        OnRuneClick = _del;
        m_Tooltip.span_press_time = OnRuneClick != null ? 0.2f : 0f;
        gameObject.GetComponent<UIButtonScale>().enabled = OnRuneClick != null;
        m_Tooltip.SetDisableTooltip(false);
    }

    public void RefreshRuneInfo(Rune rune)
    {
        m_LabelGrade.text = rune.Info.Grade.ToString();
        m_LabelLevel.text = Localization.Format("HeroLevel", rune.Level);
        m_SpriteIcon.spriteName = rune.Info.IconID;
    }

    public void InitDummy()
    {
        Rune = null;
        OnRuneClick = null;
        gameObject.SetActive(true);
        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = false);
        GetComponent<BoxCollider2D>().enabled = true;
        m_Tooltip.SetDisableTooltip(true);
        gameObject.GetComponent<UIButtonScale>().enabled = false;
    }

    //---------------------------------------------------------------------------
    public void SetSelected(bool bSelected)
    {
        m_SellCheckMark.gameObject.SetActive(bSelected);
    }
    public void OnClickRune()
    {
        if (m_Tooltip.Showed) return;
        if (Rune == null)
        {
        //    if (m_lock_toggle.value == true)
        //        Tooltip.Instance.ShowMessage(GetOpenConditionString());
            return;
        }
        //Debug.LogFormat("OnClick : {0}", Rune.Info.ID);
        if (OnRuneClick != null)
            OnRuneClick(this);
    }

    public void SetBlockSprite(bool isActive)
    {
        m_SpriteBlock.gameObject.SetActive(isActive);
    }

    public void RefreshUpgradeRune(Rune rune, bool is_active)
    {
        if (is_active == true)
        {
            m_LabelGrade.text = (rune.Info.Grade + 1).ToString();
            m_LabelLevel.text = Localization.Format("HeroLevel", 1);
        }
        m_toggle.value = !is_active;
    }
    public void OnShowTooltip(SHTooltip target)
    {
        if (Rune == null)
        {
            if (m_lock_toggle.value == true)
                Tooltip.Instance.ShowTarget(Localization.Format("RuneTooltip",Localization.Get("RuneSlot"), GetOpenConditionString()), target);
            else
                Tooltip.Instance.ShowTarget(Localization.Format("RuneTooltip",Localization.Get("RuneSlot"), Localization.Get("RuneEquipAvailable")), target);
        }
        else
            Tooltip.Instance.ShowTarget(Rune.GetTooltip(), target);
    }
    string GetOpenConditionString()
    {
        int need_creature_grade = slot_number + 1 >= 6 ? 6 : slot_number + 2;
        int need_creature_enchant = slot_number + 1 >= 6 ? slot_number + 1 : 0;
        if (need_creature_enchant == 0)
            return Localization.Format("UnlockRune", Localization.Format("CreatureGrade", need_creature_grade), "");
        else
            return Localization.Format("UnlockRune", Localization.Format("CreatureGrade", need_creature_grade), string.Format("+{0}", need_creature_enchant));
    }
}
