using System.Collections.Generic;
using PacketInfo;
using System.Linq;

public class EquipManager : SaveDataSingleton<List<pd_EquipData>, EquipManager>
{
    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////
    override public void Init(List<pd_EquipData> datas, List<pd_EquipData> file_datas)
    {
        Equips = new List<Equip>();
        if (datas == null) return;
        foreach (pd_EquipData data in datas)
        {
            Equip equip = new Equip(data);
            Equips.Add(equip);
        }
    }

    override protected List<pd_EquipData> CreateSaveData()
    {
        return Equips.Select(c => c.CreateSaveData()).ToList();
    }

    ////////////////////////////////////////////////////////////////
    public List<Equip> Equips { get; private set; }

    public void Add(pd_EquipData data)
    {
        Equip equip = GetEquipByIdx(data.equip_idx);
        if (equip == null)
            Equips.Add(new Equip(data));
        else
            equip.Set(data);
        Save();
    }

    public void Remove(long equip_idx)
    {
        Equips.RemoveAll(e => e.EquipIdx == equip_idx);
        Save();
    }

    public void RemoveByCreatureIdx(long creature_idx)
    {
        Equips.RemoveAll(e => e.CreatureIdx == creature_idx);
        Save();
    }

    public void Reset(pd_EquipData data)
    {
        Equip equip = GetEquipByIdx(data.equip_idx);
        equip.Set(data);
        Save();
    }

    public Equip GetEquipByIdx(long idx)
    {
        return Equips.Find(c => c.EquipIdx == idx);
    }

}

public class Equip
{
    public bool IsNotify { get; private set; }

    public EquipInfo Info { get; private set; }
    public EquipEnchantInfo EnchantInfo { get; private set; }

    public long EquipIdx { get; private set; }
    public short EnchantLevel { get { return EnchantInfo==null?(short)0:EnchantInfo.Enchant; } }
    public long CreatureIdx { get; private set; }

    public List<Item> Stuffs { get; private set; }

    public Equip(pd_EquipData data)
    {
        SetInternal(data);
    }
    public Equip(EquipInfo info)
    {//For tutorial dummy
        Info = info;

        EquipIdx = -1;
        CreatureIdx = -1;
        if (Stuffs != null)
            Stuffs.Clear();
        else
            Stuffs = new List<Item>();

        if (Info.Enchants.Count > 0)
        {
            EnchantInfo = Info.Enchants[0];
            foreach (StuffInfo stuff_info in EnchantInfo.Stuffs)
            {
                Stuffs.Add(ItemManager.Instance.GetOrCreateItem(stuff_info));
            }
        }
    }
    public Equip Clone()
    {
        Equip clone = new Equip(Info);
        clone.EnchantInfo = EnchantInfo;
        clone.Stuffs = Stuffs;

        return clone;
    }
    public void Enchant()
    {
        EnchantInfo = Info.Enchants[EnchantLevel+1];
    }
    public void Set(pd_EquipData data)
    {
        SetInternal(data);
    }
    void SetInternal(pd_EquipData data)
    {
        Info = EquipInfoManager.Instance.GetInfoByIdn(data.equip_idn);

        EquipIdx = data.equip_idx;

        CreatureIdx = data.creature_idx;
        if (Stuffs != null)
            Stuffs.Clear();
        else
            Stuffs = new List<Item>();

        if (Info.Enchants.Count > 0)
        {
            EnchantInfo = Info.Enchants[data.equip_enchant];
            foreach (StuffInfo stuff_info in EnchantInfo.Stuffs)
            {
                Stuffs.Add(ItemManager.Instance.GetOrCreateItem(stuff_info));
            }
        }
    }
    public pd_EquipData CreateSaveData()
    {
        pd_EquipData data = new pd_EquipData();
        data.equip_idx = EquipIdx;
        data.creature_idx = CreatureIdx;
        data.equip_idn = Info.IDN;
        data.equip_level = (short)Info.Grade;
        data.equip_enchant = EnchantLevel;
        return data;
    }

    public void AddStats(StatInfo info)
    {
        EquipInfoManager.Instance.AddStats(Info, EnchantLevel, info);
    }

    public void CheckNotify(short creature_level)
    {
        IsNotify = AvailableEnchant() || AvailableUpgrade();
    }

    public bool AvailableEnchant()
    {
        bool available = EnchantLevel < 5 && Stuffs.GroupBy(s => s).All(s => s.Key.Count >= s.Count());
        Stuffs.ForEach(i => i.Notify = available);
        return available;
    }

    public bool AvailableUpgrade()
    {
        return EnchantLevel == 5 && Stuffs.Count > 0 && Stuffs.GroupBy(s => s).All(s => s.Key.Count >= s.Count());
    }

    public string GetTooltip()
    {
        StatInfo stat = new StatInfo();
        EquipInfoManager.Instance.AddStats(Info, EnchantLevel, stat);
        if (EnchantLevel > 0)
            return Localization.Format("EquipTooltipEnchantLevel", Info.Name, EnchantLevel) + stat.Tooltip(Info.CategoryInfo.AttackType);
        return Localization.Format("EquipTooltip", Info.Name) + stat.Tooltip(Info.CategoryInfo.AttackType);
    }

    public string GetName()
    {
        string name = Localization.Format("EquipTitle", Info.Grade, Info.Name);
        if (EnchantLevel > 0)
            name += string.Format(" +{0}", EnchantLevel);
        return name;
    }

    public int EnchantCost { get { return Info.EnchantCost(EnchantLevel); } }
}
