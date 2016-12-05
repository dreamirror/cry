using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;
using System.Linq;

public class PopupLeaderSkillSelect : PopupBase
{
    public GameObject LeaderSkillSelectItemPrefab;
    public UIGrid m_Grid;

    public LeaderSkillConditionInfo m_LeaderSkillCondition;
    LeaderSkillSelectItem.OnChangedLeaderSkillDelegate m_OnChangedLeaderSkillChanged;
    TeamData m_TeamData;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        m_TeamData = (TeamData)parms[0];
        m_OnChangedLeaderSkillChanged = parms[1] as LeaderSkillSelectItem.OnChangedLeaderSkillDelegate;

        m_LeaderSkillCondition.Init(m_TeamData.UseLeaderSkillType, parms[2] as LeaderSkillConditionInfo.OnChangedLeaderSkillConditionDelegate, m_TeamData.TeamType);
        InitItem();
    }

    void InitItem()
    {
        List<Transform> childs = m_Grid.GetChildList();

        var creatures = m_TeamData.Creatures.Where(e => e.creature.TeamSkill != null).OrderByDescending(c => c.creature.TeamSkill.Info.Type).ToList();
        int loop_count = Math.Max(childs.Count, creatures.Count);
        for (int i = 0; i < loop_count; ++i)
        {
            if (i < creatures.Count)
            {
                if (i >= childs.Count)
                {
                    GameObject obj = NGUITools.AddChild(m_Grid.gameObject, LeaderSkillSelectItemPrefab);
                    childs.Add(obj.transform);
                }

                Transform transform = childs[i];
                Creature creature = creatures[i].creature;

                transform.GetComponent<LeaderSkillSelectItem>().Init(creature, m_TeamData.LeaderCreature == creature, OnChangedLeaderSkillChanged);
            }
            else
            {
                Transform transform = childs[i];
                NGUITools.SetActive(transform.gameObject, false);
            }
        }

        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();

    }

    public void OnChangedLeaderSkillChanged(Creature creature)
    {
        if (m_OnChangedLeaderSkillChanged != null)
        {
            m_OnChangedLeaderSkillChanged(creature);
            OnClose();
        }
    }

    public void OnHelpCondition()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_UseLeaderSkillType_Title"), Localization.Get("Help_UseLeaderSkillType"));
    }

    public void OnHelp()
    {

    }
}
