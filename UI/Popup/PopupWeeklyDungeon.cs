using UnityEngine;
using System.Collections;
using LinqTools;
using System.Collections.Generic;

public class PopupWeeklyDungeon : PopupBase {

    public UIScrollView ScrollViewStages;
    public UIScrollView ScrollViewRewards;

    public UIGrid GridStages;
    public UIGrid GridRewards;

    public PrefabManager WeeklyPrefab;
    public PrefabManager RewardItemPrefab;

    public UILabel LabelDungeonInfo;
    public UILabel LabelTryCount;
    public UILabel LabelTitle;

    public UIToggle[] m_ToggleDifficulty;
    public UISprite[] m_ToggleLock;

    List<MapInfo> map_infos;
    MapInfo m_SelectedMap = null;
    MapStageDifficulty m_StageInfo = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        map_infos = MapInfoManager.Instance.GetWeeklyDungeons();

        PopupWeeklyItem select_map = null;
        foreach (var map_info in map_infos)
        {
            var stage_btn = WeeklyPrefab.GetNewObject<PopupWeeklyItem>(GridStages.transform, Vector3.zero);
            stage_btn.Init(map_info, SetMap);
            if (is_new)
            {
                if (select_map == null && stage_btn.IsLock == false)
                    select_map = stage_btn;
            }
            else if (map_info == m_SelectedMap)
                select_map = stage_btn;
        }

        select_map.Select();

        GridStages.Reposition();
        ScrollViewStages.ResetPosition();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();

        SetMap(m_SelectedMap);
    }

    public void OnClickBattleEnter()
    {
        if (MapClearDataManager.Instance.GetMapDailyClearCount(m_SelectedMap.IDN) >= m_SelectedMap.TryLimit)
        {
            Tooltip.Instance.ShowMessageKey("NotEnoughTryCount");
            return;
        }

        if (CheckCondition() == false)
            return;

        if (m_StageInfo.Waves.Count > 0)
        {
            GameMain.Instance.StackPopup();

            MenuParams parms = new MenuParams();
            parms.AddParam<MapStageDifficulty>(m_StageInfo);
            GameMain.Instance.ChangeMenu(GameMenu.DungeonInfo, parms);
        }
        else
            Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void SetMap(MapInfo map_info)
    {
        m_SelectedMap = map_info;
        var last_stage = MapClearDataManager.Instance.GetLastDifficulty(map_info.IDN, 0);

        var map_condition = m_SelectedMap.CheckCondition();

        int select_stage_index = 0;
        for (int i = 0; i < m_ToggleLock.Length; ++i)
        {
            var stage = m_SelectedMap.Stages[0].Difficulty[i];
            var condition = map_condition == null ? stage.CheckCondition : map_condition;
            m_ToggleLock[i].gameObject.SetActive(condition != null);
            if (last_stage != null && stage.Difficulty == last_stage.difficulty)
            {
                select_stage_index = i;
            }

            m_ToggleDifficulty[i].Set(false);
        }

        if (m_ToggleDifficulty[select_stage_index].value == false)
            m_ToggleDifficulty[select_stage_index].value = true;
        else
            SetDifficulty(select_stage_index);

        LabelTryCount.text = Localization.Format("LeftTryCount", (m_SelectedMap.TryLimit - MapClearDataManager.Instance.GetMapDailyClearCount(m_SelectedMap.IDN)), m_SelectedMap.TryLimit);
    }

    void SetDifficulty(int index)
    {
        m_StageInfo = m_SelectedMap.Stages[0].Difficulty[index];
        LabelTitle.text = m_StageInfo.ShowName;
        LabelDungeonInfo.text = m_SelectedMap.Description + "\n";
        LabelDungeonInfo.text += Localization.Get("WeeklyAvailable");
        foreach (var tag in m_SelectedMap.AvailableTags)
        {
            LabelDungeonInfo.text += string.Format("[url={0}]{1}[/url] ","Tag_"+tag, Localization.Get("Tag_"+tag));
        } 
        RewardItemPrefab.Clear();

        foreach (var reward in m_StageInfo.DropItems)
        {
            if (reward.IsShow == false)
                continue;
            var reward_item = RewardItemPrefab.GetNewObject<RewardItem>(GridRewards.transform, Vector3.zero);
            reward_item.InitReward(reward);
        }

        GridRewards.Reposition();
        ScrollViewRewards.ResetPosition();
    }
    public void SetDifficulty1(UIToggle toggle) { if (toggle.value == true) SetDifficulty(0); }
    public void SetDifficulty2(UIToggle toggle) { if (toggle.value == true) SetDifficulty(1); }
    public void SetDifficulty3(UIToggle toggle) { if (toggle.value == true) SetDifficulty(2); }
    public void SetDifficulty4(UIToggle toggle) { if (toggle.value == true) SetDifficulty(3); }
    public void SetDifficulty5(UIToggle toggle) { if (toggle.value == true) SetDifficulty(4); }

    bool CheckCondition()
    {
        MapCondition condition = m_SelectedMap.CheckCondition();
        if (condition != null)
        {
            Tooltip.Instance.ShowMessage(condition.Condition);
            return false;
        }
        condition = m_StageInfo.CheckCondition;
        if (condition != null)
        {
            Tooltip.Instance.ShowMessage(condition.Condition);
            return false;
        }

        return true;
    }

    public override void OnClose()
    {   
        base.OnClose();
    }
}
