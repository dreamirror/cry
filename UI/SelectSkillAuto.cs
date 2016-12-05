using UnityEngine;
using System.Collections;

public class SelectSkillAuto : MonoBehaviour
{
    public UIToggle []m_ToggleSkill;
    public UISprite[] m_Skill;
    public GameObject[] m_SkillEmpty;
    public UISprite m_Character;
    public SHTooltip[] m_SkillTooltip;

    System.Action<Creature, int> OnSelectSkillCallback = null;
    TeamCreature m_TeamCreature;
    Creature m_Creature;

    public void Init(TeamCreature team_creature, System.Action<Creature, int> callback = null)
    {
        gameObject.SetActive(false);
        m_TeamCreature = team_creature;
        m_Creature = m_TeamCreature.creature;
        OnSelectSkillCallback = callback;

        string sprite_name = string.Format("cs_{0}", m_Creature.Info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_Character.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_Character.spriteName = new_sprite_name;

        if (m_Creature.Skills.Count > 1 && m_Creature.Skills[1].Info.Type == eSkillType.active)
        {
            m_Skill[0].spriteName = m_Creature.Skills[1].Info.IconID;
            m_SkillEmpty[0].SetActive(true);
        }
        else
        {
            m_SkillEmpty[0].SetActive(false);
            m_SkillEmpty[1].SetActive(false);
        }
        if (m_Creature.Skills.Count > 2 && m_Creature.Skills[2].Info.Type == eSkillType.active)
        {
            m_Skill[1].spriteName = m_Creature.Skills[2].Info.IconID;
            m_SkillEmpty[1].SetActive(true);
        }
        else
        {
            m_SkillEmpty[1].SetActive(false);
        }

        m_ToggleSkill[0].group = m_Creature.Info.IDN + 1000;
        m_ToggleSkill[1].group = m_Creature.Info.IDN + 1000;
        m_ToggleSkill[2].group = m_Creature.Info.IDN + 1000;

        if (m_TeamCreature.auto_skill_index != -1)
            m_ToggleSkill[m_TeamCreature.auto_skill_index].value = true;
        gameObject.SetActive(true);
    }

    public void SelectSkill(int index)
    {
        if (index >= m_Creature.Skills.Count || m_Creature.Skills[index].Info.Type != eSkillType.active || m_ToggleSkill[index].value == true) return;

        if (index > 0 && m_SkillTooltip[index-1].Showed == true) return;

        m_ToggleSkill[index].value = true;
        m_TeamCreature.auto_skill_index = (short)index;

        if (OnSelectSkillCallback != null)
            OnSelectSkillCallback(m_Creature, index);
    }
    public void OnClickSkill_0() { SelectSkill(0); }
    public void OnClickSkill_1() { SelectSkill(1); }
    public void OnClickSkill_2() { SelectSkill(2); }

    void ShowTooltip(int index, SHTooltip target)
    {
        Tooltip.Instance.ShowTarget(m_Creature.Skills[index].Info.GetTooltip(), target);
    }
    public void ShowTooltipSkill_1(SHTooltip target) { ShowTooltip(1, target); }
    public void ShowTooltipSkill_2(SHTooltip target) { ShowTooltip(2, target); }

}
