using UnityEngine;
using System.Collections;
using System;
using HeroFX;
using System.Collections.Generic;

[RequireComponent(typeof(HFX_TweenSystem))]
[AddComponentMenu("SmallHeroes/CharacterActionHit")]
public class CharacterActionHitComponent : MonoBehaviour
{
    public CharacterActionHitData data;
}
public enum eCharacterTweenTarget
{
    Character,
    Tween,
}

[Serializable]
public class CharacterActionHitData : ICharacterActionData
{
    public eCharacterTweenTarget TweenTarget = eCharacterTweenTarget.Tween;
    public string TweenName;
    public CharacterAction_Effect[] effects;

    public CharacterActionHitData()
    {
        AnimationName = "damage";
    }

    public CharacterActionHit CreateAction(CharacterActionHitComponent component, float playback_time, Character self, bool is_lighting, float action_scale, eSkillTargetHit hit)
    {
        return new CharacterActionHit(component, playback_time, self, is_lighting, action_scale, hit);
    }

    public CharacterActionHitData Clone(bool forPlay, Character self)
    {
        CharacterActionHitData action = new CharacterActionHitData();
        action.AnimationName = this.AnimationName;
        action.TweenName = this.TweenName;
        action.TweenTarget = this.TweenTarget;

        action.effects = new CharacterAction_Effect[this.effects.Length];
        for (int i = 0; i < this.effects.Length; ++i)
        {
            action.effects[i] = this.effects[i].Clone(forPlay==true?action:null, self);
        }

        return action;
    }
}

public class CharacterActionHit : ICharacterAction
{
    public CharacterActionHitData Data { get; private set; }
    Character self;

    override public string Name { get { return m_Name; } }
    string m_Name;

    public CharacterActionHit(CharacterActionHitComponent component, float playback_time, Character _self, bool is_lighting, float action_scale, eSkillTargetHit hit)
    {
        self = _self;
        this.m_Name = component.name;
        this.Data = component.data.Clone(true, self);
        StartTime = playback_time;

        if (hit == eSkillTargetHit.Hit)
        {
            m_Length = self.PlayAnimation(Data.AnimationName);

            foreach (var effect in Data.effects)
            {
                effect.Play(is_lighting, action_scale);
            }
        }

        if (string.IsNullOrEmpty(Data.TweenName) == false)
        {
            HFX_TweenSystem tween_system = component.GetComponent<HFX_TweenSystem>();
            if (tween_system != null && self.IgnoreTween == false)
            {
                tween_system.Play(Data.TweenName, null, self.GetComponent<HFX_TweenSystem>(), component.data.TweenTarget==eCharacterTweenTarget.Character?self.transform:self.transform.GetChild(0), hit==eSkillTargetHit.Hit?action_scale:0.6f);
            }
        }
    }

    override public bool Update(float playback_time)
    {
        playback_time -= StartTime;

        bool is_playing = false;
        foreach (var effect in Data.effects)
        {
            if (effect.Update(playback_time))
                is_playing = true;
        }
        return is_playing;
    }

    public bool IsFinished { get; private set; }
    virtual public void Finish()
    {
        if (IsFinished == true)
            return;

        IsFinished = true;

        foreach (var effect in Data.effects)
        {
            effect.Finish();
        }
    }

    override public void Cancel(bool stopAll)
    {
        if (stopAll)
            Finish();
    }

    public override void SetHidden(bool hidden)
    {
        foreach (var effect in Data.effects)
        {
            effect.SetHidden(hidden);
        }
        IsPause = true;
    }
}

