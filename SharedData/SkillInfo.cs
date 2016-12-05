using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MNS;
using System.Xml;
using System.Linq;
using SharedData;

public enum eEnemyType
{
    enemy,
    team,
}

public enum eActionType
{
    damage,
    damage_mana,
    heal,
    heal_mana,
    stun,
    hidden,
    buff,
    debuff,
    buff_percent,
    debuff_percent,
    dot_damage,
    dot_damage_mana,
    dot_heal,
    dot_heal_mana,
    sleep,
    shield,
    immune,
    provoke,
    passive,
    passive_percent,
    hp_drain,
    ignore_defense_damage,
    ignore_defense_damaged,
    worldboss,     // for worldboss
}

public enum eActionDirection
{
    forward,
    backward,
    both,
}

public enum eSkillType
{
    active,
    active_plus,
    passive,
    passive_etc,
    leader_active,
}

public enum eTargetType
{
    position,
    position_reverse,
    all,
    all_reverse,
    all_center,
    self,
    hp_min,
    hp_min_percent,
    attack_max,
    position_next,
}

public enum eImmuneType
{
    debuff, // includes : debuff, debuff_percent
    dot,    // includes : dot_damage, dot_damage_mana
    cc,     // includes : stun, sleep
    damage  // includes : damage
}

public class SkillInfoManager : InfoManager<SkillInfo, SkillInfo, SkillInfoManager>
{
}

public class SkillInfo : InfoBaseString
{
    public string Name { get; private set; }
    public string Desc { get; private set; }
    public eSkillType Type { get; private set; }
    public eEnemyType EnemyType { get; private set; }
    public eTargetType TargetType { get; private set; }
    public List<Action> Actions { get; private set; }
    public string ActionName { get; private set; }
    public string IconID { get; private set; }
    public bool ShowIcon { get; private set; }
    public bool CanStack { get; private set; }
    public eMoveTarget MoveTarget { get; private set; }

    public string DescPerLevel(float grade_percent, int level, int new_level)
    {
        if (Actions == null) return "";
        string res = "";

        for (int i = 0; i < Actions.Count; ++i)
        {
            Action action = Actions[i];

            for (int sub = -1; sub < action.SubActions.Count; ++sub)
            {
                Action select_action = action;
                if (sub != -1)
                    select_action = action.SubActions[sub];

                string action_desc = select_action.DescPerLevel(grade_percent, level, new_level);
                if (string.IsNullOrEmpty(action_desc) == false)
                {
                    if (string.IsNullOrEmpty(res) == false)
                        res += "\n";
                    res += action_desc;
                }
            }
        }
        return res.TrimEnd();
    }

    public string DescTotal(float grade_percent, int skill_level)
    {
        if (Actions == null) return "";
        string res = "";

        for (int i = 0; i < Actions.Count; ++i)
        {
            Action action = Actions[i];

            for (int sub = -1; sub < action.SubActions.Count; ++sub)
            {
                Action select_action = action;
                if (sub != -1)
                    select_action = action.SubActions[sub];

                string action_desc = select_action.DescPerLevel(grade_percent, skill_level, 0);
                if (string.IsNullOrEmpty(action_desc) == false)
                {
                    if (string.IsNullOrEmpty(res) == false)
                        res += "\n";
                    res += action_desc;
                }
            }
        }
        return res.TrimEnd();
    }
    public int GetValue(int index, int skill_level)
    {
        if (Actions == null || Actions.Count < index)
            return 0;
        return Actions[index].GetValue(1f, skill_level);
    }
    public class ActionValue
    {
        public bool IsMultiTarget;

        public float value_percent_total;
        public float[] value_percent = new float[5] { 1f, 0f, 0f, 0f, 0f };

        public ActionValue(XmlNode node)
        {
            string[] child_nodes = { "Value" };
            float[][] tmp_value = { value_percent };
            for (int i = 0; i < child_nodes.Length; ++i)
            {
                XmlNode valueNode = node.SelectSingleNode(child_nodes[i]);
                if (valueNode != null)
                {
                    for (int j = 0; j < 5; ++j)
                    {
                        XmlAttribute att = valueNode.Attributes[string.Format("R{0}", j + 1)];
                        if (att != null)
                            tmp_value[i][j] = float.Parse(att.Value);
                        else
                            tmp_value[i][j] = 0f;
                    }
                }
            }
            value_percent_total = value_percent.Sum();
            IsMultiTarget = value_percent.Count(v => v > 0f) > 1;
        }
    }

    public class Action
    {
        public SkillInfo SkillInfo { get; private set; }

        public List<Action> SubActions = new List<Action>();

        //attribute
        public eActionType actionType;
        public int value;
        public float piercing, duration;
        public bool check_distance = true;
        public bool skip_dead = false;
        public bool loop_target = false;
        public bool IsSubAction = false;

        public bool show_message = true;

        public eStatType statType; // for buff/debuff
        public List<eImmuneType> immune_types;
        public List<eAttackType> attack_types;

        public void Fire(BattleSkill skill, eActionType fire_action, int fire_value)
        {
            SubActions.ForEach(a => a.Fire(skill, fire_action, fire_value));

            switch (actionType)
            {
                case eActionType.hp_drain:
                    if (fire_action == eActionType.damage || fire_action == eActionType.dot_damage)
                    {
                        int apply_value = -fire_value * GetValue(skill.Creature.GradePercent, skill.Level) / 10000;
                        skill.Creature.SetHeal(apply_value, fire_action == eActionType.dot_damage);
                        TextManager.Instance.PushHeal(skill.Creature, apply_value, fire_action == eActionType.dot_damage?eTextPushType.Dot : eTextPushType.Normal);
                    }
                    break;
            }
        }

        public int GetValue(float GradePercent, int skill_level)
        {
            int return_value = value + skill_level * increasePerLevel;
            switch (actionType)
            {
                case eActionType.buff:
                case eActionType.debuff:
//                     if (StatInfo.IsPercentValue(statType))
//                         return return_value;
                    return Mathf.RoundToInt(return_value * GradePercent);

                case eActionType.buff_percent:
                case eActionType.debuff_percent:
                case eActionType.stun:
                case eActionType.hidden:
                case eActionType.sleep:
                case eActionType.provoke:
                case eActionType.damage_mana:
                case eActionType.heal_mana:
                case eActionType.dot_damage_mana:
                case eActionType.dot_heal_mana:
                case eActionType.hp_drain:
                    return return_value;

                default:
                    return Mathf.RoundToInt(return_value * GradePercent);
            }
        }

        public int GetValuePerLevel(float GradePercent, int skill_level, int new_skill_level)
        {
            if (new_skill_level == 0)
                return GetValue(GradePercent, skill_level);
            return GetValue(GradePercent, new_skill_level) - GetValue(GradePercent, skill_level);
        }

        public bool CanDistance(float first_action_time, ICreature target)
        {
            if (check_distance == false)
                return true;

            if (target.Character.transform.localPosition == Vector3.zero)
                return true;

            var target_main_action = target.Character.MainAction;
            if (target_main_action != null && target_main_action.IsPlayingAnimation == true)
            {
                if (target_main_action.AnimationLeftTime / BattleConfig.Instance.SlowSpeed <= first_action_time + BattleConfig.Instance.SkillDelay)
                    return true;
            }

            return false;
        }

        //child node
        public int increasePerLevel;
        public ActionValue action_value;

        public string DescPerLevel(float grade_percent, int level, int new_level)
        {
            string DescPreset = (new_level == 0) ? "SkillDesc_" : "SkillDescPerLevel_";
            switch (actionType)
            {
                case eActionType.buff_percent:
                case eActionType.passive_percent:
                    return Localization.Format(DescPreset+"BuffPercent", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level) / 100f);

                case eActionType.buff:
                case eActionType.passive:
                    if (StatInfo.IsPercentValue(statType))
                        return Localization.Format(DescPreset + "BuffPercent", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level) / 100f);
                    return Localization.Format(DescPreset + "Buff", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level));

                case eActionType.debuff_percent:
                    return Localization.Format(DescPreset + "DebuffPercent", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level) / 100f);

                case eActionType.debuff:
                    if (StatInfo.IsPercentValue(statType))
                        return Localization.Format(DescPreset + "DebuffPercent", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level) / 100f);
                    return Localization.Format(DescPreset + "Debuff", Localization.Get("StatType_" + statType), GetValuePerLevel(grade_percent, level, new_level));

                case eActionType.stun:
                    return Localization.Get(DescPreset + "Stun");

                case eActionType.hidden:
                    return Localization.Get(DescPreset + "Hidden");

                case eActionType.sleep:
                    return Localization.Get(DescPreset + "Sleep");

                case eActionType.provoke:
                    return Localization.Get(DescPreset + "Provoke");

                case eActionType.shield:
                    {
                        string desc = "";
                        string str_attack_type = "";

                        if (attack_types.Count == 1)
                            str_attack_type = Localization.Format("AttackType_Desc", Localization.Get("AttackType_" + attack_types[0]));

                        if (value != -1)
                            desc = Localization.Format(DescPreset + "Shield", str_attack_type, Mathf.CeilToInt(GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total));

                        return desc;
                    }

                case eActionType.immune:
                    {
                        string desc = "";
                        string str_attack_type = "";

                        if (attack_types.Count == 1)
                            str_attack_type = Localization.Format("AttackType_Desc", Localization.Get("AttackType_" + attack_types[0]));

                        if (immune_types.Count > 0)
                        {
                            if (string.IsNullOrEmpty(desc) == false)
                                desc += "\n";

                            string str_immunes = "";
                            foreach (eImmuneType immune_type in immune_types)
                            {
                                if (string.IsNullOrEmpty(str_immunes) == false)
                                    str_immunes += ", ";
                                str_immunes += Localization.Get("ImmuneType_" + immune_type);
                            }
                            desc += Localization.Format(DescPreset + "Immune", str_attack_type, str_immunes);
                        }
                        return desc;
                    }

                case eActionType.heal:
                    return Localization.Format(DescPreset + "Heal", Mathf.CeilToInt(GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total));

                case eActionType.heal_mana:
                    return Localization.Format(DescPreset + "HealMana", GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total * 0.01f);

                case eActionType.damage:
                    return Localization.Format(DescPreset + "Damage", Mathf.CeilToInt(GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total));

                case eActionType.damage_mana:
                    return Localization.Format(DescPreset + "DamageMana", GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total * 0.01f);

                case eActionType.dot_damage:
                    return Localization.Format(DescPreset + "DotDamage", Mathf.CeilToInt(GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total));

                case eActionType.dot_damage_mana:
                    return Localization.Format(DescPreset + "DotDamageMana", GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total * 0.01f);

                case eActionType.dot_heal:
                    return Localization.Format(DescPreset + "DotHeal", Mathf.CeilToInt(GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total));

                case eActionType.dot_heal_mana:
                    return Localization.Format(DescPreset + "DotHealMana", GetValuePerLevel(grade_percent, level, new_level) * action_value.value_percent_total * 0.01f);

                case eActionType.hp_drain:
                    if (new_level > 0)
                        return null;
                    return Localization.Format(DescPreset + "HPDrain", GetValuePerLevel(grade_percent, level, new_level) * 0.01f);

                default:
                    return "Unknown";
            }
        }

        public bool IsMultiTarget
        {
            get
            {
                return action_value.IsMultiTarget;
            }
        }

        public bool IsBuff
        {
            get
            {
                switch (actionType)
                {
                    case eActionType.buff:
                    case eActionType.debuff:
                    case eActionType.buff_percent:
                    case eActionType.debuff_percent:
                        return true;
                }
                return false;
            }
        }

        public bool IsDot
        {
            get
            {
                switch (actionType)
                {
                    case eActionType.dot_damage:
                    case eActionType.dot_damage_mana:
                    case eActionType.dot_heal:
                    case eActionType.dot_heal_mana:
                        return true;
                }
                return false;
            }
        }

        public bool IsPassive
        {
            get
            {
                switch (actionType)
                {
                    case eActionType.passive:
                    case eActionType.passive_percent:
                        return true;
                }
                return false;
            }
        }

        public bool IsDirect
        {
            get
            {
                switch (actionType)
                {
                    case eActionType.damage:
                    case eActionType.damage_mana:
                    case eActionType.heal:
                    case eActionType.heal_mana:
                        return true;
                }
                return false;
            }
        }

        public bool IsLevelup
        {
            get
            {
                switch (actionType)
                {
                    case eActionType.stun:
                    case eActionType.hidden:
                    case eActionType.sleep:
                    case eActionType.shield:
                    case eActionType.provoke:
                    case eActionType.immune:

                    case eActionType.dot_damage_mana:
                    case eActionType.damage_mana:
                    case eActionType.dot_heal_mana:
                    case eActionType.heal_mana:
                        return false;
                }
                return true;
            }
        }

        public Action(XmlNode node, SkillInfo info, Action parent)
        {
            this.SkillInfo = info;

            //load attribute
            actionType = (eActionType)Enum.Parse(typeof(eActionType), node.Attributes["type"].Value);
            //Debug.LogFormat("{0}", node.Attributes["value"].Value);
            value = int.Parse(node.Attributes["value"].Value);
            XmlAttribute piercingAttr = node.Attributes["piercing"];
            if (piercingAttr != null) piercing = float.Parse(piercingAttr.Value);

            if (parent != null)
            {
                switch(actionType)
                {
                    case eActionType.shield:
                        throw new System.Exception(string.Format("[{0}] shield action should not be subaction", info.ID));
                }
                IsSubAction = true;
                duration = parent.duration;
                check_distance = parent.check_distance;
                skip_dead = parent.skip_dead;
                loop_target = parent.loop_target;
                action_value = parent.action_value;
            }
            else
            {
                if (IsPassive == false)
                {
                    if (IsDirect == false)
                        duration = float.Parse(node.Attributes["duration"].Value);

                    XmlAttribute checkDistanceAttr = node.Attributes["check_distance"];
                    if (checkDistanceAttr != null)
                        check_distance = bool.Parse(checkDistanceAttr.Value);

                    XmlAttribute skipDeadAttr = node.Attributes["skip_dead"];
                    if (skipDeadAttr != null)
                        skip_dead = bool.Parse(skipDeadAttr.Value);

                    XmlAttribute loopTargetAttr = node.Attributes["loop_target"];
                    if (loopTargetAttr != null)
                        loop_target = bool.Parse(loopTargetAttr.Value);
                }

                action_value = new ActionValue(node);
            }

            switch(actionType)
            {
                case eActionType.shield:
                    {
                        attack_types = new List<eAttackType>();
                        foreach (var attack_type in node.Attributes["attack_type"].Value.Split(','))
                        {
                            if (string.IsNullOrEmpty(attack_type) == false)
                                attack_types.Add((eAttackType)Enum.Parse(typeof(eAttackType), attack_type));
                        }
                    }
                    break;

                case eActionType.immune:
                    {
                        immune_types = new List<eImmuneType>();
                        foreach (var immune_type in node.Attributes["immune_type"].Value.Split(','))
                        {
                            if (string.IsNullOrEmpty(immune_type) == false)
                                immune_types.Add((eImmuneType)Enum.Parse(typeof(eImmuneType), immune_type));
                        }
                        attack_types = new List<eAttackType>();
                        foreach (var attack_type in node.Attributes["attack_type"].Value.Split(','))
                        {
                            if (string.IsNullOrEmpty(attack_type) == false)
                                attack_types.Add((eAttackType)Enum.Parse(typeof(eAttackType), attack_type));
                        }
                    }
                    break;
            }

            if (IsBuff == true || IsPassive)
            {
                statType = (eStatType)Enum.Parse(typeof(eStatType), node.Attributes["stat"].Value);
                if (IsBuff)
                {
                    XmlAttribute show_messageAttr = node.Attributes["show_message"];
                    if (show_messageAttr != null)
                        show_message = bool.Parse(show_messageAttr.Value);
                }
            }

            //load child node
            if (IsLevelup)
            {
                XmlNode increasePerLevelNode = node.SelectSingleNode("IncreasePerLevel");
                if (increasePerLevelNode != null)
                {
                    increasePerLevel = int.Parse(increasePerLevelNode.Attributes["value"].Value);
                }
            }

            var sub_action_nodes = node.SelectNodes("SubAction");
            foreach (XmlNode sub_action_node in sub_action_nodes)
            {
                SubActions.Add(new Action(sub_action_node, info, this));
            }
        }
    }

    override public void Load(XmlNode skillInfoNode)
    {
        base.Load(skillInfoNode);
        Name = skillInfoNode.Attributes["name"].Value;
        Desc = skillInfoNode.Attributes["desc"].Value;
        Type = (eSkillType)Enum.Parse(typeof(eSkillType), skillInfoNode.Attributes["skill_type"].Value);

        XmlAttribute icon_id_attr = skillInfoNode.Attributes["icon_id"];
        if (icon_id_attr != null)
            IconID = icon_id_attr.Value;
        else
            IconID = ID;

        XmlAttribute show_icon_attr = skillInfoNode.Attributes["show_icon"];
        if (show_icon_attr != null)
            ShowIcon = bool.Parse(show_icon_attr.Value);
        else
            ShowIcon = true;

        switch (Type)
        {
            case eSkillType.passive:
            case eSkillType.passive_etc:
                {
                    CanStack = true;
                    XmlAttribute action_name_attr = skillInfoNode.Attributes["action_name"];
                    if (action_name_attr != null)
                        ActionName = action_name_attr.Value;
                }
                break;

            default:
                EnemyType = (eEnemyType)Enum.Parse(typeof(eEnemyType), skillInfoNode.Attributes["enemy_type"].Value);
                TargetType = (eTargetType)Enum.Parse(typeof(eTargetType), skillInfoNode.Attributes["target_type"].Value);
                ActionName = skillInfoNode.Attributes["action_name"].Value;
                XmlAttribute move_target_attr = skillInfoNode.Attributes["move_target"];
                if (move_target_attr != null)
                    MoveTarget = (eMoveTarget)Enum.Parse(typeof(eMoveTarget), move_target_attr.Value);
                break;
        }

        switch (Type)
        {
            case eSkillType.active_plus:
                return;
        }

        Actions = new List<Action>();
        foreach(XmlNode actionNode in skillInfoNode.SelectNodes("Action"))
        {
            Actions.Add(new Action(actionNode, this, null));
        }

    }

    public string GetTooltip()
    {
        return Localization.Format("SkillTooltip", Name, Desc);
    }

    public void AddStats(StatInfo info, StatInfo base_stat, eAttackType attack_type, float grade_percent, short level)
    {
        foreach (var action in Actions)
        {
            switch (action.actionType)
            {
                case eActionType.passive:
                    switch (action.statType)
                    {
                        case eStatType.Defense:
                            info.AddValue(eStatType.PhysicDefense, action.GetValue(grade_percent, level));
                            info.AddValue(eStatType.MagicDefense, action.GetValue(grade_percent, level));
                            break;

                        case eStatType.Attack:
                            switch (attack_type)
                            {
                                case eAttackType.heal:
                                    info.AddValue(eStatType.Heal, action.GetValue(grade_percent, level));
                                    break;

                                case eAttackType.physic:
                                    info.AddValue(eStatType.PhysicAttack, action.GetValue(grade_percent, level));
                                    break;

                                case eAttackType.magic:
                                    info.AddValue(eStatType.MagicAttack, action.GetValue(grade_percent, level));
                                    break;
                            }
                            break;

                        default:
//                             if (StatInfo.IsPercentValue(action.statType))
//                                 info.AddValue(action.statType, action.GetValue(1f, level));
//                             else
                                info.AddValue(action.statType, action.GetValue(grade_percent, level));
                            break;
                    }
                    break;

                case eActionType.passive_percent:
                    switch (action.statType)
                    {
                        case eStatType.Defense:
                            info.AddValue(eStatType.PhysicDefense, base_stat.GetValue(eStatType.PhysicDefense) * action.GetValue(1f, level) / 10000);
                            info.AddValue(eStatType.MagicDefense, base_stat.GetValue(eStatType.MagicDefense) * action.GetValue(1f, level) / 10000);
                            break;

                        case eStatType.Attack:
                            switch (attack_type)
                            {
                                case eAttackType.heal:
                                    info.AddValue(eStatType.Heal, base_stat.GetValue(eStatType.Heal) * action.GetValue(1f, level) / 10000);
                                    break;

                                case eAttackType.physic:
                                    info.AddValue(eStatType.PhysicAttack, base_stat.GetValue(eStatType.PhysicAttack) * action.GetValue(1f, level) / 10000);
                                    break;

                                case eAttackType.magic:
                                    info.AddValue(eStatType.MagicAttack, base_stat.GetValue(eStatType.MagicAttack) * action.GetValue(1f, level) / 10000);
                                    break;
                            }
                            break;

                        default:
                            if (StatInfo.IsPercentValue(action.statType))
                                info.AddValue(action.statType, action.GetValue(1f, level));
                            else
                                info.AddValue(action.statType, base_stat.GetValue(action.statType) * action.GetValue(1f, level) / 10000);
                            break;
                    }
                    break;
            }
        }

    }
}
