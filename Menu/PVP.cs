using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketInfo;

public class PVP : MenuBase
{
    static public bool bUpdateInfo = false;
    public GameObject PrefabProfile;
    public GameObject ProfileIndicator;

    public PrefabManager m_DefenseTeamManager;

    public UILabel m_LabelRank;
    public UILabel m_LabelPvpMessage;

    public UILeaderSkill m_LeaderSkill;
    public UIGrid m_GridDefenseTeam;
    public UILabel m_LabelDefensePower;

    public UIGrid m_GridEnemy;
    public PvpItem[] m_Enemies;

    public UILabel m_LabelPvpLimit;
    public UILabel m_LabelPvpDelay;

    public UIToggle m_ToggleRefresh;

    pd_PvpPlayerInfo m_MyPvpInfo;
    List<pd_PvpPlayerInfo> m_EnemiesInfo;
    PlayerProfile m_Profile;
    TeamData m_DefenseTeam = null;
    //TeamData m_PVPTeam = null;
    DateTime m_NextAvailableBattleTime = DateTime.MinValue;

    bool bNeedDefenseTeamSet = false;
    double ShowPopupSetDefenseTeamTime = 0f, ShowPopupSetDefenseTeamTimeGap = 0;
    override public bool Init(MenuParams parms)
    {
        bUpdateInfo = false;

        m_DefenseTeam = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);
        //m_PVPTeam = TeamDataManager.Instance.GetTeam(pe_Team.PVP);
        GetPvpInfo();

        return true;
    }

    private void GetPvpInfo()
    {
        C2G.PVPGetInfo packet = new C2G.PVPGetInfo();
        if (m_DefenseTeam == null || m_MyPvpInfo != null && m_MyPvpInfo.team_power == m_DefenseTeam.Power)
            packet.defense_team_power = 0;
        else
            packet.defense_team_power = m_DefenseTeam.Power;
        Network.GameServer.JsonAsync<C2G.PVPGetInfo, C2G.PVPGetInfoAck>(packet, OnPVPGetInfo);
    }

    override public void UpdateMenu()
    {
        UpdateInfo();
    }

    public override bool Uninit(bool bBack)
    {
        m_DefenseTeamManager.Clear();
        DefenseList.Clear();
        return true;
    }

    void OnDisable()
    {
        m_DefenseTeamManager.Clear();
    }

    void Start()
    {
        //m_BattleDelay = GameConfig.Get<int>("pvp_battle_delay");
    }

    void Update()
    {
        if (bUpdateInfo == true)
        {
            bUpdateInfo = false;

            m_DefenseTeamManager.Clear();

            GetPvpInfo();
            return;
        }
        if (m_NextAvailableBattleTime > Network.Instance.ServerTime)
        {
            TimeSpan span = m_NextAvailableBattleTime - Network.Instance.ServerTime;
            if (span.TotalSeconds < 60)
                m_LabelPvpDelay.text = Localization.Format("Seconds", span.Seconds);
            else
                m_LabelPvpDelay.text = Localization.Format("MinuteSeconds", span.Minutes, span.Seconds);
        }
        else if (m_NextAvailableBattleTime != DateTime.MinValue)
        {
            UpdateInfo();
        }

        if (bNeedDefenseTeamSet && ShowPopupSetDefenseTeamTime < Time.realtimeSinceStartup)
        {
            bNeedDefenseTeamSet = false;
            Popup.Instance.ShowCallback(new PopupCallback.Callback(new Action(OnClickDefenseTeam), null), Localization.Get("PVPDefenseTeamNotSet"));
        }
    }
    void ShowPopupSetDefenseTeam()
    {
        OnClickDefenseTeam();
    }

    int m_AvailableBattleCount = 0;
    DateTime m_LastOffenseTime = DateTime.MinValue;
    //int m_BattleDelay = 0;
    void OnPVPGetInfo(C2G.PVPGetInfo packet, C2G.PVPGetInfoAck ack)
    {
        if (ack.pvp_player_infos == null || ack.pvp_player_infos.Count == 0)
        {
            bNeedDefenseTeamSet = true;
            ShowPopupSetDefenseTeamTime = Time.realtimeSinceStartup + ShowPopupSetDefenseTeamTimeGap;
            return;
        }

        m_LastOffenseTime = ack.last_offense_at;
        //m_AvailableBattleCount = ack.available_daily_battle_count;
        m_MyPvpInfo = ack.pvp_player_infos[0];
        m_EnemiesInfo = ack.pvp_player_infos.GetRange(1, ack.pvp_player_infos.Count - 1);

        if (m_Profile == null)
        {
            m_Profile = NGUITools.AddChild(ProfileIndicator, PrefabProfile).GetComponent<PlayerProfile>();
        }
        m_Profile.UpdateProfile(Network.PlayerInfo.leader_creature, Network.PlayerInfo.nickname, Network.PlayerInfo.player_level);

        UpdateInfo();
    }

    public void ResetAvailableBattleTime()
    {
        m_NextAvailableBattleTime = DateTime.MinValue;
    }

    public void ResetBattleCount()
    {
        m_LastOffenseTime = DateTime.MinValue;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        m_LabelRank.text = Localization.Format("PVPRank", m_MyPvpInfo.rank);
        m_LabelPvpMessage.text = m_MyPvpInfo.message;


        int pvp_daily_battle_count_max = GameConfig.Get<int>("pvp_daily_battle_count_max");
        int pvp_battle_regen_time = GameConfig.Get<int>("pvp_battle_regen_time");
        //DateTime BattleCountMaxTime = Network.Instance.ServerTime.AddMinutes(-pvp_battle_regen_time * pvp_daily_battle_count_max);
        m_AvailableBattleCount = (int)(Math.Min(pvp_daily_battle_count_max, (Network.Instance.ServerTime - m_LastOffenseTime).TotalMinutes / pvp_battle_regen_time));

        if (m_AvailableBattleCount == pvp_daily_battle_count_max)
        {
            m_NextAvailableBattleTime = DateTime.MinValue;
            m_LabelPvpDelay.text = "--:--";
        }
        else
            m_NextAvailableBattleTime = m_LastOffenseTime.AddMinutes((m_AvailableBattleCount + 1) * pvp_battle_regen_time);

        m_LabelPvpLimit.text = Localization.Format("PVPBattleLimit", m_AvailableBattleCount, pvp_daily_battle_count_max);

        m_ToggleRefresh.value = m_AvailableBattleCount > 0;

        InitDefense();
        InitEnemies();
    }

    List<DungeonHero> DefenseList = new List<DungeonHero>();
    void InitDefense()
    {
        if (m_DefenseTeam == null)
        {
            bNeedDefenseTeamSet = true;
            ShowPopupSetDefenseTeamTime = Time.realtimeSinceStartup + ShowPopupSetDefenseTeamTimeGap;
            return;
        }

        m_LeaderSkill.Init(m_DefenseTeam.LeaderCreature, m_DefenseTeam.UseLeaderSkillType);
        //m_DefenseTeamManager.Clear();
        int count = m_DefenseTeam.Creatures.Count;
        for (int i = 0; i < count; ++i)
        {
            if (DefenseList.Count > i)
            {
                DefenseList[i].Init(m_DefenseTeam.Creatures[count - 1 - i].creature, false, false);
            }
            else
            {
                DungeonHero hero_item = m_DefenseTeamManager.GetNewObject<DungeonHero>(m_GridDefenseTeam.transform, Vector3.zero);
                hero_item.Init(m_DefenseTeam.Creatures[count - 1 - i].creature, false, false);
                DefenseList.Add(hero_item);
            }
        }
        for (int j = count; j < DefenseList.Count; ++j)
        {
            DefenseList[j].gameObject.SetActive(false);
        }
        m_GridDefenseTeam.Reposition();

        m_LabelDefensePower.text = Localization.Format("PowerValue", m_MyPvpInfo.team_power);
    }

    void InitEnemies()
    {
        for (int i = 0; i < m_Enemies.Length; ++i)
        {
            if (m_EnemiesInfo.Count > i)
                m_Enemies[i].Init(m_EnemiesInfo[i], OnBattleStart);
            else
                m_Enemies[i].Init(null);
        }
    }

    public void OnClickMessage()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void OnClickRank()
    {
        C2G.PvpRankInfoGet packet = new C2G.PvpRankInfoGet();
        Network.GameServer.JsonAsync<C2G.PvpRankInfoGet, C2G.PvpRankInfoGetAck>(packet, OnPvpRankInfoGet);
    }

    class PVPRankingInfo : PopupRanking.RankingInfo
    {
        public PVPRankingInfo(List<pd_PvpPlayerInfo> rankers)
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

        List<pd_PvpPlayerInfo> rankers;
    }

    void OnPvpRankInfoGet(C2G.PvpRankInfoGet packet, C2G.PvpRankInfoGetAck ack)
    {
        PVPRankingInfo info = new PVPRankingInfo(ack.pvp_rankers);
        info.title = Localization.Get("PVPRankingTitle");
        Popup.Instance.Show(ePopupMode.Ranking, info);
    }

    public void OnClickResult()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void OnClickReward()
    {
        PopupRankingReward.RankingRewardInfo info = new PopupRankingReward.RankingRewardInfo();
        info.title = Localization.Get("PVPRankingRewardTitle");
        info.description = Localization.Get("PVPRankingRewardDescription");
        info.token_type = pe_GoodsType.token_arena;
        info.ranking = m_MyPvpInfo != null ? m_MyPvpInfo.rank : 0;
        info.data = PvpRewardDataManager.Instance.GetList();
        Popup.Instance.Show(ePopupMode.RankingReward, info);
    }

    public void OnClickDefenseTeam()
    {
        m_DefenseTeamManager.Clear();
        MenuParams parm = new MenuParams();
        parm.AddParam("deck_type", "defense");
        parm.AddParam("is_regist", m_MyPvpInfo == null);
        GameMain.Instance.ChangeMenu(GameMenu.PVPDeckInfo, parm);
    }
    public void OnClickRefreshEnemy()
    {
        if (m_AvailableBattleCount > 0)
            GetPvpInfo();
        else
            Popup.Instance.Show(ePopupMode.PVPDelayReset, false);
    }

    pd_PvpPlayerInfo m_SelectedEnemyInfo = null;
    public void OnBattleStart(pd_PvpPlayerInfo info)
    {
        if (m_AvailableBattleCount <= 0)
        {
            Popup.Instance.Show(ePopupMode.PVPDelayReset, false);
            //Tooltip.Instance.ShowMessageKey("PVPNotAvailableLimit");
            return;
        }
        //else if (m_NextAvailableBattleTime > Network.Instance.ServerTime)
        //{
        //    //Tooltip.Instance.ShowMessageKey("PVPNotAvailableTime");
        //    Popup.Instance.Show(ePopupMode.PVPDelayReset, true);
        //    return;
        //}

        m_SelectedEnemyInfo = info;
        C2G.PvpGetBattleInfo packet = new C2G.PvpGetBattleInfo();
        packet.enemy_account_idx = info.account_idx;
        Network.GameServer.JsonAsync<C2G.PvpGetBattleInfo, C2G.PvpGetBattleInfoAck>(packet, OnPvpGetBattleInfoHandler);
    }

    void OnPvpGetBattleInfoHandler(C2G.PvpGetBattleInfo packet, C2G.PvpGetBattleInfoAck ack)
    {
        Network.BattleStageInfo = null;
        Network.PVPBattleInfo = new PVPBattleInfo(m_SelectedEnemyInfo, ack);

        Popup.Instance.Show(ePopupMode.PVPBattleReady);
    }
}

public class PVPBattleInfo
{
    public pd_PvpPlayerInfo enemy_info;
    public TeamData enemy_team_data;

    List<Creature> Creatures = new List<Creature>();

    public PVPBattleInfo(pd_PvpPlayerInfo enemy_info, C2G.PvpGetBattleInfoAck ack)
    {
        this.enemy_info = enemy_info;

        for (int i = 0; i < ack.creatures.Count; ++i)
        {
            List<pd_EquipData> equips = ack.equips.FindAll(e => e.creature_idx == ack.creatures[i].creature_idx);
            pd_EquipData weapon = equips.Find(e => EquipInfoManager.Instance.GetInfoByIdn(e.equip_idn).CategoryInfo.EquipType == SharedData.eEquipType.weapon);
            pd_EquipData armor = equips.Find(e => EquipInfoManager.Instance.GetInfoByIdn(e.equip_idn).CategoryInfo.EquipType == SharedData.eEquipType.armor);
            List<Rune> runes = ack.runes.FindAll(r => r.creature_idx == ack.creatures[i].creature_idx).Select(e => new Rune(e)).ToList();
            Creatures.Add(new Creature(ack.creatures[i], weapon, armor, runes));
        }

        this.enemy_team_data = new TeamData(ack.team_data.team_type, null);
        enemy_team_data.SetCreatures(ack.team_data.creature_infos.Select(c => new TeamCreature(Creatures.Find(lc => lc.Idx == c.team_creature_idx), c.auto_skill_index)).ToList(), false);
        if (ack.team_data.leader_creature_idx > 0)
        {
            Creature leader_creature = Creatures.Find(c => c.Idx == ack.team_data.leader_creature_idx);
            if (leader_creature != null)
                enemy_team_data.SetLeaderCreature(leader_creature, ack.team_data.use_leader_skill_type);
        }
    }
}
