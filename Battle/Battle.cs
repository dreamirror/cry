using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

abstract public class Battle : BattleBase
{
    static new public Battle Instance { get; protected set; }

    static public string scene_name = "";

    BattleUICharacter[] battleUICharacter = new BattleUICharacter[5];

    protected float battleEndLeftTime = 0f;
    protected pe_EndBattle battleEndType = pe_EndBattle.Invalid;

    public GameObject m_Auto;
    public GameObject m_Fast;

    public GameObject m_SkillIndicator, m_SkillEnemyIndicator;

    public GameObject[] CharIndicator;

    BattleCreature m_Leader;
    public UILeaderSkill m_UILeaderSkill;
    public MeshRenderer m_BG;
    public BossHP m_BossHP;

    protected float WaveStartTime = 0f, WaveEndTime = 0f, WaveStartEnemyTime = 0f;

    protected float ShowPopupTime = 0f;

    virtual protected bool UseRun { get { return true; } }

    protected float TimeLimit = 0f;
    virtual protected float GetTimeLimit() { return GameConfig.Get<float>("stage_time_limit"); }

    public float TimeLeft
    {
        get
        {
            if (IsBattleEnd == true)
                return battleEndLeftTime;
            return Mathf.Clamp(TimeLimit - PlaybackTime, 0f, TimeLimit);
        }
    }

    protected TeamData m_TeamData, m_TeamDataBackup;
    protected int first_tick = 0;

    override protected void Awake()
    {
        base.Awake();
        Battle.Instance = this;
    }

    protected bool CheckFirstTick()
    {
        switch (first_tick)
        {
            case 0:
                ++first_tick;
                return false;

            case 1:
                ++first_tick;
                OnFirstTick();
                return false;
        }
        return true;
    }

    virtual protected void SetBattleStart()
    {
        IsBattleStart = true;
    }

    override protected void Update()
    {
        base.Update();

        if (Time.time > WaveStartTime && WaveStartTime != 0f)
        {
            WaveStartTime = 0f;
            SetBattleStart();
            return;
        }

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

        if (IsBattleEnd == false && Input.GetKeyDown(KeyCode.Escape))
        {
            Popup.PopupInfo popup = Popup.Instance.GetCurrentPopup();
            if (popup != null)
                popup.Obj.OnClose();
            else
            {
                OnPause();
            }
        }
    }

    virtual protected void Clear()
    {   
        ShowPopupTime = 0f;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
            SetPause(true);
    }
    protected GameObject character_hpbar_prefab, character_skill_prefab;

    // Use this for initialization
    override protected void Start()
    {
//         UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(scene_name);
//         if (UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene) == false)
//             Debug.LogErrorFormat("SetActiveScene({0}) Failed!!", scene_name);

        GameMain.Instance.gameObject.SetActive(false);

        TimeLimit = GetTimeLimit();
        base.Start();

        tween_system = AssetManager.GetCharacterPrefab("000_tween").GetComponent<HFX_TweenSystem>();
        color_container = tween_system.gameObject.GetComponent<ColorContainer>();

        character_hpbar_prefab = Resources.Load("Prefab/Battle/CharacterHPBar", typeof(GameObject)) as GameObject;
        character_skill_prefab = Resources.Load("Prefab/Battle/CharacterSkill", typeof(GameObject)) as GameObject;

        characters = new List<ICreature>();
        dead_characters = new List<ICreature>();
        GameObject prefab = Resources.Load<GameObject>("Prefab/Battle/BattleUICharacter");

        if (prefab == null)
        {
            Debug.LogErrorFormat("Failed to load BattleUICharacter.prefab");
            return;
        }
        for (int i = 0; i < CharIndicator.Length && i < m_TeamData.Creatures.Count; i++)
        {
            BattleCreature creature = new BattleCreature(m_TeamData.Creatures[i].creature, battle_layout.m_Mine.m_Characters[i], 0f, character_hpbar_prefab, character_skill_prefab);
            characters.Add(creature);

            var ui_character = (Instantiate(prefab) as GameObject).GetComponent<BattleUICharacter>();
            ui_character.transform.parent = CharIndicator[i].transform;
            ui_character.transform.localPosition = Vector3.zero;
            ui_character.transform.localScale = Vector3.one;
            ui_character.SetCharacter(creature);
            ui_character.gameObject.SetActive(true);
            battleUICharacter[i] = ui_character;

            ui_character.AutoSkillSelect(m_TeamData.Creatures[i].auto_skill_index);
        }

        if (Tutorial.Instance.Completed == false && Network.BattleStageInfo.MapInfo.IDN == 1 && Network.BattleStageInfo.StageIndex == 0)
        {
            SetAuto(false);
            SetFast(false, true);
        }
        else
        {
            SetAuto(m_TeamData.IsAuto);
            SetFast(m_TeamData.IsFast, true);
        }

        if (m_TeamData.LeaderCreature != null)
        {
            m_Leader = characters.Find(c => c.Info.IDN == m_TeamData.LeaderCreature.Info.IDN) as BattleCreature;
            m_Leader.SetLeader(m_TeamData.UseLeaderSkillType, OnUseLeaderSkill);
            m_UILeaderSkill.Init(m_TeamData.LeaderCreature, m_TeamData.UseLeaderSkillType, OnLeaderSkill);
        }
        else
            m_UILeaderSkill.gameObject.SetActive(false);

        LoadSkill();

        m_SkillCasting = AssetManager.GetParticleSystem("skill_casting");
        m_SkillTargetTeam = AssetManager.GetParticleSystem("skill_target_team");
        m_SkillTargetEnemy = AssetManager.GetParticleSystem("skill_target_enemy");
        m_Miss = AssetManager.GetParticleSystem("miss");

        Network.HideIndicator();
    }

    protected void OnSkill(BattleSkill skill)
    {
        if (skill.IsDefault == true)
            return;
    }

    public void OnAuto()
    {
        if(Network.PVPBattleInfo != null)
        {
            Tooltip.Instance.ShowMessageKey("PVPOnlyAutoBattle");
            return;
        }
        SetAuto(!IsAuto);
    }

    void SetAuto(bool auto)
    {
        IsAuto = auto;
        m_Auto.SetActive(IsAuto);
        m_TeamData.SetAuto(IsAuto, Tutorial.Instance.Completed);
    }

    public void OnFast()
    {
        if (Network.PVPBattleInfo != null)
        {
            Tooltip.Instance.ShowMessageKey("PVPOnlyFastBattle");
            return;
        }
        SetFast(!IsFast, false);
    }

    void SetFast(bool fast, bool init)
    {
        if (BattleBase.CurrentBattleMode != eBattleMode.Battle)
            return;

        if (IsBattleEnd == true)
            return;

        if (Tutorial.Instance.Completed == false)
        {
            if (init == false)
                Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            IsFast = false;
            m_Fast.SetActive(false);
            TimeManager.Instance.ResetTimeScale();
            return;
        }

        if (fast == true)
        {
            if (Network.BattleStageInfo.MapInfo.MapType == "main")
            {
                var map_clear_data = MapClearDataManager.Instance.GetData(Network.BattleStageInfo);
                if (map_clear_data == null || map_clear_data.clear_count == 0)
                {
                    if (init == false)
                        Tooltip.Instance.ShowMessageKey("UseAfterStageCleared");
                    IsFast = false;
                    m_Fast.SetActive(false);
                    TimeManager.Instance.ResetTimeScale();
                    return;
                }
            }
        }

        IsFast = fast;
        m_Fast.SetActive(IsFast);
        m_TeamData.SetFast(IsFast, Tutorial.Instance.Completed);
        TimeManager.Instance.SetTimeScale(IsFast ? 2f : 1f);
    }

    virtual protected void CheckBattleEnd()
    {
        if (IsBattleStart == false)
            return;

        if (IsBattleEnd == false)
        {
            bool battle_end = true;
            if (TimeLeft <= 0)
                battleEndType = pe_EndBattle.Timeout;
            else if (enemies.All(c => c == null || c.IsDead == true))
                battleEndType = pe_EndBattle.Win;
            else if (characters.All(c => c.IsDead == true))
                battleEndType = pe_EndBattle.Lose;
            else
                battle_end = false;

            if (battle_end == true)
            {
                battleEndLeftTime = TimeLeft;
                IsBattleEnd = battle_end;
            }
        }

        if (m_LightingCreatures.Count > 0 || characters.Any(c => c.Character.IsPlayingActionAnimation) || enemies.Any(c => c != null && c.Character.IsPlayingActionAnimation))
            return;

        if (IsBattleEnd == true)
        {
            SetBattleEnd();
        }
    }

    public void SetBattleExit()
    {
        Clear();
        battleEndType = pe_EndBattle.Exit;
        SetBattleEnd();
    }

    virtual public void SetBattleEnd()
    {
        ConfigData.Instance.RefreshSleep();

        IsBattleStart = false;

        if (battleEndType != pe_EndBattle.Exit)
        {
            SoundManager.Instance.StopBGM();
            switch (battleEndType)
            {
                case pe_EndBattle.Win:
                    ShowPopupTime = Time.time + 4f;
                    SoundManager.PlaySound(AssetManager.GetSound("battle_win"), 1f);
                    break;

                case pe_EndBattle.Timeout:
                case pe_EndBattle.Lose:
                    ShowPopupTime = Time.time + 4f;
                    SoundManager.PlaySound(AssetManager.GetSound("battle_lose"), 1f);
                    break;

                default:
                    ShowPopupTime = Time.time + 2f;
                    break;
            }

            dead_characters.AddRange(characters.Where(c => c.IsDead));
//            characters = characters.Where(c => c.IsDead == false).ToList();

            foreach (ICreature icreature in characters)
            {
                if (icreature.IsDead == true)
                    continue;

                BattleCreature creature = icreature as BattleCreature;
                creature.Finish(battleEndType == pe_EndBattle.Win);
            }
            foreach (ICreature icreature in enemies)
            {
                if (icreature.IsDead == true)
                    continue;

                BattleCreature creature = icreature as BattleCreature;
                if (creature == null) continue;
                creature.Finish(battleEndType == pe_EndBattle.Lose);
            }

            foreach (var creature in m_LightingCreatures)
                creature.SetEnd(true);
        }
    }

    void OnTooltip(bool show)
    {
        if (show)
            UITooltip.Show("test");
        else
            UITooltip.Hide();
    }

    UISkillInfo m_SkillInfo = null, m_SkillEnemyInfo = null;
    protected void LoadSkill()
    {
        if (m_SkillIndicator == null)
            return;

        if (m_SkillInfo == null)
        {
            GameObject obj = Resources.Load("Prefab/Battle/SkillInfo", typeof(GameObject)) as GameObject;
            m_SkillInfo = (Instantiate(obj) as GameObject).GetComponent<UISkillInfo>();
            m_SkillInfo.transform.SetParent(m_SkillIndicator.transform, false);
            m_SkillInfo.transform.localScale = Vector3.one;
            m_SkillInfo.gameObject.SetActive(false);
        }

        if (m_SkillEnemyInfo == null)
        {
            GameObject obj = Resources.Load("Prefab/Battle/SkillInfoEnemy", typeof(GameObject)) as GameObject;
            m_SkillEnemyInfo = (Instantiate(obj) as GameObject).GetComponent<UISkillInfo>();
            m_SkillEnemyInfo.transform.SetParent(m_SkillEnemyIndicator.transform, false);
            m_SkillEnemyInfo.transform.localScale = Vector3.one;
            m_SkillEnemyInfo.gameObject.SetActive(false);
        }
    }

    public void OnPause()
    {
        SetPause(false);
    }

    public void SetPause(bool bShowImmediately)
    {
        //         if (IsBattleEnd == true)
        //             return;

        if (Tutorial.Instance.CutsceneInfo != null) return;

        Popup.PopupInfo popup = Popup.Instance.GetCurrentPopup();
        if (popup != null) return;

        Debug.Log("OnPause");
        if (bShowImmediately)
            Popup.Instance.ShowImmediately(ePopupMode.BattleOption);
        else
            Popup.Instance.Show(ePopupMode.BattleOption);
    }

    public void OnChat()
    {
        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }
        if (GameConfig.Get<bool>("contents_chatting_maintenance") == true)
        {
            Tooltip.Instance.ShowMessage(Localization.Get("ChatMaintenance"));
            return;
        }
        ChattingMain.Instance.ShowChattingPopup();
    }

    public void OnLeaderSkill()
    {
        m_Leader.UseLeaderSkill();
    }

    void OnUseLeaderSkill(BattleSkill skill)
    {
        m_UILeaderSkill.SetDisable();
        m_SkillInfo.Init(skill.Info);
        m_SkillInfo.Show();
    }

    public void OnUseEnemyLeaderSkill(BattleSkill skill)
    {
        m_SkillEnemyInfo.Init(skill.Info);
        m_SkillEnemyInfo.Show();
    }

    protected void UpdateCharacterTouch()
    {
        if (Camera.main == null)
            return;

        Vector3 inPos;
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Ended)
            return;

        inPos = touch.position;
#else
        if (Input.GetMouseButtonUp(0) == false)
            return;

        inPos = Input.mousePosition;
#endif

        Ray main_ray = Camera.main.GetComponent<CameraPerspectiveEditor>().ScreenPointToRay(inPos);
        PlayHead(Camera.main, main_ray);

    }

    bool PlayHead(Camera camera, Ray ray)
    {
        int mask = camera.cullingMask;
        float dist = camera.farClipPlane - camera.nearClipPlane;

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, dist, mask))
        {
            Character selected_character = CoreUtility.GetParentComponent<Character>(hitInfo.collider.transform);
            if (selected_character)
            {
                selected_character.PlayHead();
                return true;
            }
        }
        return false;
    }

    public void CheckManaFill(BattleCreature creature)
    {
        if (creature.IsTeam)
        {
            if (update_mana_creature_index != -1 && characters[update_mana_creature_index] == creature)
            {
                creature.IsManaFill = false;
                NextCreature(true);
            }
        }
        else
        {
            if (update_mana_enemy_index != -1 && enemies[update_mana_enemy_index] == creature)
            {
                creature.IsManaFill = false;
                NextCreature(false);
            }
        }
    }

    protected int update_mana_creature_index = 0;
    protected int update_mana_enemy_index = 0;
    protected void UpdateMana(float deltaTime)
    {
        float mp_fill = BattleConfig.Instance.MPFill;

        BattleCreature creature = update_mana_creature_index == -1 ? null : characters[update_mana_creature_index] as BattleCreature;
        if (creature != null)
        {
            if (creature.IsDead == true || creature.IsPlayingSkill == true)
            {
                creature.IsManaFill = false;
                NextCreature(true);
            }
            else
            {
                creature.IsManaFill = true;
                creature.UpdateMana(Mathf.RoundToInt(deltaTime * mp_fill));
            }

            if (creature.Stat.IsMPFull == true)
            {
                creature.IsManaFill = false;
                NextCreature(true);
                if(Tutorial.Instance.CheckConditionManaFull(creature.Info) == true)
                    Tutorial.Instance.SetConditionOK();
            }
        }
        else
            NextCreature(true);

        BattleCreature enemy = update_mana_enemy_index == -1 ? null : enemies[update_mana_enemy_index] as BattleCreature;
        ICreature boss = enemies.Find(e => e.MapCreature != null && e.MapCreature.CreatureType == eMapCreatureType.Boss);
        if (boss != null && boss != enemy)
        {
            if (enemy != null)
                enemy.IsManaFill = false;
            
            enemy = boss as BattleCreature;
            enemy.IsManaFill = true;
        }
        if (enemy != null)
        {
            if (enemy.IsDead == true || enemy.IsPlayingSkill == true)
            {
                enemy.IsManaFill = false;
                NextCreature(false);
            }
            else
            {
                enemy.IsManaFill = true;
                enemy.UpdateMana(Mathf.RoundToInt(deltaTime * mp_fill));
            }

            if (enemy.Stat.IsMPFull == true)
            {
                enemy.IsManaFill = false;
                NextCreature(false);
            }
        }
        else
            NextCreature(false);
    }

    void NextCreature(bool is_team)
    {
        int count_limit = 5;

        int next_creature_index = is_team ? update_mana_creature_index:update_mana_enemy_index;
        List<ICreature> targets = is_team ? characters : enemies;

        while (true && count_limit-- > 0)
        {
            ++next_creature_index;
            if (next_creature_index >= targets.Count)
                next_creature_index = 0;

            BattleCreature creature = targets[next_creature_index] as BattleCreature;
            if (creature != null && creature.IsDead == false && creature.IsPlayingSkill == false && creature.IsMPFull == false)
            {
                creature.IsManaFill = true;
                if (is_team)
                    update_mana_creature_index = next_creature_index;
                else
                    update_mana_enemy_index = next_creature_index;
                return;
            }
        }
        if (is_team)
            update_mana_creature_index = -1;
        else
            update_mana_enemy_index = -1;
    }

    void OnDisable()
    {
        Clear();
        //         foreach (var action in m_PlayingActions)
        //         {
        //             action.Cancel(true);
        //         }
        //         m_PlayingActions.Clear();
    }

    void OnFirstTick()
    {
        PlayStart();
    }

    public const float play_start_delay = 0.5f;
    virtual protected void PlayStart()
    {
        if (UseRun == true)
        {
            var start_bundle = BattleBase.Instance.tween_system.GetBundle("start");
            start_bundle.Delay = play_start_delay;

            foreach (ICreature creature in characters)
            {
                var creature_tween = creature.Character.GetComponent<HFX_TweenSystem>();

                var sp_bundle = creature_tween.GetPlayingBundle("start_preset");
                if (sp_bundle != null)
                    sp_bundle.groups[0].tweens[0].Duration = sp_bundle.PlaybackTime + play_start_delay;

                start_bundle.Play(creature_tween, creature.Character.transform, null, 1f);
                creature_tween.UpdatePlay(creature_tween.PlaybackTime);
            }

            if (WaveStartEnemyTime == 0f)
                PlayEnemy();
        }
        else
            WaveStartTime = Time.time + play_start_delay;
    }

    protected void PlayEnemy()
    {
        if (UseRun == true)
        {
            var start_bundle = BattleBase.Instance.tween_system.GetBundle("start");
            start_bundle.Delay = play_start_delay;

            foreach (var creature in enemies)
            {
                if (creature == null)
                    continue;

                var creature_tween = creature.Character.GetComponent<HFX_TweenSystem>();

                var sp_bundle = creature_tween.GetPlayingBundle("start_preset");
                if (sp_bundle != null)
                    sp_bundle.groups[0].tweens[0].Duration = sp_bundle.PlaybackTime + play_start_delay;

                start_bundle.Play(creature_tween, creature.Character.transform, null, 1f);
                creature_tween.UpdatePlay(creature_tween.PlaybackTime);
            }
            if (Tutorial.Instance.CheckCutScene(eSceneType.PreAll) == true)
                WaveStartTime = Time.time + 1.5f + play_start_delay;
            else
                WaveStartTime = Time.time + 1f + play_start_delay;
        }

    }

    protected void InitCreatures()
    {
        List<float> start_times = new List<float>();
        for (int i = 0; i < characters.Count; ++i)
            start_times.Add(i * 2f + Rand.NextRange(0f, BattleConfig.Instance.StartRange) * BattleConfig.Instance.AttackCoolTimeMax);

        for (int i = 0; i < characters.Count; i++)
        {
            int start_time_index = Rand.NextRange(0, start_times.Count - 1);
            float start_time = start_times[start_time_index];
            start_times.RemoveAt(start_time_index);

            BattleCreature creature = characters[i] as BattleCreature;
            creature.InitContainer(battle_layout.m_Mine.m_Characters[i]);

            if (UseRun == true)
            {
                var creature_tween = creature.Character.GetComponent<HFX_TweenSystem>();
                creature_tween.Stop();

                BattleBase.Instance.tween_system.Play("start_preset", null, creature_tween, creature.Character.transform);
                creature_tween.UpdatePlay(0f);
            }

            creature.Restart(start_time);
        }

        for (int i = 0; i < enemies.Count; ++i)
            start_times.Add(i * 2f + Rand.NextRange(0f, BattleConfig.Instance.StartRange) * BattleConfig.Instance.AttackCoolTimeMax);

        for (int i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i] == null)
                continue;

            int start_time_index = Rand.NextRange(0, start_times.Count - 1);
            float start_time = start_times[start_time_index];
            start_times.RemoveAt(start_time_index);

            BattleCreature creature = enemies[i] as BattleCreature;

            if (UseRun == true)
            {
                var creature_tween = creature.Character.GetComponent<HFX_TweenSystem>();
                creature_tween.Stop();

                BattleBase.Instance.tween_system.Play("start_preset", null, creature_tween, creature.Character.transform);
                creature_tween.UpdatePlay(0f);
            }

            creature.Restart(start_time);
        }
    }

    bool is_vibrate = false;
    virtual public void OnFinishBattle(bool isPlaying = false)
    {
        if (is_vibrate == false && ConfigData.Instance.UseVibrate && isPlaying == false)
        {
            is_vibrate = true;
            Handheld.Vibrate();
        }

        Popup.PopupInfo info = Popup.Instance.GetCurrentPopup();
        if (info != null)
            info.Obj.OnClose();
    }

    virtual public void OnWin() { }

    public override void SetActionMode(bool set_mode, eActionMode mode, ICreature leader_creature, bool need_backup_scale)
    {
        Tooltip.Instance.CloseAllTooltip();
        base.SetActionMode(set_mode, mode, leader_creature, need_backup_scale);
    }
}
