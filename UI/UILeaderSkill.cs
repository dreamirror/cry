using UnityEngine;
using System.Collections;
using PacketEnums;

public delegate void OnLeaderSkillDelegate();

public class UILeaderSkill : MonoBehaviour
{
    public UISprite m_SpriteLeaderSkill;
    public UISprite m_SpriteLeaderChar;
    public UISprite m_SpriteLeaderCharBorder;
    public BoxCollider2D m_Collider;

    public Creature LeaderCreature { get; private set; }
    public pe_UseLeaderSkillType UseLeaderSkillType { get; private set; }

    public UISprite m_LockLeaderSkill;

    OnLeaderSkillDelegate OnLeaderSkill = null;
    public void Init(Creature creature, pe_UseLeaderSkillType condition, OnLeaderSkillDelegate _del = null)
    {
        LeaderCreature = creature;
        UseLeaderSkillType = condition;

        gameObject.SetActive(true);
        OnLeaderSkill = _del;

        if (creature == null)
        {
            m_SpriteLeaderChar.spriteName = "black";
            m_SpriteLeaderSkill.spriteName = "black";
            return;
        }
        if (creature.TeamSkill != null)
        {
            if (m_SpriteLeaderSkill.atlas.Contains(creature.TeamSkill.Info.ID) == true)
                m_SpriteLeaderSkill.spriteName = creature.TeamSkill.Info.ID;
            else
                m_SpriteLeaderSkill.spriteName = "skill_default";
        }
        else
            m_SpriteLeaderSkill.spriteName = "none";

        string sprite_name = string.Format("cs_{0}", creature.Info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_SpriteLeaderChar.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;
        m_SpriteLeaderChar.spriteName = new_sprite_name;
    }

    public void OnClick()
    {
        if (bTooltipShowed == true)
        {
            bTooltipShowed = false;
            return;
        }

        if (m_LockLeaderSkill != null && m_LockLeaderSkill.gameObject.activeInHierarchy == true)
        {
            Tooltip.Instance.ShowMessage(lock_message);
            return;
        }

        if (OnLeaderSkill != null)
            OnLeaderSkill();
    }

    public void SetDisable()
    {
        foreach (UIButtonColor button_color in GetComponentsInChildren<UIButtonColor>())
        {
            button_color.isEnabled = false;
            button_color.SetState(UIButtonColor.State.Disabled, false);
        }
        m_SpriteLeaderSkill.color = Color.grey;
        m_SpriteLeaderChar.color = Color.grey;
        m_SpriteLeaderCharBorder.color = Color.grey;
        m_Collider.enabled = false;
    }

    bool bTooltipShowed = false;
    public void OnShowTooltip(SHTooltip tooltip)
    {
        if (tooltip == null || LeaderCreature == null) return;
        bTooltipShowed = true;
        Tooltip.Instance.ShowTarget(LeaderCreature.TeamSkill.GetTooltip()+ "\n\n" + Localization.Format("Tooltip_LeaderSkillCondition", Localization.Get("Setup_LeaderSkill"), Localization.Get("LeaderSkillCondition_" + UseLeaderSkillType)), tooltip);
    }

    string lock_message;
    public void Lock(string message)
    {
        m_LockLeaderSkill.gameObject.SetActive(true);
        lock_message = message;
    }
}