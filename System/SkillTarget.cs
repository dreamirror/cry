using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using SharedData;

public class SkillTarget : ISkillTarget
{
    SkillTargetContainer target_container;

    BattleCreature m_Creature;
    override public Character Character { get { return m_Creature.Character; } }

    public BattleSkill Skill { get; private set; }
    Queue<HitInfo> HitValues;
    int m_TargetIndex;

    struct HitInfo
    {
        public HitInfo(float chance)
        {
            this.percent = chance;
        }

        public float percent;
    }

    public SkillTarget(SkillTargetContainer target_container, BattleSkill skill, ICreature target, int target_index)
    {
        this.target_container = target_container;
        this.Skill = skill;
        m_TargetIndex = target_index;
        m_Creature = target as BattleCreature;
    }

    int CalculateHitValue(SkillInfo.Action action)
    {
        int damage_value = Skill.Creature.GetDamageValue();
        int value = Skill.GetHitValue(action);
        float value_percent = Skill.GetHitValuePercent(action, m_TargetIndex);

        if (Skill.Creature != null)
        {
            switch (action.actionType)
            {
                case eActionType.damage:
                    {
                        if (Skill.Creature.IsIgnoreDefenseDamage == true || m_Creature.IsIgnoreDefenseDamaged == true)
                        {
                            value = damage_value;
                        }
                        else
                        {
                            int defense = 0;
                            if (Skill.Creature.Info.AttackType == eAttackType.physic)
                                defense = m_Creature.GetValue(eStatType.PhysicDefense);
                            else
                                defense = m_Creature.GetValue(eStatType.MagicDefense);

                            value = Math.Max(BattleConfig.Instance.MinDamage, Mathf.RoundToInt(value * Mathf.Sqrt((float)damage_value / Math.Max(BattleConfig.Instance.MinDefense, defense))));
                        }

                        value = Mathf.RoundToInt(value * value_percent * (1f + Skill.Creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f));

                        return -value;
                    }

                case eActionType.damage_mana:
                    {
                        float decrease = (10000-(m_Creature.Level - Skill.Level) * GameConfig.Get<int>("mana_level_decrease"))*0.0001f;
                        return -Mathf.RoundToInt(value * value_percent * (1f + Skill.Creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f) * decrease);
                    }

                case eActionType.heal:
                    return Mathf.RoundToInt(value * value_percent * (1f + Skill.Creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f));

                case eActionType.heal_mana:
                    {
                        float decrease = (10000 - (m_Creature.Level - Skill.Level) * GameConfig.Get<int>("mana_level_decrease")) * 0.0001f;
                        return Mathf.RoundToInt(value * value_percent * (1f + Skill.Creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f) * decrease);
                    }

                default:
                    Debug.LogErrorFormat("not hit type : {0}", Skill.Info.Actions[0].actionType);
                    break;
            }
        }
        else value = Mathf.RoundToInt(value * value_percent);
        return value;
    }

    public override void InitHit(List<float> hits)
    {
        if (hits.Count == 0)
            return;

        buff_index += Skill.Info.Actions.Count(a => a.IsDirect);

        HitValues = new Queue<HitInfo>();

        if (hits.Count == 1)
        {
            HitValues.Enqueue(new HitInfo(1f));
        }
        else
        {
            float per_value_gap = 0.15f;

            MNS.Random random = BattleBase.Instance.Rand;
            float left_hit_chance = 1f;
            for (int i = 0; i < hits.Count; ++i)
            {
                float hit_chance = hits[i];

                if (i == hits.Count - 1)
                {
                    HitValues.Enqueue(new HitInfo(left_hit_chance));
                }
                else
                {
                    hit_chance += random.NextRange(-hit_chance * per_value_gap, hit_chance * per_value_gap);
                    left_hit_chance -= hit_chance;
                    HitValues.Enqueue(new HitInfo(hit_chance));
                }
            }
        }
    }

    override public eSkillTargetHit OnHit()
    {
        HitInfo hit_info = HitValues.Dequeue();
        SkillInfo.Action main_action = Skill.Info.Actions[0];
        if (BattleBase.Instance.IsBattleEnd == true)
        {
            return eSkillTargetHit.Miss;
        }

        if (m_Creature.IsDead == true || main_action.check_distance == true && Character.MoveDistance > BattleConfig.Instance.HitDistance)
        {
            bool next = false;
            if (Skill.Info.TargetType == eTargetType.position_next)
            {
                int target_index = target_container.target_creatures.FindIndex(c => c == m_Creature);
                if (target_index != -1)
                {
                    for (int i = target_index+1; i < target_container.target_creatures.Count + target_index; ++i)
                    {
                        ICreature target_creature = target_container.target_creatures[i % target_container.target_creatures.Count];
                        if (target_creature != null && target_creature.IsDead == false && (main_action.check_distance == false || target_creature.Character.MoveDistance < BattleConfig.Instance.HitDistance))
                        {
                            // 당첨
                            m_Creature = target_creature as BattleCreature;
                            next = true;
                            break;
                        }
                    }
                }
            }
            if (next == false)
            {
                if (m_Creature.IsDead == false)
                {
                    if (target_container.main_target != null)
                    {
                        Battle.Instance.PlayParticle(target_container.main_target, Battle.Instance.m_Miss);
                        TextManager.Instance.PushMessagePosition(target_container.main_target, Skill.Creature, Localization.Get("AttackFail"), eBuffColorType.Immune, eTextPushType.Normal, -4f);
                    }
                    return eSkillTargetHit.MissPosition;
                }
                return eSkillTargetHit.Miss;
            }
        }

        if (m_Creature.IsTeam != Skill.Creature.IsTeam)
        {
            int hit_rate = Skill.Creature.GetValue(eStatType.HitRate);
            int evade_rate = m_Creature.GetValue(eStatType.EvadeRate);
            if (MNS.Random.Instance.NextRange(1, 10000) > (hit_rate - evade_rate))
            {
                TextManager.Instance.PushMessage(m_Creature, Localization.Get("Evade"), eBuffColorType.Immune, eTextPushType.Normal);
                return eSkillTargetHit.Evade;
            }
        }

        int critical_power = Skill.Creature.GetValue(eStatType.CriticalPower);
        bool is_critical = false;
        if (BattleConfig.Instance.UseCritical && Skill.IsLeaderActive == false)
        {
            is_critical = MNS.Random.Instance.NextRange(1, 10000) <= Skill.Creature.GetValue(eStatType.CriticalChance);
            if (is_critical == false)
            {
                is_critical = ((m_Creature.IsTeam != Skill.Creature.IsTeam && (m_Creature.CanAction(Skill) == false || m_Creature.IsPlayingAction) || m_Creature.IsTeam == Skill.Creature.IsTeam && !(m_Creature.CanAction() == false || m_Creature.IsPlayingAction)));
            }
        }

        var direct_actions = Skill.Info.Actions.Where(a => a.IsDirect);
        bool is_damage_mana = direct_actions.Any(a => a.actionType == eActionType.damage_mana);

        foreach (SkillInfo.Action action in direct_actions)
        {
            int hit_value = Mathf.RoundToInt(CalculateHitValue(action) * hit_info.percent);

            eTextPushType push_type = is_critical ? eTextPushType.Critical : eTextPushType.Normal;
            if (is_critical)
                hit_value = hit_value * critical_power / 10000;

            switch (action.actionType)
            {
                case eActionType.damage:
                    {
                        if (m_Creature.InvokeImmune(eImmuneType.damage, Skill.Creature.Info.AttackType, Skill.Level) == true)
                            return eSkillTargetHit.Immune;

                        if (Skill.Creature != null)
                            Skill.Creature.AddDeal(-hit_value);

                        hit_value = m_Creature.ApplyShield(Skill.Creature.Info.AttackType, hit_value);

                        if (hit_value < 0)
                        {
                            TextManager.Instance.PushDamage(Skill.Creature.Info.AttackType == SharedData.eAttackType.physic, m_Creature, hit_value, push_type);
                            m_Creature.SetDamage(hit_value, false);

                            m_Creature.SetWait();

                            float damage_percent = -hit_value / (float)m_Creature.Stat.MaxHP;

                            if (is_damage_mana == false)
                                m_Creature.AddMP(eMPFillType.Damage, damage_percent);

                            if (Skill.IsDefault == true)
                                Skill.Creature.AddMP(eMPFillType.Deal, damage_percent);

                            action.Fire(Skill, eActionType.damage, hit_value);
                        }
                        else
                            return eSkillTargetHit.Shield;
                    }
                    break;

                case eActionType.heal:
                    {
                        TextManager.Instance.PushHeal(m_Creature, hit_value, push_type);
                        m_Creature.SetHeal(hit_value, false);
                        m_Creature.SetWait();

                        if (Skill.IsDefault == true)
                        {
                            float heal_percent = hit_value / (float)m_Creature.Stat.MaxHP;
                            Skill.Creature.AddMP(eMPFillType.Heal, heal_percent);
                        }
                    }
                    break;

                case eActionType.damage_mana:
                    {
                        if (m_Creature.InvokeImmune(eImmuneType.damage, Skill.Creature.Info.AttackType, Skill.Level) == true)
                            return eSkillTargetHit.Immune;

                        TextManager.Instance.PushMana(m_Creature, hit_value, push_type);
                        m_Creature.SetDamageMana(hit_value, false);
                        m_Creature.SetWait();
                    }
                    break;

                case eActionType.heal_mana:
                    {
                        TextManager.Instance.PushMana(m_Creature, hit_value, push_type);
                        m_Creature.SetHealMana(hit_value, false);
                        m_Creature.SetWait();

                        float heal_percent = hit_value / (float)m_Creature.Stat.MaxMP;
                        if (Skill.IsDefault == true)
                            Skill.Creature.AddMP(eMPFillType.HealMana, heal_percent);
                    }
                    break;
            }
        }

        //Debug.LogFormat("{0} -> {1} : {2}", self.Character.name, target.Character.name, skill.Info.Name);
        return eSkillTargetHit.Hit;
    }

    int buff_index = 0;
    static public ISkillBuff DoBuff(SkillInfo.Action buff_action, BattleCreature creature, BattleCreature target_creature, BattleSkill skill, int target_index, ISkillBuff parent)
    {
        switch (buff_action.actionType)
        {
            case eActionType.stun:
            case eActionType.hidden:
            case eActionType.sleep:
                {
                    if (target_creature.InvokeImmune(eImmuneType.cc, skill.Creature.Info.AttackType, skill.Level) == true)
                    {
                        return null;
                    }

                    int value = skill.GetValueWithTargetIndex(buff_action, target_index, -(target_creature.Level - skill.Level) * GameConfig.Get<int>("stun_level_decrease"));
                    if (BattleBase.Instance.Rand.NextRange(1, 10000) > value)
                    {
                        TextManager.Instance.PushMessage(target_creature, Localization.Get("Miss"), eBuffColorType.Immune, eTextPushType.Normal);
                        return null;
                    }

                    float duration = skill.GetDuration(buff_action, target_index);
                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                        TextManager.Instance.PushMessage(target_creature, Localization.Get(buff_action.actionType.ToString()), eBuffColorType.Stun, eTextPushType.Normal);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);
                    return buff;
                }

            case eActionType.ignore_defense_damage:
            case eActionType.ignore_defense_damaged:
            case eActionType.worldboss:
            case eActionType.shield:
            case eActionType.immune:
            case eActionType.provoke:
            case eActionType.buff:
            case eActionType.buff_percent:
                {
                    int value = skill.GetValueWithTargetIndex(buff_action, target_index, 0);

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);
                    target_creature.SetWait();

                    if (buff_action.statType == eStatType.AttackSpeed && duration > 0f)
                        buff.Duration *= (1f + value * 0.0001f);

                    return buff;
                }

            case eActionType.debuff:
            case eActionType.debuff_percent:
                {
                    eImmuneType immune_type = eImmuneType.debuff;
                    switch (buff_action.actionType)
                    {
                        default:
                            if (target_creature.InvokeImmune(immune_type, skill.Creature.Info.AttackType, skill.Level) == true)
                                return null;
                            break;
                    }

                    int value = -skill.GetValueWithTargetIndex(buff_action, target_index, 0);

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);

                    target_creature.SetWait();

                    if (buff_action.statType == eStatType.AttackSpeed && duration > 0f)
                        buff.Duration *= (1f - value * 0.0001f);

                    return buff;
                }

            case eActionType.dot_damage:
                {
                    int value = -skill.GetValueWithTargetIndex(buff_action, target_index, creature.GetDamageValue());

                    value = Mathf.RoundToInt(value * (1f + (creature.GetValue(eStatType.IncreaseDamagePercent) - creature.GetValue(eStatType.DecreaseDamagePercent)) * 0.0001f));

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);

                    target_creature.SetWait();

                    return buff;
                }

            case eActionType.dot_damage_mana:
                {
                    int value = -skill.GetValueWithTargetIndex(buff_action, target_index, 0);

                    float decrease = (10000 - (target_creature.Level - skill.Level) * GameConfig.Get<int>("mana_level_decrease")) * 0.0001f;
                    value = Mathf.RoundToInt(value * (1f + (creature.GetValue(eStatType.IncreaseDamagePercent) - creature.GetValue(eStatType.DecreaseDamagePercent)) * 0.0001f + decrease));

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);

                    target_creature.SetWait();
                    return buff;
                }

            case eActionType.dot_heal:
                {
                    int value = skill.GetValueWithTargetIndex(buff_action, target_index, creature.GetDamageValue());
                    value = Mathf.RoundToInt(value * (1f + creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f));

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);
                    target_creature.SetWait();

                    return buff;
                }

            case eActionType.dot_heal_mana:
                {
                    int value = skill.GetValueWithTargetIndex(buff_action, target_index, 0);
                    float decrease = (10000 - (target_creature.Level - skill.Level) * GameConfig.Get<int>("mana_level_decrease")) * 0.0001f;
                    value = Mathf.RoundToInt(value * (1f + creature.GetValue(eStatType.IncreaseDamagePercent) * 0.0001f) * decrease);

                    float duration = skill.GetDuration(buff_action, target_index);

                    Buff buff = new Buff(target_creature, skill, buff_action, value, duration, parent);
                    if (duration == 0f)
                        buff.DoAction();
                    else if (duration > 0f || duration == -1f)
                    {
                        target_creature.AddBuff(buff, skill.Info.CanStack);
                    }
                    else
                        Debug.LogErrorFormat("duration is invalid : {0}", buff_action.SkillInfo.Name);
                    target_creature.SetWait();

                    return buff;
                }

        }
        return null;
    }

    override public void OnBuff(CharacterActionBuffComponent buff_component, bool is_lighting, float apply_scale)
    {
        if (buff_index >= Skill.Info.Actions.Count)
        {
            Debug.LogErrorFormat("buff index failed : {0} in {1}", buff_index, Skill.Info.ID);
            return;
        }
        SkillInfo.Action buff_action = Skill.Info.Actions[buff_index++];

        if (m_Creature.IsDead == true || buff_action.check_distance == true && Character.MoveDistance > BattleConfig.Instance.HitDistance)
        {
            return;
        }

        ISkillBuff apply_buff = DoBuff(buff_action, Skill.Creature, m_Creature, Skill, m_TargetIndex, null);
        if (apply_buff != null)
        {
            CharacterActionBuff new_action = buff_component.data.CreateAction(buff_component, Character.PlaybackTime, Character, apply_buff.Duration, is_lighting, apply_scale);
            apply_buff.Action = new_action;
        }

        for (int sub_index = 0; sub_index < buff_action.SubActions.Count; ++sub_index)
        {
            var sub_action = buff_action.SubActions[sub_index];
            var sub_buff = DoBuff(sub_action, Skill.Creature, m_Creature, Skill, m_TargetIndex, apply_buff);
            if (sub_buff == null)
                continue;

            if (apply_buff == null)
            {
                apply_buff = sub_buff;

                if (buff_component.sub_components.Length == 0)
                {
                    CharacterActionBuff new_action = buff_component.data.CreateAction(buff_component, Character.PlaybackTime, Character, apply_buff.Duration, is_lighting, apply_scale);
                    apply_buff.Action = new_action;
                }
            }

            if (sub_index < buff_component.sub_components.Length)
            {
                var sub_component = buff_component.sub_components[sub_index];
                CharacterActionBuff new_action = sub_component.data.CreateAction(sub_component, Character.PlaybackTime, Character, apply_buff.Duration, is_lighting, apply_scale);
                sub_buff.Action = new_action;
            }
        }
    }
}

