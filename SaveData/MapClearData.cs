using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using PacketEnums;

public class MapClearDataManager : SaveDataSingleton<List<pd_MapClearData>, MapClearDataManager>
{
    public List<pd_MapClearData> Data { get; private set; }

    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////
    override public void Init(List<pd_MapClearData> datas, List<pd_MapClearData> file_datas)
    {
        if (datas == null)
            Data = new List<pd_MapClearData>();
        else
            Data = datas;
    }

    override protected List<pd_MapClearData> CreateSaveData()
    {
        return Data;
    }

    ////////////////////////////////////////////////////////////////
    public void Set(pd_MapClearData map_clear_data)
    {
        pd_MapClearData clearData = GetData(map_clear_data.map_idn, map_clear_data.stage_index, map_clear_data.difficulty);
        clearData.clear_rate = map_clear_data.clear_rate;
    }

    public bool IsNewClear(MapStageDifficulty stage_info)
    {
        pd_MapClearData clear_data = GetData(stage_info);
        if (clear_data == null) return true;
        return clear_data.clear_count == 0;
    }

    public void SetNew(MapStageDifficulty stage_info)
    {
        pd_MapClearData clearData = CreateMapClearData(stage_info.MapInfo.IDN, stage_info.StageIndex, stage_info.Difficulty);
        clearData.updated_at = Network.Instance.ServerTime;
        Save();
    }

    public void SetTry(MapStageDifficulty stage_info)
    {
        SetTry(stage_info.MapInfo.IDN, stage_info.StageIndex, stage_info.Difficulty);
    }

    void SetTry(int map_idn, short stage_index, pe_Difficulty difficulty)
    {
        pd_MapClearData clearData = GetData(map_idn, stage_index, difficulty);
        if (clearData == null)
        {
            clearData = CreateMapClearData(map_idn, stage_index, difficulty);
        }

        clearData.updated_at = Network.Instance.ServerTime;
        clearData.try_count += 1;

        Save();
    }

    public bool SetClearRate(MapStageDifficulty stage_info, short clear_rate)
    {
        return SetClearRate(stage_info.MapInfo.IDN, stage_info.StageIndex, clear_rate, stage_info.Difficulty);
    }

    bool SetClearRate(int map_idn, short stage_index, short clear_rate, pe_Difficulty difficulty)
    {
        bool new_clear = false;
        pd_MapClearData clearData = GetData(map_idn, stage_index, difficulty);
        if (clearData.clear_rate == 0)
            new_clear = true;

        clearData.clear_rate =  System.Math.Max(clearData.clear_rate, clear_rate);
        clearData.updated_at = Network.Instance.ServerTime;
        if (clearData.daily_index != Network.DailyIndex)
        {
            clearData.daily_index = Network.DailyIndex;
            clearData.daily_clear_count = 0;
        }
        clearData.daily_clear_count++;
        clearData.clear_count++;

        Save();
        return new_clear;
    }

    private pd_MapClearData CreateMapClearData(int map_idn, short stage_index, pe_Difficulty difficulty)
    {
        pd_MapClearData clearData = new pd_MapClearData();
        clearData.map_idn = map_idn;
        clearData.stage_index = stage_index;
        clearData.difficulty = difficulty;
        Data.Add(clearData);
        return clearData;
    }

    pd_MapClearData GetData(int map_idn, int stage_index, pe_Difficulty difficulty)
    {
        return Data.Find(d => d.map_idn == map_idn && d.stage_index == stage_index && d.difficulty == difficulty);
    }

    public pd_MapClearData GetData(MapStageInfo stage_info, pe_Difficulty difficulty = pe_Difficulty.Normal)
    {
        return GetData(stage_info.MapInfo.IDN, stage_info.StageIndex, difficulty);
    }

    public pd_MapClearData GetData(MapStageDifficulty stage_info)
    {
        return GetData(stage_info.MapInfo.IDN, stage_info.StageIndex, stage_info.Difficulty);
    }

    public bool AvailableMap(string map_id, pe_Difficulty difficulty = pe_Difficulty.Normal)
    {
        MapInfo map_info = MapInfoManager.Instance.GetInfoByID(map_id);
        if (map_info != null)
        {
            MapCondition condition = map_info.CheckCondition(difficulty);
            if (condition == null) return true;
        }

        return false;
    }

    public bool AvailableStage(MapStageDifficulty info)
    {
        if (info.MapInfo.CheckCondition(info.Difficulty) != null) return false;
        int map_idn = info.MapInfo.IDN;
        int stage_index = info.StageIndex;
        pd_MapClearData clear_data = GetData(info);
        if (clear_data != null) return true;
        if (map_idn == 1 && stage_index == 0)
        {
            return true;
        }

        if (stage_index == 0)
        {
            if (MapInfoManager.Instance.ContainsIdn(map_idn - 1))
            {
                MapInfo map_info = MapInfoManager.Instance.GetInfoByIdn(map_idn - 1);
                clear_data = GetData(map_info.Stages.Last(), info.Difficulty);
                if (clear_data != null && clear_data.clear_count > 0)
                    return true;
            }
        }
        if (stage_index != 0)
        {
            clear_data = GetData(map_idn, stage_index - 1, info.Difficulty);
            if (clear_data != null && clear_data.clear_count > 0)
                return true;
        }

        return false;
    }

    public int GetMapDailyClearCount(int map_idn, pe_Difficulty difficulty)
    {
        return Data.Where(c => c.map_idn == map_idn && c.difficulty == difficulty).Sum(c => c.GetDailyClearCount());
    }

    public int GetMapDailyClearCount(int map_idn)
    {
        return Data.Where(c => c.map_idn == map_idn).Sum(c => c.GetDailyClearCount());
    }

    public int GetTotalClearRate(int map_idn, pe_Difficulty difficulty)
    {
        return Data.Where(c => c.map_idn == map_idn && c.difficulty == difficulty).Sum(e => e.clear_rate);
    }

    public string GetLastClearedMapID()
    {
        var map_count = MapInfoManager.Instance.GetMainMapCount();
        for (int i= map_count; i> 0; --i)
        {
            var map_info = MapInfoManager.Instance.GetInfoByIdn(i);
            var stage_info = map_info.Stages.Last();
            var clear_data = MapClearDataManager.Instance.GetData(stage_info);
            if (clear_data != null && clear_data.clear_rate > 0)
                return map_info.ID;
        }

        return "";
    }

    public pd_MapClearData GetLastStage(int map_idn, pe_Difficulty difficulty)
    {
        var datas = Data.Where(c => c.map_idn == map_idn && c.difficulty == difficulty).ToList();
        if (datas.Count() == 0)
            return null;
        return datas.OrderByDescending(c => c.updated_at).First();
    }

    public pd_MapClearData GetLastDifficulty(int map_idn, int stage_index)
    {
        var datas = Data.Where(c => c.map_idn == map_idn && c.stage_index == stage_index).ToList();
        if (datas.Count() == 0)
            return null;
        return datas.OrderByDescending(c => c.updated_at).First();
    }

    public pd_MapClearData GetLastMainStage()
    {
        int open_idn = GameConfig.Get<int>("contents_open_main_map");
        var datas = Data.Where(c => c.map_idn <= open_idn).ToList();
        if (datas.Count() == 0)
            return null;
        return datas.OrderByDescending(c => c.updated_at).First();
    }

    public pd_MapClearData GetLastMainStage(pe_Difficulty difficulty)
    {
        int open_idn = GameConfig.Get<int>("contents_open_main_map");
        var datas = Data.Where(c => c.map_idn <= open_idn && c.difficulty == difficulty).ToList();
        if (datas.Count() == 0)
            return null;
        return datas.OrderByDescending(c => c.updated_at).First();
    }

}
