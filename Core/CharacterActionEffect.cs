#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using HeroFX;
using System.Collections.Generic;

abstract public class CharacterActionBase
{
    public bool IsEnable = true;
    public float time = 0f;
    public bool IsPlaying { get; protected set; }
    public bool IsStarted { get; protected set; }
    public bool IsLighting { get; protected set; }
    public bool IsHidden { get; protected set; }
    public float ActionScale { get; protected set; }

    virtual public bool Update(float playback_time)
    {
        if (IsStarted == false)
        {
            if (playback_time >= time)
                Start(playback_time);
        }
        return true;
    }

    virtual public void Cancel(bool stopAll)
    {
        if (IsStarted == false)
            IsPlaying = false;
    }

    virtual public void Play(bool is_lighting, float scale)
    {
        IsPlaying = true;
        IsLighting = is_lighting;
        ActionScale = scale;
    }

    virtual protected void Start(float playback_time)
    {
        IsStarted = true;
    }

    virtual public void SetHidden(bool hidden)
    {
        IsHidden = hidden;
    }
}

[Serializable]
public class SoundInfo
{
    public AudioClip sound;
    public float time = 0f;
}

abstract public class CharacterAction_EffectBase : CharacterActionBase
{
    public HFX_ParticleSystem particle_system_prefab;
    public SoundInfo[] sound_list;
    public float sound_tick = 0f;
    public int sound_count = 1;
    public bool sound_loop = false;
    public Vector3 particle_position = Vector3.zero;
    public Vector3 particle_scale = Vector3.one;

    [NonSerialized]
    protected ICharacterActionData m_ActionData;

    protected SoundPlay sound_play;

    [NonSerialized]
    protected Character target;

    [NonSerialized]
    protected HFX_ParticleSystem particle_system;
    [NonSerialized]
    protected HFX_TweenSystem tween_system;

    virtual protected bool IsCancelParticle { get { return true; } }

    protected void CloneData(ICharacterActionData action_data, CharacterAction_EffectBase new_effect)
    {
        new_effect.m_ActionData = action_data;
        new_effect.time = this.time;
        new_effect.particle_system_prefab = this.particle_system_prefab;
        new_effect.particle_position = this.particle_position;
        new_effect.particle_scale = this.particle_scale;
        if (action_data == null)
            new_effect.sound_list = this.sound_list.ToArray();
        else
            new_effect.sound_list = this.sound_list;
        new_effect.sound_tick = this.sound_tick;
        new_effect.sound_count = this.sound_count;
        new_effect.sound_loop = this.sound_loop;
    }

    virtual public void Finish()
    {
        if (particle_system != null)
        {
            GameObject.Destroy(particle_system.gameObject);
            particle_system = null;
        }
        if (sound_play != null)
        {
            sound_play.Finish();
            sound_play = null;
        }
    }

    override public bool Update(float playback_time)
    {
        base.Update(playback_time);
        
        if (IsStarted == false)
            return true;

        playback_time -= time;

        bool is_playing = false;
        if (particle_system != null)
        {
            if (particle_system.IsPlaying)
            {
                is_playing = true;
                particle_system.UpdatePlay(playback_time);
            }
            else
            {
                Finish();
            }
        }
        if (tween_system != null)
            tween_system.UpdatePlay(playback_time);
        IsPlaying = is_playing;
        return is_playing;
    }

    override protected void Start(float playback_time)
    {
        base.Start(playback_time);
        if (particle_system_prefab != null)
        {
            particle_system = GameObject.Instantiate<HFX_ParticleSystem>(particle_system_prefab);
            particle_system.transform.SetParent(target.transform, false);
            particle_system.transform.localPosition += particle_position;
            particle_system.transform.localScale = Vector3.Scale(particle_system.transform.localScale, particle_scale);
            particle_system.SetLightingMax(IsLighting ? 1f : 0f);
            particle_system.Play(true, 0);
            particle_system.SetLensTilt(target.CharacterAnimation.IsUIMode ? 0.15f : 0f);
            if (IsHidden)
                particle_system.SetHidden(true);
        }
        if (sound_list != null && sound_list.Length > 0 && sound_list[0].sound != null)
        {
            sound_play = SoundManager.PlaySound(sound_list, - (playback_time - time), sound_tick, sound_count, sound_loop);
        }
    }

    public override void Cancel(bool stopAll)
    {
        base.Cancel(stopAll);

        if (stopAll || IsStarted == false)
            Finish();
        else if (IsCancelParticle && particle_system != null)
            particle_system.Finish();
    }

    public override void SetHidden(bool hidden)
    {
        base.SetHidden(hidden);
        if (particle_system != null)
            particle_system.SetHidden(hidden);
    }

#if UNITY_EDITOR
    virtual public void OnInspectorItem(int index, CharacterAction_EffectBase selected)
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 40f;
        selected.IsEnable = EditorGUILayout.Toggle(selected.IsEnable, GUILayout.Width(20f));
        selected.time = EditorGUILayout.FloatField("Time", selected.time, GUILayout.Width(100f));
        EditorGUIUtility.labelWidth = 50f;
        selected.particle_system_prefab = EditorGUILayout.ObjectField("Particle", selected.particle_system_prefab, typeof(HFX_ParticleSystem), false) as HFX_ParticleSystem;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        selected.particle_position = EditorGUILayout.Vector3Field("Pos", selected.particle_position);
        selected.particle_scale = EditorGUILayout.Vector3Field("Scale", selected.particle_scale);
        EditorGUIUtility.labelWidth = 0f;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 40f;
        if (selected.sound_list == null || selected.sound_list.Length == 0)
        {
            selected.sound_list = new SoundInfo[1];
            selected.sound_list[0] = new SoundInfo();
        }

        if (GUILayout.Button("+", GUILayout.Width(22f)))
        {
            Array.Resize(ref selected.sound_list, selected.sound_list.Length + 1);
            selected.sound_list[selected.sound_list.Length - 1] = new SoundInfo();
        }

        EditorGUIUtility.labelWidth = 40f;
        selected.sound_list[0].sound = EditorGUILayout.ObjectField("Sound", selected.sound_list[0].sound, typeof(AudioClip), false) as AudioClip;
        selected.sound_list[0].time = EditorGUILayout.FloatField("Time", selected.sound_list[0].time, GUILayout.Width(90f));
        selected.sound_count = 1;
        selected.sound_tick = 0;
//        selected.sound_count = EditorGUILayout.IntField("Count", selected.sound_count, GUILayout.Width(90f));
//        selected.sound_tick = EditorGUILayout.FloatField("Tick", selected.sound_tick, GUILayout.Width(90f));
        selected.sound_loop = EditorGUILayout.Toggle("Loop", selected.sound_loop, GUILayout.Width(90f));

        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.EndVertical();

        int remove_index = -1;
        if (selected.sound_list != null && selected.sound_list.Length > 1)
        {
            for (int i = 1; i < selected.sound_list.Length; ++i )
            {
                SoundInfo info = selected.sound_list[i];

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(22f)))
                    remove_index = i;

                info.sound = EditorGUILayout.ObjectField("Sound", info.sound, typeof(AudioClip), false) as AudioClip;
                info.time = EditorGUILayout.FloatField("Time", info.time, GUILayout.Width(70f));
                EditorGUILayout.EndHorizontal();
            }
        }
        if (remove_index != -1)
        {
            List<SoundInfo> temp_list = new List<SoundInfo>(selected.sound_list);
            temp_list.RemoveAt(remove_index);
            selected.sound_list = temp_list.ToArray();
        }

        EditorGUIUtility.labelWidth = 0f;
        EditorGUILayout.EndVertical();
    }
#endif
}

[Serializable]
public class CharacterAction_Effect : CharacterAction_EffectBase
{
    public eAttachParticle AttachType = eAttachParticle.Target;
    [NonSerialized]
    GameObject particle_system_container;

    public CharacterAction_Effect Clone(ICharacterActionData action_data, Character self)
    {
        CharacterAction_Effect new_effect = new CharacterAction_Effect();
        base.CloneData(action_data, new_effect);
        if (action_data != null)
            new_effect.target = self;
        new_effect.AttachType = AttachType;

        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        HFX_ParticleSystem temp_particle_system_prefab = particle_system_prefab;
        particle_system_prefab = null;

        base.Start(playback_time);

        if (temp_particle_system_prefab != null)
        {
            particle_system = GameObject.Instantiate<HFX_ParticleSystem>(temp_particle_system_prefab);
            particle_system.SetLightingMax(IsLighting ? 1f : 0f);
            particle_system.transform.localPosition += particle_position;
            particle_system.ApplyScale(ActionScale);

            switch (AttachType)
            {
                case eAttachParticle.Target:
                case eAttachParticle.TargetScale:
                    particle_system.transform.SetParent(target.transform, false);
                    if (AttachType == eAttachParticle.TargetScale)
                    {
                        Vector3 scale = particle_system.transform.localScale;
                        scale *= ActionScale;
                        particle_system.transform.localScale = scale;
                    }
                    break;

                case eAttachParticle.World:
                case eAttachParticle.WorldScale:
                case eAttachParticle.Center:
                case eAttachParticle.SelfCenter:
                case eAttachParticle.TargetCenter:
                    particle_system_container = new GameObject(particle_system.name + " Container");
                    particle_system.transform.SetParent(particle_system_container.transform, false);
                    particle_system_container.transform.position = target.transform.position;
                    if (AttachType == eAttachParticle.TargetCenter && target.transform.lossyScale.x < 0f || AttachType != eAttachParticle.TargetCenter && target.transform.lossyScale.x < 0f)
                    {
                        Vector3 scale = particle_system_container.transform.localScale;
                        scale.x = -1f;
                        particle_system_container.transform.localScale = scale;
                    }

                    switch (AttachType)
                    {
                        case eAttachParticle.WorldScale:
                            {
                                Vector3 scale = particle_system.transform.localScale;
                                scale *= ActionScale;
                                particle_system.transform.localScale = scale;
                            }
                            break;

                        case eAttachParticle.SelfCenter:
                            {
                                var self_layout = target.transform.parent.parent.GetComponent<CharacterLayout>();
                                particle_system_container.transform.position = self_layout.Center.transform.position;
                            }
                            break;

                        case eAttachParticle.TargetCenter:
                            {
                                var target_layout = target.transform.parent.parent.GetComponent<CharacterLayout>();
                                particle_system_container.transform.position = target_layout.Center.transform.position;
                            }
                            break;

                        case eAttachParticle.Center:
                            particle_system_container.transform.position = Vector3.zero;
                            break;
                    }
                    break;
            }
            particle_system.Play(true, 0);
            particle_system.SetLensTilt(target.CharacterAnimation.IsUIMode ? 0.15f : 0f);
        }
    }

    public override void Finish()
    {
        base.Finish();
        if (particle_system_container != null)
            GameObject.Destroy(particle_system_container);
    }
}

[Serializable]
public class CharacterAction_EffectCasting : CharacterAction_EffectBase
{
    public eAttachParticle AttachType = eAttachParticle.Target;

    [NonSerialized]
    GameObject particle_system_container;

    public CharacterAction_EffectCasting Clone(CharacterActionData action_data, Character self)
    {
        CharacterAction_EffectCasting new_effect = new CharacterAction_EffectCasting();
        base.CloneData(action_data, new_effect);
        new_effect.AttachType = this.AttachType;
        if (action_data != null)
            new_effect.target = self;

        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        HFX_ParticleSystem temp_particle_system_prefab = particle_system_prefab;
        particle_system_prefab = null;

        base.Start(playback_time);

        if (temp_particle_system_prefab != null)
        {
            particle_system = GameObject.Instantiate<HFX_ParticleSystem>(temp_particle_system_prefab);
            particle_system.transform.localPosition += particle_position;
            particle_system.ApplyScale(ActionScale);

            switch (AttachType)
            {
                case eAttachParticle.Self:
                case eAttachParticle.Target:
                    {
                        particle_system.transform.SetParent(target.transform, false);
                    }
                    break;

                case eAttachParticle.World:
                case eAttachParticle.WorldZero:
                case eAttachParticle.WorldScale:
                case eAttachParticle.Center:
                case eAttachParticle.SelfCenter:
                case eAttachParticle.TargetCenter:
                    {
                        particle_system_container = new GameObject(particle_system.name + " Container");
                        particle_system.transform.SetParent(particle_system_container.transform, false);
                        if (AttachType != eAttachParticle.WorldZero)
                            particle_system_container.transform.position = target.transform.position;
                        if (target.transform.lossyScale.x < 0f)
                        {
                            Vector3 scale = particle_system_container.transform.localScale;
                            scale.x = -1f;
                            particle_system_container.transform.localScale = scale;
                        }

                        switch (AttachType)
                        {
                            case eAttachParticle.WorldScale:
                                {
                                    float creature_scale = target.transform.lossyScale.x;
                                    Vector3 scale = particle_system.transform.localScale;
                                    scale *= creature_scale;
                                    particle_system.transform.localScale = scale;

                                    Vector3 pos = particle_system.transform.localPosition;
                                    pos *= creature_scale;
                                    particle_system.transform.localPosition = pos;

                                }
                                break;

                            case eAttachParticle.SelfCenter:
                            case eAttachParticle.TargetCenter:
                                {
                                    Debug.LogWarningFormat("[{0}] Do Not use SelfCenter or TargetCenter in casting", particle_system.name);
                                    var target_layout = target.transform.parent.parent.GetComponent<CharacterLayout>();
                                    if (target_layout)
                                        particle_system_container.transform.position = target_layout.Center.transform.position;
                                }
                                break;

                            case eAttachParticle.Center:
                                particle_system_container.transform.position = Vector3.zero;
                                break;
                        }
                    }
                    break;
            }
            particle_system.SetLightingMax(IsLighting ? 1f : 0f);
            particle_system.Play(true, 0);
            particle_system.SetLensTilt(target.CharacterAnimation.IsUIMode ? 0.15f : 0f);
        }
    }

    public override void Finish()
    {
        base.Finish();
        if (particle_system_container != null)
            GameObject.Destroy(particle_system_container);
    }
}

[Serializable]
public class CharacterAction_EffectTarget : CharacterAction_EffectBase
{
    public enum eFinishParticle
    {
        None,
        Finish,
        Stop,
        Duration,
    }

    public float time_tick = 0.2f;
    public int count = 1;

    public string TweenName;
    public eAttachParticle AttachParticle = eAttachParticle.Target;
    public eFinishParticle FinishParticleAfterTween;

    protected override bool IsCancelParticle { get { return false; } }

    [NonSerialized]
    HFX_TweenBundle tween_bundle;
    [NonSerialized]
    Character self;
    [NonSerialized]
    GameObject particle_system_container;

    public CharacterAction_EffectTarget Clone(CharacterActionData action_data, Character self, Character main_target)
    {
        CharacterAction_EffectTarget new_effect = new CharacterAction_EffectTarget();
        base.CloneData(action_data, new_effect);
        if (action_data != null)
            new_effect.target = main_target;

        new_effect.time_tick = time_tick;
        new_effect.count = count;
        new_effect.self = self;
        new_effect.TweenName = this.TweenName;
        new_effect.AttachParticle = this.AttachParticle;
        new_effect.FinishParticleAfterTween = this.FinishParticleAfterTween;

        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        HFX_ParticleSystem temp_particle_system_prefab = particle_system_prefab;
        particle_system_prefab = null;

        base.Start(playback_time);

        if (temp_particle_system_prefab != null)
        {
            particle_system = GameObject.Instantiate<HFX_ParticleSystem>(temp_particle_system_prefab);
            particle_system.SetLightingMax(IsLighting?1f:0f);
            particle_system.transform.localPosition += particle_position;
            particle_system.ApplyScale(ActionScale);

            switch (AttachParticle)
            {
                case eAttachParticle.Target:
                case eAttachParticle.TargetScale:
                    particle_system.transform.SetParent(target.transform.parent, false);
                    if (AttachParticle == eAttachParticle.TargetScale)
                    {
                        Vector3 scale = particle_system.transform.localScale;
                        scale *= ActionScale;
                        particle_system.transform.localScale = scale;
                    }
                    break;

                case eAttachParticle.Self:
                    particle_system.transform.SetParent(self.transform.parent, false);
                    break;

                case eAttachParticle.World:
                case eAttachParticle.WorldScale:
                case eAttachParticle.Center:
                case eAttachParticle.SelfCenter:
                case eAttachParticle.TargetCenter:
                    particle_system_container = new GameObject(particle_system.name + " Container");
                    particle_system.transform.SetParent(particle_system_container.transform, false);
                    particle_system_container.transform.position = self.transform.position;
                    if (AttachParticle == eAttachParticle.TargetCenter && target.transform.lossyScale.x < 0f || AttachParticle != eAttachParticle.TargetCenter && self.transform.lossyScale.x < 0f)
                    {
                        Vector3 scale = particle_system_container.transform.localScale;
                        scale.x = -1f;
                        particle_system_container.transform.localScale = scale;
                    }

                    switch (AttachParticle)
                    {
                        case eAttachParticle.WorldScale:
                            {
                                Vector3 scale = particle_system.transform.localScale;
                                scale *= ActionScale;
                                particle_system.transform.localScale = scale;
                            }
                            break;

                        case eAttachParticle.SelfCenter:
                            {
                                var self_layout = self.transform.parent.parent.GetComponent<CharacterLayout>();
                                particle_system_container.transform.position = self_layout.Center.transform.position;
                            }
                            break;

                        case eAttachParticle.TargetCenter:
                            {
                                var target_layout = target.transform.parent.parent.GetComponent<CharacterLayout>();
                                particle_system_container.transform.position = target_layout.Center.transform.position;
                            }
                            break;

                        case eAttachParticle.Center:
                            particle_system_container.transform.position = Vector3.zero;
                            break;
                    }
                    break;
            }
            particle_system.Play(true, 0);
            particle_system.SetLensTilt(target.CharacterAnimation.IsUIMode ? 0.15f : 0f);

            if (string.IsNullOrEmpty(TweenName) == false)
            {
                HFX_TweenSystem tween = self.GetComponent<HFX_TweenSystem>();
                if (tween != null)
                {
                    tween_system = particle_system.GetComponent<HFX_TweenSystem>();
                    if (tween_system == null)
                        tween_system = particle_system.gameObject.AddComponent<HFX_TweenSystem>();
                    tween_system.IsPause = true;
                    switch(AttachParticle)
                    {
                        case eAttachParticle.World:
                            tween_bundle = tween.Play(TweenName, target.transform.parent, tween_system, null, 1f);
                            tween_bundle.SetClearWhenFinish(false);
                            break;

                        case eAttachParticle.WorldScale:
                            tween_bundle = tween.Play(TweenName, target.transform.parent, tween_system, null, ActionScale);
                            tween_bundle.SetClearWhenFinish(false);
                            break;

                        default:
                            tween_bundle = tween.Play(TweenName, target.transform.parent, tween_system, target.transform.GetChild(0), ActionScale);
                            break;
                    }
                }
            }
        }
    }

    public override void Finish()
    {
        base.Finish();
        if (particle_system_container != null)
            GameObject.Destroy(particle_system_container);
    }

    public override bool Update(float playback_time)
    {
        bool is_playing = base.Update(playback_time);
        if (IsStarted == false)
            return true;

        if (particle_system != null && particle_system.IsFinish == false)
        {
            if (FinishParticleAfterTween == eFinishParticle.Duration)
            {
                if (particle_system.PlaybackTime > (m_ActionData as CharacterActionData).Duration)
                    particle_system.Finish();
            }
            else if (FinishParticleAfterTween != eFinishParticle.None && tween_bundle != null && tween_bundle.IsPlaying == false)
            {
                if (FinishParticleAfterTween == eFinishParticle.Finish)
                    particle_system.Finish();
                else
                    particle_system.Stop();
            }
        }

        return is_playing;
    }
}


public abstract class CharacterAction_EffectResultBase : CharacterActionBase
{
    [NonSerialized]
    protected ISkillTarget target;
    [NonSerialized]
    protected ICreature m_SkillCreature;

    public bool target_linked = false;

    [NonSerialized]
    protected CharacterActionData m_ActionData = null;
    [NonSerialized]
    protected CharacterAction_EffectTarget LinkedTarget;

    override public bool Update(float playback_time)
    {
        base.Update(playback_time);

        return !IsStarted;
    }

    override public void Cancel(bool stopAll)
    {
        if (stopAll == true || IsStarted == false && (target_linked == false || LinkedTarget != null && LinkedTarget.IsPlaying == false))
            IsPlaying = false;
    }
}


[Serializable]
public class CharacterAction_EffectHit : CharacterAction_EffectResultBase
{
    public int HitIndex { get; set; }
    public int chance = 100;

    public float time_tick = 0.2f;
    public float time_gap = 0f;
    public int count = 1;

    public CharacterActionHitComponent action_component_prefab;

    public CharacterAction_EffectHit Clone(CharacterActionData action_data, ICreature skill_creature, ISkillTarget target, CharacterAction_EffectTarget linked_target)
    {
        CharacterAction_EffectHit new_effect = new CharacterAction_EffectHit();
        new_effect.m_ActionData = action_data;
        new_effect.chance = this.chance;
        new_effect.time = this.time;
        new_effect.action_component_prefab = this.action_component_prefab;
        new_effect.target = target;
        new_effect.target_linked = target_linked;
        new_effect.LinkedTarget = linked_target;
        new_effect.time_tick = this.time_tick;
        new_effect.time_gap = this.time_gap;
        new_effect.count = this.count;
        new_effect.m_SkillCreature = skill_creature;
        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        if (m_ActionData.DelayedApply == true && BattleBase.Instance.IsPause == ePauseType.Pause)
            return;

        base.Start(playback_time);

        eSkillTargetHit hit = target.OnHit();
        if (action_component_prefab == null || hit == eSkillTargetHit.Miss || hit == eSkillTargetHit.MissPosition || hit == eSkillTargetHit.Evade)
        {
            return;
        }

        target.Character.AddAction(action_component_prefab.data.CreateAction(action_component_prefab, target.Character.PlaybackTime, target.Character, IsLighting, ActionScale, hit));
    }
}

[Serializable]
public class CharacterAction_EffectBuff : CharacterAction_EffectResultBase
{
    public CharacterActionBuffComponent action_component_prefab;

    public CharacterAction_EffectBuff Clone(CharacterActionData action_data, ICreature skill_creature, ISkillTarget target, CharacterAction_EffectTarget linked_target)
    {
        CharacterAction_EffectBuff new_effect = new CharacterAction_EffectBuff();
        new_effect.m_ActionData = action_data;
        new_effect.time = this.time;
        new_effect.action_component_prefab = this.action_component_prefab;
        new_effect.target = target;
        new_effect.target_linked = target_linked;
        new_effect.LinkedTarget = linked_target;
        new_effect.m_SkillCreature = skill_creature;
        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        if (BattleBase.Instance.IsPause == ePauseType.Pause)
            return;

        base.Start(playback_time);

        target.OnBuff(action_component_prefab, IsLighting, ActionScale);
    }
}

[Serializable]
public class CharacterAction_EffectCamera : CharacterAction_EffectResultBase
{
    public float power = 5f, duration = 0.1f;

    public CharacterAction_EffectCamera Clone(CharacterActionData action_data, ICreature skill_creature, ISkillTarget target, CharacterAction_EffectTarget linked_target)
    {
        CharacterAction_EffectCamera new_effect = new CharacterAction_EffectCamera();
        new_effect.m_ActionData = action_data;
        new_effect.time = this.time;
        new_effect.target = target;
        new_effect.target_linked = target_linked;
        new_effect.LinkedTarget = linked_target;
        new_effect.m_SkillCreature = skill_creature;
        new_effect.power = power;
        new_effect.duration = duration;
        return new_effect;
    }

    protected override void Start(float playback_time)
    {
        if (BattleBase.Instance.IsPause == ePauseType.Pause)
            return;

        base.Start(playback_time);

        BattleBase.Instance.DoCamera(power, duration);
    }
}

[Serializable]
public class CharacterAction_EffectContainer
{
    public bool UseSingTarget = true;
    public float TargetTimeGap = 0f;
    public int TargetTimeGroup = 2;
    public float ScaleTime = 0f, MoveScale = 1f, JumpScale = 1f, FxHeight = 0f;

    [NonSerialized]
    CharacterActionData m_ActionData = null;
    public CharacterAction_EffectCasting[] Casting;
    public CharacterAction_EffectTarget[] Target;
    public CharacterAction_EffectHit[] Hit;
    public CharacterAction_EffectBuff[] Buff;
    public CharacterAction_EffectCamera[] Camera;
    public bool IsPlaying { get; private set; }
    public float FirstActionTime { get; private set; }
    public float LastActionTime { get; private set; }

    public float GetFirstActionTime()
    {
        float action_time = 999f;
        if (Hit.Length > 0)
        {
            action_time = Hit[0].time;
            if (Hit[0].target_linked)
                action_time += Target[0].time;
        }
        if (Buff.Length > 0)
        {
            action_time = Mathf.Min(action_time, Buff[0].time);
        }

        return action_time;
    }

    public CharacterAction_EffectContainer Clone(CharacterActionData action_data, Character self, List<ISkillTarget> targets)
    {
        CharacterAction_EffectContainer new_container = new CharacterAction_EffectContainer();
        new_container.m_ActionData = action_data;
        new_container.ScaleTime = this.ScaleTime;
        new_container.MoveScale = this.MoveScale;
        new_container.JumpScale = this.JumpScale;
        new_container.FxHeight = this.FxHeight;

        List<CharacterAction_EffectCasting> casting = new List<CharacterAction_EffectCasting>();
        for (int i = 0; i < Casting.Length; ++i)
        {
            if (action_data == null || Casting[i].IsEnable == true)
                casting.Add(Casting[i].Clone(action_data, self));
        }
        new_container.Casting = casting.ToArray();

        if (action_data != null)
        {
            float first_action_time = -1f, last_action_time = 0f;

            int target_count = targets == null ? 0 : targets.Count;

            List<CharacterAction_EffectTarget> effect_target = new List<CharacterAction_EffectTarget>();
            List<CharacterAction_EffectHit> effect_hit = new List<CharacterAction_EffectHit>();
            List<CharacterAction_EffectBuff> effect_buff = new List<CharacterAction_EffectBuff>();
            List<CharacterAction_EffectCamera> effect_camera = new List<CharacterAction_EffectCamera>();

            List<CharacterAction_EffectTarget> temp_effect_target = new List<CharacterAction_EffectTarget>();
            for (int target_index = 0; target_index < target_count; ++target_index)
            {
                if (targets[target_index] == null)
                    continue;

                float time_gap = (target_index == 0 || TargetTimeGroup == 0) ? 0f : (TargetTimeGap * ((target_index - 1) / TargetTimeGroup + 1));

                if (UseSingTarget == false)
                    temp_effect_target.Clear();

                if (UseSingTarget == false || target_index == 0)
                {
                    for (int i = 0; i < Target.Length; ++i)
                    {
                        if (Target[i].IsEnable == false)
                            continue;

                        int effect_target_count = Math.Max(1, Target[i].count);
                        for (int et = 0; et < effect_target_count; ++et)
                        {
                            CharacterAction_EffectTarget new_data = Target[i].Clone(action_data, self, targets[target_index].Character);
                            new_data.time += time_gap + et * Target[i].time_tick;
                            effect_target.Add(new_data);
                            temp_effect_target.Add(new_data);
                        }
                    }
                }

                if (targets[target_index] == null || targets[target_index].Character == null || targets[target_index].Character.Creature.IsDead == true)
                    continue;

                if (Hit.Length > 0)
                {
                    List<float> hits = new List<float>();
                    int hit_chance_total = 0;
                    for (int i = 0; i < Hit.Length; ++i)
                    {
                        if (Hit[i].IsEnable == false)
                            continue;

                        if (Hit[i].target_linked == true)
                        {
                            for (int j = 0; j < temp_effect_target.Count; ++j)
                            {
                                hits.Add(Hit[i].chance);
                                hit_chance_total += Hit[i].chance;
                            }
                        }
                        else
                        {
                            for (int hit_index = 0; hit_index < Hit[i].count; ++hit_index)
                            {
                                hits.Add(Hit[i].chance);
                                hit_chance_total += Hit[i].chance;
                            }
                        }
                    }
                    hits = hits.Select(h => h / hit_chance_total).ToList();

                    targets[target_index].InitHit(hits);
                    for (int i = 0, hit_index = 0; i < Hit.Length; ++i)
                    {
                        CharacterAction_EffectHit hit = Hit[i];
                        if (hit.IsEnable == false)
                            continue;

                        if (hit.target_linked)
                        {
                            foreach (CharacterAction_EffectTarget target in temp_effect_target)
                            {
                                CharacterAction_EffectHit new_data = hit.Clone(action_data, self.Creature, targets[target_index], target);
                                new_data.time += target.time + (UseSingTarget == true?time_gap:0f);
                                new_data.HitIndex = hit_index++;
                                effect_hit.Add(new_data);
                            }
                        }
                        else
                        {
                            for (int hit_count_index = 0; hit_count_index < hit.count; ++hit_count_index)
                            {
                                float hit_time_gap = (target_index == 0 || TargetTimeGroup == 0) ? 0f : ((hit.time_gap==0f?TargetTimeGap:hit.time_gap) * ((target_index - 1) / TargetTimeGroup + 1));

                                CharacterAction_EffectHit new_data = hit.Clone(action_data, self == null ? null : self.Creature, targets[target_index], null);
                                new_data.time += hit_time_gap + hit_count_index * new_data.time_tick;
                                new_data.HitIndex = hit_index++;
                                effect_hit.Add(new_data);
                            }
                        }
                    }
                    if (effect_hit.Count > 0)
                    {
                        first_action_time = effect_hit[0].time;
                        last_action_time = effect_hit[effect_hit.Count - 1].time;
                    }
                }

                if (Buff.Length > 0)
                {
                    for (int i = 0; i < Buff.Length; ++i)
                    {
                        CharacterAction_EffectBuff buff = Buff[i];
                        if (buff.IsEnable == false)
                            continue;

                        if (buff.target_linked)
                        {
                            foreach (CharacterAction_EffectTarget target in temp_effect_target)
                            {
                                CharacterAction_EffectBuff new_data = buff.Clone(action_data, self.Creature, targets[target_index], null);
                                new_data.time += target.time + (UseSingTarget == true ? time_gap : 0f);
                                effect_buff.Add(new_data);
                            }
                        }
                        else
                        {
                            CharacterAction_EffectBuff new_data = buff.Clone(action_data, self==null?null:self.Creature, targets[target_index], null);
                            new_data.time += time_gap;
                            effect_buff.Add(new_data);
                        }
                    }
                    if (effect_buff.Count > 0)
                    {
                        if (first_action_time == -1f)
                            first_action_time = effect_buff[0].time;
                        else
                            first_action_time = Mathf.Min(first_action_time, effect_buff[0].time);

                        last_action_time = Mathf.Max(last_action_time, effect_buff[effect_buff.Count - 1].time);
                    }
                }

                if (Camera.Length > 0)
                {
                    for (int i = 0; i < Camera.Length; ++i)
                    {
                        CharacterAction_EffectCamera camera = Camera[i];
                        if (camera.IsEnable == false)
                            continue;

                        if (camera.target_linked)
                        {
                            foreach (CharacterAction_EffectTarget target in temp_effect_target)
                            {
                                CharacterAction_EffectCamera new_data = camera.Clone(action_data, self.Creature, targets[target_index], null);
                                new_data.time += target.time + (UseSingTarget == true ? time_gap : 0f);
                                effect_camera.Add(new_data);
                            }
                        }
                        else
                        {
                            CharacterAction_EffectCamera new_data = camera.Clone(action_data, self == null ? null : self.Creature, targets[target_index], null);
                            new_data.time += time_gap;
                            effect_camera.Add(new_data);
                        }
                    }
                    if (effect_camera.Count > 0)
                    {
                        if (first_action_time == -1f)
                            first_action_time = effect_camera[0].time;
                        else
                            first_action_time = Mathf.Min(first_action_time, effect_camera[0].time);

                        last_action_time = Mathf.Max(last_action_time, effect_camera[effect_camera.Count - 1].time);
                    }
                }
            }

            new_container.Target = effect_target.ToArray();
            new_container.Hit = effect_hit.ToArray();
            new_container.Buff = effect_buff.ToArray();
            new_container.Camera = effect_camera.ToArray();
            new_container.FirstActionTime = first_action_time;
            new_container.LastActionTime = last_action_time;
        }
        else
        {
            new_container.Target = new CharacterAction_EffectTarget[Target.Length];
            for (int i = 0; i < Target.Length; ++i)
                new_container.Target[i] = Target[i].Clone(null, null, null);

            new_container.Hit = new CharacterAction_EffectHit[Hit.Length];
            for (int i = 0; i < Hit.Length; ++i)
                new_container.Hit[i] = Hit[i].Clone(null, null, null, null);

            new_container.Buff = new CharacterAction_EffectBuff[Buff.Length];
            for (int i = 0; i < Buff.Length; ++i)
                new_container.Buff[i] = Buff[i].Clone(null, null, null, null);

            new_container.Camera = new CharacterAction_EffectCamera[Camera.Length];
            for (int i = 0; i < Camera.Length; ++i)
                new_container.Camera[i] = Camera[i].Clone(null, null, null, null);
        }

        return new_container;
    }

    public void Play(bool is_lighting, float move_scale)
    {
        foreach (var effect in Camera)
        {
            effect.Play(is_lighting, move_scale);
        }
        foreach (var effect in Buff)
        {
            effect.Play(is_lighting, move_scale);
        }
        foreach (var effect in Casting)
        {
            effect.Play(is_lighting, move_scale);
        }
        foreach (var effect in Target)
        {
            effect.Play(is_lighting, move_scale);
        }
        foreach (var effect in Hit)
        {
            effect.Play(is_lighting, move_scale);
        }
        IsPlaying = true;
    }

    public bool Update(float playback_time)
    {
        bool is_playing = false;

        foreach (var effect in Buff)
        {
            if (effect.IsPlaying == true && effect.Update(playback_time))
                is_playing = true;
        }

        foreach (var effect in Camera)
        {
            if (effect.IsPlaying == true && effect.Update(playback_time))
                is_playing = true;
        }

        foreach (var effect in Casting)
        {
            if (effect.IsPlaying == true && effect.Update(playback_time))
                is_playing = true;
        }

        foreach (var effect in Target)
        {
            if (effect.IsPlaying == true && effect.Update(playback_time))
                is_playing = true;
        }

        foreach (var effect in Hit)
        {
            if (effect.IsPlaying == true && effect.Update(playback_time))
                is_playing = true;
        }

        IsPlaying = is_playing;
        return IsPlaying;
    }

    [Flags]
    public enum eEffectFlag
    {
        None = 0,
        Casting = 1,
        Target = 2,
        Hit = 4,
        Buff = 8,
        Camera = 16,
        All = Casting | Target | Hit | Buff | Camera
    }

    public void Cancel(bool stopAll, eEffectFlag flag = eEffectFlag.All)
    {
        if ((flag & eEffectFlag.Casting) != eEffectFlag.None)
        {
            foreach (var effect in Casting)
            {
                effect.Cancel(stopAll);
            }
        }

        if ((flag & eEffectFlag.Target) != eEffectFlag.None)
        {
            foreach (var effect in Target)
            {
                effect.Cancel(stopAll);
            }
        }

        if ((flag & eEffectFlag.Hit) != eEffectFlag.None)
        {
            foreach (var effect in Hit)
            {
                effect.Cancel(stopAll);
            }
        }

        if ((flag & eEffectFlag.Buff) != eEffectFlag.None)
        {
            foreach (var effect in Buff)
            {
                effect.Cancel(stopAll);
            }
        }

        if ((flag & eEffectFlag.Camera) != eEffectFlag.None)
        {
            foreach (var effect in Camera)
            {
                effect.Cancel(stopAll);
            }
        }
    }

    public void SetHidden(bool hidden)
    {
        foreach (var effect in Casting)
        {
            effect.SetHidden(hidden);
        }

        foreach (var effect in Target)
        {
            effect.SetHidden(hidden);
        }

        foreach (var effect in Hit)
        {
            effect.SetHidden(hidden);
        }

        foreach (var effect in Buff)
        {
            effect.SetHidden(hidden);
        }

        foreach (var effect in Camera)
        {
            effect.SetHidden(hidden);
        }
    }
}
