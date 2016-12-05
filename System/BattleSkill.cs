using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedData;

public class BattleSkill
{
    public bool IsLeaderActive { get { return Info.Type == eSkillType.leader_active; } }
    public bool IsDefault { get { return this == Creature.Skills[0]; } }
    public bool IsSecond { get { return Creature.Skills.Count >= 3 && this == Creature.Skills[2]; } }
    public BattleCreature Creature { get; private set; }
    public SkillInfo Info { get; private set; }
    public short Level { get; private set; }

    public float Duration { get; private set; }

    public static Transform GetCenter(bool is_team)
    {
        if (is_team)
            return BattleBase.Instance.battle_layout.m_Mine.Center.transform;
        return BattleBase.Instance.battle_layout.m_Enemy.Center.transform;
    }

    public static List<ICreature> GetTargetList(bool is_team, eEnemyType enemy_type)
    {
        if (is_team == true)
        {
            if (enemy_type == eEnemyType.team)
                return BattleBase.Instance.characters.ToList();
            return
                BattleBase.Instance.enemies.ToList();
        }
        else
        {
            if (enemy_type == eEnemyType.team)
                return BattleBase.Instance.enemies.ToList();
            return
                BattleBase.Instance.characters.ToList();
        }
    }

    public BattleSkill(SkillInfo info, BattleCreature creature, short skill_level)
    {
        this.Info = info;
        this.Creature = creature;
        Level = skill_level;

        foreach (SkillInfo.Action action in info.Actions)
        {
            if (action.IsDirect == false)
                Duration = Mathf.Max(Duration, action.duration);
        }
    }

    public SkillTargetContainer GetTargets(bool ignore_distance, eMoveTarget move_target)
    {
        return GetTargets(Creature, Creature.IsTeam, ignore_distance, move_target);
    }

    //     public List<ISkillTarget> GetTargets(bool is_team, bool ignore_distance)
    //     {
    //         return GetTargets(null, is_team, ignore_distance);
    //     }

    SkillTargetContainer GetTargets(BattleCreature self, bool is_team, bool ignore_distance, eMoveTarget move_target)
    {
        SkillTargetContainer skill_target = new SkillTargetContainer();
        skill_target.targets = new List<ISkillTarget>();

        try
        {
            switch (Info.TargetType)
            {
                case eTargetType.self:
                    if (self == null)
                        Debug.LogError("self is null");
                    skill_target.targets.Add(new SkillTarget(skill_target, this, self, 0));
                    break;

                case eTargetType.position:
                case eTargetType.position_reverse:
                case eTargetType.all:
                case eTargetType.all_reverse:
                case eTargetType.all_center:
                case eTargetType.position_next:
                    {
                        List<ICreature> target_list = GetTargetList(is_team, Info.EnemyType);

                        switch (Info.TargetType)
                        {
                            case eTargetType.position_reverse:
                            case eTargetType.all_reverse:
                                target_list = target_list.Reverse<ICreature>().ToList();
                                break;

                            case eTargetType.position_next:
                                skill_target.target_creatures = target_list;
                                break;
                        }

                        var main_action = Info.Actions[0];

                        int selected_target_index = -1, first_target_index = -1;

                        // provoke
                        if (main_action.IsMultiTarget == false)
                        {
                            for (int target_index = 0; target_index < target_list.Count; ++target_index)
                            {
                                var creature = target_list[target_index] as BattleCreature;
                                Buff provoke = creature.GetProvoke();
                                if (provoke != null)
                                {
                                    int value = provoke.Value - (Level - provoke.Skill.Level) * GameConfig.Get<int>("stun_level_decrease");
                                    if (BattleBase.Instance.Rand.NextRange(1, 10000) > value)
                                        continue;

                                    TextManager.Instance.PushMessage(creature, Localization.Get("Aggro"), eBuffColorType.Aggro, eTextPushType.Normal);
                                    selected_target_index = target_index;
                                    break;
                                }
                            }
                        }

                        if (selected_target_index == -1)
                        {
                            for (int target_index = 0; target_index < target_list.Count; ++target_index)
                            {
                                BattleCreature target = target_list[target_index] as BattleCreature;
                                if (target == null || target.CanTarget() == false)
                                {
                                    continue;
                                }

                                if (first_target_index == -1)
                                    first_target_index = target_index;
                                if (Info.TargetType != eTargetType.all_center && Info.TargetType != eTargetType.all && Info.TargetType != eTargetType.all_reverse && main_action.CanDistance(self.Character.GetAction(Info.ActionName).Effect.GetFirstActionTime(), target) == false)
                                {
                                    continue;
                                }

                                selected_target_index = target_index;
                                break;
                            }
                            if (selected_target_index == -1)
                            {
                                if (ignore_distance == true)
                                    selected_target_index = first_target_index;

                                if (selected_target_index == -1)
                                    return null;
                            }
                        }

                        skill_target.targets.Add(new SkillTarget(skill_target, this, target_list[selected_target_index], 0));
                        var value_percent = Info.Actions[0].action_value;
                        for (int value_index = 1, target_index = 1; value_index < value_percent.value_percent.Length; value_index++, target_index++)
                        {
                            if (value_percent.value_percent[value_index] > 0)
                            {
                                if (main_action.loop_target == true || selected_target_index + target_index < target_list.Count)
                                {
                                    var target = target_list[(selected_target_index + target_index) % target_list.Count] as BattleCreature;
                                    if (target == null || target.CanTarget() == false)
                                    {
                                        if (main_action.skip_dead)
                                        {
                                            --value_index;
                                            continue;
                                        }
                                        skill_target.targets.Add(null);
                                    }
                                    else
                                        skill_target.targets.Add(new SkillTarget(skill_target, this, target, value_index));
                                }
                                else
                                    skill_target.targets.Add(null);
                                skill_target.targets.Add(null);
                            }
                        }

                        //                     if (Info.TargetType != eTargetType.all && main_action.check_distance == true && !targets.Any(t => t != null && t.Character.Creature.IsDead == false && (t.Character.transform.localPosition == Vector3.zero || ignore_distance == true)))
                        //                         return null;
                    }
                    break;

                case eTargetType.hp_min:
                    {
                        List<ICreature> target_list = GetTargetList(is_team, Info.EnemyType);

                        BattleCreature selected_target = null;
                        foreach (BattleCreature creature in target_list)
                        {
                            if (creature != null && creature.CanTarget() == true && (selected_target == null || creature.Stat.HP < selected_target.Stat.HP))
                                selected_target = creature;
                        }
                        skill_target.targets.Add(new SkillTarget(skill_target, this, selected_target, 0));
                    }
                    break;

                case eTargetType.hp_min_percent:
                    {
                        List<ICreature> target_list = GetTargetList(is_team, Info.EnemyType);

                        BattleCreature selected_target = null;
                        foreach (BattleCreature creature in target_list)
                        {
                            if (creature != null && creature.CanTarget() == true && (selected_target == null || creature.Stat.HPPercent < selected_target.Stat.HPPercent))
                                selected_target = creature;
                        }
                        skill_target.targets.Add(new SkillTarget(skill_target, this, selected_target, 0));
                    }
                    break;

                case eTargetType.attack_max:
                    {
                        List<ICreature> target_list = GetTargetList(is_team, Info.EnemyType);

                        BattleCreature selected_target = null;
                        foreach (BattleCreature creature in target_list)
                        {
                            if (creature != null && creature.CanTarget() == true && (selected_target == null || creature.GetDamageValue() > selected_target.GetDamageValue()))
                                selected_target = creature;
                        }
                        skill_target.targets.Add(new SkillTarget(skill_target, this, selected_target, 0));
                    }
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(string.Format("[{0}] {1}", Info.ID, ex.Message), ex);
        }

        if (skill_target.main_target == null && Info.TargetType != eTargetType.position_next)
        {
            if (Info.TargetType == eTargetType.all_center)
                skill_target.main_target = GetCenter(!is_team);
            else
            {
                if (move_target == eMoveTarget.Character)
                    skill_target.main_target = skill_target.targets[0].Character.transform;
                else
                    skill_target.main_target = skill_target.targets[0].Character.transform.parent;
            }
        }

        return skill_target;
    }
    
    public int GetHitValue(SkillInfo.Action action)
    {
        switch (action.actionType)
        {
            case eActionType.damage:
                return GetValue(action, Creature.GetDamageValue());

            case eActionType.heal:
                return GetValue(action, Creature.GetValue(eStatType.Heal));

            case eActionType.damage_mana:
            case eActionType.heal_mana:
                return GetValue(action, 0);
        }
        return 0;
    }

    public float GetHitValuePercent(SkillInfo.Action action, int target_index)
    {
        return action.action_value.value_percent[target_index];
    }

    public int GetValue(SkillInfo.Action action, int add_value)
    {
        return action.GetValue(Creature.GradePercent, Level) + add_value;
    }

    public int GetValueWithTargetIndex(SkillInfo.Action action, int target_index, int add_value)
    {
        return Mathf.CeilToInt(GetValue(action, add_value) * action.action_value.value_percent[target_index]);
    }

    public float GetDuration(SkillInfo.Action action, int target_index)
    {
        return action.duration;
    }
}
