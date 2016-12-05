using UnityEngine;
using System.Collections;
using System;
using HeroFX;
using System.Collections.Generic;

[RequireComponent(typeof(HFX_TweenSystem))]
[AddComponentMenu("SmallHeroes/CharacterActionBuff")]
public class CharacterActionBuffComponent : MonoBehaviour
{
    public CharacterActionBuffData data;
    public CharacterActionBuffComponent[] sub_components;
}

[Serializable]
public class CharacterActionBuffData : ICharacterActionData
{
    public bool Freeze = false;
    public string TweenName;
    public CharacterAction_Effect[] loop;
    public CharacterAction_Effect[] hit;

    public string StateColorName = "none";

    public CharacterActionBuffData()
    {
        AnimationName = "";
    }

    public CharacterActionBuff CreateAction(CharacterActionBuffComponent component, float playback_time, Character self, float buff_time, bool is_lighting, float move_scale)
    {
        return new CharacterActionBuff(component, playback_time, self, buff_time, is_lighting, move_scale);
    }

    public CharacterActionBuffData Clone(bool forPlay, Character self)
    {
        CharacterActionBuffData action = new CharacterActionBuffData();
        action.AnimationName = this.AnimationName;
        action.TweenName = this.TweenName;
        action.Freeze = this.Freeze;
        action.StateColorName = this.StateColorName;

        action.loop = new CharacterAction_Effect[loop.Length];
        for (int i = 0; i < loop.Length; ++i)
        {
            action.loop[i] = loop[i].Clone(forPlay==true?action:null, self);
        }

        action.hit = this.hit;

        return action;
    }
}

abstract public class ICharacterActionBuff : ICharacterAction
{
    [NonSerialized]
    protected Character self;

    override public string Name { get { return m_Name; } }
    protected string m_Name;

    protected HFX_TweenBundle m_TweenBundle;

    protected float m_BuffTime = 0f;

    public Action OnFinish = null;

    protected List<CharacterAction_Effect> Hits = new List<CharacterAction_Effect>();

    public bool IsFinished { get; private set; }

    virtual public void Finish()
    {
        if (IsFinished == true)
            return;

        IsFinished = true;

        if (m_TweenBundle != null)
            m_TweenBundle.Stop();

        if (OnFinish != null)
            OnFinish();
    }

    override public bool Update(float playback_time)
    {
        if (playback_time < StartTime)
            return true;

        playback_time -= StartTime;

        bool is_finished = playback_time >= m_BuffTime;
        if (is_finished == true)
            Finish();

        bool is_playing = !IsFinished;

        return is_playing;
    }

    public override void Cancel(bool stopAll)
    {
        Finish();
    }

    virtual public void OnHit()
    {

    }
}

public class CharacterActionBuff : ICharacterActionBuff
{
    float PlaybackTime = 0f;
    public CharacterActionBuffData Data { get; private set; }

    public CharacterActionBuff(CharacterActionBuffComponent component, float playback_time, Character self, float buff_time, bool is_lighting, float move_scale)
    {
        this.m_Name = component.name;
        this.Data = component.data.Clone(true, self);
        this.self = self;
        StartTime = playback_time;
        m_BuffTime = buff_time;
        PlaybackTime = playback_time;

        if (Data.Freeze == true)
            self.SetFreeze();

        if (string.IsNullOrEmpty(Data.AnimationName) == false)
        {
            m_Length = self.PlayAnimation(Data.AnimationName);
        }

        foreach (var effect in Data.loop)
        {
            if (self.CharacterAnimation.DummyMode == eCharacterDummyMode.Hidden)
                effect.SetHidden(true);
            effect.Play(is_lighting, 0f);
            effect.Update(0f);
        }

        if (string.IsNullOrEmpty(Data.TweenName) == false)
        {
            HFX_TweenSystem tween_system = component.GetComponent<HFX_TweenSystem>();
            if (tween_system != null)
            {
                m_TweenBundle = tween_system.Play(Data.TweenName, null, self.GetComponent<HFX_TweenSystem>(), self.transform.GetChild(0), move_scale);
            }
        }
    }

    override public void Finish()
    {
        if (IsFinished == true)
            return;

        if (Data.Freeze == true)
            self.UnsetFreeze();

        base.Finish();
        foreach (var effect in Data.loop)
        {
            effect.Finish();
        }
    }

    override public bool Update(float playback_time)
    {
        if (playback_time < StartTime)
            return true;

        bool is_playing = base.Update(playback_time);

        playback_time -= StartTime;
        PlaybackTime = playback_time;

        if (string.IsNullOrEmpty(Data.AnimationName) == false && self.CharacterAnimation.CheckPlaying(Data.AnimationName) == false)
        {
            self.PlayAnimation(Data.AnimationName);
        }

        foreach (var effect in Data.loop)
        {
            if (effect.Update(playback_time))
                is_playing = true;
        }

        foreach (var effect in Hits)
        {
            if (effect.Update(playback_time))
                is_playing = true;
        }
        return is_playing;
    }

    public override void OnHit()
    {
        foreach (CharacterAction_Effect hit in Data.hit)
        {
            CharacterAction_Effect new_hit = hit.Clone(Data, self);
            new_hit.time = PlaybackTime;
            new_hit.Play(false, 0f);
            new_hit.Update(PlaybackTime);

            Hits.Add(new_hit);
        }
    }

    public override void SetHidden(bool hidden)
    {
        foreach (var effect in Data.loop)
        {
            effect.SetHidden(hidden);
        }

        foreach (var effect in Hits)
        {
            effect.SetHidden(hidden);
        }
    }
}