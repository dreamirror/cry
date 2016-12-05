using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;
using System;
using PacketEnums;

public class CreatureBookNotifyData
{
    public bool is_notify;
    public pd_CreatureBook data; //Creature 的数据
    public CreatureBookNotifyData(pd_CreatureBook data, bool is_notify = false) //构造函数
    {
        this.is_notify = is_notify;
        this.data = data;
    }

    public pd_CreatureBook BookSaveData()
    {
        return data;
    }
}

public class CreatureBookManager : SaveDataSingleton<List<pd_CreatureBook>, CreatureBookManager> 
{
    public List<CreatureBookNotifyData> BookInfo { get; private set; }

    private bool IsChanged { get; set; } 
    private bool IsLoaded { get; set; }

    public bool IsNotify { get { return BookInfo.Any(b => b.is_notify == true); }  }
    
    public CreatureBookManager()
    {
        BookInfo = new List<CreatureBookNotifyData>();
    }

    protected override List<pd_CreatureBook> CreateSaveData()
    {
        return BookInfo.Select( b => b.BookSaveData()).ToList();
    }

    public override void Init(List<pd_CreatureBook> data, List<pd_CreatureBook> file_data)
    {
        BookInfo = CreateNotifyData(data);
        IsLoaded = true;
    }

    public void CreatureInfoChanged(int creature_idn)
    {
        int index = BookInfo.FindIndex(c => c.data.creature_idn == creature_idn);

        if (index < 0)
            BookInfo.Add(new CreatureBookNotifyData(new pd_CreatureBook { creature_idn = creature_idn, take_count = 1 }, true));
        else
            BookInfo[index].data.take_count++;
        IsChanged = true;

        Save();
    }

    public bool NeedRefresh()
    {
        return IsLoaded == false || IsChanged == true;
    }

    public List<pd_CreatureBook> GetNotifyData()
    {
        List<pd_CreatureBook> data = new List<pd_CreatureBook>();
        BookInfo.Where(b => b.is_notify == true).ToList().ForEach(b => data.Add(b.data));
        return data;
    }

    public bool IsExistBook(int creature_idn)
    {
        int index = BookInfo.FindIndex(b => b.data.creature_idn == creature_idn);
        return index >= 0;
    }

    public bool IsNotifyBook(int creature_idn)
    {
        int index = BookInfo.FindIndex(b => b.data.creature_idn == creature_idn);
        if (index >= 0 && BookInfo[index].is_notify == true)
        {
            BookInfo[index].is_notify = false;
            return true;
        }
        return false;
    }

    public List<CreatureBookNotifyData> CreateNotifyData(List<pd_CreatureBook> data)
    {
        List<CreatureBookNotifyData> notify_info = new List<CreatureBookNotifyData>();
        data.ForEach(i => notify_info.Add(new CreatureBookNotifyData(i)));
        return notify_info;
    }
}
