using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TooltipCharacter : TooltipBase
{
    public UICharacterContainer CharacterContainer;

    public UILabel m_CreatureTitle;
    public UIGrid m_StarGrid;
    public UIToggleSprite[] m_Stars;
    public UISprite m_TeamSkill;    
    public SkillItem[] m_ActiveSkills, m_PassiveSkills;


    public override void Init(params object[] parms)
    {
        H2C.NotifyLootCreature creature = parms[0] as H2C.NotifyLootCreature;
        InitCharacter(creature);
    }

    CreatureInfo m_CreatureInfo = null;

    public void InitCharacter(H2C.NotifyLootCreature packet)
    {
        m_CreatureInfo = CreatureInfoManager.Instance.GetInfoByIdn(packet.creature_idn);
        
        CharacterContainer.Init(AssetManager.GetCharacterAsset(m_CreatureInfo.ID, m_CreatureInfo.GetSkinName(packet.skin_index)), UICharacterContainer.Mode.UI_Normal);
        CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);

        if (m_CreatureInfo.TeamSkill != null)
        {
            if (m_TeamSkill.atlas.Contains(m_CreatureInfo.TeamSkill.ID) == true)
                m_TeamSkill.spriteName = m_CreatureInfo.TeamSkill.ID;
            else
                m_TeamSkill.spriteName = "skill_default";
            m_TeamSkill.gameObject.transform.parent.parent.gameObject.SetActive(true);
        }
        else
            m_TeamSkill.gameObject.transform.parent.parent.gameObject.SetActive(false);

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].SetSpriteActive(packet.creature_grade > i);
            m_Stars[i].gameObject.SetActive(packet.creature_grade > i);
        }
        
        m_CreatureTitle.text = m_CreatureInfo.Name;

        List<SkillInfo> active_skills = m_CreatureInfo.Skills.Where(s => s.Type == eSkillType.active && s.ActionName.Equals("attack") == false).ToList();
        for (int i = 0; i < m_ActiveSkills.Length; ++i)
        {
            bool active = i < active_skills.Count;
            m_ActiveSkills[i].gameObject.SetActive(active);
            if (active)
                m_ActiveSkills[i].Init(active_skills[i]);
        }

        List<SkillInfo> passive_skills = m_CreatureInfo.Skills.Where(s => s.Type == eSkillType.passive).ToList();
        for (int i = 0; i < m_PassiveSkills.Length; ++i)
        {
            bool active = i < passive_skills.Count;
            m_PassiveSkills[i].gameObject.SetActive(active);
            if (active)
                m_PassiveSkills[i].Init(passive_skills[i]);
        }

        m_StarGrid.Reposition();
    }

    public void OnLeaderSkillShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_CreatureInfo.TeamSkill.GetTooltip(), tooltip);
    }

    public void Close()
    {
        OnFinished();
    }
}
