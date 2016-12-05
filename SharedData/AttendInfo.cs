using MNS;
using PacketEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using PacketInfo;


public class AttendInfo : InfoBaseString
{
    public byte max_day { get; private set; }
    public DateTime start_at { get; private set; }
    public DateTime end_at { get; private set; }
    public string description { get; private set; }

    public List<RewardBase> rewards;

    public override void Load(XmlNode node)
    {
        base.Load(node);

        max_day = byte.Parse(node.Attributes["max_day"].Value);
        start_at = DateTime.Parse(node.Attributes["start_at"].Value);
        end_at = DateTime.Parse(node.Attributes["end_at"].Value);
        description = node.Attributes["desc"].Value;

        rewards = new List<RewardBase>();
        foreach (XmlNode child_node in node.SelectNodes("Reward"))
        {
            rewards.Add(new RewardBase(child_node));
        }
    }
}

public class AttendInfoManager : InfoManager<AttendInfo, AttendInfo, AttendInfoManager>
{
    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);
    }
}