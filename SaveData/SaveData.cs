using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;

public class SaveDataManger : MNS.Singleton<SaveDataManger>
{
    public bool IsInit { get; private set; }

    List<ISaveData> m_DataContainer;

    public SaveDataManger()
    {
        m_DataContainer = new List<ISaveData>(); //下面的这些类全部都是继承了ISaveData 接口 会在遍历的时候将这些类中的数据全部保存起来
        m_DataContainer.Add(ItemManager.Instance);
        m_DataContainer.Add(EquipManager.Instance);
        m_DataContainer.Add(RuneManager.Instance);
        m_DataContainer.Add(CreatureManager.Instance);
        m_DataContainer.Add(MapClearDataManager.Instance);
        m_DataContainer.Add(TeamDataManager.Instance);
        m_DataContainer.Add(MapClearRewardManager.Instance);
        m_DataContainer.Add(CreatureBookManager.Instance);
    }

    public bool InitFromFile()
    {
        try
        {
            Load();
            Save();
            IsInit = true;
        }
        catch(System.Exception ex)
        {
            Debug.LogWarningFormat("SaveData:InitFromFile : {0}", ex);
            IsInit = false;
        }
        return IsInit;
    }

    public void InitFromData(PacketInfo.pd_PlayerDetailData data)
    {
        ItemManager.Instance.InitFromData(data.items);
        EquipManager.Instance.InitFromData(data.equips);
        RuneManager.Instance.InitFromData(data.runes);
        CreatureManager.Instance.InitFromData(data.creatures);
        MapClearDataManager.Instance.InitFromData(data.maps);
        MapClearRewardManager.Instance.InitFromData(data.map_rewards);
        TeamDataManager.Instance.InitFromData(data.teams);
        CreatureBookManager.Instance.InitFromData(data.books);

        Save();

        IsInit = true;
    }

    public void Save()
    {
        m_DataContainer.ForEach(c => c.Save());
    }

    void Load()
    {
        m_DataContainer.ForEach(c => c.Load());
    }

    public void Clear()
    {
        m_DataContainer.ForEach(c => c.Clear());
    }
}

public interface ISaveData // 這是一個什麼鬼接口
{
    void Save();
    void Load();
    void Clear();
}

// abstract public class SaveDataBase<DataT> : ISaveData
// {
//     abstract public string FileName { get; }
// 
//     public object SaveData() { return CreateSaveData(); }
//     public void InitFromSaveData(string data) { Init(JsonConvert.DeserializeObject<DataT>(data)); }
// 
//     abstract protected DataT CreateSaveData();
//     abstract protected void Init(DataT data);
// }

abstract public class SaveDataSingleton<DataT, SingletonT> : MNS.Singleton<SingletonT>, ISaveData where SingletonT : class, new() where DataT : class
{
    virtual public bool IsAdditionalLoad { get { return false; } }

    string Filename { get { return typeof(SingletonT).Name + ".dat"; } } //以dat文件的形式来保存了数据
    virtual public void Save() { SHSavedData.SaveSaveData(Filename, JsonConvert.SerializeObject(CreateSaveData())); }
    public void Load()
    {
        string data = SHSavedData.LoadSaveData(Filename);
        if (string.IsNullOrEmpty(data) == false)
            Init(JsonConvert.DeserializeObject<DataT>(data), null);
        else
            throw new System.Exception(Filename);
    }

    public void InitFromData(DataT data)
    {
        if (IsAdditionalLoad && SHSavedData.FileExists(Filename))
        {
            Init(data, JsonConvert.DeserializeObject<DataT>(SHSavedData.LoadSaveData(Filename)));
        }
        else
            Init(data, null);
    }

    abstract protected DataT CreateSaveData();
    abstract public void Init(DataT data, DataT file_data);

    public void Clear()
    {
        string path = SHSavedData.GetDocumentsFilePath(Filename);
        System.IO.File.Delete(path);
    }
}

