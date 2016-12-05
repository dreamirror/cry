using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using PacketInfo;
using System;
using LinqTools;
using PacketEnums;

public enum eAdventureState
{
    IDLE,
    PROGRESS,
    COMPLETE,
}
public class PopupAdventure : PopupBase
{
    [FormerlySerializedAs("ScrollViewStages")]
    public UIScrollView ScrollVieAdventure;
    public UIScrollView ScrollViewRewards;
    public UIScrollView ScrollViewRewardsComplete;

    [FormerlySerializedAs("GridStages")]
    public UIGrid GridAdventure;
    public UIGrid GridRewards;
    public UIGrid GridHeroes;
    public UIGrid GridRewardsComplete;
    public PrefabManager AdventureItemPrefab;
    public PrefabManager RewardItemPrefab;
    [FormerlySerializedAs("EnchantHeroPrefab")]
    public PrefabManager DungeonHeroPrefab;

    public UIToggle ToggleStage;

    public UILabel LabelDungeonInfo;
    public UILabel LabelCondition;
    public UILabel LabelTimeLimit;
    public UILabel LabelTimeRemain;
    public UILabel LabelTitle;

    public UIButton m_BtnComplete;
    public UIButton m_BtnInstant;
    public UILabel m_LabelCost;
    public UISprite m_SpriteCost;

    List<AdventureInfo> m_AdventureInfos;
    AdventureInfo m_SelectedAdventure;
    eAdventureState m_State = eAdventureState.IDLE;
    DateTime m_EndTime = DateTime.MinValue;
    void Update()
    {
        if (m_EndTime != DateTime.MinValue)
        {
            if (m_EndTime > Network.Instance.ServerTime)
            {
                var remain = m_EndTime - Network.Instance.ServerTime;
                if (remain.TotalSeconds < 60)
                    LabelTimeRemain.text = Localization.Format("Seconds", (int)remain.TotalSeconds);
                else if (remain.TotalMinutes < 60)
                    LabelTimeRemain.text = Localization.Format("MinuteSeconds", (int)remain.TotalMinutes, remain.Seconds);
                else
                    LabelTimeRemain.text = Localization.Format("HourMinute", (int)remain.TotalHours, remain.Minutes);

                long cost = (long)Math.Ceiling(remain.TotalMinutes / AdventureInfoManager.Instance.InstantCompletePeriod) * AdventureInfoManager.Instance.Price.goods_value;
                m_LabelCost.text = Localization.Format("GoodsFormat", cost);
            }
            else
            {
                m_EndTime = DateTime.MinValue;
                SetAdventure(m_SelectedAdventure);
            }
        }
    }
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_AdventureInfos = AdventureInfoManager.Instance.GetList();

        PopupAdventureItem select_adventure = null;
        foreach (var adventure in m_AdventureInfos)
        {
            var adventure_btn = AdventureItemPrefab.GetNewObject<PopupAdventureItem>(GridAdventure.transform, Vector3.zero);

            adventure_btn.Init(adventure, SetAdventure);
            if (is_new && select_adventure == null)
            {
                select_adventure = adventure_btn;
                m_SelectedAdventure = adventure;
            }
            else if (adventure == m_SelectedAdventure)
                select_adventure = adventure_btn;
        }

        select_adventure.Select();

        GridAdventure.Reposition();
        ScrollVieAdventure.ResetPosition();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();

        SetAdventure(m_SelectedAdventure);
        //GetComponent<Collider2D>().enabled = false;
        GetComponent<Collider2D>().enabled = true;
    }
    public override void OnFinishedHide()
    {
        base.OnFinishedHide();
        AdventureItemPrefab.Clear();
        RewardItemPrefab.Clear();
        DungeonHeroPrefab.Clear();
    }
    public void SetAdventure(AdventureInfo map_info)
    {
        m_SelectedAdventure = map_info;

        var detail = AdventureInfoManager.Instance.GetInfo(m_SelectedAdventure.IDN);
        m_State = eAdventureState.IDLE;
        m_EndTime = DateTime.MinValue;
        if (detail != null && detail.is_rewarded == false)
        {
            if (Network.Instance.ServerTime > detail.end_at)
            {
                m_State = eAdventureState.COMPLETE;
            }
            else if (detail.is_begin && Network.Instance.ServerTime < detail.end_at)
            {
                m_EndTime = detail.end_at;
                m_State = eAdventureState.PROGRESS;
            }
        }
        DungeonHeroPrefab.Clear();
        RewardItemPrefab.Clear();
        ToggleStage.value = m_State == eAdventureState.IDLE;
        if (m_State == eAdventureState.IDLE)
        {
            LabelTitle.text = m_SelectedAdventure.ShowName;
            LabelDungeonInfo.text = m_SelectedAdventure.Description;
            LabelCondition.text = m_SelectedAdventure.ShowCondition;
            foreach (var loot_group in m_SelectedAdventure.DropInfo[0].groups)
            {
                if (string.IsNullOrEmpty(loot_group.show_id) == true)
                    continue;
                var reward_item = RewardItemPrefab.GetNewObject<RewardItem>(GridRewards.transform, Vector3.zero);
                reward_item.InitReward(loot_group.show_id, loot_group.show_value);
            }
            GridRewards.Reposition();
            ScrollViewRewards.ResetPosition();
            LabelTimeLimit.text = Localization.Format("HourMinute", m_SelectedAdventure.Period / 60, m_SelectedAdventure.Period % 60);
        }
        else if (detail != null)
        {
            var team_data = TeamDataManager.Instance.GetTeam((pe_Team)m_SelectedAdventure.IDN);
            foreach (var hero in team_data.Creatures)
            {
                var item = DungeonHeroPrefab.GetNewObject<DungeonHero>(GridHeroes.transform, Vector3.zero);
                item.Init(hero.creature, false, false);
            }
            GridHeroes.Reposition();
            foreach (var reward in detail.rewards)
            {
                var reward_item = RewardItemPrefab.GetNewObject<RewardItem>(GridRewardsComplete.transform, Vector3.zero);
                if (CreatureInfoManager.Instance.ContainsIdn(reward.reward_idn) == true)
                    reward_item.InitCreature(CreatureInfoManager.Instance.GetInfoByIdn(reward.reward_idn), (short)reward.reward_value);
                else
                    reward_item.InitReward(reward.reward_idn, reward.reward_value);
            }
            GridRewardsComplete.Reposition();
            ScrollViewRewardsComplete.ResetPosition();
            if (m_State == eAdventureState.COMPLETE)
            {
                LabelTimeRemain.text = Localization.Get("RemainComplete");
                m_BtnComplete.gameObject.SetActive(true);
                m_BtnInstant.gameObject.SetActive(false);
            }
            else
            {
                var remain = m_EndTime - Network.Instance.ServerTime;
                long cost = (long)Math.Ceiling(remain.TotalMinutes / AdventureInfoManager.Instance.InstantCompletePeriod) * AdventureInfoManager.Instance.Price.goods_value;
                m_LabelCost.text = Localization.Format("GoodsFormat", cost);
                m_SpriteCost.spriteName = AdventureInfoManager.Instance.Price.goods_type.ToString();
                m_BtnComplete.gameObject.SetActive(false);
                m_BtnInstant.gameObject.SetActive(true);
            }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public void OnClickReady()
    {
        var condition = m_SelectedAdventure.CheckCondition();
        if (condition != null)
        {
            Tooltip.Instance.ShowMessage(condition.Condition);
            return;
        }
        Popup.Instance.Show(ePopupMode.AdventureReady, m_SelectedAdventure);
    }

    public void OnClickComplete()
    {
        OnConfirmComplete(true);
    }
    public void OnclickInstnt()
    {
        var remain = m_EndTime - Network.Instance.ServerTime;
        long cost = (long)Math.Ceiling(remain.TotalMinutes / AdventureInfoManager.Instance.InstantCompletePeriod) * AdventureInfoManager.Instance.Price.goods_value;
        if (Network.Instance.CheckGoods(AdventureInfoManager.Instance.Price.goods_type, cost) == false)
            return;
        Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnConfirmComplete), "AdventureInstantComplete");
    }
    void OnConfirmComplete(bool is_confirm)
    {
        if (is_confirm)
        {
            var detail = AdventureInfoManager.Instance.GetInfo(m_SelectedAdventure.IDN);
            C2G.AdventureGetReward packet = new C2G.AdventureGetReward();
            packet.map_id = m_SelectedAdventure.ID;
            packet.rewards = detail.rewards;
            packet.end_time = detail.end_at;
            Network.GameServer.JsonAsync<C2G.AdventureGetReward, C2G.AdventureGetRewardAck>(packet, OnAdventureGetReward);

            return;
        }
    }

    void OnAdventureGetReward(C2G.AdventureGetReward packet, C2G.AdventureGetRewardAck ack)
    {
        var team_data = TeamDataManager.Instance.GetTeam((pe_Team)m_SelectedAdventure.IDN);
        team_data.SetCompleteAdventure();

        Network.PlayerInfo.UseGoods(ack.use_goods);
        Network.Instance.ProcessReward3Ack(ack.reward_ack);
        var detail = AdventureInfoManager.Instance.GetInfo(m_SelectedAdventure.IDN);
        Popup.Instance.Show(ePopupMode.Reward, detail.rewards.Select(r => new RewardBase(r.reward_idn, r.reward_value)).ToList(), Localization.Get("AdventureReward"), Localization.Get("GetThisRewards"), ack.reward_ack);
        detail.is_rewarded = true;
        GameMain.Instance.UpdatePlayerInfo();
        SetAdventure(m_SelectedAdventure);
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_Adventure_Title"), Localization.Get("Help_Adventure"));
    }
}