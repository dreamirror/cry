using UnityEngine;
using System.Collections;
using System;
using HeroFX;
using System.Collections.Generic;

public enum eMoveTarget
{
    Container,
    Character,
}

[Serializable]
public class CharacterActionData : ICharacterActionData
{
    public AnimationClip CameraAnimation;
    public BattleBase.eActionMode ActionMode = BattleBase.eActionMode.TeamHidden;
    public bool DelayedApply = false;
    public CharacterAction_EffectContainer Effect = new CharacterAction_EffectContainer();
    public float Duration { get; private set; }

    public CharacterActionData()
    {
        this.AnimationName = "attack";
    }

    public CharacterAction CreateAction(float playback_time, Character self, SkillTargetContainer target, bool is_lighting, float move_scale, float duration)
    {
        return new CharacterAction(this.Clone(true, self, target!=null?target.targets:null, duration), playback_time, self, target!=null?target.main_target:null, is_lighting, move_scale);
    }

    public CharacterActionData Clone(bool forPlay, Character self, List<ISkillTarget> targets, float duration)
    {
        CharacterActionData action = new CharacterActionData();
        action.Duration = duration;
        action.DelayedApply = DelayedApply;
        action.AnimationName = this.AnimationName;
        action.Effect = this.Effect.Clone(forPlay==true?action:null, self, targets);
        action.CameraAnimation = this.CameraAnimation;
        action.ActionMode = this.ActionMode;

        return action;
    }

}

public class CharacterAction : ICharacterAction
{
    public CharacterActionData Data { get; private set; }
    Character self;
    public bool IsPlayingAnimation { get; private set; }
    //     CharacterContainer target_container;

    override public string Name { get { return Data.AnimationName; } }

    public float FirstActionTime
    {
        get
        {
            return Data.Effect.FirstActionTime;
        }
    }

    public float LastActionTime
    {
        get
        {
            return Data.Effect.LastActionTime;
        }
    }

    public float AnimationTime
    {
        get
        {
            return m_Length;
        }
    }

    public float AnimationLeftTime
    {
        get
        {
            return StartTime + m_Length - self.PlaybackTime;
        }
    }

    public CharacterAction(CharacterActionData data, float playback_time, Character self, Transform main_target, bool is_lighting, float scale)
    {
        this.Data = data;
        this.self = self;
//         this.target_container = target_container;
//         if (target_container != null)
//             this.target = target_container.Creature;
        StartTime = playback_time;

        m_Length = self.PlayAnimationMove(playback_time, Data.AnimationName, main_target, new Character.MoveProperty(scale, data.Effect.MoveScale, data.Effect.FxHeight));
        IsPlayingAnimation = true;
        Data.Effect.Play(is_lighting, scale);

        if (data.CameraAnimation != null && BattleBase.Instance != null && BattleBase.Instance.m_SkillCamera != null)
        {
#if SH_ASSETBUNDLE
            BattleBase.Instance.m_SkillCamera.CheckFrame(data.CameraAnimation);
#endif

            BattleBase.Instance.SetActionMode(true, data.ActionMode, self.Creature, self.CharacterAnimation.IsMoveState(Data.AnimationName) == false);
            if (data.DelayedApply)
            {
                foreach (var hit in Data.Effect.Hit)
                {
                    hit.time = 0f;
                }
            }
            foreach (var buff in Data.Effect.Buff)
            {
                buff.time = 0f;
            }
        }
    }

    override public bool Update(float playback_time)
    {
        playback_time -= StartTime;
        bool is_playing = false;
        if (IsPlayingAnimation == true && self.IsPlaying)
        {
            is_playing = true;
            IsPlayingAnimation = true;
        }
        else
            IsPlayingAnimation = false;

        if (Data.Effect.Update(playback_time) == true)
            is_playing = true;

        if (Data.CameraAnimation != null && BattleBase.Instance && BattleBase.Instance.m_SkillCamera && BattleBase.Instance.m_SkillCamera.gameObject.activeSelf == true)
        {
            BattleBase.Instance.m_SkillCamera.ApplyAnimation(Data.CameraAnimation, playback_time, !self.Creature.IsTeam);
            if (playback_time > Data.CameraAnimation.length)
            {
                BattleBase.Instance.SetActionMode(false, Data.ActionMode, self.Creature, false);
                Data.Effect.Cancel(true, CharacterAction_EffectContainer.eEffectFlag.Casting | CharacterAction_EffectContainer.eEffectFlag.Target);
                self.CancelAnimation();
                BattleBase.Instance.RemoveLighting(self.Creature);

                if (Data.Effect.Update(playback_time) == true)
                    is_playing = true;
            }
            else
                is_playing = true;
        }

        IsPlaying = is_playing;
        return IsPlaying;
    }

    override public void Cancel(bool stopAll)
    {
        if (IsPlayingAnimation)
        {
//            self.CancelAnimation();
            IsPlayingAnimation = false;
        }

        if (Data.CameraAnimation != null && BattleBase.Instance && BattleBase.Instance.m_SkillCamera && BattleBase.Instance.m_SkillCamera.gameObject.activeSelf == true)
        {
            self.Creature.Character.transform.localPosition = Vector3.zero;
            BattleBase.Instance.SetActionMode(false, Data.ActionMode, self.Creature, false);
            Data.Effect.Cancel(true, CharacterAction_EffectContainer.eEffectFlag.Casting | CharacterAction_EffectContainer.eEffectFlag.Target);
        }

        Data.Effect.Cancel(stopAll);
    }

    public override void SetHidden(bool hidden)
    {
        Data.Effect.SetHidden(hidden);
        IsPause = true;
    }
}