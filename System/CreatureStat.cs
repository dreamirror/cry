using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CreatureStat
{
    public StatInfo Base;

    public StatInfo Stat { get; private set; }

    public int HP, MP;
    public int Level;
    public int MaxHP { get; set; }
    public int MaxMP { get { return BattleConfig.Instance.MPMax; } }
    public float HPPercent { get { return HP / (float)MaxHP; } }
    public float MPPercent { get { return MP / (float)MaxMP; } }

    public bool IsMPFull { get { return MP == MaxMP; } }

    public int DealHP { get { return MaxHP - HP + MaxHP * DieCount; } }
    public short DieCount { get; set; }

    public CreatureStat(StatInfo base_stat)
    {
        this.Base = base_stat;

        CalculateStat(null);
        HP = MaxHP = Stat.MaxHP;
        MP = Stat.ManaInit;
    }

    public void AddMP(int add_mp)
    {
        MP = Math.Min(MaxMP, MP + Mathf.RoundToInt(add_mp * (1f + Stat.ManaGainPercent / 10000f)));
    }

    public void CalculateStat(List<Buff> buffs)
    {
        Stat = new StatInfo();
        foreach (eStatType type in Enum.GetValues(typeof(eStatType)))
        {
            if ((int)type >= 100)
                continue;

            int value = Base.GetValue(type);
            Stat.SetValue(type, value);
        }

        if (buffs != null)
        {
            foreach (Buff buff in buffs)
            {
                if (buff.IsFinish == true)
                    continue;

                switch (buff.ActionInfo.actionType)
                {
                    case eActionType.buff:
                    case eActionType.debuff:
                        {
                            int buff_value = buff.Value;
                            switch (buff.ActionInfo.statType)
                            {
                                case eStatType.Attack:
                                    {
                                        Stat.AddValue(eStatType.PhysicAttack, buff_value);
                                        Stat.AddValue(eStatType.MagicAttack, buff_value);
                                        Stat.AddValue(eStatType.Heal, buff_value);
                                    }
                                    break;

                                case eStatType.Defense:
                                    {
                                        Stat.AddValue(eStatType.PhysicDefense, buff_value);
                                        Stat.AddValue(eStatType.MagicDefense, buff_value);
                                    }
                                    break;

                                default:
                                    Stat.AddValue(buff.ActionInfo.statType, buff_value);
                                    break;
                            }
                        }
                        break;

                    case eActionType.buff_percent:
                    case eActionType.debuff_percent:
                        {
                            long buff_value = buff.ActionInfo.actionType == eActionType.buff_percent ? buff.Value : -buff.Value;
                            switch (buff.ActionInfo.statType)
                            {
                                case eStatType.Attack:
                                    {
                                        Stat.AddValue(eStatType.PhysicAttack, (int)(Base.GetValue(eStatType.PhysicAttack) * buff_value / 10000));
                                        Stat.AddValue(eStatType.MagicAttack, (int)(Base.GetValue(eStatType.MagicAttack) * buff_value / 10000));
                                        Stat.AddValue(eStatType.Heal, (int)(Base.GetValue(eStatType.Heal) * buff_value / 10000));
                                    }
                                    break;

                                case eStatType.Defense:
                                    {
                                        Stat.AddValue(eStatType.PhysicDefense, (int)(Base.GetValue(eStatType.PhysicAttack) * buff_value / 10000));
                                        Stat.AddValue(eStatType.MagicDefense, (int)(Base.GetValue(eStatType.MagicDefense) * buff_value / 10000));
                                    }
                                    break;

                                default:
//                                     if (StatInfo.IsPercentValue(buff.ActionInfo.statType) == true)
//                                         Stat.AddValue(buff.ActionInfo.statType, buff_value);
//                                     else
                                        Stat.AddValue(buff.ActionInfo.statType, (int)(Base.GetValue(buff.ActionInfo.statType) * buff_value / 10000));
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }

    public int GetValue(eStatType type)
    {
        return Stat.GetValue(type);
    }
}
