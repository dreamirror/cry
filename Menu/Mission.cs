using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;
using System.Linq;
using SharedData;

public enum eMissionState
{
    none,
    Daily,
    Weekly,
    Achievement,
}
public class Mission : MenuBase
{
    public PrefabManager m_MissionItemPrefabManager;

    public UIGrid m_Grid;

    public GameObject m_NotifyDaily;
    public GameObject m_NotifyWeekly;
    public GameObject m_NotifyAchievement;

    public UILabel m_LabelEmpty;

    public UIToggle m_toggleDaily;
    public UIToggle m_toggleWeekly;
    public UIToggle m_toggleAchievement;

    eMissionState m_State = eMissionState.Daily;
    override public bool Init(MenuParams parms)
    {
        UpdateNotify(eMissionState.none);

        if (m_NotifyDaily.gameObject.activeSelf == false)
        {
            if (m_toggleWeekly.gameObject.activeSelf == false || m_NotifyWeekly.gameObject.activeSelf == false)
            {
                if (m_NotifyAchievement.gameObject.activeSelf == true)
                {
                    m_toggleAchievement.value = true;
                    OnClickAchievement();
                }
                else
                {
                    m_toggleDaily.value = true;
                    OnClickDaily();
                }
            }
            else
            {
                m_toggleWeekly.value = true;
                OnClickWeekly();
            }
        }
        else
        {
            m_toggleDaily.value = true;
            OnClickDaily();
        }

        return true;
    }

    override public void UpdateMenu()
    {
        UpdateNotify(m_State);
        InitItem();
    }

    // Use this for initialization
    void Start ()
    {
	}

    void UpdateNotify(eMissionState state)
    {
        QuestManager.Instance.CheckComplete();
        switch (state)
        {
            case eMissionState.none:
                m_NotifyDaily.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Daily));
                m_NotifyWeekly.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Weekly));
                m_NotifyAchievement.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Achievement));
                break;
            case eMissionState.Daily:
                m_NotifyDaily.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Daily));
                break;
            case eMissionState.Weekly:
                m_NotifyWeekly.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Weekly));
                break;
            case eMissionState.Achievement:
                m_NotifyAchievement.SetActive(QuestManager.Instance.Data.Exists(q => q.IsComplete && q.IsRewarded == false && q.Info.Type == eQuestType.Achievement));
                break;
        }
    }
    void InitItem()
    {
        List<Quest> list = null;
        switch (m_State)
        {
            case eMissionState.Daily:
                list = QuestManager.Instance.Data.Where(q => q.IsShow && q.Info.Type == eQuestType.Daily).OrderByDescending(q => q.IsComplete).ToList();
                break;
            case eMissionState.Weekly:
                list = QuestManager.Instance.Data.Where(q => q.IsShow && q.Info.Type == eQuestType.Weekly).OrderByDescending(q => q.IsComplete).ToList();
                break;
            case eMissionState.Achievement:
                list = QuestManager.Instance.Data.Where(q => q.IsShow && q.Info.Type == eQuestType.Achievement).OrderByDescending(q => q.IsComplete).ToList();
                break;
        }
        m_Grid.gameObject.SetActive(false);
        m_MissionItemPrefabManager.Clear();
        foreach (var quest in list)
        {
            var mission_item = m_MissionItemPrefabManager.GetNewObject<PopupMissionItem>(m_Grid.transform, Vector3.zero);
            mission_item.Init(quest, OnReward);
        }
        m_Grid.gameObject.SetActive(true);

        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();

        GameMain.Instance.UpdateNotify(false);

        m_LabelEmpty.gameObject.SetActive(list.Count == 0);
    }
    public void OnClickDaily()
    {
        m_State = eMissionState.Daily;
        InitItem();
    }
    public void OnClickWeekly()
    {
        m_State = eMissionState.Weekly;
        InitItem();
    }
    public void OnClickAchievement()
    {
        m_State = eMissionState.Achievement;
        InitItem();
    }
    public void OnValueChanged(UIToggle toggle)
    {
    }

    void OnReward()
    {
        m_MissionItemPrefabManager.Clear();
        UpdateNotify(m_State);
        InitItem();
    }

    static public void MissionMove(Quest quest)
    {
        QuestMoveBase move = quest.Info.Move;
        switch (move.MoveType)
        {
            case eQuestMove.Menu:
                //GameMain.Instance.StackPopup();
                {
                    QuestMoveMenu menu = move as QuestMoveMenu;
                    if (menu.menu == GameMenu.HeroesInfo)
                        GameMain.MoveShortCut(menu.menu);
                    else
                    {
                        string value2 = (move as QuestMoveMenu).value2;
                        if (string.IsNullOrEmpty(value2) == true)
                            GameMain.Instance.ChangeMenu((move as QuestMoveMenu).menu);
                        else
                        {
                            eDifficult difficulty = (move as QuestMoveMenu).difficulty;
                            MenuParams parm = new MenuParams();
                            parm.AddParam("menu_parm_1", value2);
                            parm.AddParam("menu_parm_2", difficulty.ToString());
                            GameMain.Instance.ChangeMenu((move as QuestMoveMenu).menu, parm);
                        }
                    }
                }
                break;

            case eQuestMove.Popup:
                Popup.Instance.Show((move as QuestMovePopup).popup);
                break;
        }
    }
}
