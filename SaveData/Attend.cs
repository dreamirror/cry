using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;
using System;

public class AttendManager : MNS.Singleton<AttendManager>
{
    public bool IsInit { get; private set; }
    public int LastRequestDailyIndex { get; private set; }

    public void Init(List<pd_AttendInfo> datas)
    {
        IsInit = true;
        Attends = datas.Select(a => new Attend(a)).ToList();
        LastRequestDailyIndex = Network.DailyIndex;
    }

    ////////////////////////////////////////////////////////////////
    public List<Attend> Attends { get; private set; }

    public Attend GetAttendByID(string id)
    {
        return Attends.Find(c => c.Info.ID == id);
    }

    public Attend GetAttendByIdn(int idn)
    {
        return Attends.Find(c => c.Info.IDN == idn);
    }

    public bool IsNewReward
    {
        get
        {
            return Attends.Any(a => a.IsNewReward);
        }
    }

    public bool isAvailableReward
    {
        get
        {
            return Attends.Any(a => a.IsAvailableReward);
        }
    }

    public int OngoingAttendCount
    {
        get
        {
            return Attends.Count( a => a.Info.start_at < Network.Instance.ServerTime && a.Info.end_at > Network.Instance.ServerTime);
        }
    }
    
}

public class Attend
{
    public AttendInfo Info { get; private set; }
    public pd_AttendInfo Data { get; private set; }

    public Attend(pd_AttendInfo data)
    {
        Data = data;
        this.Info = AttendInfoManager.Instance.GetInfoByIdn(data.attend_idn) as AttendInfo;
    }

    public string GetTooltip()
    {
        return Info.description;
    }

    public void SetReward(short take_count)
    {
        Data.take_count = take_count;
        Data.last_daily_index = Network.DailyIndex;
    }

    public bool IsNewReward
    {
        get
        {
            return Data.take_count < Data.take_count_max;
        }
    }

    public bool IsAvailableReward
    { 
     get
        {
            return Data.take_count < Info.max_day && (Network.Instance.ServerTime - Info.start_at).TotalDays > Data.take_count;
        }
    }
}
