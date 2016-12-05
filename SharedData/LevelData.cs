using MNS;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : CSVData<short>
{
    public short Level;
    public long ExpMax;

    static bool check_index = false;
    static int player_exp_index = 0, character_exp_index = 0, energy_max_index = 0, energy_bonus_index = 0;

    public int PlayerExpMax, CharacterExpMax;
    public short EnergyMax, EnergyBonus;

    void CheckIndex(CSVReader reader)
    {
        if (check_index == true)
            return;

        player_exp_index = reader.GetFieldIndex("player_exp");
        character_exp_index = reader.GetFieldIndex("character_exp");
        energy_max_index = reader.GetFieldIndex("energy_max");
        energy_bonus_index = reader.GetFieldIndex("energy_bonus");

        check_index = true;
    }

    public short Key { get { return Level; } }
    public void Load(CSVReader reader, CSVReader.RowData row)
    {
        CheckIndex(reader);

        Level = short.Parse(row.GetData(0));
        PlayerExpMax = int.Parse(row.GetData(player_exp_index));
        CharacterExpMax = int.Parse(row.GetData(character_exp_index));
        EnergyMax = short.Parse(row.GetData(energy_max_index));
        EnergyBonus = short.Parse(row.GetData(energy_bonus_index));
    }
}

public class LevelInfoManager : InfoManagerCSV<LevelInfoManager, short, LevelData>
{
    public float GetCharacterPercent(short level, int exp)
    {
        LevelData data;
        if (m_Datas.TryGetValue(level, out data) == false || data.Level != level || data.CharacterExpMax == 0)
            return 0f;

        return (float)((double)exp / data.CharacterExpMax);
    }

    public float GetPlayerPercent(short level, int exp)
    {
        LevelData data;
        if (m_Datas.TryGetValue(level, out data) == false || data.Level != level || data.PlayerExpMax == 0)
            return 0f;

        return (float)((double)exp / data.PlayerExpMax);
    }

    public int GetCharacterExpMax(short level)
    {
        return m_Datas[level].CharacterExpMax;
    }

    public int GetPlayerExpMax(short level)
    {
        return m_Datas[level].PlayerExpMax;
    }

    public short GetEnergyMax(short level)
    {
        return m_Datas[level].EnergyMax;
    }

    public short GetEnergyBonus(short level)
    {
        return m_Datas[level].EnergyBonus;
    }
}
