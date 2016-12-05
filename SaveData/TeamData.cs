using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using PacketEnums;
using System;

public class TeamCreature
{
    public TeamCreature(Creature creature, short auto_skill_index)
    {
        this.creature = creature;
        this.auto_skill_index = auto_skill_index;
    }

    public TeamCreature(pd_TeamCreature team_creature)
    {
        creature = CreatureManager.Instance.GetInfoByIdx(team_creature.team_creature_idx);
        auto_skill_index = team_creature.auto_skill_index;
    }

    public Creature creature;
    public short auto_skill_index;
}

public class TeamData
{
    public pe_Team TeamType { get; private set; }
    public bool IsAuto { get; private set; }
    public bool IsFast { get; private set; }
    public pe_UseLeaderSkillType UseLeaderSkillType { get; private set; }
    Creature m_LeaderCreature = null;
    public Creature LeaderCreature { get { return m_LeaderCreature != null && m_Creatures2.Exists(c=>c.creature.Idx == m_LeaderCreature.Idx) ? m_LeaderCreature : null ; } }
    List<TeamCreature> m_Creatures = null;
    List<TeamCreature> m_Creatures2 = null;
    public List<TeamCreature> Creatures { get { return m_Creatures2; } }
    public bool NoDuplicate { get; private set; }
    public bool IsNotify
    {
        get
        {
            return TeamDataManager.IsAdventureTeam(TeamType) ? false : Creatures.Any(c => c.creature.IsNotify);
        }
    }

    public long LeaderCreatureIdx { get { return LeaderCreature == null ? 0 : LeaderCreature.Idx; } }

    public int Power { get { return Creatures == null ? 0 : Creatures.Sum(e => e.creature.Power); } }

    public TeamData Clone()
    {
        return new TeamData(TeamType, this);
    }

    public List<TeamCreature> CloneCreatures(List<TeamCreature> creatures)
    {
        return creatures.Select(c => new TeamCreature(c.creature, c.auto_skill_index)).ToList();
    }

    public TeamData(pd_TeamData data)
    {
        TeamType = data.team_type;
        IsAuto = data.is_auto;
        IsFast = data.is_fast;
        UseLeaderSkillType = data.use_leader_skill_type;
        m_Creatures = data.creature_infos.Select(c => new TeamCreature(c)).Where(c => c.creature != null).ToList();
        m_Creatures2 = m_Creatures;
        if (data.leader_creature_idx != 0)
        {
            var leader_creature = m_Creatures.Find(e => e.creature.Idx == data.leader_creature_idx);
            if (leader_creature != null)
                m_LeaderCreature = leader_creature.creature;
        }
        NoDuplicate = data.no_duplicate;
    }

    public TeamData(pe_Team team_type, TeamData data)
    {
        TeamType = team_type;
        if (data != null)
        {
            IsAuto = data.IsAuto;
            IsFast = data.IsFast;
            UseLeaderSkillType = data.UseLeaderSkillType;
            SetCreatures(CloneCreatures(data.Creatures), false);
            m_LeaderCreature = data.LeaderCreature;
            NoDuplicate = data.NoDuplicate;
        }
        else
        {
            UseLeaderSkillType = pe_UseLeaderSkillType.TeamDanger;
            IsAuto = GameConfig.Get<bool>("default_auto_battle");
            IsFast = false;
            m_Creatures = new List<TeamCreature>();
            NoDuplicate = false;
        }
        m_Creatures2 = m_Creatures;
    }

    public void UpdateCreature()
    {
        m_Creatures2 = TeamDataManager.IsAdventureTeam(TeamType) ? m_Creatures : m_Creatures.Where(c => TeamDataManager.Instance.CheckAdventureTeam(c.creature.Idx) == false).ToList();
    }
    public pd_TeamData CreateSaveData()
    {
        pd_TeamData data = new pd_TeamData();
        data.team_type = TeamType;
        data.is_auto = IsAuto;
        data.is_fast = IsFast;
        data.use_leader_skill_type = UseLeaderSkillType;
        data.leader_creature_idx = m_LeaderCreature == null ? 0 : m_LeaderCreature.Idx;
        data.creature_infos = m_Creatures.Select(c => new pd_TeamCreature { team_creature_idx = c.creature.Idx, auto_skill_index = c.auto_skill_index }).ToList();
        data.no_duplicate = NoDuplicate;

        return data;
    }

    public void SetCreatures(List<TeamCreature> creatures, bool save = true)
    {
        m_Creatures = creatures;
        m_Creatures2 = m_Creatures;
        if (m_LeaderCreature != null && m_Creatures.Exists(c => c.creature == m_LeaderCreature) == false)
            m_LeaderCreature = null;

        if (save)
            TeamDataManager.Instance.Save();
    }

    public bool Contains(long creature_idx)
    {
        return Creatures.Exists(c => c.creature.Idx == creature_idx);
    }

    public bool ContainsIDN(int creature_idn)
    {
        return Creatures.Exists(c => c.creature.Info.IDN == creature_idn);
    }

    public void SetLeaderCreature(Creature creature, pe_UseLeaderSkillType use_leader_skill_type, bool save = true)
    {
        if (LeaderCreature == creature && UseLeaderSkillType == use_leader_skill_type)
            return;

        m_LeaderCreature = creature;
        this.UseLeaderSkillType = use_leader_skill_type;

        if (save == true)
        {
            TeamDataManager.Instance.Save();
        }
    }

    public bool IsEqual(TeamData team_data)
    {
        if (team_data == null)
            return false;

        if (IsAuto != team_data.IsAuto || IsFast != team_data.IsFast || UseLeaderSkillType != team_data.UseLeaderSkillType)
            return false;

        if (Creatures.Count != team_data.Creatures.Count)
            return false;

        if (LeaderCreatureIdx != team_data.LeaderCreatureIdx)
            return false;

        for (int i=0; i<Creatures.Count; ++i)
        {
            if( Creatures[i].creature.Idx != team_data.Creatures[i].creature.Idx || Creatures[i].auto_skill_index != team_data.Creatures[i].auto_skill_index)
            {
                return false;
            }
        }
        return true;
    }

    public void SetAuto(bool is_auto, bool save = false)
    {
        if (IsAuto == is_auto)
            return;

        IsAuto = is_auto;
        if(save)
            TeamDataManager.Instance.Save();
    }

    public void SetFast(bool is_fast, bool save = false)
    {
        if (IsFast == is_fast)
            return;

        IsFast = is_fast;
        if(save)
            TeamDataManager.Instance.Save();
    }
    public void SetCompleteAdventure()
    {
        NoDuplicate = false;
        TeamDataManager.Instance.UpdateAdventure();
        TeamDataManager.Instance.Save();
    }
    public void Set(TeamData team_data)
    {
        IsAuto = team_data.IsAuto;
        IsFast = team_data.IsFast;
        m_LeaderCreature = team_data.LeaderCreature;
        UseLeaderSkillType = team_data.UseLeaderSkillType;
        m_Creatures = CloneCreatures(team_data.Creatures);
        NoDuplicate = team_data.NoDuplicate;
        UpdateCreature();

        if(TeamDataManager.IsAdventureTeam(TeamType))
        {
            Creatures.ForEach(c => TeamDataManager.Instance.RemoveAdventureCreature(c));
        }
        TeamDataManager.Instance.Save();
    }

    public bool RemoveCreature(long creature_idx)
    {
        int index = Creatures.FindIndex(c => c.creature.Idx == creature_idx);
        if (index != -1)
        {
            if (m_LeaderCreature != null && creature_idx == m_LeaderCreature.Idx)
                m_LeaderCreature = null;
            Creatures.RemoveAt(index);
            return true;
        }
        return false;
    }
}

public class TeamDataManager : SaveDataSingleton<List<pd_TeamData>, TeamDataManager>
{
    override public bool IsAdditionalLoad { get { return true; } }

    public List<TeamData> Teams { get; private set; }

    List<TeamData> AdventureTeams { get; set; }

    // SaveDataSingleton implementation
    ////////////////////////////////////////////////////////////////
    override public void Init(List<pd_TeamData> datas, List<pd_TeamData> file_data)
    {
        if (datas == null)
            Teams = new List<TeamData>();
        else
            Teams = datas.Select(d => new TeamData(d)).ToList();

        if (Teams.Exists(c => IsAdventureTeam(c.TeamType)))
            AdventureTeams = Teams.Where(c => IsAdventureTeam(c.TeamType)).ToList();
        else
            AdventureTeams = new List<TeamData>();
    }

    override protected List<pd_TeamData> CreateSaveData()
    {
        return Teams.Select(t => t.CreateSaveData()).ToList();
    }

    ////////////////////////////////////////////////////////////////

    public TeamData GetTeam(pe_Team idn)
    {
        return Teams.Find(d => d.TeamType == idn);
    }

    public bool Contains(pe_Team idn)
    {
        return Teams.Find(d => d.TeamType == idn) != null;
    }

    int Comparer(TeamData d1, TeamData d2)
    {
        return d1.TeamType.CompareTo(d2.TeamType);
    }

    public void AddTeam(TeamData data, bool save)
    {
        if (Teams.Exists(t => t.TeamType == data.TeamType))
        {
            Debug.LogErrorFormat("Already exists team : {0}", data.TeamType);
            return;
        }
        Teams.Add(data);
        Teams.Sort(Comparer);
        if (IsAdventureTeam(data.TeamType))
        {
            foreach(var creature in data.Creatures)
            {
                RemoveAdventureCreature(creature);
            }
            AdventureTeams.Add(data);
        }
        if (save)
            Save();
        else
            SetUpdateNotify();
    }

    public void RemoveAdventureCreature(TeamCreature creature)
    {
        AdventureTeams.ForEach(c => 
        {
            if (c.NoDuplicate == false) c.Creatures.RemoveAll(e => e.creature.Idx == creature.creature.Idx);
        });
    }

    public string GetTeamString(Creature creature)
    {
        if (creature == null) return "";

        if (CheckAdventureTeam(creature.Idx))
            return Localization.Get("IN_Adventure");


        if (CheckTeam(creature.Idx, pe_Team.PVP_Defense))
            return Localization.Get("IN_PVP");

        pe_Team team_type = CheckTeam(creature.Idx);
        if (team_type != pe_Team.Invalid)
            return Localization.Get("IN_TEAM");
        
        if (creature.IsLock)
            return Localization.Get("Lock");

        return "";
    }
    static public bool IsAdventureTeam(pe_Team team)
    {
        return (int)team >= 20000 && (int)team < 30000;
    }
    public pe_Team CheckTeam(long creature_idx)
    {
        foreach (var team in Teams)
        {
            if (IsAdventureTeam(team.TeamType) && team.NoDuplicate == false)
                continue;

            if (team.Creatures.Any(c => c.creature.Idx == creature_idx) == true)
                return team.TeamType;
        }
        return pe_Team.Invalid;
    }

    public bool CheckTeam(long creature_idx, pe_Team team_type)
    {
        foreach (var team in Teams)
        {
            if (team.TeamType != team_type)
                continue;

            if (IsAdventureTeam(team.TeamType) && team.NoDuplicate == false)
                continue;

            if (team.Creatures.Any(c => c.creature.Idx == creature_idx) == true)
                return true;
        }
        return false;
    }

    public bool CheckAdventureTeam(long creature_idx)
    {
        foreach (var team in AdventureTeams)
        {
            if (IsAdventureTeam(team.TeamType) == false)
                continue;

            if (team.NoDuplicate == true && team.Creatures.Any(c => c.creature.Idx == creature_idx) == true)
                return true;
        }

        return false;
    }

    public List<Creature> GetCreaturesInAdventure()
    {
        List<Creature> res = new List<Creature>();
        foreach(var team in AdventureTeams)
        {
            if (team.NoDuplicate == false) continue;
            team.Creatures.ForEach(c => res.Add(c.creature));
        }
        return res;
    }

    public void RemoveCreature(long creature_idx)
    {
        bool removed = false;
        foreach (var team in Teams)
        {
            removed = team.RemoveCreature(creature_idx) || removed;
        }
        if (removed)
            Save();
    }

    public bool IsNotify { get; private set; }
    public bool IsUpdateNotify { get; private set; }

    public void SetUpdateNotify()
    {
        IsUpdateNotify = true;
    }

    public void UpdateAdventure()
    {
        Teams.ForEach(t => t.UpdateCreature());
    }

    public void UpdateNotify()
    {
        if (IsUpdateNotify == false || Teams == null)
            return;

        IsUpdateNotify = false;

        IsNotify = Teams.Any(t => t.IsNotify);
    }

    public override void Save()
    {
        base.Save();
        SetUpdateNotify();
    }
}