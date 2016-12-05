using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;
using System.Linq;

public class PopupMission : PopupBase
{
    public PrefabManager m_MissionItemPrefabManager;

    public UIGrid m_Grid;
	// Use this for initialization
	void Start ()
    {
	}

    public override void OnFinishedShow()
    {
        m_Grid.gameObject.SetActive(true);
        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
    }

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        InitItem();
    }

    void InitItem()
    {
        QuestManager.Instance.CheckComplete();

        m_Grid.gameObject.SetActive(false);
        foreach (var quest in QuestManager.Instance.Data.Where(q => q.IsShow).OrderByDescending(q => q.IsComplete))
        {
            var mission_item = m_MissionItemPrefabManager.GetNewObject<PopupMissionItem>(m_Grid.transform, Vector3.zero);
            mission_item.Init(quest, OnReward);
        }

        GameMain.Instance.UpdateNotify(false);
    }

    void OnReward()
    {
        m_MissionItemPrefabManager.Clear();
        InitItem();
        OnFinishedShow();
    }
}
