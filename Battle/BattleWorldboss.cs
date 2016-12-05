using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

public class BattleWorldboss : Battle
{
    static new public BattleWorldboss Instance { get; protected set; }

    public UIAtlas m_AtlasCharacterShots, m_AtlasPF;

    override protected bool UseRun { get { return false; } }

    public UILabel StageTime;
    public UITweener StageTimeTween;

    public UILabel BestScore, CurrentScore;
    public TweenScale ScoreTween;

    public UILabel up_score, die_count;
    public TweenScale DieTween;

    public Transform BossIndicator;

    // worldboss
    public PrefabManager m_WorldBossCharacterPrefabManager;
    public GameObject WorldBossCanvas;
    public List<WorldBossCharacter> m_Characters;

    public int column_count = 20;
    public int row_count = 10;
    public float RowGap = 50f, ColumnGap = 5f;

    override protected float GetTimeLimit() { return GameConfig.Get<float>("worldboss_time_limit"); }

    override protected void Awake()
    {
        base.Awake();
        BattleWorldboss.Instance = this;
    }

    int time_animation = -1;
    void UpdateTime()
    {
        int time_left = Mathf.FloorToInt(TimeLeft);
        if (StageTime != null)
        {
            StageTime.text = string.Format("{0}:{1:D2}", time_left / 60, time_left % 60);
        }

        if (IsBattleStart == true && (time_left % 10 == 0 || time_animation == -1) && time_animation != time_left)
        {
            StageTimeTween.ResetToBeginning();
            StageTimeTween.Play(true);
            time_animation = time_left;
        }
    }

    protected override void Start()
    {
        m_TeamData = TeamDataManager.Instance.GetTeam(Network.BattleStageInfo.TeamID);
        m_TeamDataBackup = m_TeamData.Clone();
        m_BossHP.m_Label.gameObject.SetActive(false);

        up_score.gameObject.SetActive(false);
        die_count.gameObject.SetActive(false);

        base.Start();
        m_BG.material.mainTexture = AssetManager.LoadBG(Network.BattleStageInfo.BG_ID);

        enemies = new List<ICreature>();
        List<MapCreatureInfo> enemy_infos = Network.BattleStageInfo.Waves[0].Creatures;

        for (int i = 0; i < enemy_infos.Count; ++i)
        {
            if (enemy_infos[i].CreatureInfo == null)
                continue;

            BattleCreature creature = new BattleCreature(enemy_infos[i], battle_layout.m_Enemy.m_Characters[i], 0f, character_hpbar_prefab, character_skill_prefab);
            enemies.Add(creature);
        }

        InitWorldBoss();

        update_mana_creature_index = 0;
        update_mana_enemy_index = 0;

        battleEndType = pe_EndBattle.Invalid;
        IsBattleEnd = false;
        PlaybackTime = 0f;

        first_tick = 0;

        InitCreatures();

        if (WorldBossInfo.Info != null)
            BestScore.text = Localization.Format("WorldBossBestInBattle", WorldBossInfo.Info.score);
        else
            BestScore.text = Localization.Format("WorldBossBestInBattle", "-");

        SoundManager.Instance.PlayBGM("PVP");
    }

    int m_die_count = 0;
    int last_score = -1;
    void UpdateScore()
    {
        int current_score = m_BossHP.Creature.Stat.DealHP;
        if (current_score != last_score)
        {
            ScoreTween.ResetToBeginning();
            ScoreTween.PlayForward();
            last_score = current_score;
            CurrentScore.text = Localization.Format("WorldBossScoreInBattle", current_score);
            if (WorldBossInfo.Info != null)
            {
                if (WorldBossInfo.Info.score < current_score)
                {
                    up_score.gameObject.SetActive(true);
                    up_score.text = string.Format("{0:n0}", current_score - WorldBossInfo.Info.score);
                }
            }

            if (m_BossHP.Creature.Stat.DieCount > 0 && m_die_count != m_BossHP.Creature.Stat.DieCount)
            {
                die_count.gameObject.SetActive(true);
                die_count.text = m_BossHP.Creature.Stat.DieCount.ToString();
                DieTween.ResetToBeginning();
                DieTween.PlayForward();
                m_die_count = m_BossHP.Creature.Stat.DieCount;
            }
        }
    }

    protected override void Update()
    {
        if (BattleBase.CurrentBattleMode == eBattleMode.None)
            return;

        UpdateTime();
        UpdateScore();

        if (CheckFirstTick() == false)
            return;

        base.Update();

        if (Time.timeScale == 0f) return;
        if (IsPause == ePauseType.Pause)
            return;

        if (IsBattleEnd == true)
        {
            if (_battle_end_param != null && Time.time > ShowPopupTime && ShowPopupTime != 0f)
            {
                ShowPopupTime = 0f;

                OnFinishBattle(false);

                Popup.Instance.Show(ePopupMode.WorldBossBattleEnd, _battle_end_param);
            }
        }
        else
            m_Characters.ForEach(p => p.UpdateCharacter());

    }

    override protected void Clear()
    {
        base.Clear();
        m_WorldBossCharacterPrefabManager.Clear();
        m_Characters.Clear();
    }

    void InitWorldBoss()
    {
        if (m_AtlasCharacterShots.spriteMaterial == null)
            m_AtlasCharacterShots.replacement = AssetManager.LoadCharacterShotsAtlas();

        m_Characters = new List<WorldBossCharacter>();

        Vector3 worldboss_position = WorldBossCanvas.transform.worldToLocalMatrix.MultiplyPoint(BossIndicator.position);
        int character_index = 0;
        foreach (var creature in CreatureManager.Instance.Creatures.Except(m_TeamData.Creatures.Select(c => c.creature)))
        {
            int row_index = character_index / column_count;
            int col_index = character_index % column_count;

            Vector3 position = Vector3.zero;
            position.x -= col_index * ColumnGap;
            position.z += row_index * RowGap + (col_index % 2 == 0 ? -10f : 10f);

            var character = m_WorldBossCharacterPrefabManager.GetNewObject<WorldBossCharacter>(WorldBossCanvas.transform, position);
            character.Init(creature, creature.Info.Position != SharedData.eCreaturePosition.rear, worldboss_position);
            m_Characters.Add(character);

            ++character_index;
        }
    }
    public override void SetBattleEnd()
    {
        base.SetBattleEnd();

        C2G.EndWorldBoss _packet = new C2G.EndWorldBoss();
        _packet.end_type = battleEndType;
        _packet.map_id = Network.BattleStageInfo.MapInfo.ID;
        _packet.score = m_BossHP.Creature.Stat.DealHP;

        //List<int> skill_indice = new List<int>();
        for (int i = 0; i < m_TeamData.Creatures.Count; ++i)
        {
            Creature creature = m_TeamData.Creatures[i].creature;
            BattleCreature bc = characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            if (bc == null) bc = dead_characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            m_TeamData.Creatures[i].auto_skill_index = bc.AutoSkillIndex;
        }
        if (m_TeamData.IsEqual(m_TeamDataBackup) == false)
        {
            _packet.team_data = m_TeamData.CreateSaveData();
        }

        Network.GameServer.JsonAsync<C2G.EndWorldBoss, C2G.EndWorldBossAck>(_packet, OnEndWorldBossHandler);
    }

    protected EventParamWorldBossBattleEnd _battle_end_param = null;
    void OnEndWorldBossHandler(C2G.EndWorldBoss packet, C2G.EndWorldBossAck ack)
    {
        if (packet.end_type == pe_EndBattle.Exit)
        {
            GameMain.SetBattleMode(eBattleMode.None);
            return;
        }

        MapStageDifficulty stage_info = Network.BattleStageInfo;

        _battle_end_param = new EventParamWorldBossBattleEnd();
        _battle_end_param.rank = ack.info.rank;
        _battle_end_param.score = packet.score;
        if (WorldBossInfo.Info != null)
        {
            _battle_end_param.rank_up = ack.info.rank - WorldBossInfo.Info.rank;
            _battle_end_param.score_up = ack.info.score - WorldBossInfo.Info.score;
        }
        else
            _battle_end_param.is_first = true;

        WorldBossInfo.Info = ack.info;

        Network.PlayerInfo.UseEnergy(stage_info.Energy);

        MapClearDataManager.Instance.SetClearRate(stage_info, 3);
    }
}
