using UnityEngine;
using System.Collections;


public class SkillItem : MonoBehaviour
{
    public UISprite m_SkillSprite;
    public UILabel m_Level;
    //public SHTooltip m_Tooltip;

    // Use this for initialization
	void Start () {
        //m_Tooltip.OnShowTooltip = OnShowTooltip;
        //m_Tooltip.span_press_time = System.TimeSpan.FromMilliseconds(0);
    }

    Skill m_Skill = null;
    SkillInfo m_SkillInfo = null;

    //---------------------------------------------------------------------------
    public void Init(Skill skill)
    {
        m_Skill = skill;
        m_SkillInfo = skill.Info;
        m_SkillSprite.spriteName = m_SkillInfo.IconID;

        m_Level.gameObject.SetActive(true);
        m_Level.text = Localization.Format("Level", skill.Level);
        gameObject.SetActive(true);
    }

    //---------------------------------------------------------------------------
    public void Init(SkillInfo info)
    {
        m_Skill = null;
        m_SkillInfo = info;
        if (m_SkillSprite.atlas.Contains(info.IconID))
            m_SkillSprite.spriteName = info.IconID;
        else
            m_SkillSprite.spriteName = "passive1";

        m_Level.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    //---------------------------------------------------------------------------
    public void OnShowTooltip(SHTooltip tooltip)
    {
        if (m_Skill != null)
            Tooltip.Instance.ShowTarget(m_Skill.GetTooltip(), tooltip);
        else
        {
            string res = m_SkillInfo.GetTooltip() + "\n\n" + m_SkillInfo.DescTotal(1f, 0);
            Tooltip.Instance.ShowTarget(res.Trim(), tooltip);
        }
    }
}
