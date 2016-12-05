using MNS;
using PacketInfo;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GuildConfig
{
    List<short> member_limit = new List<short>();
    List<int> guild_require_exp = new List<int>();
    List<short> guild_skill1_level = new List<short>();
    List<short> guild_skill2_level = new List<short>();
    public int GuildCountPerPage;
    public int ChangeGuildEmblemCost;
    public int CreateGuildCost;
    public short GuildRequestCount;
    public short RequestCount;
    public int GuildAttendPoint;
    public int GuildEmblemCount;
    public string GuildEmblemPrefix;
    public short AtLeastPlayerLevel;
    public long GiveGoldMax;
    int GivePointRatio;
    double BuffRatioPerLevel = 0.5;

    public GuildConfig(XmlNode node)
    {
        GuildCountPerPage = int.Parse(node.Attributes["guild_count_per_page"].Value);
        ChangeGuildEmblemCost = int.Parse(node.Attributes["change_guild_emblem_cost"].Value);
        CreateGuildCost = int.Parse(node.Attributes["create_guild_cost"].Value);
        GuildRequestCount = short.Parse(node.Attributes["guild_request_count"].Value);
        RequestCount = short.Parse(node.Attributes["request_count"].Value);
        GuildAttendPoint = int.Parse(node.Attributes["guild_attend_point"].Value);
        GivePointRatio = int.Parse(node.Attributes["give_point_ratio"].Value);
        GuildEmblemCount = int.Parse(node.Attributes["guild_emblem_count"].Value);
        GuildEmblemPrefix = node.Attributes["guild_emblem_prefix"].Value;
        AtLeastPlayerLevel = short.Parse(node.Attributes["atleast_player_level"].Value);
        GiveGoldMax = long.Parse(node.Attributes["max_give"].Value);
        
        member_limit.Add(0);
        guild_skill1_level.Add(0);
        guild_skill2_level.Add(0);
        foreach (XmlNode item_node in node.SelectSingleNode("LevelConfig").ChildNodes)
        {
            member_limit.Add(short.Parse(item_node.Attributes["member_limit"].Value));
            guild_require_exp.Add(int.Parse(item_node.Attributes["require_exp"].Value));
            guild_skill1_level.Add(short.Parse(item_node.Attributes["guild_skill_level_1"].Value));
            guild_skill2_level.Add(short.Parse(item_node.Attributes["guild_skill_level_2"].Value));
        }
        GuildLevelMax = guild_require_exp.Count;
        guild_require_exp.Add(0);
    }
    public short GetLimitMemberCount(short level)
    {
        return member_limit[level];
    }

    public int GuildLevelMax { get; private set; }
    public int RequiredExp(short level)
    {
        if (GuildLevelMax <= level) return 0;
        return guild_require_exp[level];
    }
    public int GetExpPercent(short level, int exp)
    {
        int required_exp = RequiredExp(level);
        if (required_exp == 0) return 0;
        return Mathf.FloorToInt((float)exp / required_exp * 100);
    }

    public int GetGivePoint(int gold)
    {
        return gold / GivePointRatio;
    }

    public string GuildBuffString(int index, int level)
    {
        if (index == 1 && guild_skill1_level[level] > 0)
            return Localization.Format("GuildBuff1", guild_skill1_level[level] * BuffRatioPerLevel);
        if (index == 2 && guild_skill2_level[level] > 0)
            return Localization.Format("GuildBuff2", guild_skill2_level[level] * BuffRatioPerLevel);

        return "";
    }

}

public class GuildInfo : InfoBaseString
{
    override public void Load(XmlNode node)
    {
        base.Load(node);
    }
}


public class GuildInfoManager : InfoManager<GuildInfo, GuildInfo, GuildInfoManager>
{
    static public GuildConfig Config;

    protected override void PreLoadData(XmlNode node)
    {
        base.PreLoadData(node);
        Config = new GuildConfig(node.SelectSingleNode("GuildConfig"));
    }
}

public class GuildManager : Singleton<GuildManager>
{
    public List<long> RequestList = new List<long>();
    public pd_GuildInfoDetail GuildInfo { get; private set; }
    public pd_GuildMemberInfo State { get; private set; }
    public List<pd_GuildMemberInfoDetail> GuildMembers { get; private set; }
    
    public bool IsGuildJoined { get { return GuildInfo != null && GuildInfo.info != null; } }
    public string GuildChannelName { get { return GuildInfo.info == null ? "" : string.Format("GuildChannel{0}", GuildInfo.info.guild_idx); } }
    public long GuildIdx { get { return GuildInfo.info == null ? 0 : GuildInfo.info.guild_idx; } }
    public string GuildMaster { get { return GuildInfo.info == null ? string.Empty : GuildInfo.guild_master; } }
    public bool IsAttendance { get { return State != null && State.attend_daily_index == Network.DailyIndex; } }
    public bool AvailableGuildManagement { get { return State != null && State.member_state <= 1; } }
    public short GuildMemberCount { get { return GuildInfo.info == null ? (short)0 : GuildInfo.info.member_count; } }
    public short GuildMemberMax { get { return GuildInfo.info == null ? (short)0 : GuildInfo.info.guild_limit_member; } }
    public void SetGuildInfo(pd_GuildInfo info)
    {
        if(GuildInfo == null)
             GuildInfo = new pd_GuildInfoDetail();
        if (info == null) return;
        GuildInfo.info = info;
    }

    public void SetGuildMembers(List<pd_GuildMemberInfoDetail> members)
    {
        GuildMembers = members;
        if (GuildMembers != null)
        {
            GuildInfo.guild_master = GuildMembers.Find(m => m.member_state == 0).nickname;
            State = GuildMembers.Find(e => e.account_idx == SHSavedData.AccountIdx);
        }
        else
        {
            State = null;
            GuildInfo.guild_master = string.Empty;
        }
    }
    public void RemoveMember(long account_idx)
    {
        if (GuildMembers == null) return;
        GuildMembers.RemoveAll(e => e.account_idx == account_idx);
        GuildInfo.info.member_count--;
        GuildInfo.guild_master = GuildMembers.Find(m => m.member_state == 0).nickname;
    }

    public void UpdateGuildMembers(long account_idx, int exp, bool isAttend = false)
    {
        if (GuildMembers == null) return;
        var member = GuildMembers.Find(e => e.account_idx == account_idx);
        if (member == null) return;
        member.give += exp;
        if (isAttend)
            member.attend_daily_index = Network.DailyIndex;
    }

    public void Attend()
    {
        State.attend_daily_index = Network.DailyIndex;
    }

    public void GiveGold(long account_idx, int gold)
    {
        if (GuildMembers == null) return;
        var member = GuildMembers.Find(e => e.account_idx == account_idx);
        if (member == null) return;
        member.give += GuildInfoManager.Config.GetGivePoint(gold);
    }

    public void LeaveGuild()
    {
        Network.ChatServer.LeaveGuildChannel();
        GuildInfo.info = null;
        if (State == null) return;
        State.member_state = 3;
        State.created_at = Network.Instance.ServerTime;
    }

    public void SetGuildMaster(string nickname)
    {
        GuildInfo.guild_master = nickname;
    }

    public void AddGuildMember(pd_GuildMemberInfoDetail member)
    {
        GuildInfo.info.member_count++;
        GuildMembers.Add(member);
    }
}