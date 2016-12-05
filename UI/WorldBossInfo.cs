using UnityEngine;
using System.Collections;
using System;
using PacketInfo;
using System.Collections.Generic;

public class WorldBossInfo : MenuBase {

    static public pd_WorldBoss Info = null;

    //LEFT FRAME
    public PrefabManager RecommendPrefabManager;
    public UIGrid RecommendGrid;
    public UILabel BestScoreLabel;
    public UILabel CurrentRankLabel;
    public UILabel TryCountLabel;
    public UILabel RemainTimeLabel;

    //RIGHT FRAME
    public UICharacterContainer CharacterContainer;
    public UILabel LabelTitle;

    PacketInfo.pd_EventHottime m_EventInfo = null;
    MapStageDifficulty m_MapStageInfo = null;
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        Init(parms.bBack);

        return true;
    }

    override public void UpdateMenu()
    {
        Init(false);
    }

    void Update()
    {
        var now = Network.Instance.ServerTime;
        if (m_EventInfo == null || m_EventInfo.state != PacketInfo.pe_EventHottimeState.Hottime)
            RemainTimeLabel.text = Localization.Format("WorldBossRemain", "--", "--", "--");
        else
        {
            TimeSpan left_time = m_EventInfo.end_date - now;
            if (left_time.TotalMilliseconds < 0f)
                left_time = TimeSpan.FromSeconds(0);
            RemainTimeLabel.text = Localization.Format("WorldBossRemain", (int)left_time.TotalHours, left_time.Minutes, left_time.Seconds);
        }
        if (IsDraggingCharacter)
            UpdateDragCharacter();
    }

    ////////////////////////////////////////////////////////////////
    void Start()
    {
        //m_TooltipLeaderSkill.OnShowTooltip = OnShowTooltip;
        //m_TooltipLeaderSkill.span_press_time = TimeSpan.FromMilliseconds(0);

        //m_TooltipType.OnShowTooltip = OnShowTooltipType;
        //m_TooltipType.span_press_time = TimeSpan.FromMilliseconds(0);
    }

    public void Init(bool back)
    {
        m_EventInfo = EventHottimeManager.Instance.GetInfoByID("worldboss", true);
        if (m_EventInfo != null && (m_EventInfo.state == PacketInfo.pe_EventHottimeState.Hottime || m_EventInfo.state == PacketInfo.pe_EventHottimeState.WaitingHottime))
        {
            var map_info = MapInfoManager.Instance.GetInfoByIdn(m_EventInfo.Value);
            if (map_info != null)
            {
                m_MapStageInfo = map_info.Stages[0].Difficulty[0];
                var world_boss_info = m_MapStageInfo.Waves[0].Creatures.Find(c => c.CreatureType == eMapCreatureType.WorldBoss);
                if (world_boss_info != null)
                {
                    CharacterContainer.transform.parent.gameObject.SetActive(true);

                    CharacterContainer.Init(AssetManager.GetCharacterAsset(world_boss_info.CreatureInfo.ID, "default"), UICharacterContainer.Mode.UI_Normal, m_EventInfo.state == PacketInfo.pe_EventHottimeState.Hottime ? "idle" : "disabled", true);
                    LabelTitle.text = world_boss_info.CreatureInfo.Name;
                }
            }
        }

        foreach (CreatureInfo hero in m_MapStageInfo.Recommends)
        {
            var item = RecommendPrefabManager.GetNewObject<DungeonHeroRecommend>(RecommendGrid.transform, Vector3.zero);
            item.Init(hero);
        }

        RecommendGrid.Reposition();

        BestScoreLabel.text = Localization.Format("WorldBossBest", "-");
        CurrentRankLabel.text = Localization.Format("WorldBossRank", "-");
        TryCountLabel.text = Localization.Format("WorldBossTry", m_MapStageInfo.MapInfo.TryLimit - MapClearDataManager.Instance.GetMapDailyClearCount(m_MapStageInfo.MapInfo.IDN, PacketEnums.pe_Difficulty.Normal),m_MapStageInfo.MapInfo.TryLimit);

        RefreshInfo();
        Update();
        Network.GameServer.JsonAsync<C2G.GetWorldBossInfo, C2G.GetWorldBossInfoAck>(new C2G.GetWorldBossInfo(), OnGetWorldBossInfo);
    }

    void OnGetWorldBossInfo(C2G.GetWorldBossInfo packet, C2G.GetWorldBossInfoAck ack)
    {
        Info = ack.info;
        RefreshInfo();
    }

    void RefreshInfo()
    {
        if (Info == null)
        {
            BestScoreLabel.text = Localization.Format("WorldBossBest", "-");
            CurrentRankLabel.text = Localization.Format("WorldBossRank", "-");
        }
        else
        {
            BestScoreLabel.text = Localization.Format("WorldBossBest", Info.score);
            CurrentRankLabel.text = Localization.Format("WorldBossRank", Info.rank);
        }
    }

    public void OnClickClose()
    {
        GameMain.Instance.BackMenu();
    }

    public void OnCharacterPress()
    {
        m_FirstTouchPosition = m_TouchPosition = UICamera.lastTouchPosition;
        IsDraggingCharacter = true;
    }

    public void OnCharacterRelease()
    {
        if (m_FirstTouchPosition == UICamera.lastTouchPosition)
        {
            CharacterContainer.PlayRandomAction();
        }
        m_TouchPosition = Vector2.zero;
        IsDraggingCharacter = false;
    }

    bool IsDraggingCharacter = false;
    Vector2 m_TouchPosition = Vector2.zero, m_FirstTouchPosition = Vector2.zero;

    void UpdateDragCharacter()
    {
        Vector2 pos = UICamera.lastTouchPosition;
        float delta = m_TouchPosition.x - pos.x;
        float speed = 0.5f;
        m_TouchPosition = pos;

        CharacterContainer.transform.localRotation *= Quaternion.Euler(0f, delta * speed, 0f);
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_RaidBoss_Title"),Localization.Get("Help_RaidBoss"));
    }

    class WorldBossRankingInfo : PopupRanking.RankingInfo
    {
        public WorldBossRankingInfo(List<pd_WorldBossPlayerInfo> rankers)
        {
            this.rankers = rankers;
        }

        override public void OnCreate(PrefabManager prefab_manager, Transform transform)
        {
            long account_idx = SHSavedData.AccountIdx;
            foreach (var info in rankers)
            {
                var item = prefab_manager.GetNewObject<RankingItem>(transform, Vector3.zero);
                item.Init(info, info.account_idx == account_idx);
            }
        }

        List<pd_WorldBossPlayerInfo> rankers;
    }

    public void OnClickRank()
    {
        Network.GameServer.JsonAsync<C2G.GetWorldBossRanking, C2G.GetWorldBossRankingAck>(new C2G.GetWorldBossRanking(), OnGetWorldBossRanking);
    }

    void OnGetWorldBossRanking(C2G.GetWorldBossRanking packet, C2G.GetWorldBossRankingAck ack)
    {
        WorldBossRankingInfo info = new WorldBossRankingInfo(ack.players);
        info.title = Localization.Get("WorldBossRankingTitle");
        Popup.Instance.Show(ePopupMode.Ranking, info);
    }

    public void OnClickReward()
    {
        PopupRankingReward.RankingRewardInfo info = new PopupRankingReward.RankingRewardInfo();
        info.title = Localization.Get("WorldBossRankingRewardTitle");
        info.description = Localization.Get("WorldBossRankingRewardDescription");
        info.token_type = pe_GoodsType.token_raid;
        info.ranking = Info != null ? Info.rank : 0;
        info.data = WorldBossRewardDataManager.Instance.GetList();
        Popup.Instance.Show(ePopupMode.RankingReward, info);
    }

    public void OnClickWorldBossShop()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void OnClickBossEnter()
    {
        var event_info = EventHottimeManager.Instance.GetInfoByID("worldboss");
//#if !(SH_DEV || UNITY_EDITOR)
//        DateTime now = Network.Instance.ServerTime;
//        if (event_info == null || event_info.end_date < now)
//        {
//            Tooltip.Instance.ShowMessageKey("WorldBossNotHottime");
//            return;
//        }
//#endif
        MenuParams parms = new MenuParams();
        parms.AddParam<MapStageDifficulty>(m_MapStageInfo);
        GameMain.Instance.ChangeMenu(GameMenu.DungeonInfo, parms);
    }

    public void OnClickBossInfo()
    {
        Popup.Instance.Show(ePopupMode.BossDetail, m_MapStageInfo);
    }

}
