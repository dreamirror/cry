using UnityEngine;
using System.Collections;
using LinqTools;

public class PopupTraining : PopupBase {

    public UIScrollView ScrollViewStages;
    public UIScrollView ScrollViewRewards;

    public UIGrid GridStages;
    public UIGrid GridRewards;

    public PrefabManager StagePrefab;
    public PrefabManager RewardItemPrefab;

    public UILabel LabelDungeonInfo;
    public UILabel LabelTryCount;
    public UILabel LabelTitle;

    MapInfo map_info;
    MapStageDifficulty m_StageInfo;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        map_info = MapInfoManager.Instance.GetInfoByID(parms[0].ToString());
        if (map_info.Stages.Count == 0)
        {
            OnClose();
            return;
        }
        LabelTryCount.text = Localization.Format("LeftTryCount", (map_info.TryLimit - MapClearDataManager.Instance.GetMapDailyClearCount(map_info.IDN, PacketEnums.pe_Difficulty.Normal)), map_info.TryLimit);

        PacketInfo.pd_MapClearData last_updated_stage = MapClearDataManager.Instance.GetLastStage(map_info.IDN, PacketEnums.pe_Difficulty.Normal);

        PopupTrainingItem last_stage = null;
        foreach (MapStageDifficulty stage in map_info.Stages.Select(e => e.Difficulty[0]).ToList())
        {
            var stage_btn = StagePrefab.GetNewObject<PopupTrainingItem>(GridStages.transform, Vector3.zero);
            stage_btn.Init(stage, SetStage);
            if (last_updated_stage == null && last_stage == null)
                last_stage = stage_btn;
            else if (last_updated_stage != null && last_updated_stage.stage_index == stage.StageIndex)
                last_stage = stage_btn;
        }
        last_stage.Select();
        GridStages.Reposition();
        ScrollViewStages.ResetPosition();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();

        SetStage(m_StageInfo);
    }
    //void OnStageSelect(MapStageDifficulty stage_info)
    //{
    //    SetStage(stage_info);
    //}
    public override void OnFinishedHide()
    {
        Network.NewStageInfo = null;
        base.OnFinishedHide();
    }
    public void OnClickBattleEnter()
    {
        if (MapClearDataManager.Instance.GetMapDailyClearCount(map_info.IDN, PacketEnums.pe_Difficulty.Normal) >= map_info.TryLimit)
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

    public void SetStage(MapStageDifficulty stage_info)
    {
        m_StageInfo = stage_info;

        LabelTitle.text = m_StageInfo.ShowName;
        LabelDungeonInfo.text = m_StageInfo.Description;

        RewardItemPrefab.Destroy();

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

    bool CheckCondition()
    {
        if (m_StageInfo.Condition != null)
        {
            MapCondition condition = m_StageInfo.Condition.CheckCondition();
            if (condition != null)
            {
                Tooltip.Instance.ShowMessage(condition.Condition);
                return false;
            }
        }
        return true;
    }

    public override void OnClose()
    {   
        base.OnClose();
    }

}
