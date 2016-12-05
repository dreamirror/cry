using UnityEngine;
using System.Collections;

public class LeaderSkillSelectItem : MonoBehaviour
{
    public delegate void OnChangedLeaderSkillDelegate(Creature creature);

    public UIToggle m_Toggle;
    public UILabel m_LabelName, m_LabelDesc;
    //public UISprite m_SpriteSkill, m_SpriteCharacter;
    public UILeaderSkill m_LeaderSkill;

    OnChangedLeaderSkillDelegate OnChangedLeaderSkill = null;
    Creature m_Creature = null;
    SkillInfo m_TeamSkill = null;

    //---------------------------------------------------------------------------
    public void Init(Creature creature, bool is_checked, OnChangedLeaderSkillDelegate LeaderSkillChangedDelegate = null)
    {
        m_Toggle.Set(is_checked);

        m_Creature = creature;

        OnChangedLeaderSkill = LeaderSkillChangedDelegate;

        m_TeamSkill = creature.TeamSkill.Info;

        m_LeaderSkill.Init(m_Creature, PacketEnums.pe_UseLeaderSkillType.Manual);
        m_LabelName.text = m_TeamSkill.Name;
        m_LabelDesc.text = m_TeamSkill.Desc;

        gameObject.SetActive(true);
    }
    //---------------------------------------------------------------------------

    public void OnSelectChanged()
    {
        if (m_TeamSkill.IsEnabled == false)
        {
            Tooltip.Instance.ShowMessageKey("NotImplement");
            return;
        }

        m_Toggle.value = !m_Toggle.value;
        if (m_Toggle.value == true && OnChangedLeaderSkill != null)
        {
            OnChangedLeaderSkill(m_Creature);
        }
    }
}
