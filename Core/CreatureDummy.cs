using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SkillTargetDummy : ISkillTarget
{
    public const float BuffDuration = 5f;

    Character m_Self, m_Target;
    override public Character Character { get { return m_Target; } }

    public SkillTargetDummy(Character self, Character target)
    {
        m_Self = self;
        m_Target = target;
    }

    override public eSkillTargetHit OnHit()
    {
        eTextPushType push_type = MNS.Random.Instance.NextRange(1, 10) <= 3 ? eTextPushType.Critical : eTextPushType.Normal;
        if (m_Self.Creature.IsTeam != m_Target.Creature.IsTeam)
#if !SH_ASSETBUNDLE
            TextManager.Instance.PushDamage(m_Self.Creature.Info.AttackType == SharedData.eAttackType.physic, m_Target.Creature, -100, push_type);
#else
            TextManager.Instance.PushDamage(true, m_Target.Creature, -100, push_type);
#endif
        else
            TextManager.Instance.PushHeal(m_Target.Creature, 100, push_type);
        return eSkillTargetHit.Hit;
    }

    override public void OnBuff(CharacterActionBuffComponent buff_component, bool is_lighting, float apply_scale)
    {
        ISkillBuff buff = new ISkillBuff(BuffDuration);
        (m_Target.Creature as CreatureDummy).AddBuff(buff);

        CharacterActionBuff new_action = buff_component.data.CreateAction(buff_component, Character.PlaybackTime, Character, buff.Duration, is_lighting, apply_scale);
        buff.Action = new_action;
    }
}

public class BuffDummy : ISkillBuff
{
    public BuffDummy(float duration)
        : base(duration)
    {

    }
}

public class CreatureDummy : ICreature
{
    List<ISkillBuff> Buffs = new List<ISkillBuff>();

    public override bool IsDead
    {
        get
        {
            return base.IsDead;
        }
        set
        {
            if (base.IsDead == value)
                return;

            base.IsDead = value;

            if (value == true)
                Character.PlayAction("die");
            else
                Character.CancelAnimation();
        }
    }

    public CreatureDummy(bool is_team, CharacterContainer container)
    {
        IsTeam = is_team;
        this.Container = container;
        Character.IsPause = true;
    }

    override public void Update(float deltaTime, float deltaTimeIgnore)
    {
        if (Character == null)
            return;

        if (IgnoreSpeed == true)
            PlaybackTime += deltaTimeIgnore * PlaybackSpeed;
        else
            PlaybackTime += deltaTime * PlaybackSpeed;

        Character.UpdatePlay(PlaybackTime);

        UpdateBuffs();
    }

    void UpdateBuffs()
    {
        Character.CharacterAnimation.StateColor = BattleBase.Instance.color_container.Colors[0].color;

        if (Buffs.Count == 0)
            return;

        List<ISkillBuff> buffs = new List<ISkillBuff>(Buffs);
        foreach (ISkillBuff buff in buffs)
        {
            if (buff.Action != null && buff.Action.Update(Character.PlaybackTime) == false)
            {
                RemoveBuff(buff);
            }
        }

        if (Buffs.Count == 0)
            return;

        var state_list = Buffs.Where(b => b.UseStateColor);
        if (state_list.Count() == 0)
            return;

        var state_buff = state_list.Last();
        if (state_buff != null)
            Character.CharacterAnimation.StateColor = state_buff.StateColor;
    }

    public void AddBuff(ISkillBuff buff)
    {
        buff.StartTime = PlaybackTime;
        buff.EndTime = PlaybackTime + buff.Duration;
        Buffs.Add(buff);
    }

    public void RemoveBuff(ISkillBuff buff)
    {
        Buffs.Remove(buff);
    }

    override public void SetDummyMode(eCharacterDummyMode mode)
    {
        Character.SetDummyMode(mode);
    }
}
