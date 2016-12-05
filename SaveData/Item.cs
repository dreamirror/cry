using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;

public class ItemEvent
{
    public Item item;
    public short add_count;
}

public class ItemManager : SaveDataSingleton<List<pd_ItemData>, ItemManager>
{
    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////

    override public void Init(List<pd_ItemData> datas, List<pd_ItemData> file_datas)
    {
        Items = new List<Item>();
        ItemMadeList = new List<EventParamItemMade>();
        if (datas == null) return;
        foreach (pd_ItemData data in datas)
        {
            Item item = new Item(data);
            if (item.IsItem)
                Items.Insert(0, item);
            else
                Items.Add(item);
        }
    }

    override protected List<pd_ItemData> CreateSaveData()
    {
        return Items.Select(c => c.CreateSaveData()).ToList();
    }

    public bool IsPieceNotify { get { return Items.Where(i => i.IsSoulStone && i.Count > 0).Any(s => s.Count >= s.SoulStoneInfo.LootCount); } }

    ////////////////////////////////////////////////////////////////
    public List<Item> Items { get; private set; }
    public List<EventParamItemMade> ItemMadeList { get; private set; } 

    public List<Item> AvailableItems
    {
        get
        {
            return Items.Where(i => i.Info.InvetoryShow==true && i.Count > 0 || i.PieceCount > 0).ToList();
        }
    }

    public List<Item> NotInventoryItems
    {
        get
        {
            return Items.Where(i => i.Info.InvetoryShow == false).ToList();
        }
    }

    public void AddEvent(EventParamItemMade made)
    {
        ItemMadeList.Add(made);
    }

    public void Add(pd_ItemData data)
    {
        Items.Add(new Item(data));
    }

    public void Add(Item item)
    {
        Items.Add(item);
        Save();
    }

    public void Add(pd_ItemLootInfo data)
    {
        Item item = Items.Find(i => i.Info.IDN == data.item_idn);
        if (item == null)
        {
            var item_info = ItemInfoManager.Instance.GetInfoByIdn(data.item_idn);
            if (item_info.IsLootItem == false)
            {
                Debug.LogErrorFormat("{0} is not loot item", item_info.ID);
                return;
            }
            item = new Item(item_info);
            if (item.IsItem)
                Items.Insert(0, item);
            else
            {
                Items.Add(item);
                CreatureManager.Instance.SetUpdateNotify();
            }
        }
        item.AddPiece(data.add_piece_count);
        Save();
    }

    public void Add(pd_StoreItem data)
    {
        Item item = Items.Find(i => i.Info.IDN == data.item_idn);
        if (item == null)
        {
            item = new Item(ItemInfoManager.Instance.GetInfoByIdn(data.item_idn));
            if (item.IsItem)
                Items.Insert(0, item);
            else
            {
                Items.Add(item);
                CreatureManager.Instance.SetUpdateNotify();
            }
        }
        item.AddPiece(data.item_count);
        Save();
    }

    public Item GetOrCreateItem(StuffInfo info)
    {
        Item item = GetItemByID(info.ID);
        if (item == null)
        {
            item = new Item(info);
            Items.Add(item);
        }
        Save();
        return item;
    }

    public Item GetItemByID(string id)
    {
        return Items.Find(c => c.Info.ID == id);
    }

    public Item GetItemByIdn(int idn)
    {
        return Items.Find(c => c.Info.IDN == idn);
    }

    public void Reset(List<pd_ItemData> data)
    {
        for(int i=0; i<data.Count; ++i)
        {
            Reset(data[i], false);
        }
        Save();
        //CreatureManager.Instance.CheckNotify();
    }

    public void Reset(pd_ItemData data, bool is_save = true)
    {
        Item item = GetItemByIdn(data.item_idn);
        if(item != null)
        {
            item.Set(data);
        }
        if (is_save) Save();
    }
}

public class Item
{
    public ItemInfoBase Info { get; private set; }
    public short Count { get; private set; }
    public short PieceCount { get; private set; }

    public bool IsItem { get { return (Info as ItemInfo) != null; } }
    public bool IsStuff { get { return (Info as StuffInfo) != null; } }
    public bool IsSoulStone { get { return (Info as SoulStoneInfo) != null; } }

    public StuffInfo StuffInfo { get { return (Info as StuffInfo); } }
    public SoulStoneInfo SoulStoneInfo { get { return (Info as SoulStoneInfo); } }

    public bool Notify { get; set; }

    public Item(pd_ItemData data)
    {
        this.Info = ItemInfoManager.Instance.GetInfoByIdn(data.item_idn);

        Count = data.item_count;
        PieceCount = data.item_piece_count;

        Notify = false;
    }

    public Item(ItemInfoBase info)
    {
        Info = info;
        Notify = false;
    }

    public void Set(pd_ItemData data)
    {
        Count = data.item_count;
        PieceCount = data.item_piece_count;

        if (Count == 0) Notify = false;
    }

    public pd_ItemData CreateSaveData()
    {
        pd_ItemData data = new pd_ItemData();
        data.item_idn = Info.IDN;
        data.item_count = Count;
        data.item_piece_count = PieceCount;
        return data;
    }

    public void UseItem(short use_count)
    {
        if (use_count > Count)
            throw new System.Exception("Item Use Invalid");
        Count -= use_count;

        if (Count == 0) Notify = false;
        if (IsStuff == true)
            CreatureManager.Instance.SetUpdateNotify();
    }

    public void AddCount(short count)
    {
        Count += count;

        if (IsStuff == true)
            CreatureManager.Instance.SetUpdateNotify();
    }

    public void AddPiece(short piece_count)
    {
        PieceCount += piece_count;
        if (PieceCount >= Info.PieceCountMax)
        {
            if (Info.PieceCountMax > 1)
                ItemManager.Instance.AddEvent(new EventParamItemMade(this, (short)(PieceCount / Info.PieceCountMax)));
            AddCount((short)(PieceCount / Info.PieceCountMax));
            PieceCount = (short)(PieceCount % Info.PieceCountMax);
        }
    }

    public List<long> GetUseFor()
    {
        return EquipManager.Instance.Equips.FindAll(e => e.EnchantInfo.Stuffs.Exists(s => s.IDN == Info.IDN)).Select(c => c.CreatureIdx).Distinct().ToList();
    }
}
