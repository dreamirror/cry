using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using PacketEnums;

public class MapClearRewardManager : SaveDataSingleton<List<pd_MapClearReward>, MapClearRewardManager>
{
    public List<pd_MapClearReward> Data { get; private set; }

    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////
    override public void Init(List<pd_MapClearReward> datas, List<pd_MapClearReward> file_datas)
    {
        if (datas == null)
            Data = new List<pd_MapClearReward>();
        else
            Data = datas;
    }

    override protected List<pd_MapClearReward> CreateSaveData()
    {
        return Data;
    }

    ////////////////////////////////////////////////////////////////

    public pd_MapClearReward GetRewardedData(int map_idn, pe_Difficulty difficulty)
    {
        return Data.Find(e => e.map_idn == map_idn && e.difficulty == difficulty);
    }

    public void SetReward(string map_id, int idx, pe_Difficulty difficulty)
    {
        int map_idn = MapInfoManager.Instance.GetInfoByID(map_id).IDN;
        pd_MapClearReward rewarded_info = GetRewardedData(map_idn, difficulty);
        if(rewarded_info == null)
        {
            rewarded_info = new pd_MapClearReward();
            rewarded_info.map_idn = map_idn;
            rewarded_info.difficulty = difficulty;
            Data.Add(rewarded_info);
        }

        rewarded_info.SetAt(idx, true);

        Save();
    }
}
