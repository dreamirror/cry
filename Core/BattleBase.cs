using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;

public enum eBattleMode
{
    None,
    Battle,
    BattleWorldboss,
    PVP,
    RVR,
}

public enum ePauseType
{
    None,
    Slow,
    Pause,
}

public class BattleConfig
{
    static BattleConfig m_Instance = null;
    static public BattleConfig Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new BattleConfig();
            return m_Instance;
        }
    }

    public class MPData
    {
        public MPData(int mp)
        {
            this.MP = mp;
        }
        public int MP;
    }

    public bool UseCritical = true;
    public bool UseSkill = true;

    public float SlowSpeed = 0.5f;
    public float SkillDelay = 0.2f;

    public float StartRange = 0.04f;

    public float AttackCoolTimeMin = 5f;
    public float AttackCoolTimeMax = 7f;

    public float AttackCoolTimePercent = 0.5f;

    public float HitDistance = 10f;
    public int MinDamage = 100;
    public int MinDefense = 310;

    public int MPMax = 10000;

    public MPData[] MP = new MPData[(int)eMPFillType.Max];

    public BattleConfig()
    {
        MP[(int)eMPFillType.Deal] = new MPData(5000);
        MP[(int)eMPFillType.Damage] = new MPData(5000);
        MP[(int)eMPFillType.Heal] = new MPData(5000);
        MP[(int)eMPFillType.HealMana] = new MPData(5000);
        MP[(int)eMPFillType.Action] = new MPData(2500);
    }

    public float MPFill = 10000f / 15f;
}

abstract public class BattleBase : MonoBehaviour
{
    static public eBattleMode CurrentBattleMode { get; set; }

    static public BattleBase Instance { get; protected set; }

    public TweenPosition m_CameraTween;

    public SkillCamera m_SkillCamera;
    public HFX_ParticleSystem m_SkillCasting, m_SkillTargetTeam, m_SkillTargetEnemy, m_Miss;

    public GameObject m_Bottom;

    public HeroFX.HFX_TweenSystem tween_system;
    public ColorContainer color_container;
    public GameObject HPBarCanvas, CharacterSkillCanvas;

    MNS.Random m_Rand = new MNS.Random();
    public MNS.Random Rand { get { return m_Rand; } }
    public List<ICreature> characters { get; protected set; }
    public List<ICreature> dead_characters { get; protected set; }
    public List<ICreature> enemies { get; protected set; }

    [NonSerialized]
    public bool IsBattleEnd = false;
    [NonSerialized]
    public bool IsBattleStart = false;

    public bool IsAuto { get; protected set; }
    public bool IsFast { get; protected set; }

    [NonSerialized]
    public ePauseType IsPause = ePauseType.None;

    protected float deltaTimeIgnore;
    protected float deltaTime;

    public Light m_Light;

    public GameObject[] resources;
    public BattleLayout battle_layout;

    virtual protected void Awake()
    {
        BattleBase.Instance = this;
    }

    virtual protected void Start()
    {
        if (m_SkillCamera != null)
            m_SkillCamera.gameObject.SetActive(false);
    }

    public float PlaybackTime { get; protected set; }
    List<HFX_ParticleSystem> m_PlayingParticles = new List<HFX_ParticleSystem>();

    void OnDestroy()
    {
        Instance = null;
    }

    void OnDisable()
    {
    }

    void UpdateParticles()
    {
        if (m_PlayingParticles.Count > 0)
        {
            for (int i = 0; i < m_PlayingParticles.Count;)
            {
                HFX_ParticleSystem particle = m_PlayingParticles[i];
                if (particle.UpdatePlay(PlaybackTime) == false && BattleBase.Instance.IsPause != ePauseType.Pause)
                {
                    GameObject.DestroyImmediate(particle.gameObject);
                    m_PlayingParticles.RemoveAt(i);
                }
                else
                    ++i;
            }
        }
    }

    void UpdateCreatures()
    {
        if (characters == null || characters.Count == 0)
            return;

        characters.ForEach(c => c.Update(deltaTime, deltaTimeIgnore));
        if (enemies != null)
            enemies.ForEach(c => { if (c != null) c.Update(deltaTime, deltaTimeIgnore); });
    }

    virtual protected void Update()
    {
        deltaTimeIgnore = Time.deltaTime;
        deltaTime = deltaTimeIgnore;
        switch (IsPause)
        {
            case ePauseType.Pause:
                deltaTime = deltaTimeIgnore;
                break;

            case ePauseType.Slow:
                deltaTime = deltaTimeIgnore * BattleConfig.Instance.SlowSpeed;
                break;
        }

        if ((IsBattleStart == true || IsBattleEnd == true) && IsPause != ePauseType.Pause)
        {
            PlaybackTime += deltaTimeIgnore;
//             Debug.LogFormat("PlaybackTime : {0}", PlaybackTime);
        }

        UpdateCreatures();

        UpdateParticles();
        UpdateLighting();
    }

    protected List<LightingCreature> m_LightingCreatures = new List<LightingCreature>();
    protected List<LightingTarget> m_LightingTargets = new List<LightingTarget>();
    public bool IsLighting { get; private set; }
    static readonly float LightingFadeTime = 0.08f, LightingPercent = 0.5f;
    float m_LightingStart = 0f, m_LightingEnd = 0f;
    static public readonly float LightingScaleValue = 2f;

    protected class LightingCreature
    {
        public float StartTime { get; private set; }
        public float EndSlowTime { get; private set; }
        public float EndScaleTime { get; private set; }

        float JumpScale { get; set; }

        static readonly float ScaleUpTime = 0.08f, ScaleDownTime = 0.08f;

        public LightingCreature(ICreature creature, float slow_time, float scale_time, float jump_Scale)
        {
            //            Debug.LogFormat("LightingCreature : {0}", creature.Info.Name);

            BattleBase.LightingTarget.SetType(creature.Character, BattleBase.eLightingType.Active);

            StartTime = creature.PlaybackTime;
            EndSlowTime = StartTime + slow_time;
            if (scale_time == 0f)
                EndScaleTime = 0f;
            else
                EndScaleTime = StartTime + scale_time;
            JumpScale = jump_Scale;

            Creature = creature;
            Creature.SetLighting(true);
            Creature.IgnoreSpeed = true;
        }

        public void SetEnd(bool finish)
        {
            float playback_time = Creature.PlaybackTime;
            if (playback_time < EndSlowTime)
                EndSlowTime = playback_time;
            if (finish == true && playback_time < EndScaleTime)
                EndScaleTime = playback_time;
        }

        public bool Update()
        {
            if (IsFinish == true)
                return false;

            float playback_time = Creature.PlaybackTime;
            float local_time = playback_time - StartTime;
            float percent = 0f;
            if (IsEnd == false && EndSlowTime <= playback_time)
            {
                IsEnd = true;
                Creature.IgnoreSpeed = false;
                Creature.SetLighting(false);
            }

            if (EndScaleTime != 0f)
            {
                if (EndScaleTime <= playback_time)
                {
                    if (EndScaleTime + ScaleDownTime <= playback_time)
                    {
                        if (IsEnd == true)
                        {
                            Finish();
                            return false;
                        }
                        else
                            return true;
                    }
                    else
                    {
                        percent = 1f - (playback_time - EndScaleTime) / ScaleDownTime;
                    }
                }
                else if (local_time < ScaleUpTime)
                {
                    percent = local_time / ScaleUpTime;
                }
                else
                    percent = 1f;
            }
            else
            {
                if (IsEnd == true)
                {
                    Finish();
                    return false;
                }
            }

            float scale = Mathf.Lerp(Creature.Scale, LightingScaleValue, percent);
            Creature.Character.transform.localScale = Vector3.one * scale;
            Creature.Character.CharacterAnimation.JumpScaleValue = scale * (1f-JumpScale) - 1f;


            return true;
        }

        public void Finish()
        {
            //            Debug.LogFormat("LightingCreature Finish : {0}", Creature.Info.Name);

            IsFinish = true;
            Creature.Character.transform.localScale = Vector3.one * Creature.Scale;
            Creature.Character.CharacterAnimation.JumpScaleValue = 0f;
            BattleBase.LightingTarget.SetType(Creature.Character, BattleBase.eLightingType.None);
        }

        public ICreature Creature { get; private set; }
        public bool IsEnd { get; private set; }
        public bool IsFinish { get; private set; }
    }

    public void AddLighting(ICreature lighting_creature, float slow_time, float scale_time, float jump_scale)
    {
        //        time -= 0.2f;
        ClearLightingTarget();

#if !SH_ASSETBUNDLE
        if (ConfigData.Instance.UseBattleEffect == false)
            scale_time = 0f;
#endif

        foreach (var creature in m_LightingCreatures)
        {
            if (creature.Creature.IsTeam != lighting_creature.IsTeam)
                creature.SetEnd(false);
        }

        m_LightingCreatures.Add(new LightingCreature(lighting_creature, Mathf.Min(slow_time, 1.2f + BattleConfig.Instance.SkillDelay), scale_time, jump_scale));

        IsLighting = true;
        if (IsPause == ePauseType.None)
        {
            m_LightingStart = PlaybackTime;

            IsPause = ePauseType.Slow;
        }
        m_LightingEnd = 0f;
    }

    public enum eLightingType
    {
        None,
        Active,
        Enemy,
        Team,
    }

    public class LightingTarget
    {
        public Character Character { get; private set; }
        public eLightingType Type { get; private set; }

        public LightingTarget(Character character, eLightingType type)
        {
            Character = character;
            Type = type;
            SetType();
        }

        public void SetType()
        {
            SetType(Character, Type);
        }

        public void Clear()
        {
            SetType(Character, eLightingType.None);
        }

        static public void SetType(Character character, eLightingType type)
        {
            if (character == null) return;
            switch (type)
            {
                case eLightingType.None:
                    character.CharacterAnimation.SetRimColor(new Color32(255, 255, 255, 0));
                    break;

                case eLightingType.Active:
                    character.CharacterAnimation.SetRimColor(new Color32(255, 255, 255, 255));
                    break;

                case eLightingType.Team:
                    {
                        character.CharacterAnimation.SetRimColor(new Color32(64, 255, 64, 255));
                    }
                    break;

                case eLightingType.Enemy:
                    {
                        character.CharacterAnimation.SetRimColor(new Color32(255, 64, 64, 255));
                    }
                    break;
            }
        }
    }

    public void AddLightingTargets(bool is_team, List<ISkillTarget> targets, ICreature active_creature)
    {
        foreach (var target in targets)
        {
            if (target == null || target.Character == null || target.Character.Creature.IsDead == true || target.Character.Creature == active_creature)
                continue;

            if (IsPause == ePauseType.Pause)
            {
                if (is_team == target.Character.Creature.IsTeam)
                    LightingTarget.SetType(target.Character, eLightingType.Team);
                else
                    LightingTarget.SetType(target.Character, eLightingType.Enemy);
            }
            else
            {
                m_LightingTargets.RemoveAll(t => t.Character == target.Character);
                if (is_team == target.Character.Creature.IsTeam)
                    m_LightingTargets.Add(new LightingTarget(target.Character, eLightingType.Team));
                else
                    m_LightingTargets.Add(new LightingTarget(target.Character, eLightingType.Enemy));
            }
        }

    }


    public void RemoveLighting(ICreature creature)
    {
        LightingCreature lighting_creature = m_LightingCreatures.Find(c => c.Creature == creature);
        if (lighting_creature == null)
            return;

        lighting_creature.SetEnd(true);
    }

    void UpdateLightingCreatures()
    {
        if (m_LightingCreatures.Count == 0)
            return;

        bool end = true;
        for (int i = 0; i < m_LightingCreatures.Count;)
        {
            LightingCreature lighting_creature = m_LightingCreatures[i];
            if (lighting_creature.Update() == true)
            {
                if (lighting_creature.IsEnd == false)
                    end = false;
                ++i;
            }
            else
                m_LightingCreatures.RemoveAt(i);
        }
        if (end == true)
            EndLighting();
    }

    void UpdateLighting()
    {
        if (IsLighting == false || IsPause == ePauseType.Pause)
        {
            return;
        }

        UpdateLightingCreatures();

        if (m_LightingCreatures.Count == 0 && m_LightingEnd + LightingFadeTime < PlaybackTime)
        {
            ClearLighting();
            return;
        }

        if (m_LightingStart != 0f && PlaybackTime - m_LightingStart < LightingFadeTime)
        {
            float percent = (PlaybackTime - m_LightingStart) / LightingFadeTime;
            m_Light.intensity = Mathf.Clamp01(1f - LightingPercent * percent);
        }
        else if (m_LightingEnd != 0f)
        {
            float percent = 1f - (PlaybackTime - m_LightingEnd) / LightingFadeTime;
            m_Light.intensity = Mathf.Clamp01(1f - LightingPercent * percent);
        }
        else
        {
            m_Light.intensity = Mathf.Clamp01(1f - LightingPercent);
        }
    }

    void EndLighting()
    {
        if (IsLighting == true && m_LightingEnd == 0f)
        {
            m_LightingEnd = PlaybackTime;
            if (IsPause == ePauseType.Slow)
                IsPause = ePauseType.None;
        }
    }

    void ClearLightingTarget()
    {
        if (m_LightingTargets.Count == 0)
            return;

        m_LightingTargets.ForEach(c => { c.Clear(); });
        m_LightingTargets.Clear();
    }

    void ClearLighting(bool force = false)
    {
        ClearLightingTarget();
        if (force == true)
        {
            foreach (var creature in m_LightingCreatures)
            {
                creature.Finish();
            }
        }

        IsLighting = false;
        m_Light.intensity = 1f;

        m_LightingStart = 0f;
        m_LightingEnd = 0f;
    }

    public float NextAttackTime()
    {
        return Rand.NextRange(BattleConfig.Instance.AttackCoolTimeMin, BattleConfig.Instance.AttackCoolTimeMax);
    }

    public float NextAttackTimePercent()
    {
        int creature_count = Math.Max(BattleBase.Instance.characters.Count(c => c.Character != null && c.IsDead == false), BattleBase.Instance.enemies.Count(c => c != null && c.Character != null && c.IsDead == false));
        return (1f - (BattleConfig.Instance.AttackCoolTimePercent * (1f - ((creature_count - 1) * 0.25f))));
    }

    public enum eActionMode
    {
        TeamHidden,
        AllHidden,
        EnemyHidden,
        NotHidden,
    }

    float backup_scale = 1f;
    CharacterContainer backup_container = null;
    Camera backup_camera = null;
    public bool IsActionMode { get { return backup_container != null; } }
    virtual public void SetActionMode(bool set_mode, eActionMode mode, ICreature leader_creature, bool need_backup_scale)
    {
        if (BattleBase.CurrentBattleMode == eBattleMode.None)
            return;

        if (set_mode)
        {
            IsPause = ePauseType.Pause;
            m_Light.intensity = 1f;
            TextManager.Instance.Clear();

            backup_container = leader_creature.Container;

            foreach (var creature in m_LightingCreatures)
            {
                creature.Creature.Character.transform.localScale = Vector3.one * creature.Creature.Scale;
            }

            List<ICreature> team_creatures = null;
            List<ICreature> enemy_creatures = null;

            if (leader_creature.IsTeam)
            {
                leader_creature.Character.transform.SetParent(battle_layout.m_Mine.Center.transform, false);
                team_creatures = characters;
                enemy_creatures = enemies;
            }
            else
            {
                leader_creature.Character.transform.SetParent(battle_layout.m_Enemy.Center.transform, false);
                team_creatures = enemies;
                enemy_creatures = characters;
            }

            backup_scale = leader_creature.Scale;
            if (need_backup_scale)
            {
                leader_creature.Character.transform.localScale = Vector3.one;
                leader_creature.Scale = 1f;
            }

            foreach (var creature in team_creatures)
            {
                eCharacterDummyMode dummy_mode = eCharacterDummyMode.Hidden;
                if (creature == leader_creature)
                    dummy_mode = eCharacterDummyMode.Active;
                else if (mode == eActionMode.NotHidden || mode == eActionMode.EnemyHidden)
                    dummy_mode = eCharacterDummyMode.Dummy;

                creature.SetDummyMode(dummy_mode);
                if (dummy_mode == eCharacterDummyMode.Active)
                    LightingTarget.SetType(creature.Character, eLightingType.Active);
            }

            foreach (var creature in enemy_creatures)
            {
                eCharacterDummyMode dummy_mode = eCharacterDummyMode.Hidden;
                if (mode == eActionMode.NotHidden || mode == eActionMode.TeamHidden)
                    dummy_mode = eCharacterDummyMode.Dummy;
                creature.SetDummyMode(dummy_mode);
            }

            foreach (var particle in m_PlayingParticles)
            {
                particle.SetHidden(true);
            }

            m_SkillCamera.transform.localPosition = Vector3.zero;
            m_SkillCamera.transform.localRotation = Quaternion.identity;
            m_SkillCamera.gameObject.SetActive(true);
            backup_camera = Camera.main;
            backup_camera.enabled = false;
            if (m_Bottom != null)
                m_Bottom.SetActive(false);
        }
        else
        {
            backup_camera.enabled = true;
            m_SkillCamera.gameObject.SetActive(false);
            IsPause = ePauseType.None;
            if (m_Bottom != null)
                m_Bottom.SetActive(true);

            leader_creature.Character.transform.SetParent(backup_container.transform, false);
            leader_creature.Scale = backup_scale;
            leader_creature.Character.transform.localScale = Vector3.one * backup_scale;
            backup_container = null;

            //             LightingTarget.SetType(leader_creature.Character, eLightingType.None);

            List<ICreature> team_creatures = null;
            List<ICreature> enemy_creatures = null;
            if (leader_creature.IsTeam)
            {
                team_creatures = characters;
                enemy_creatures = enemies;
            }
            else
            {
                team_creatures = enemies;
                enemy_creatures = characters;
            }

            foreach (var creature in team_creatures)
            {
                if (creature != null)
                {
                    creature.SetDummyMode(eCharacterDummyMode.None);
                    var target = m_LightingTargets.Find(t => t.Character == creature.Character);
                    if (target != null)
                        target.SetType();
                    else
                        LightingTarget.SetType(creature.Character, eLightingType.None);
                }
            }
            foreach (var creature in enemy_creatures)
            {
                if (creature != null)
                {
                    creature.SetDummyMode(eCharacterDummyMode.None);
                    var target = m_LightingTargets.Find(t => t.Character == creature.Character);
                    if (target != null)
                        target.SetType();
                    else
                        LightingTarget.SetType(creature.Character, eLightingType.None);
                }
            }

            foreach (var particle in m_PlayingParticles)
            {
                particle.SetHidden(false);
            }

        }
    }

    public HFX_ParticleSystem PlayParticle(Transform parent, HFX_ParticleSystem prefab)
    {
        HFX_ParticleSystem system = GameObject.Instantiate<HFX_ParticleSystem>(prefab);
        system.transform.SetParent(parent, false);
        system.SetLightingMax(1f);
        system.Delay = PlaybackTime;
        system.Play(true, 0);

        m_PlayingParticles.Add(system);
        return system;
    }

    public void DoCamera(float power, float duration)
    {
        if (m_CameraTween == null)
        {
            Debug.LogError("no camera tween");
            return;
        }
        m_CameraTween.ResetToBeginning();
        m_CameraTween.duration = duration;
        m_CameraTween.to.y = power;
        m_CameraTween.PlayForward();
    }
}

