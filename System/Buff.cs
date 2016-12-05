using UnityEngine;
using System.Collections;
using SharedData;

public class Buff : ISkillBuff
{
    public SkillInfo.Action ActionInfo { get; private set; }
    public int Value { get; private set; }
    public int AffectValue { get; private set; }
    public int AffectValueMax { get; private set; }
    public float AffectValuePercent { get { return (float)AffectValue / AffectValueMax; } }
    
    BattleCreature m_Creature;

    public BattleSkill Skill { get; private set; }

    public CharacterBuffContainer BuffContainer = new CharacterBuffContainer();

    public Buff(BattleCreature creature, BattleSkill skill, SkillInfo.Action action_info, int value, float duration, ISkillBuff parent)
        : base(duration)
    {
        Parent = parent;
        m_Creature = creature;
        Skill = skill;
        this.ActionInfo = action_info;
        switch(action_info.actionType)
        {
            case eActionType.dot_damage:
            case eActionType.dot_damage_mana:
            case eActionType.dot_heal:
            case eActionType.dot_heal_mana:
                this.Value = Mathf.RoundToInt(value / duration);
                break;

            default:
                this.Value = value;
                break;
        }
        this.StartTime = m_Creature.PlaybackTime;

        eBuffColorType buff_color = eBuffColorType.None;

        if (action_info.actionType == eActionType.shield && action_info.value > 0)
        {
            AffectValue = value;
            AffectValueMax = value;

            buff_color = eBuffColorType.Shield;
        }
        else
        {
            AffectValue = 1;
            AffectValueMax = 1;
        }

        if (skill != null && skill.Creature != null && IsMainBuff == true && skill.Info.ShowIcon == true)
        {
            if (buff_color != eBuffColorType.Shield)
                buff_color = creature.IsTeam == skill.Creature.IsTeam ? eBuffColorType.Buff : eBuffColorType.DeBuff;

            BuffContainer.Alloc();
            BuffContainer.Asset.Init(Skill.Info.IconID, buff_color, Duration > 0f);
            BuffContainer.Asset.OnUpdate(0f, 1f);
        }
    }

    public void DoAction()
    {
        bool show_message = ActionInfo.show_message;

        string text_statType = Localization.Get("StatType_" + ActionInfo.statType);

        switch (ActionInfo.actionType)
        {
            case eActionType.stun:
            case eActionType.hidden:
            case eActionType.sleep:
                m_Creature.CancelAction(false);
                break;

            case eActionType.shield:
            case eActionType.immune:
            case eActionType.provoke:
                break;

            case eActionType.buff:
                if (show_message)
                {
                    if (StatInfo.IsPercentValue(ActionInfo.statType) == true)
                        TextManager.Instance.PushMessage(m_Creature, string.Format("{0} +{1}%", text_statType, Value / 100), eBuffColorType.Buff, eTextPushType.Normal);
                    else
                        TextManager.Instance.PushMessage(m_Creature, string.Format("{0} +{1}", text_statType, Value), eBuffColorType.Buff, eTextPushType.Normal);
                }
                break;

            case eActionType.buff_percent:
                if (show_message)
                    TextManager.Instance.PushMessage(m_Creature, string.Format("{0} +{1:p0}", text_statType, (float)Value / 10000), eBuffColorType.Buff, eTextPushType.Normal);
                break;

            case eActionType.debuff:
                if (show_message)
                {
                    if (StatInfo.IsPercentValue(ActionInfo.statType) == true)
                        TextManager.Instance.PushMessage(m_Creature, string.Format("{0} -{1}%", text_statType, -Value / 100), eBuffColorType.DeBuff, eTextPushType.Normal);
                    else
                        TextManager.Instance.PushMessage(m_Creature, string.Format("{0} -{1}", text_statType, -Value), eBuffColorType.DeBuff, eTextPushType.Normal);
                }
                break;

            case eActionType.debuff_percent:
                if (show_message)
                    TextManager.Instance.PushMessage(m_Creature, string.Format("{0} -{1:p0}", text_statType, -(float)Value / 10000), eBuffColorType.DeBuff, eTextPushType.Normal);
                break;
        }
    }

    float m_LastUpdateTime = 0f;
    float m_DotValue = 0f;
    float m_DotTime = 0f, m_DotTick = 0.5f;

    public bool Update()
    {
        float playback_time = m_Creature.Character.PlaybackTime;
        float past_time = playback_time - StartTime;

        float percent = past_time / Duration;
        if (BuffContainer.Asset != null)
            BuffContainer.Asset.OnUpdate(percent, AffectValuePercent);

        bool is_finish = false;
        if (Action != null && Action.Update(playback_time) == false || Parent != null && Parent.IsFinish == true)
        {
            is_finish = true;
        }
        if (Action == null && Parent == null && past_time >= Duration && Duration != -1f)
        {
            is_finish = true;
        }

        if (IsFinish == true)
        {
            if (Action == null || is_finish == true)
            {
                return false;
            }
            return true;
        }

        if (BattleBase.Instance.IsBattleEnd == true)
            return true;

        switch (ActionInfo.actionType)
        {
            case eActionType.dot_damage:
            case eActionType.dot_heal:
                if ((past_time - m_DotTime >= m_DotTick || is_finish == true))
                {
                    m_DotValue = Value * (past_time - m_LastUpdateTime);
                    int hit_value = (int)m_DotValue;
                    m_DotValue -= hit_value;
                    m_LastUpdateTime = past_time;

                    bool is_critical = false;
                    if (BattleConfig.Instance.UseCritical && Skill.IsLeaderActive == false)
                    {
                        is_critical = MNS.Random.Instance.NextRange(1, 10000) <= Skill.Creature.GetValue(eStatType.CriticalChance);
                        if (is_critical == false)
                        {
                            is_critical = ((m_Creature.IsTeam != Skill.Creature.IsTeam && (m_Creature.CanAction(Skill) == false || m_Creature.IsPlayingAction) || m_Creature.IsTeam == Skill.Creature.IsTeam && !(m_Creature.CanAction() == false || m_Creature.IsPlayingAction)));
                        }
                    }

                    int critical_power = Skill.Creature.GetValue(eStatType.CriticalPower);

                    eTextPushType push_type = is_critical ? eTextPushType.CriticalDot : eTextPushType.Dot;
                    if (is_critical)
                        hit_value = hit_value * critical_power / 10000;

                    if (ActionInfo.actionType == eActionType.dot_damage)
                    {
                        if (m_Creature.InvokeImmune(eImmuneType.dot, Skill.Creature.Info.AttackType, Skill.Level) == false)
                        {
                            if (Skill.Creature != null)
                                Skill.Creature.AddDeal(-hit_value);

                            m_Creature.SetDamage(hit_value, true);
                            TextManager.Instance.PushDamage(Skill.Creature.Info.AttackType == SharedData.eAttackType.physic, m_Creature, hit_value, push_type);

                            float damage_percent = -hit_value / (float)m_Creature.Stat.MaxHP;
                            m_Creature.AddMP(eMPFillType.Damage, damage_percent);

                            if (Skill.IsDefault == true)
                                Skill.Creature.AddMP(eMPFillType.Deal, damage_percent);

                            ActionInfo.Fire(Skill, eActionType.dot_damage, hit_value);
                        }
                    }
                    else
                    {
                        m_Creature.SetHeal(hit_value, true);
                        TextManager.Instance.PushHeal(m_Creature, hit_value, push_type);

                        if (Skill.IsDefault == true)
                        {
                            float heal_percent = hit_value / (float)m_Creature.Stat.MaxHP;
                            Skill.Creature.AddMP(eMPFillType.Heal, heal_percent);
                        }
                    }
                    m_DotTime += m_DotTick;
                }

                break;

            case eActionType.dot_damage_mana:
            case eActionType.dot_heal_mana:
                if ((past_time - m_DotTime >= m_DotTick || is_finish == true))
                {
                    m_DotValue = Value * (past_time - m_LastUpdateTime);
                    int hit_value = (int)m_DotValue;
                    m_DotValue -= hit_value;
                    m_LastUpdateTime = past_time;

                    bool is_critical = false;
                    if (BattleConfig.Instance.UseCritical && Skill.IsLeaderActive == false)
                    {
                        is_critical = MNS.Random.Instance.NextRange(1, 10000) <= Skill.Creature.GetValue(eStatType.CriticalChance);
                        if (is_critical == false)
                        {
                            is_critical = ((m_Creature.IsTeam != Skill.Creature.IsTeam && (m_Creature.CanAction(Skill) == false || m_Creature.IsPlayingAction) || m_Creature.IsTeam == Skill.Creature.IsTeam && !(m_Creature.CanAction() == false || m_Creature.IsPlayingAction)));
                        }
                    }

                    int critical_power = Skill.Creature.GetValue(eStatType.CriticalPower);

                    eTextPushType push_type = is_critical ? eTextPushType.CriticalDot : eTextPushType.Dot;
                    if (is_critical)
                        hit_value = hit_value * critical_power / 10000;

                    if (ActionInfo.actionType == eActionType.dot_damage_mana)
                    {
                        if (m_Creature.InvokeImmune(eImmuneType.dot, Skill.Creature.Info.AttackType, Skill.Level) == false)
                        {
                            m_Creature.SetDamageMana(hit_value, true);
                            TextManager.Instance.PushMana(m_Creature, hit_value, push_type);
                        }
                    }
                    else
                    {
                        m_Creature.SetHealMana(hit_value, true);
                        TextManager.Instance.PushMana(m_Creature, hit_value, push_type);
                    }
                    m_DotTime += m_DotTick;
                }

                break;
        }

        if (is_finish)
        {
            IsFinish = true;
            return false;
        }
        return true;
    }

    public void Finish()
    {
        if (IsFinish == true)
            return;

        IsFinish = true;
        if (Action != null)
            Action.Finish();
        Update();
    }

    public void Clear()
    {
        if (BuffContainer.IsInit)
            BuffContainer.Free();
    }

    public int ApplyShield(eAttackType attack_type, int damage)
    {
        if (ActionInfo.value == -1)
            return damage;

        if (Action != null)
            Action.OnHit();
        m_Creature.Character.CharacterAnimation.SetShield();

        AffectValue += damage;
        if (AffectValue > 0)
        {
            TextManager.Instance.PushMessage(m_Creature, GetImmuneMessage(eImmuneType.damage, attack_type), eBuffColorType.Shield, eTextPushType.Normal);

            return 0;
        }

        Finish();
        return AffectValue;
    }

    public bool ApplyImmune(eImmuneType immune_type, eAttackType attack_type, short skill_level)
    {
        if (ActionInfo.value < 100000)
        {
            int value = ActionInfo.value - (skill_level - Skill.Level) * GameConfig.Get<int>("stun_level_decrease");
            if (BattleBase.Instance.Rand.NextRange(1, 10000) > value)
                return false;
        }

        if (Action != null)
            Action.OnHit();

        m_Creature.Character.CharacterAnimation.SetShield();

        if (immune_type == eImmuneType.dot)
            TextManager.Instance.PushMessage(m_Creature, GetImmuneMessage(immune_type, attack_type), eBuffColorType.Immune, eTextPushType.Dot);
        else
            TextManager.Instance.PushMessage(m_Creature, GetImmuneMessage(immune_type, attack_type), eBuffColorType.Immune, eTextPushType.Normal);
        return true;
    }

    string GetImmuneMessage(eImmuneType immune_type, eAttackType attack_type)
    {
        bool shield = ActionInfo.actionType == eActionType.shield;

        string message = "";

        if (attack_type == eAttackType.heal)
            message = Localization.Get("ImmuneHeal");
        else
        {
            bool immune_damage = shield == true || ActionInfo.immune_types.Contains(eImmuneType.damage);
            bool immune_dot = shield == false && ActionInfo.immune_types.Contains(eImmuneType.dot);
            bool immune_debuff = shield == false && ActionInfo.immune_types.Contains(eImmuneType.debuff);
            bool immune_cc = shield == false && ActionInfo.immune_types.Contains(eImmuneType.cc);

            if (immune_damage && immune_dot && immune_debuff && immune_cc)
                message = Localization.Get("ImmuneAll");
            else
            {
                switch (immune_type)
                {
                    case eImmuneType.damage:
                    case eImmuneType.dot:
                        if (immune_damage && immune_dot)
                            message = Localization.Get("ImmuneDamageDotDamage");
                        else if (immune_damage)
                            message = Localization.Get("ImmuneDamage");
                        else
                            message = Localization.Get("ImmuneDotDamage");
                        break;

                    case eImmuneType.debuff:
                    case eImmuneType.cc:
                        if (immune_debuff && immune_cc)
                            message = Localization.Get("ImmuneDebuffCC");
                        else if (immune_debuff)
                            message = Localization.Get("ImmuneDebuff");
                        else
                            message = Localization.Get("ImmuneCC");
                        break;
                }
            }

            bool immune_attack_type_physic = ActionInfo.attack_types.Contains(eAttackType.physic);
            bool immune_attack_type_magic = ActionInfo.attack_types.Contains(eAttackType.magic);

            if (immune_attack_type_physic == true && immune_attack_type_magic == false)
                message = Localization.Format("ImmunePhysicFormat", message);
            else if (immune_attack_type_physic == false && immune_attack_type_magic == true)
                message = Localization.Format("ImmuneMagicFormat", message);
        }

        if (shield)
            return Localization.Format("ShieldFormat", message);
        else
            return Localization.Format("ImmuneFormat", message);
    }

    public void SetHidden(bool hidden)
    {
        if (Action != null)
            Action.SetHidden(hidden);
    }
}
