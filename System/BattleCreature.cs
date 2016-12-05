using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeroFX;
using System;
using System.Linq;
using SharedData;
using PacketEnums;

public class BattleCreature : ICreature
{
    public CreatureStat Stat { get; private set; }
    public short Grade { get; private set; }
    public short Enchant { get; private set; }
    public short Level { get; private set; }


    public int Deal = 0;

    public List<BattleSkill> Skills { get; private set; }

    public BattleSkill LeaderSkill { get; private set; }

    public List<Buff> Buffs { get; private set; }

    public float GradePercent { get; private set; }

    public float LowHP = 0.3f;

    public bool IsPlayingAction { get { return Character.IsPlayingActionAnimation; } }
    public bool IsPlayingSkill { get { return m_UseSkill && IsPlayingAction; } }
    public bool IsManaFill { get; set; }
    bool m_UseSkill = false;

    public bool IsIgnoreDefenseDamage { get { return Buffs.Any(b => b.ActionInfo.actionType == eActionType.ignore_defense_damage); } }
    public bool IsIgnoreDefenseDamaged { get { return Buffs.Any(b => b.ActionInfo.actionType == eActionType.ignore_defense_damaged); } }
    public bool IsWorldBoss { get { return Buffs.Any(b => b.ActionInfo.actionType == eActionType.worldboss); } }

    public void AddDeal(int value)
    {
        Deal += value;
    }

    public bool IsHidden()
    {
        return Buffs.Any(b => b.IsFinish == false && b.ActionInfo.actionType == eActionType.hidden);
    }

    public bool IsStunned(BattleSkill skill = null)
    {
        return Buffs.Any(b => b.IsFinish == false && (b.ActionInfo.actionType == eActionType.stun || b.ActionInfo.actionType == eActionType.hidden) && b.Skill != skill);
    }

    public bool IsSlept(BattleSkill skill = null)
    {
        return Buffs.Any(b => b.IsFinish == false && b.ActionInfo.actionType == eActionType.sleep && b.Skill != skill);
    }

    public bool CanAttack
    {
        get
        {
            return CanAction() && PlaybackTime > m_WaitTime && IsPlayingAction == false;
        }
    }

    public bool IsMPFull { get { return Stat.IsMPFull; } }
    public bool CanSkill
    {
        get
        {
            return BattleConfig.Instance.UseSkill && CanAction() && Stat.IsMPFull == true;
        }
    }

    public bool CanAction(BattleSkill skill = null)
    {
        return BattleBase.Instance.IsBattleStart == true && BattleBase.Instance.IsBattleEnd == false && !IsDead && !IsWin && !IsStunned(skill) && !IsSlept(skill);
    }

    public bool CanTarget()
    {
        return IsDead == false && IsHidden() == false;
    }

    public bool IsWin { get; private set; }
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
                SetDie();
            else
                Character.CancelAnimation();
        }
    }

    public short AutoSkillIndex = 0;
    public bool IgnoreTween { get; private set; }

    public float AttackNextTime { get; private set; }
    public CharacterHPBar HPBar { get; private set; }
    public CharacterSkill SkillName { get; private set; }

    public void Restart(float attack_next_time)
    {
        PlaybackTime = 0f;
        AttackNextTime = attack_next_time;
        IsManaFill = false;
        m_WaitTime = 0f;

        HPBar.UpdateHPBar();

        //        Character.UpdatePlay(0f);
    }

    public void InitContainer(CharacterContainer character_container)
    {
        if (character_container == Container)
            return;

        if (Container != null)
            Container.Uninit();

        Container = character_container;
        character_container.Init(AssetManager.GetCharacterAsset(Info.ID, SkinName));
        Character.Creature = this;
        Character.IsPause = true;
        Character.IgnoreTween = IgnoreTween;

        Character.transform.localScale = Vector3.one * Scale;
    }

    void InitCommon(CharacterContainer character_container, float attack_next_time, GameObject hpBarPrefab, GameObject skillPrefab)
    {
        InitContainer(character_container);

        HPBar = (GameObject.Instantiate(hpBarPrefab) as GameObject).GetComponent<CharacterHPBar>();
        HPBar.transform.SetParent(BattleBase.Instance.HPBarCanvas.transform, false);
        HPBar.Init(this, IsTeam == false, Scale);

        SkillName = (GameObject.Instantiate(skillPrefab) as GameObject).GetComponent<CharacterSkill>();
        SkillName.transform.SetParent(BattleBase.Instance.CharacterSkillCanvas.transform, false);
        SkillName.Init(Character.transform, IsTeam);


        Skills = new List<BattleSkill>();
        Buffs = new List<Buff>();

        Restart(attack_next_time);
    }

    public BattleCreature(MapCreatureInfo map_creature_info, CharacterContainer character_container, float attack_next_time, GameObject hpBarPrefab, GameObject skillPrefab)
    {
        MapStageDifficulty stage_info = Network.BattleStageInfo;

        IsTeam = false;
        Info = map_creature_info.CreatureInfo;
        SkinName = map_creature_info.SkinName;
        MapCreature = map_creature_info;

        Grade = map_creature_info.Grade;
        Level = map_creature_info.Level;
        GradePercent = map_creature_info.GradePercent;
        Enchant = map_creature_info.Enchant;

        if (map_creature_info.UseLeaderSkillType != pe_UseLeaderSkillType.Manual && Info.TeamSkill != null)
        {
            SetLeader(map_creature_info.UseLeaderSkillType, BattleStage.Instance.OnUseEnemyLeaderSkill);
        }

        switch (map_creature_info.CreatureType)
        {
            case eMapCreatureType.Elite:
                Scale = 1.2f;
                break;

            case eMapCreatureType.Boss:
                Scale = 1.4f;

                if (stage_info.MapInfo.MapType == "boss")
                {
                    Level = Boss.CalculateLevel(Level, stage_info);
                    Grade = Boss.CalculateGrade(Level);
                    Enchant = Boss.CalculateEnchant(Level);
                    GradePercent = CreatureInfoManager.Instance.Grades[Grade].enchants[Enchant].stat_percent*GameConfig.Get<float>("boss_grade_percent");
                }
                Battle.Instance.m_BossHP.Init(this);

                break;

            case eMapCreatureType.WorldBoss:
                Scale = 2.5f;
                BattleWorldboss.Instance.m_BossHP.Init(this, true);
                IgnoreTween = true;
                TextOffset = -20f;
//                IsShowText = false;
                break;

        }
        Stat = new CreatureStat(map_creature_info.GetStat(Level, GradePercent, Enchant));

        AutoSkillIndex = map_creature_info.AutoSkillIndex;
        InitCommon(character_container, attack_next_time, hpBarPrefab, skillPrefab);

        if (map_creature_info.CreatureType == eMapCreatureType.WorldBoss)
            HPBar.gameObject.SetActive(false);

        foreach (SkillInfo skill_info in Info.Skills)
        {
            if (skill_info.Type != eSkillType.active) continue;
            Skills.Add(new BattleSkill(skill_info, this, map_creature_info.Level));
        }

        if (map_creature_info.PassiveInfos.Count > 0)
        {
            foreach (var passive_info in map_creature_info.PassiveInfos)
            {
                bool first = true;
                var battle_skill = new BattleSkill(passive_info.SkillInfo, this, map_creature_info.Level);
                foreach (var action in passive_info.SkillInfo.Actions)
                {
                    ISkillBuff buff = SkillTarget.DoBuff(action, this, this, battle_skill, 0, null);
                    if (first == true && string.IsNullOrEmpty(passive_info.SkillInfo.ActionName) == false)
                    {
                        first = false;
                        var comp = AssetManager.GetCharacterPrefab("PassiveEtc_" + passive_info.SkillInfo.ActionName).GetComponent<CharacterActionBuffComponent>();
//                         comp.data.InitEffect();
                        CharacterActionBuff new_action = comp.data.CreateAction(comp, PlaybackTime, Character, 999f, false, 1f);
                        buff.Action = new_action;
                    }
                }
            }
            var buff_worldboss = Buffs.Find(b => b.ActionInfo.actionType == eActionType.worldboss);
            if (buff_worldboss != null)
            {
                Stat.HP = Stat.MaxHP = buff_worldboss.ActionInfo.value;
            }
            else
                Stat.HP = Stat.MaxHP = Stat.Stat.MaxHP;
            Stat.MP = Stat.Stat.ManaInit;
        }
    }

    public BattleCreature(Creature creature, CharacterContainer character_container, float attack_next_time, GameObject hpBarPrefab, GameObject skillPrefab, bool is_team = true)
    {
        IsTeam = is_team;
        this.Info = creature.Info;
        SkinName = creature.SkinName;
        Idx = creature.Idx;
        Scale = 1f;

        GradePercent = creature.GradePercent;
        if (Network.BattleStageInfo != null && Network.BattleStageInfo.MapInfo.MapType == "boss" && Network.BattleStageInfo.MapInfo.MapType == "worldboss" && Network.BattleStageInfo.Recommends.Exists(r => r.ID == Info.ID))
            GradePercent *= GameConfig.Get<float>("boss_recommend_grade_percent");

        Stat = new CreatureStat(creature.CalculateBattleStat(GradePercent));
        Debug.LogFormat("{0} : hp:{1}, a:{2}, pd:{3}, md:{4}", Info.ID, Stat.HP, Stat.Stat.GetAttack(), Stat.Stat.PhysicDefense, Stat.Stat.MagicDefense);
        Grade = creature.Grade;
        Level = creature.Level;

        InitCommon(character_container, attack_next_time, hpBarPrefab, skillPrefab);

        foreach (SkillInfo skill_info in Info.Skills)
        {
            if (skill_info.Type != eSkillType.active) continue;
            Skills.Add(new BattleSkill(skill_info, this, creature.Skills.Find(s => s.Info.ID == skill_info.ID).Level));
        }
    }

    override public void Update(float deltaTime, float deltaTimeIgnore)
    {
        if (IgnoreSpeed == true)
            PlaybackTime += deltaTimeIgnore * PlaybackSpeed;
        else
            PlaybackTime += deltaTime * PlaybackSpeed;

        Character.UpdatePlay(PlaybackTime);

        if (BattleBase.Instance.IsPause == ePauseType.Pause || Time.timeScale == 0f)
            return;

        UpdateWin();
        UpdateBuffs();

        if (IsTeam == false && Tutorial.Instance.IsWaitManaFull() == true)
            return;

        if (CanAttack == false || BattleBase.Instance.IsBattleEnd == true)
            return;

#if UNITY_EDITOR
        //         if (Stat.HP > 10) Stat.HP = 10;
        //      if (IsTeam == true) Stat.MP = 10000;
#endif
        if (UpdateLeaderSkill() == true)
            return;

        if (CanSkill && (IsTeam == false || BattleBase.Instance.IsAuto))// && BattleBase.Instance.IsLightingEnd == false)
        {
//             if (IsTeam == false)
//             {
                if (Info.HasSkill == true)
                {
                    if (AutoSkillIndex == 0)
                        DoAction(BattleBase.Instance.Rand.NextRange(1, Math.Min(2, Skills.Count - 1)), true, false);
                    else
                        DoAction(AutoSkillIndex, true, false);
                }
                else
                    DoAction(0, true, false);
//             }
//             else
//             {
//                 DoAction(AutoSkillIndex, true, false);
//             }
        }
        else if (PlaybackTime >= AttackNextTime)
        {
            DoAction(0, false, true);

            float next_attack_time = BattleBase.Instance.NextAttackTime() * BattleBase.Instance.NextAttackTimePercent();
            AttackNextTime = PlaybackTime + next_attack_time;
        }
    }

    bool UpdateLeaderSkill()
    {
        if ((IsTeam == true && BattleBase.Instance.IsAuto == false) || LeaderSkill == null || m_UsedLeaderSkill == true || UseLeaderSkillType == pe_UseLeaderSkillType.Manual || BattleBase.Instance.IsActionMode)
            return false;

        switch(UseLeaderSkillType)
        {
            case pe_UseLeaderSkillType.Start:
                if (BattleBase.Instance.IsBattleStart)
                {
                    UseLeaderSkill();
                    return true;
                }
                break;

            case pe_UseLeaderSkillType.LastWave:
                if (BattleStage.Instance != null && BattleBase.Instance.IsBattleStart && BattleStage.Instance.IsLastWave)
                {
                    UseLeaderSkill();
                    return true;
                }
                break;

            case pe_UseLeaderSkillType.SelfDanger:
                if (BattleBase.Instance.IsBattleStart && Stat.HPPercent < 0.4f)
                {
                    UseLeaderSkill();
                    return true;
                }
                break;

            case pe_UseLeaderSkillType.TeamDanger:
                if (BattleBase.Instance.IsBattleStart && BattleBase.Instance.characters.Any(c => c.IsDead == true || (c as BattleCreature).Stat.HPPercent < 0.4f))
                {
                    UseLeaderSkill();
                    return true;
                }
                break;
        }
        return false;
    }

    void UpdateBuffs()
    {
        Character.CharacterAnimation.StateColor = BattleBase.Instance.color_container.Colors[0].color;

        for (int i = 0; i < Buffs.Count; )
        {
            Buff buff = Buffs[i];
            if (buff.Update() == false)
            {
                RemoveBuff(buff);
            }
            else
                ++i;
        }

        if (Buffs.Count == 0)
            return;

        var state_list = Buffs.Where(b => b.IsFinish == false && b.UseStateColor);
        if (state_list.Count() == 0)
            return;

        // TODO : state color priorty
        var state_buff = state_list.Last();
        if (state_buff != null)
            Character.CharacterAnimation.StateColor = state_buff.StateColor;
    }

    public bool DoAction(int index, bool is_skill, bool ignore_distance)
    {
        index = Math.Min(index, Skills.Count - 1);
        BattleSkill skill = Skills[index];

        return DoAction(skill, is_skill, ignore_distance);
    }

    public void CancelAction(bool stopAll)
    {
//        if (stopAll == true || IsPlayingAction == true || forced == true)
        {
            Character.CancelAction(stopAll);
            BattleBase.Instance.RemoveLighting(this);
        }
    }

    public void SetWait()
    {
        if (m_DummyMode != eCharacterDummyMode.None)
            return;

        m_WaitTime = PlaybackTime + 0.3f;
    }
    float m_WaitTime = 0f;

    void PlayTargetEffect(bool is_team, List<ISkillTarget> targets, bool attach_target)
    {
        foreach (var target in targets)
        {
            if (target == null || target.Character == null || target.Character.Creature.IsDead == true)
                continue;

            ICreature target_creature = target.Character.Creature;

            Transform parent = null;
            if (attach_target)
                parent = target_creature.Character.transform;
            else
                parent = target_creature.Character.transform.parent;

            BattleBase.Instance.PlayParticle(parent, target_creature.IsTeam == is_team ? BattleBase.Instance.m_SkillTargetTeam : BattleBase.Instance.m_SkillTargetEnemy);
        }
    }

    public bool DoAction(BattleSkill skill, bool is_skill, bool ignore_distance)
    {
        if (CanAction() == false)
            return false;

        if (is_skill == true)
        {
            if (Stat.IsMPFull == false)
                return false;
        }

        SkillTargetContainer target = skill.GetTargets(ignore_distance, skill.Info.MoveTarget);
        if (target == null)
            return false;

        CancelAction(false);

        float delay = skill.IsDefault == false ? BattleConfig.Instance.SkillDelay : 0f;
        CharacterAction action = Character.DoAction(delay, skill.Info.ActionName, target, skill.IsDefault == false, (ConfigData.Instance.UseBattleEffect == true && skill.IsDefault == false)?BattleBase.LightingScaleValue:Scale-1f, skill.Duration);
        if (is_skill == true || skill.Info.Type == eSkillType.leader_active)
        {
            if (skill.Info.Type == eSkillType.leader_active)
            {
                BattleBase.Instance.AddLightingTargets(IsTeam, target.targets, this);
            }
            else
            {
                BattleBase.Instance.PlayParticle(Character.transform, BattleBase.Instance.m_SkillCasting);

                float slow_time = action.FirstActionTime;

                if (slow_time > 0f)
                {
                    BattleBase.Instance.AddLighting(this, slow_time + delay, action.Data.Effect.ScaleTime == 0f ? 0f : action.Data.Effect.ScaleTime + delay, action.Data.Effect.JumpScale);
                    BattleBase.Instance.AddLightingTargets(IsTeam, target.targets, this);

                    PlayTargetEffect(IsTeam, target.targets, skill.Info.MoveTarget == eMoveTarget.Character || skill.Info.Actions[0].check_distance == false);
                }
                else
                {
                    Debug.LogErrorFormat("SetLightingTime Error : {0}, {1}, {2}", Info.Name, skill.Info.Name, slow_time);
                }
            }
        }

        if (skill.Info.Type != eSkillType.leader_active)
            AttackNextTime += action.Length;

        m_UseSkill = is_skill;
        if (Stat.IsMPFull == true && is_skill == true)
        {
            Stat.MP = 0;
            Battle.Instance.CheckManaFill(this);
        }
        else
            AddMP(eMPFillType.Action, 1f);

        if (skill.IsDefault == false && skill.IsLeaderActive == false)
        {
            SkillName.Show(skill.Info.Name, ConfigData.Instance.UseBattleEffect ? (1f+BattleBase.LightingScaleValue*0.3f):Scale);
        }

        return true;
    }

    public int GetDamageValue()
    {
        switch(Info.AttackType)
        {
            case eAttackType.physic:
                return Stat.GetValue(eStatType.PhysicAttack);

            case eAttackType.magic:
                return Stat.GetValue(eStatType.MagicAttack);

            case eAttackType.heal:
                return Stat.GetValue(eStatType.Heal);
        }
        return 0;
    }

    public int GetValue(eStatType type)
    {
        return Stat.GetValue(type);
    }

    public void UpdateMana(int add_mp)
    {
        if (IsDead == true)
            return;

        add_mp = Mathf.RoundToInt(add_mp * (1f + Stat.Stat.ManaRegenPercent / 10000f));

        Stat.AddMP(add_mp);
    }

    public void AddMPValue(int add_mp)
    {
        if (IsDead == true)
            return;

        Stat.AddMP(add_mp);
    }

    public int AddMP(eMPFillType type, float mp_percent)
    {
        if (IsDead == true)
            return 0;

        var mp_data = BattleConfig.Instance.MP[(int)type];

        int get_mp = Mathf.CeilToInt(mp_data.MP * mp_percent);

        Stat.AddMP(get_mp);
        return get_mp;
    }

    public void SetDamage(int damage, bool is_dot)
    {
        if (IsDead == true || BattleBase.Instance.IsBattleEnd == true)
            return;

        if (is_dot == false && IsSlept() == true)
        {
            var buffs = Buffs.Where(b => b.ActionInfo.actionType == eActionType.sleep).ToList();
            foreach (var sleep in buffs)
            {
                sleep.Finish();
            }
        }

        damage = Mathf.RoundToInt(damage * (1f - Stat.Stat.DecreaseDamagePercent * 0.0001f));
        int set_hp = Stat.HP + damage;
        if (set_hp < 0 && IsWorldBoss == false)
            set_hp = 0;
        Stat.HP = set_hp;
        CheckDie();
    }

    public void SetWaveHeal(int heal)
    {
        if (IsDead == true) return;
        Stat.HP = Math.Min(Stat.GetValue(eStatType.MaxHP), Stat.HP + heal);
    }
    public void SetHeal(int heal, bool is_dot)
    {
        if (IsDead == true || BattleBase.Instance.IsBattleEnd == true)
            return;

        if (is_dot == false && IsSlept() == true)
        {
            var buffs = Buffs.Where(b => b.ActionInfo.actionType == eActionType.sleep).ToList();
            foreach (var sleep in buffs)
            {
                sleep.Finish();
            }
        }

        Stat.HP = Math.Min(Stat.GetValue(eStatType.MaxHP), Stat.HP + heal);
    }

    public void SetDamageMana(int damage, bool is_dot = false)
    {
        if (IsDead == true || BattleBase.Instance.IsBattleEnd == true)
            return;

        if (is_dot == false && IsSlept() == true)
        {
            var buffs = Buffs.Where(b => b.ActionInfo.actionType == eActionType.sleep).ToList();
            foreach (var sleep in buffs)
            {
                sleep.Finish();
            }
        }

        damage = Mathf.RoundToInt(damage * (1f - Stat.Stat.DecreaseDamagePercent * 0.0001f));

        Stat.MP = Math.Max(0, Stat.MP + damage);
    }

    public void SetHealMana(int heal, bool is_dot)
    {
        if (IsDead == true || BattleBase.Instance.IsBattleEnd == true)
            return;

        if (is_dot == false && IsSlept() == true)
        {
            var buffs = Buffs.Where(b => b.ActionInfo.actionType == eActionType.sleep).ToList();
            foreach (var sleep in buffs)
            {
                sleep.Finish();
            }
        }

        Stat.MP = Math.Min(Stat.MaxMP, Stat.MP + heal);
    }

    void CheckDie()
    {
        if (Stat.HP <= 0 && IsDead == false)
        {
            if (IsWorldBoss == true)
            {
                Stat.HP += Stat.MaxHP;
                ++Stat.DieCount;
            }
            else
            {
                IsDead = true;
                HPBar.gameObject.SetActive(false);

                if (LeaderSkill != null && OnUseLeaderSkill != null)
                    Battle.Instance.m_UILeaderSkill.SetDisable();
            }
        }
    }

    void SetDie()
    {
        Stat.MP = 0;
        Character.CharacterAnimation.UseMove = false;
        Character.CancelAction(false);
        Character.PlayAction("die");
        BattleBase.Instance.tween_system.Play("die", null, Character.GetComponent<HFX_TweenSystem>(), Character.transform.GetChild(0));
        ClearBuff();

        BattleBase.Instance.RemoveLighting(this);
    }

    float WinTime = 0f;
    public void Finish(bool is_win)
    {
        if (IsDead == false && is_win == true)
        {
            ClearBuff();
            IsWin = is_win;
            WinTime = PlaybackTime + 1f;
        }
        else
        {
            HPBar.gameObject.SetActive(false);
            Clear();
        }
    }

    void UpdateWin()
    {
        if (IsWin == false || WinTime == 0f)
            return;

        if (WinTime < PlaybackTime)
        {
            Clear();
            WinTime = 0f;
            if (IsDead == false)
                Character.PlayAction("win");
        }
    }

    public void Clear()
    {
        if (IsDead == false)
            Character.CancelAction(true);
        ClearBuff();
        BattleBase.Instance.RemoveLighting(this);
    }

    public void AddBuff(Buff buff, bool can_stack)
    {
        if (BattleBase.Instance.IsBattleEnd == true)
            return;

        if (buff.IsMainBuff == true && can_stack == false)
        {
            foreach (Buff find_buff in Buffs.FindAll(b => b.Skill == buff.Skill))
            {
                find_buff.Finish();
            }
        }

        buff.EndTime = PlaybackTime + buff.Duration;
        buff.DoAction();
        Buffs.Add(buff);
        //        Buffs.Sort(Buff.SortByEndTime);

        if (buff.BuffContainer.Asset)
            HPBar.AddBuff(buff.BuffContainer.Asset);

        switch (buff.ActionInfo.actionType)
        {
            case eActionType.buff:
            case eActionType.debuff:
            case eActionType.buff_percent:
            case eActionType.debuff_percent:
                RefreshStat();
                break;
        }
    }

    void RemoveBuff(Buff buff)
    {
        if (buff.BuffContainer.Asset != null)
            HPBar.RemoveBuff(buff.BuffContainer.Asset);
        buff.Finish();
        buff.Clear();
        if (Buffs.Remove(buff) == true)
        {
            switch (buff.ActionInfo.actionType)
            {
                case eActionType.buff:
                case eActionType.debuff:
                case eActionType.buff_percent:
                case eActionType.debuff_percent:
                    RefreshStat();
                    break;
            }
        }
    }

    void ClearBuff()
    {
        List<Buff> buffs = new List<Buff>(Buffs);
        foreach (Buff buff in buffs)
        {
            if (buff.Duration > 0)
            {
                if (buff.BuffContainer.Asset != null)
                    HPBar.RemoveBuff(buff.BuffContainer.Asset);
                buff.Finish();
                buff.Clear();
            }
        }
        RefreshStat();
    }

    void RefreshStat()
    {
        Stat.CalculateStat(Buffs);
        PlaybackSpeed = 1f + Stat.Stat.AttackSpeed*0.0001f;
    }

    public void ResetNextAttackTime()
    {
        AttackNextTime = 0f;
    }

    public Buff GetProvoke()
    {
        if (IsHidden() == true)
            return null;

        Buff provoke = Buffs.Find(b => b.IsFinish == false && b.ActionInfo.actionType == eActionType.provoke);
        if (provoke == null)
            return null;
        return provoke;
    }

    public bool InvokeImmune(eImmuneType immune_type, eAttackType attack_type, short skill_level)
    {
        if (IsHidden() == true)
            return true;

        List<Buff> shield_buffs = Buffs.Where(b => b.IsFinish == false && b.ActionInfo.actionType == eActionType.immune && b.ActionInfo.immune_types.Contains(immune_type) == true && b.ActionInfo.attack_types.Contains(attack_type) == true).ToList();
        foreach (Buff buff in shield_buffs)
        {
            if (buff.ApplyImmune(immune_type, attack_type, skill_level) == true)
                return true;
        }
        return false;
    }

    public int ApplyShield(eAttackType attack_type, int damage)
    {
        List<Buff> shield_buffs = Buffs.Where(b => b.IsFinish == false && b.ActionInfo.actionType == eActionType.shield && b.ActionInfo.attack_types.Contains(attack_type) == true && b.ActionInfo.value > 0).ToList();
        foreach (Buff buff in shield_buffs)
        {
            damage = buff.ApplyShield(attack_type, damage);
            if (damage == 0)
                return 0;
        }
        return damage;
    }

    float backup_playback_time = 0f;
    eCharacterDummyMode m_DummyMode = eCharacterDummyMode.None;
    override public void SetDummyMode(eCharacterDummyMode mode)
    {
        if (mode != eCharacterDummyMode.None)
        {
            backup_playback_time = PlaybackTime;
        }
        else if (backup_playback_time > 0f)
        {
            if (IsDead == false)
            {
                if (m_DummyMode != eCharacterDummyMode.Active)
                {
                    PlaybackTime = backup_playback_time;
                    if (Character != null)
                        Character.UpdatePlay(PlaybackTime);
                }
            }
            backup_playback_time = 0f;
        }
        m_DummyMode = mode;

        foreach (var buff in Buffs)
        {
            buff.SetHidden(mode != eCharacterDummyMode.None);
        }
        if (Character != null)
            Character.SetDummyMode(mode);
    }

    public delegate void OnUseLeaderSkillDelegate(BattleSkill skill);
    OnUseLeaderSkillDelegate OnUseLeaderSkill;

    bool m_UsedLeaderSkill = false;
    pe_UseLeaderSkillType UseLeaderSkillType = pe_UseLeaderSkillType.Manual;
    public void SetLeader(pe_UseLeaderSkillType condition, OnUseLeaderSkillDelegate callback)
    {
        UseLeaderSkillType = condition;
        OnUseLeaderSkill = callback;
        LeaderSkill = new BattleSkill(Info.TeamSkill, this, Level);
    }

    public void UseLeaderSkill()
    {
        if (m_UsedLeaderSkill == true)
            return;

        if (DoAction(LeaderSkill, false, true) == false)
            return;

        m_UsedLeaderSkill = true;
        OnUseLeaderSkill(LeaderSkill);
    }
}
