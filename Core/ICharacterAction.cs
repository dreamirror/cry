using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ISkillBuff
{
    public ISkillBuff(float duration)
    {
        Duration = duration;
    }

    public ISkillBuff Parent { get; set; }
    public float Duration { get; set; }
    public ICharacterActionBuff Action { get; set; }
    public bool IsFinish { get; protected set; }
    public bool IsMainBuff { get { return Parent == null; } }

    public float StartTime { get; set; }
    public float EndTime { get; set; }
    static public int SortByEndTime(ISkillBuff a, ISkillBuff b) { return a.EndTime.CompareTo(b.EndTime); }

    public bool UseStateColor
    {
        get
        {
            if (Action is CharacterActionBuff)
            {
                var buff_action = Action as CharacterActionBuff;
                if (string.IsNullOrEmpty(buff_action.Data.StateColorName) == false && buff_action.Data.StateColorName != "none")
                    return true;
            }
            return false;
        }
    }

    public Color32 StateColor
    {
        get
        {
            if (Action is CharacterActionBuff)
            {
                var buff_action = Action as CharacterActionBuff;
                if (string.IsNullOrEmpty(buff_action.Data.StateColorName) == false && buff_action.Data.StateColorName != "none")
                {
                    return BattleBase.Instance.color_container.GetColor(buff_action.Data.StateColorName);
                }
            }
            return Color.black;
        }
    }
}

public enum eSkillTargetHit
{
    Hit,
    Shield,
    Miss,
    Immune,
    Evade,
    MissPosition,
}

public class SkillTargetContainer
{
    public Transform main_target;
    public List<ISkillTarget> targets;
    public List<ICreature> target_creatures;
}

public abstract class ISkillTarget
{
    abstract public Character Character { get; }

    virtual public void InitHit(List<float> hits) { }
    abstract public eSkillTargetHit OnHit();
    abstract public void OnBuff(CharacterActionBuffComponent buff_component, bool is_lighting, float apply_scale);
}

abstract public class ICreature //这是一个抽象函数 抽象类不是一个完整的类 不能被实现
{
    public long Idx { get; protected set; }
    public CreatureInfo Info { get; protected set; }
    public string SkinName { get; protected set; }
#if !SH_ASSETBUNDLE
    public MapCreatureInfo MapCreature { get; protected set; }
#endif
    public CharacterContainer Container { get; set; }
    public Character Character { get { return Container.Character; } }
    virtual public bool IsDead { get; set; }
    public bool IsTeam { get; protected set; }
    public bool IsLigting { get; set; }

    public float Scale { get; set; }

    public bool IsShowText { get; set; }
    public float TextOffset { get; set; }

    virtual public void Update(float deltaTime, float deltaTimeIgnore) { }
    virtual public void SetDummyMode(eCharacterDummyMode mode) { }

    public ICreature()
    {
        Scale = 1f;
        TextOffset = -1;
        IsShowText = true;
    }

    public void SetLighting(bool is_lighting)
    {
        IsLigting = is_lighting;
        Character.SetLighting(IsLigting == true ? 1f : 0f);
    }

    [NonSerialized]
    public float PlaybackTime = 0f;
    [NonSerialized]
    public float PlaybackSpeed = 1f;
    [NonSerialized]
    public bool IgnoreSpeed = false;

}


public abstract class ICharacterAction
{
    protected float m_Length = 0f;
    public bool IsPause { get; protected set; }

    public bool IsPlaying { get; protected set; }
    public float StartTime { get; protected set; }

    abstract public bool Update(float playback_time);
    abstract public string Name { get; }
    public float Length { get { return m_Length; } }

    abstract public void Cancel(bool stopAll);
    abstract public void SetHidden(bool hidden);
}

[Serializable]
public abstract class ICharacterActionData
{
    public string AnimationName;
}