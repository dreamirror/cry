#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using HeroFX;

public enum eCharacterDummyMode
{
    None,
    Hidden,
    Dummy,
    Active,
}

[ExecuteInEditMode]
[RequireComponent(typeof(CharacterAnimation))]
[RequireComponent(typeof(HFX_TweenSystem))]
[AddComponentMenu("SmallHeroes/Character")]
public class Character : MonoBehaviour, IHFX_TweenAction, IAssetObject
{
    public class MoveProperty
    {
        public MoveProperty(float scale, float move_scale, float fx_height)
        {
            Scale = scale;
            MoveScale = move_scale;
            FxHeight = fx_height;
        }
        public float Scale;
        public float MoveScale;
        public float FxHeight;
    }

    public class AnimationAdoptor
    {
        public CharacterAnimation Animation { get; private set; }
        float PlaybackTime = 0f;

        public bool IsPlaying { get { return Animation.IsPlayingAction; } }
        public float Length { get { return Animation.CurrentState==null?0f:Animation.CurrentState.length; } }

        public AnimationAdoptor(CharacterAnimation animation)
        {
            this.Animation = animation;
            Animation.IsPause = true;
            PlaybackTime = MNS.Random.Instance.NextRange(0f, 10f);
        }

        public float Play(string name, float time)
        {
            AnimationState state = Animation.Play(true, name);
            if (state != null && state != Animation.DamageState)
            {
                PlaybackTime = time;
            }
            return Animation.CurrentStateLength;
        }

        public float PlayMove(string name, float time, Transform move_target, MoveProperty move_property)
        {
            AnimationState state = Animation.Play(true, name);
            if (state != null && state != Animation.DamageState)
            {
                PlaybackTime = time;
                if (Animation.CurrentMoveState != null || Animation.CurrentFxState != null)
                {
                    m_MoveTarget = move_target;
                    m_MoveProperty = move_property;
                }
            }
            return Animation.CurrentStateLength;
        }

        Transform m_MoveTarget = null;
        MoveProperty m_MoveProperty;
        void CalculateMoveValue()
        {
            float fixed_length = Animation.CurrentMoveState != null ? Animation.CurrentMoveState.fixed_length : 0f;
            float fixed_length_fx = Animation.CurrentFxState != null ? Animation.CurrentFxState.fixed_length : 0f;

            if (Animation.CurrentMoveState == null && Animation.CurrentFxState == null)
                m_MoveTarget = null;

            if (m_MoveTarget == null)
                return;

            Vector3 move_value = Vector3.zero;
            if (m_MoveTarget != null)
            {
                var container = m_MoveTarget.gameObject.GetComponent<CharacterContainer>();
                if (container != null)
                {
                    Vector3 delta = m_MoveTarget.localPosition + container.TargetPositionDelta;
                    delta.Scale(m_MoveTarget.lossyScale);
                    move_value = (m_MoveTarget.parent.position + delta) - Animation.transform.parent.position;
                }
                else
                    move_value = m_MoveTarget.position - Animation.transform.parent.position;
            }
            move_value.y = 0f;
            if (Animation.transform.parent.lossyScale.x < 0)
                move_value.x *= -1f;

            move_value.z -= 2f;

            if (Animation.CurrentMoveState != null)
            {
                Vector3 move_value_move = move_value;
                Vector3 move_value_unscaled = move_value;

                float length = 26f + fixed_length;
                move_value_unscaled.x = Mathf.Max(0f, move_value_move.x - length);
                move_value_move.x = move_value_move.x - length * (1f + (m_MoveProperty.Scale - 1f) * m_MoveProperty.MoveScale);
                Animation.MoveValue = move_value_move;
                Animation.MoveValueUnscaled = move_value_unscaled;
            }
            else
            {
                Animation.MoveValue = Vector3.zero;
                Animation.MoveValueUnscaled = Vector3.zero;
            }


            if (Animation.CurrentFxState != null)
            {
                float length = 26f + fixed_length_fx;
                move_value.x = move_value.x - length * (1f + (m_MoveProperty.Scale - 1f) * m_MoveProperty.MoveScale);
                move_value.y -= m_MoveProperty.FxHeight * (m_MoveProperty.Scale - 1f);
                Animation.MoveValueFx = move_value;
            }
            else
                Animation.MoveValueFx = Vector3.zero;
        }

        public void CancelAnimation(float time)
        {
            PlaybackTime = time;
            m_MoveTarget = null;
            Animation.CancelAnimation();
        }

        public void Update(float time, bool is_freeze)
        {
            if (is_freeze && Animation.DummyMode == eCharacterDummyMode.None)
            {
                float delta_time = (time - PlaybackTime) - Animation.PlaybackTime;
                PlaybackTime += delta_time;
                Animation.DamageTime -= delta_time;
                //                Animation.UpdatePlay(time - PlaybackTime);
            }
            else
            {
                CalculateMoveValue();
                Animation.UpdatePlay(time - PlaybackTime);
            }
//            if (animation.IsPlayingAction == false)
        }
    }

    //static Color m_DefaultColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    SkinnedMeshRenderer m_Renderer = null;
    SkinnedMeshRenderer Renderer
    {
        get
        {
            if (m_Renderer == null)
                m_Renderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            return m_Renderer;
        }
    }

    public ICreature Creature { get; set; }

    AnimationAdoptor m_AnimationAdoptor;
    public CharacterActionData[] Actions;
    public HFX_TweenSystem Tween { get; private set; }

    List<ICharacterAction> m_PlayingActions = new List<ICharacterAction>();
    public List<ICharacterAction> PlayingActions { get { return m_PlayingActions; } }
    public CharacterAction MainAction { get; private set; }
    public bool IsPlayingAction { get { return MainAction != null && MainAction.IsPlaying; } }
    public bool IsPlayingActionAnimation { get { return MainAction != null && MainAction.IsPlayingAnimation; } }

    public bool IgnoreTween { get; set; }

    public float PlaybackTime { get; private set; }

    [NonSerialized]
    public float PlaybackSpeed = 1f;
    [NonSerialized]
    public bool IsPause = false;

    [NonSerialized]
    public bool IsUpdateAnimation = true;

    public bool IsPlaying { get { return m_AnimationAdoptor.IsPlaying; } }
    public Animation Animation { get { return m_AnimationAdoptor.Animation.Animation; } }
    public CharacterAnimation CharacterAnimation { get { return m_AnimationAdoptor.Animation; } }
    public HFX_ParticleSystem m_DefaultEffect;
    public HFX_ParticleSystem m_PlayingDefaultEffect { get; private set; }

    List<HFX_ParticleSystem> m_PlayingParticles = new List<HFX_ParticleSystem>();

    int m_FreezeCount = 0;

    public void SetFreeze()
    {
        ++m_FreezeCount;
    }

    public void UnsetFreeze()
    {
        --m_FreezeCount;
    }

    public void ClearFreeze()
    {
        m_FreezeCount = 0;
    }

    void Awake()
    {
        Tween = GetComponent<HFX_TweenSystem>();
        Tween.IsPause = true;

        if (Application.isPlaying)
            m_AnimationAdoptor = new AnimationAdoptor(GetComponent<CharacterAnimation>());
    }

    void Start()
    {
    }

    void OnDisable()
    {
        m_PlayingParticles.ForEach(p => GameObject.Destroy(p.gameObject));
        m_PlayingParticles.Clear();
    }

    //             m_Material.SetFloat("_Gray", 1f);

    public void Update()
    {
        if (IsPause == true || gameObject.activeInHierarchy == false)
            return;

        UpdatePlay(PlaybackTime + Time.deltaTime * PlaybackSpeed);
    }

    public void UpdatePlay(float playback_time)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        try
        {
            PlaybackTime = playback_time;

            if (m_PlayingActions.Count > 0)
            {
                for (int i = 0; i < m_PlayingActions.Count; )
                {
                    ICharacterAction action = m_PlayingActions[i];
                    if ((BattleBase.Instance == null || BattleBase.Instance.IsPause != ePauseType.Pause || action.IsPause == false) && action.Update(PlaybackTime) == false)
                    {
                        m_PlayingActions.RemoveAt(i);
                        if (action == MainAction)
                            MainAction = null;
                    }
                    else
                        ++i;
                }
            }

            if (m_AnimationAdoptor != null && IsUpdateAnimation)
                m_AnimationAdoptor.Update(PlaybackTime, m_FreezeCount > 0);
            Tween.UpdatePlay(PlaybackTime);

            UpdateParticles();
            if (Application.isPlaying == true && m_DefaultEffect != null && m_PlayingDefaultEffect == null)
                m_PlayingDefaultEffect = PlayParticle(m_DefaultEffect);
            else if (m_PlayingDefaultEffect != null && m_PlayingDefaultEffect.IsPlaying == true && m_PlayingDefaultEffect.IsFinish == false && CharacterAnimation.IsDeadEnd)
            {
                m_PlayingDefaultEffect.Finish();
            }
        }
        catch(System.Exception ex)
        {
            throw new System.Exception(string.Format("[{0}] {1}", gameObject.name, ex.Message), ex);
        }
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
                    m_PlayingParticles.RemoveAt(i);
                }
                else
                    ++i;
            }
        }
    }

    void LateUpdate()
    {
    }

    public float PlayAnimation(string name)
    {
        if (m_AnimationAdoptor != null)
            return m_AnimationAdoptor.Play(name, PlaybackTime);
        return 0f;
    }

    public float PlayAnimationMove(float playback_time, string name, Transform move_target, Character.MoveProperty move_property)
    {
        if (m_AnimationAdoptor != null)
            return m_AnimationAdoptor.PlayMove(name, playback_time, move_target, move_property);
        return 0f;
    }

    public void CancelAnimation()
    {
        if (m_AnimationAdoptor != null)
            m_AnimationAdoptor.CancelAnimation(PlaybackTime);
    }

    public CharacterActionData GetAction(int index)
    {
        if (index < 0 || index >= Actions.Length)
        {
            throw new System.Exception(string.Format("invalid action index in {0} : {1}", name, index));
        }

        return Actions[index];
    }

    public CharacterActionData GetAction(string action_name)
    {
        CharacterActionData action = Array.Find(Actions, a => a.AnimationName == action_name);
        if (action == null)
        {
            throw new System.Exception(string.Format("can't find action in {0} : {1}", name, action_name));
        }
        return action;
    }

    public bool PlayAction(string action_name)
    {
        return PlayAction(action_name, false, 1f, 0f);
    }

    public bool PlayAction(string action_name, bool is_lighting, float move_scale, float duration)
    {
        CharacterActionData action = Array.Find(Actions, a => a.AnimationName == action_name);
        if (action != null)
        {
            AddAction(action.CreateAction(PlaybackTime, this, null, is_lighting, move_scale, duration));
        }
        else
        {
            if (m_AnimationAdoptor != null)
                return m_AnimationAdoptor.Play(action_name, PlaybackTime) != 0f;
        }
        return true;
    }

#if UNITY_EDITOR
    public void DoActionEditor(int index, SkillTargetContainer target, bool is_lighting, float scale, float duration)
    {
        DoAction(0f, GetAction(index).AnimationName, target, is_lighting, scale, duration);
    }
#endif

    public CharacterAction DoAction(float delay, string action_name, SkillTargetContainer target, bool is_lighting, float scale, float duration)
    {
        try
        {
            MainAction = GetAction(action_name).CreateAction(PlaybackTime + delay, this, target, is_lighting, Mathf.Max(scale, Creature.Scale), duration);
            AddAction(MainAction);
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(string.Format("[{0}:{1}] {2}", gameObject.name, action_name, ex.Message), ex);
        }

        return MainAction;
    }

    public void AddAction(ICharacterAction action)
    {
        m_PlayingActions.Add(action);
    }

    public void PlayHead()
    {
        m_AnimationAdoptor.Animation.PlayHead();
    }

    public void CancelAction()
    {
        CancelAction(false);
    }

    public void CancelAction(bool stopAll)
    {
//        if (MainAction != null)
            CancelAnimation();

        foreach (var action in m_PlayingActions)
        {
            action.Cancel(stopAll);
        }

        if (stopAll)
        {
            m_PlayingActions.Clear();
            //Tween.Stop();

//             foreach (var particle in m_Particles)
//             {
//                 particle.Finish();
//             }
//             m_Particles.Clear();
        }
    }

    public HFX_ParticleSystem PlayParticle(HFX_ParticleSystem particle_prefab)
    {
        HFX_ParticleSystem particle_system = GameObject.Instantiate<HFX_ParticleSystem>(particle_prefab);
        particle_system.transform.SetParent(transform, false);
        particle_system.Play(true, 0);
        m_PlayingParticles.Add(particle_system);

        if (CharacterAnimation.Material.HasProperty("_LightMax"))
            particle_system.SetLightingMax(CharacterAnimation.Material.GetFloat("_LightMax"));
        particle_system.SetLensTilt(CharacterAnimation.IsUIMode == true ? 0.15f : 0f);

        return particle_system;
    }

    public void SetLighting(float lighting_max)
    {
        CharacterAnimation.Material.SetFloat("_LightMax", lighting_max);
        m_PlayingParticles.ForEach(p => p.SetLightingMax(lighting_max));
    }

    public void SetUIMode(bool value)
    {
        CharacterAnimation.SetUIMode(value);
        m_PlayingParticles.ForEach(p => p.SetLensTilt(value == true ? 0.15f : 0f));
    }

    public void ResetColor()
    {
        CharacterAnimation.Reset();
    }

    public void Reset()
    {
        BattleBase.LightingTarget.SetType(this, BattleBase.eLightingType.None);
        SetDummyMode(eCharacterDummyMode.None);
        CancelAction(true);
        ResetColor();
        Tween.Stop();
        ClearFreeze();
        PlaybackTime = 0f;
    }

    public float MoveDistance
    {
        get
        {
            return CharacterAnimation.MoveDistance;
        }
    }

    public bool ContainsAction(string name)
    {
        return CharacterAnimation.ContainsAnimation(name);
    }

    public void SetDummyMode(eCharacterDummyMode mode)
    {
        CharacterAnimation.DummyMode = mode;

        if (mode != eCharacterDummyMode.None)
        {
            if (mode == eCharacterDummyMode.Hidden)
            {
                Renderer.enabled = false;
                foreach (var particle in m_PlayingParticles)
                    particle.SetHidden(true);
            }
            foreach (var action in m_PlayingActions)
            {
                action.SetHidden(true);
            }
            Tween.SetPauseMode(true);
            Transform tween_transform = transform.GetChild(0);
            tween_transform.localPosition = Vector3.zero;
            tween_transform.localRotation = Quaternion.identity;
            tween_transform.localScale = Vector3.one;
        }
        else
        {
            if (Renderer.enabled == false)
            {
                Renderer.enabled = true;
                foreach (var particle in m_PlayingParticles)
                    particle.SetHidden(false);
            }
            foreach (var action in m_PlayingActions)
            {
                action.SetHidden(false);
            }
            Tween.SetPauseMode(false);
        }
    }

    public void OnAlloc()
    {
    }

    public void OnFree()
    {
    }

    public void InitPrefab()
    {
    }
}
