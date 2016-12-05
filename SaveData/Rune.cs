using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;
using System;

public enum eRuneSort
{
    Grade,
    Level,
    IDN,
}


public class RuneManager : SaveDataSingleton<List<pd_RuneData>, RuneManager>
{
    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////

    override public void Init(List<pd_RuneData> datas, List<pd_RuneData> file_datas)
    {
        if (datas == null)
            Runes = new List<Rune>();
        else
            Runes = datas.Select(r => new Rune(r)).ToList();
    }

    override protected List<pd_RuneData> CreateSaveData()
    {
        return Runes.Select(c => c.CreateSaveData()).ToList();
    }

    ////////////////////////////////////////////////////////////////
    public List<Rune> Runes { get; private set; }

    public void Add(pd_RuneData data)
    {
        Runes.Add(new Rune(data));
        Save();
    }

    public Rune GetRuneByID(string id)
    {
        return Runes.Find(c => c.Info.ID == id);
    }

    public Rune GetRuneByIdn(int idn)
    {
        return Runes.Find(c => c.Info.IDN == idn);
    }

    public Rune GetRuneByIdx(long idx)
    {
        return Runes.Find(c => c.RuneIdx == idx);
    }

    public List<Rune> GetRunesByCreatureIdx(long creature_idx)
    {
        return Runes.Where(r => r.CreatureIdx == creature_idx).OrderBy(r => r.EquippedAt).ToList();
    }

    public void RemoveRune(Rune rune)
    {
        rune.OnRemoved();
        Runes.Remove(rune);
        Save();
    }
    public void RemoveRune(long idx)
    {
        Runes.RemoveAll(r => r.CreatureIdx == idx);
        Save();
    }
    public void EquipRune(long rune_idx, long creature_idx)
    {
        var rune = GetRuneByIdx(rune_idx);
        if (rune != null)
        {
            rune.SetCreature(creature_idx);
            Save();
            CreatureManager.Instance.Save();      
        }
    }
    public void UnEquipRune(long rune_idx)
    {
        var rune = GetRuneByIdx(rune_idx);
        if (rune != null)
        {
            rune.SetCreature(0);
            Save();
            CreatureManager.Instance.Save();
        }

    }
    public void EnchantRune(Rune rune)
    {
        var find_rune = GetRuneByIdx(rune.RuneIdx);
        if (rune != find_rune)
        {
            Debug.LogException(new System.Exception("can't find rune."));
            return;
        }
        rune.OnLevelUp();
        Save();
    }
    public List<Rune> GetSortedList(eRuneSort sort, bool is_ascending = false, List<Rune> runes = null)
    {
        if (runes == null)
            runes = Runes.Where(r => r.CreatureIdx == 0).ToList();

        IOrderedEnumerable<Rune> list = null;

        if (is_ascending == true)
        {
            switch (sort)
            {
                case eRuneSort.Grade: list = runes.OrderBy(c => c.Info.Grade); break;
                case eRuneSort.Level: list = runes.OrderBy(c => c.Level); break;
                case eRuneSort.IDN: list = runes.OrderBy(c => c.Info.IconID); break;
            }
        }
        else
        {
            switch (sort)
            {
                case eRuneSort.Grade: list = runes.OrderByDescending(c => c.Info.Grade); break;
                case eRuneSort.Level: list = runes.OrderByDescending(c => c.Level); break;
                case eRuneSort.IDN: list = runes.OrderByDescending(c => c.Info.IDN); break;
            }
        }
        if (list == null)
            return runes;

        return list.ThenByDescending(c => c.Info.Grade).ThenByDescending(c => c.Level).ThenByDescending(c => c.Info.IconID).ThenByDescending(c => c.RuneIdx).ToList();
    }
}

public class Rune
{
    public RuneInfo Info { get; private set; }

    public short Level { get; private set; }
    public short StatLevel { get { return (short)(Level + (Level / 5) * 2 - 1); } }

    public long RuneIdx { get; private set; }
    public long CreatureIdx { get; private set; }
    public DateTime EquippedAt { get; private set; }
    public Rune() { }
    public Rune(pd_RuneData data)
    {
        this.Info = RuneInfoManager.Instance.GetInfoByIdn(data.rune_idn) as RuneInfo;
        Level = data.rune_level;
        RuneIdx = data.rune_idx;
        CreatureIdx = data.creature_idx;
        EquippedAt = data.equipped_at;
    }
    public Rune Clone()
    {
        Rune rune = new Rune();
        rune.Info = Info;
        rune.Level = Level;
        rune.RuneIdx = RuneIdx;
        rune.CreatureIdx = CreatureIdx;
        rune.EquippedAt = EquippedAt;

        return rune;
    }
    public pd_RuneData CreateSaveData()
    {
        pd_RuneData data = new pd_RuneData();
        data.rune_idx = RuneIdx;
        data.creature_idx = CreatureIdx;
        data.rune_level = Level;
        data.rune_idn = Info.IDN;
        data.equipped_at = EquippedAt;
        return data;
    }

    public string GetTooltip()
    {
        return Localization.Format("RuneTooltip", GetName(), "[99ff99]" + Info.Skill.DescTotal(1f, StatLevel));
    }

    public string GetDesc()
    {
        return Info.Skill.DescTotal(1f, StatLevel);
    }

    public int GetValue()
    {
        return Info.Skill.GetValue(0, StatLevel);
    }
    public string GetName()
    {
        RuneInfo rune_info = Info as RuneInfo;
        return Localization.Format("RuneTitle", Info.Name, Level, rune_info.GradeInfo.MaxLevel);
    }

    public void SetCreature(long creature_idx)
    {
        if (creature_idx == 0)
        {
            OnRemoved();
        }
        else
        {
            var creature = CreatureManager.Instance.GetInfoByIdx(creature_idx);
            creature.AddRune(this);
        }
        this.CreatureIdx = creature_idx;
    }

    public void OnRemoved()
    {
        if (this.CreatureIdx == 0)
            return;

        var creature = CreatureManager.Instance.GetInfoByIdx(this.CreatureIdx);
        creature.RemoveRune(this);
    }

    public void OnLevelUp()
    {
        Info = RuneInfoManager.Instance.GetInfoByIdn(this.Info.IDN) as RuneInfo;
        Level = (short)(Level + 1) ;
    }

    public float GetEnchantPercent(bool is_premium)
    {
        return RuneTableManager.Instance.GetEnchantPercent(Level, is_premium);
    }

    public int GetEnchantCostValue(bool is_premium)
    {
        return RuneTableManager.Instance.GetCostValueEnchant(Info.Grade, Level, is_premium);
    }

}
