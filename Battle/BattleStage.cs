using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

public class BattleStage : Battle
{
    static new public BattleStage Instance { get; protected set; }

    public UILabel StageIndex, StageName, StageTime;
    public UITweener StageTimeTween;
    public GameObject m_BattleUI;

    public UIToggle m_ToggleContinue;
    public UILabel m_labelRetry;

    public GameObject m_btnWin;

    public GameObject m_LockContinue, m_LockFast;

    public HottimeEventIconContainer m_Hottime;
    public bool IsLastWave { get { return CurrentWave == Network.BattleStageInfo.Waves.Count; } }

    override protected void Awake()
    {
        base.Awake();
        BattleStage.Instance = this;
    }

    void OnDestroy()
    {
        BattleStage.Instance = null;
    }

    protected override void Start()
    {
        m_TeamData = TeamDataManager.Instance.GetTeam(Network.BattleStageInfo.TeamID);
        m_TeamDataBackup = m_TeamData.Clone();

        if (Tutorial.Instance.Completed == false)
        {
            m_LockContinue.SetActive(true);
            m_LockFast.SetActive(true);
        }
        else if (Network.BattleStageInfo.MapInfo.MapType.Equals("main"))
        {
            var map_clear_data = MapClearDataManager.Instance.GetData(Network.BattleStageInfo);
            if (map_clear_data == null || map_clear_data.clear_count == 0)
            {
                m_LockContinue.SetActive(true);
                m_LockFast.SetActive(true);
            }
            else
            {
                m_LockContinue.SetActive(false);
                m_LockFast.SetActive(false);
            }
        }
        else
        {
            m_LockContinue.SetActive(false);
            m_LockFast.SetActive(false);
        }

        base.Start();

#if SH_DEV
        m_btnWin.SetActive(true);
#else
        m_btnWin.SetActive(Debug.isDebugBuild);
#endif

        if (BattleContinue.Instance.IsPlaying)
        {
            m_ToggleContinue.value = true;
            UpdateBattleContinue();
        }

        NextWave();

        if (Network.BattleStageInfo != null && Network.BattleStageInfo.StageType == eStageType.Boss)
            SoundManager.Instance.PlayBGM("BattleBoss");
        else
            SoundManager.Instance.PlayBGM("Battle");
    }

    int CurrentWave = 0;

    protected override void Update()
    {
        if (CheckFirstTick() == false)
            return;

        if (Time.time > WaveEndTime && WaveEndTime != 0f)
        {
            WaveEndTime = 0f;
            NextWave();
            return;
        }

        if (Time.time > WaveStartEnemyTime && WaveStartEnemyTime != 0f)
        {
            WaveStartEnemyTime = 0f;
            Tutorial.Instance.ShowCutScene(eSceneType.PreCharacter);
            PlayEnemy();
            return;
        }

        if (Time.time > WaveStartTime && WaveStartTime != 0f)
        {
            if (CurrentWave <= 1)
            {
                Tutorial.Instance.ShowCutScene(eSceneType.PreAll);
            }
            else if(CurrentWave == 3 && Tutorial.Instance.CheckCutScene(eSceneType.PreAll_Wave3) == true)
                Tutorial.Instance.ShowCutScene(eSceneType.PreAll_Wave3);
        }

        base.Update();
        UpdateTime();

        if (Time.timeScale == 0f) return;
        if (IsPause == ePauseType.Pause)
            return;

        if (IsBattleStart == true)
        {
            if (IsBattleEnd == false)
            {
                UpdateMana(deltaTime);
            }

            UpdateCharacterTouch();
            CheckBattleEnd();
        }

        if (IsBattleEnd == true)
        {
            if (_battle_end_param != null && Time.time > ShowPopupTime && ShowPopupTime != 0f)
            {
                if (Tutorial.Instance.CutsceneInfo != null) return;

                OnFinishBattle(BattleContinue.Instance.IsPlaying);

                if (_battle_end_param.end_type == pe_EndBattle.Win && Tutorial.Instance.CheckCutScene(eSceneType.Post) == true)
                {
                    Tutorial.Instance.ShowCutScene(eSceneType.Post);
                    return;
                }

                Clear();
                if (_battle_end_param.end_type == pe_EndBattle.Win)
                    Popup.Instance.Show(ePopupMode.BattleEnd, _battle_end_param);
                else
                    Popup.Instance.Show(ePopupMode.BattleEndFail, _battle_end_param);
            }
        }
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

    public override void SetBattleEnd()
    {
        switch (battleEndType)
        {
            case pe_EndBattle.Win:
                EndWave();
                break;

            default:
                base.SetBattleEnd();
                SetBattleEndStage();
                break;
        }
    }

    void SetBattleEndStage()
    {
        C2G.EndBattle _packet = new C2G.EndBattle();
        _packet.battle_type = pe_Battle.Stage;
        _packet.end_type = battleEndType;
        _packet.difficulty = Network.BattleStageInfo.Difficulty;
        _packet.map_id = Network.BattleStageInfo.MapInfo.ID;
        _packet.stage_id = Network.BattleStageInfo.ID;
        _packet.creatures = new List<PacketInfo.pd_BattleEndCreatureInfo>();
        _packet.is_new_clear = MapClearDataManager.Instance.IsNewClear(Network.BattleStageInfo);

        //List<int> skill_indice = new List<int>();
        for (int i = 0; i < m_TeamData.Creatures.Count; ++i)
        {
            Creature creature = m_TeamData.Creatures[i].creature;
            BattleCreature bc = characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            if (bc == null) bc = dead_characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            _packet.creatures.Add(new PacketInfo.pd_BattleEndCreatureInfo(bc.Idx, bc.IsDead));
            m_TeamData.Creatures[i].auto_skill_index = bc.AutoSkillIndex;
        }
        if (m_TeamData.IsEqual(m_TeamDataBackup) == false || Tutorial.Instance.Completed == false)
        {
            _packet.team_data = m_TeamData.CreateSaveData();
        }

        if (Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = Network.PlayerInfo.tutorial_state;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.end_battle = _packet;

            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialEndBattleHandler);
        }
        else
            Network.GameServer.JsonAsync<C2G.EndBattle, C2G.EndBattleAck>(_packet, OnEndBattleHandler);
    }

    void EndWave()
    {
        if (IsLastWave)
        {
            base.SetBattleEnd();
            SetBattleEndStage();
            return;
        }

        WaveStartTime = 0f;
        IsBattleStart = false;

        float delay = 1.0f;
        var end_bundle = BattleBase.Instance.tween_system.GetBundle("end");
        end_bundle.Delay = delay;

        foreach (var creature in characters)
        {
            BattleCreature battle_creature = creature as BattleCreature;
            battle_creature.Clear();
            if (creature.IsDead == false)
            {
                end_bundle.Play(creature.Character.GetComponent<HFX_TweenSystem>(), creature.Character.transform, null, 1f);
                int heal = Mathf.RoundToInt(battle_creature.Stat.MaxHP * GameConfig.Get<float>("wave_heal"));
                if (heal > 0)
                {
                    battle_creature.SetWaveHeal(heal);
                    TextManager.Instance.PushHeal(battle_creature, heal, eTextPushType.Normal);
                }
            }
        }
        foreach (var creature in enemies)
        {
            if (creature != null)
            {
                (creature as BattleCreature).Clear();
                if (creature.IsDead == false)
                    end_bundle.Play(creature.Character.GetComponent<HFX_TweenSystem>(), creature.Character.transform, null, 1f);
            }
        }
        WaveEndTime = Time.time + 2f + delay;

        foreach (var creature in m_LightingCreatures)
            creature.SetEnd(true);
    }

    override protected void PlayStart()
    {
        if (Tutorial.Instance.CheckCutScene(eSceneType.PreCharacter) == true)
            WaveStartEnemyTime = Time.time + 1.5f + play_start_delay;
        base.PlayStart();
    }

    void NextWave()
    {
        Debug.LogFormat("NextWave : {0}/{1}", CurrentWave+1, Network.BattleStageInfo.Waves.Count);
        if (Network.BattleStageInfo.StageType == eStageType.Boss)
            m_BG.material.mainTexture = AssetManager.LoadBG(Network.BattleStageInfo.BG_ID + "_D");
        else
            m_BG.material.mainTexture = AssetManager.LoadBG(Network.BattleStageInfo.BG_ID + "_" + (char)('A' + CurrentWave));

        if (Network.BattleStageInfo.Difficulty == pe_Difficulty.Hard)
        {
            m_BG.material.SetColor("_GrayColor", GameMain.colorHard);
        }
        else
        {
            m_BG.material.SetColor("_GrayColor", GameMain.colorZero);
        }

        update_mana_creature_index = 0;
        update_mana_enemy_index = 0;

        dead_characters.AddRange(characters.Where(c => c.IsDead));
        characters = characters.Where(c => c.IsDead == false).ToList();

        battleEndType = pe_EndBattle.Invalid;
        IsBattleEnd = false;
        PlaybackTime = 0f;

        enemies = new List<ICreature>();
        List<MapCreatureInfo> enemy_infos = Network.BattleStageInfo.Waves[CurrentWave].Creatures;

        for (int i = 0; i < enemy_infos.Count; ++i)
        {
            if (enemy_infos[i].CreatureInfo == null)
                continue;

            BattleCreature creature = new BattleCreature(enemy_infos[i], battle_layout.m_Enemy.m_Characters[i], 0f, character_hpbar_prefab, character_skill_prefab);
            enemies.Add(creature);
        }

        ++CurrentWave;
        switch (Network.BattleStageInfo.MapInfo.MapType)
        {
            case "boss":
                {
                    var boss_info = enemy_infos.Find(e => e.CreatureType == eMapCreatureType.Boss);
                    StageIndex.text = Localization.Format("HeroLevel", Boss.CalculateLevel(boss_info.Level, Network.BattleStageInfo));
                    StageName.text = boss_info.CreatureInfo.Name;
                }
                break;

            case "event":
                {
                    StageIndex.text = Localization.Get("Menu_Training");
                    StageName.text = Localization.Format("StageMainBattleName", Network.BattleStageInfo.MapInfo.Name, Network.BattleStageInfo.Name, CurrentWave, Network.BattleStageInfo.Waves.Count);
                }
                break;

            case "weekly":
                {
                    StageIndex.text = Localization.Get("Menu_Training");
                    StageName.text = Localization.Format("StageMainBattleName", Network.BattleStageInfo.MapInfo.Name, Localization.Get("MapDifficulty_" + Network.BattleStageInfo.Difficulty), CurrentWave, Network.BattleStageInfo.Waves.Count);
                }
                break;

            default:
                {
                    StageIndex.text = Localization.Format("StageIndex", Network.BattleStageInfo.MapInfo.IDN, Network.BattleStageInfo.StageIndex + 1);
                    StageName.text = Localization.Format("StageMainBattleName", Network.BattleStageInfo.Name, Localization.Get("MapDifficulty_" + Network.BattleStageInfo.Difficulty), CurrentWave, Network.BattleStageInfo.Waves.Count);
                }
                break;
        }

        first_tick = 0;
        UpdateTime();

        InitCreatures();
    }

    override public void OnWin()
    {
        if (IsBattleEnd == true || IsBattleStart == false)
            return;

        CurrentWave = Network.BattleStageInfo.Waves.Count;

        enemies.ForEach(c => { if (c != null) (c as BattleCreature).SetDamage(-999999999, false); });
    }

    protected EventParamBattleEnd _battle_end_param = null;
    void OnTutorialEndBattleHandler(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        TeamDataManager.Instance.Save();
        OnEndBattleHandler(packet.end_battle, ack.end_battle);
    }
    void OnEndBattleHandler(C2G.EndBattle packet, C2G.EndBattleAck ack)
    {
        MapStageDifficulty stage_info = Network.BattleStageInfo;

        _battle_end_param = new EventParamBattleEnd();
        _battle_end_param.end_type = packet.end_type;
        _battle_end_param.is_boss = stage_info.MapInfo.MapType == "boss";
        if (packet.end_type == pe_EndBattle.Win)
        {
            _battle_end_param.player_levelup = Network.PlayerInfo.UpdateExp(ack.player_exp_add_info);
            _battle_end_param.creatures = new List<BattleEndCreature>();

            m_TeamData.Creatures.ForEach(c => _battle_end_param.creatures.Add(c.creature.UpdateExp(ack.creature_exp_add_infos.Find(i => i.creature_idx == c.creature.Idx))));
            _battle_end_param.add_goods = ack.add_goods;
            _battle_end_param.loot_items = ack.loot_items;
            _battle_end_param.loot_runes = ack.loot_runes;

            _battle_end_param.loot_creatures = ack.loot_creatures.Select(c => c.creature_idx).ToList();

            ack.loot_items.ForEach(i => ItemManager.Instance.Add(i));
            ack.loot_runes.ForEach(i => RuneManager.Instance.Add(i));
            ack.add_goods.ForEach(g => Network.PlayerInfo.AddGoods(g));
            Network.Instance.LootCreatures(ack.loot_creatures, ack.loot_creatures_equip);

            int dead_count = dead_characters.Count;
            _battle_end_param.clear_rate = battleEndType == pe_EndBattle.Win ? (short)Math.Max(1, 3 - dead_count) : (short)0;

            if (MapClearDataManager.Instance.SetClearRate(stage_info, _battle_end_param.clear_rate) == true && stage_info.MapInfo.MapType == "main")
            {
                Network.LastOpenContentsStageInfo = stage_info;
                Tutorial.Instance.first_clear = true;
            }
            else
            {
                Tutorial.Instance.first_clear = false;
            }

            Network.NewStageInfo = null;

            if (ack.set_new_map == true)
            {
                var next_stage_info = MapInfoManager.Instance.GetNextStageInfo(stage_info);
                if (next_stage_info != null)
                {
                    MapClearDataManager.Instance.SetNew(next_stage_info);
                    if (next_stage_info.MapInfo.MapType != "main" || next_stage_info.MapInfo.IDN <= GameConfig.Get<int>("contents_open_main_map"))
                        Network.NewStageInfo = next_stage_info;
                }
            }
        }
        else
        {
            _battle_end_param.player_levelup = Network.PlayerInfo.UpdateExp(null);

            short energy = stage_info.Energy;
            var energy_event = EventHottimeManager.Instance.GetInfoByID("dungeon_energy_zero");
            if (energy_event != null)
                energy = (short)(energy * energy_event.Percent);
            Network.PlayerInfo.AddEnergy(energy);
        }

        _battle_end_param.maxlevel_reward_mail_idxs = ack.maxlevel_reward_mail_idx;

        m_Hottime.Clear();

        if (packet.end_type == pe_EndBattle.Exit)
            GameMain.SetBattleMode(eBattleMode.None);
    }

    public void OnClickContinue()
    {
        if (IsBattleEnd == true)
            return;

        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }

        if (m_ToggleContinue.value == true)
        {
            m_ToggleContinue.value = false;
            BattleContinue.Instance.Clear();
        }
        else
        {
            if (Network.BattleStageInfo.MapInfo.MapType == "main")
            {
                var map_clear_data = MapClearDataManager.Instance.GetData(Network.BattleStageInfo);
                if (map_clear_data == null || map_clear_data.clear_count == 0)
                {
                    Tooltip.Instance.ShowMessageKey("UseAfterStageCleared");
                    return;
                }
            }

            m_ToggleContinue.value = true;

            int battleAvailableCount = DungeonInfoMenu.CalculateBattleAvailableCount(Network.BattleStageInfo);
            BattleContinue.Instance.SetContinue(battleAvailableCount);
            UpdateBattleContinue();
        }
    }

    void UpdateBattleContinue()
    {
        m_labelRetry.text = string.Format("{0}/{1}", BattleContinue.Instance.BattleCount, BattleContinue.Instance.RequestCount);
    }
}
