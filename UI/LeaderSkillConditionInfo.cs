using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketEnums;

public class LeaderSkillConditionInfo : MonoBehaviour
{
    public UILabel m_PopupLabel;
    public UIToggle m_PopupState;
    public UISprite m_BG;
    public UIGrid m_Grid;

    public delegate void OnChangedLeaderSkillConditionDelegate(pe_UseLeaderSkillType condition);
    OnChangedLeaderSkillConditionDelegate m_OnChangedLeaderSkillCondition;

    public pe_UseLeaderSkillType Condition = pe_UseLeaderSkillType.Manual;

    void UnlockAll()
    {
        for (int i = 0; i < m_Grid.transform.childCount; ++i)
            m_Grid.transform.GetChild(i).FindChild("lock").gameObject.SetActive(false);
    }

    void SetLock(pe_UseLeaderSkillType condition)
    {
        m_Grid.transform.FindChild(condition.ToString()).FindChild("lock").gameObject.SetActive(true);
    }

    public void Init(pe_UseLeaderSkillType condition, OnChangedLeaderSkillConditionDelegate callback, pe_Team team_type)
    {
        m_OnChangedLeaderSkillCondition = callback;
        SetCondition(condition, false);

        UnlockAll();

        switch (team_type)
        {
            case pe_Team.PVP:
            case pe_Team.PVP_Defense:
                SetLock(pe_UseLeaderSkillType.Manual);
                SetLock(pe_UseLeaderSkillType.LastWave);
                break;

            case pe_Team.Boss:
                SetLock(pe_UseLeaderSkillType.LastWave);
                break;
        }

    }

    public void OnClickSelect(GameObject obj)
    {
        SetCondition((pe_UseLeaderSkillType)Enum.Parse(typeof(pe_UseLeaderSkillType), obj.name), true);
    }

    void SetCondition(pe_UseLeaderSkillType condition, bool call_callback)
    {
        m_PopupState.value = false;

        Condition = condition;
        m_PopupLabel.text = Localization.Get("LeaderSkillCondition_" + Condition);

        if (call_callback)
            m_OnChangedLeaderSkillCondition(Condition);
    }
}
