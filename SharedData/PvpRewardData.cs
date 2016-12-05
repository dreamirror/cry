using MNS;
using System;
using System.IO;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class RankingRewardData : CSVData<int>
{
    public int Rank;
    public int Token;
    public int Gem;

    static bool check_index = false;
    static int rank_index = 0, gem_index = 0, token_index = 0;

    void CheckIndex(CSVReader reader)
    {
        if (check_index == true)
            return;

        rank_index = reader.GetFieldIndex("rank");
        gem_index = reader.GetFieldIndex("gem");
        token_index = reader.GetFieldIndex("token");

        check_index = true;
    }

    public int Key { get { return Rank; } }
    public void Load(CSVReader reader, CSVReader.RowData row)
    {
        CheckIndex(reader);

        Rank = int.Parse(row.GetData(rank_index));
        Gem = int.Parse(row.GetData(gem_index));
        Token = int.Parse(row.GetData(token_index));
    }
}

abstract public class RankingRewardDataManager<T> : InfoManagerCSV<T, int, RankingRewardData>
        where T : class, new()
{
    public RankingRewardData GetReward(int rank)
    {
        foreach (var reward in m_List)
        {
            if (rank <= reward.Rank || reward.Rank == -1)
                return reward;
        }
        return null;
    }

    public List<RankingRewardData> GetList()
    {
        return m_List;
    }
}

public class PvpRewardDataManager : RankingRewardDataManager<PvpRewardDataManager>
{
}

public class WorldBossRewardDataManager : RankingRewardDataManager<WorldBossRewardDataManager>
{
}
