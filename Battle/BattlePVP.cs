using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

public class BattlePVP : Battle
{
    static new public BattlePVP Instance { get; protected set; }

    public GameObject m_PvpBattleUI;
    public UILabel PvpTime;
    public UITweener PVPTimeTween;
    public PlayerProfile m_ProfileMine;
    public PlayerProfile m_ProfileEnemy;

    public GameObject m_btnWin;
    override protected void Awake()
    {
        base.Awake();
        BattlePVP.Instance = this;
    }

    protected override void Start()
    {
        m_TeamData = TeamDataManager.Instance.GetTeam(pe_Team.PVP);
        m_UILeaderSkill.Lock(Localization.Get("PVPOnlyAutoBattle"));
        
        base.Start();

#if SH_DEV
        m_btnWin.SetActive(true);
#else
        m_btnWin.SetActive(Debug.isDebugBuild);
#endif

        m_ProfileMine.UpdateProfile(Network.PlayerInfo.leader_creature, Network.PlayerInfo.nickname, Network.PlayerInfo.player_level);
        m_ProfileEnemy.UpdateProfile(Network.PVPBattleInfo.enemy_info.leader_creature, Network.PVPBattleInfo.enemy_info.nickname, Network.PVPBattleInfo.enemy_info.player_level);

        m_BG.material.mainTexture = AssetManager.LoadBG("000_pvp");
        enemies = new List<ICreature>();
        for (int i = 0; i < Network.PVPBattleInfo.enemy_team_data.Creatures.Count; ++i)
        {
            Creature creature = Network.PVPBattleInfo.enemy_team_data.Creatures[i].creature;
            BattleCreature battle_creature = new BattleCreature(creature, battle_layout.m_Enemy.m_Characters[i], 0f, character_hpbar_prefab, character_skill_prefab, false);
            battle_creature.AutoSkillIndex = Network.PVPBattleInfo.enemy_team_data.Creatures[i].auto_skill_index;
            if (creature.Idx == Network.PVPBattleInfo.enemy_team_data.LeaderCreatureIdx)
                battle_creature.SetLeader(Network.PVPBattleInfo.enemy_team_data.UseLeaderSkillType, OnUseEnemyLeaderSkill);
            enemies.Add(battle_creature);
        }

        InitCreatures();

        SoundManager.Instance.PlayBGM("PVP");

    }

    protected override void Update()
    {
        if (CheckFirstTick() == false)
            return;

        base.Update();
        UpdateTime();

        if (_pvp_battle_end_param != null && Time.time > ShowPopupTime && ShowPopupTime != 0f)
        {
            OnFinishBattle();
            
            Clear();
            Popup.Instance.Show(ePopupMode.PVPBattleEnd, _pvp_battle_end_param);
        }
    }

    int time_animation = -1;
    void UpdateTime()
    {
        int time_left = Mathf.FloorToInt(TimeLeft);
        PvpTime.text = string.Format("{0}:{1:D2}", time_left / 60, time_left % 60);

        if (IsBattleStart == true && (time_left % 10 == 0 || time_animation == -1) && time_animation != time_left)
        {
            PVPTimeTween.ResetToBeginning();
            PVPTimeTween.Play(true);
            time_animation = time_left;
        }
    }

    override protected void PlayStart()
    {
        base.PlayStart();
        PlayEnemy();
    }

    override public void OnWin()
    {
        if (IsBattleEnd == true || IsBattleStart == false)
            return;

        enemies.ForEach(c => { if (c != null) (c as BattleCreature).SetDamage(-999999999, false); });
    }

    public override void SetBattleEnd()
    {
        base.SetBattleEnd();

        PVP.bUpdateInfo = battleEndType == pe_EndBattle.Win;

        C2G.PvpEnd packet = new C2G.PvpEnd();
        packet.enemy_account_idx = Network.PVPBattleInfo.enemy_info.account_idx;
        packet.enemy_rank = Network.PVPBattleInfo.enemy_info.rank;
        packet.is_win = battleEndType == pe_EndBattle.Win;
        Network.GameServer.JsonAsync<C2G.PvpEnd, C2G.PvpEndAck>(packet, OnPvpBattleEnd);
    }

    protected EventParamPVPBattleEnd _pvp_battle_end_param = null;
    void OnPvpBattleEnd(C2G.PvpEnd packet, C2G.PvpEndAck ack)
    {
        _pvp_battle_end_param = new EventParamPVPBattleEnd();

        _pvp_battle_end_param.end_type = battleEndType;
        _pvp_battle_end_param.rank = ack.rank;
        _pvp_battle_end_param.rank_up = ack.rank_up;

        if (_pvp_battle_end_param.end_type == pe_EndBattle.Exit)
            GameMain.SetBattleMode(eBattleMode.None);
    }
}
