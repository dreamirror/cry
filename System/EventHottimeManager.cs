using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using PacketInfo;
using MNS;
using System.Xml;

public class EventHottimeData
{
    public TimeSpan start;
    public TimeSpan end;
}

public class EventHottimeManager : MNS.Singleton<EventHottimeManager>
{
    public List<pd_EventHottime> Events { get; private set; }

    public bool IsHeroEchantEvent { get { return GetInfoByID("hero_enchant_discount") != null; } }
    public bool IsHeroMixEvent { get { return GetInfoByID("hero_mix_discount") != null; } }
    public bool IsHeroEvolveEvent { get { return GetInfoByID("hero_evolve_discount") != null; } }
    public bool IsSkillEvent { get { return GetInfoByID("hero_skill_enchant_discount") != null; } }

    public bool IsRuneEvent { get { return Events.Exists(e => e.OnGoing && Network.Instance.ServerTime < e.end_date && e.event_id.Contains("rune_")); } }
    public bool IsRuneUpgradeEvent { get { return GetInfoByID("rune_upgrade_discount") != null; } }
    public bool IsRuneUnequipEvent { get { return GetInfoByID("rune_unequip_discount") != null;  } }
    public bool IsRuneEnchantEvent { get { return Events.Exists(e => e.OnGoing && Network.Instance.ServerTime < e.end_date && e.event_id.Contains("rune_enchant_")); } }
    public bool IsRuneEnchantPremiumEvent { get { return GetInfoByID("rune_enchant_premium_discount") != null;} }
    public bool IsRuneEnchantNormalEvent { get { return GetInfoByID("rune_enchant_discount") != null; } }
    public static List<EventHottimeData> ParseHottimes(string hottime)
    {
        if (string.IsNullOrEmpty(hottime) == true)
            return null;

        List<EventHottimeData> data = new List<EventHottimeData>();
        var hottimes = hottime.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var hot in hottimes)
        {
            var times = hot.Split("~-".ToCharArray());
            data.Add(new EventHottimeData { start = TimeSpan.Parse(times[0]), end = TimeSpan.Parse(times[1]) });
        }
        return data;
    }

    public pd_EventHottime GetInfoByID(string key, bool is_force = false)
    {
        DateTime date = DateTime.Now;
        if (Events == null)
            return null;

        pd_EventHottime evnt = Events.Find(e => e.event_id == key);
        if (evnt != null && (is_force == false && (evnt.OnGoing == false || evnt.end_date <= Network.Instance.ServerTime)))
                return null;  

        return evnt;
    }

    public void Init(List<pd_EventHottime> events)
    {
        Events = events;
    }

    public void Update(List<pd_EventHottime> events, List<long> event_idx)
    {
        Events = Events.Where(e => event_idx.Contains(e.idx) == true && events.Exists(ne => ne.idx == e.idx) == false).ToList();
        Events.AddRange(events);
    }

    public List<pd_EventHottime> GetShowEvents()
    {
        return Events.Where(e => e.show_state).ToList();
    }
}

public class HottimeEventInfo : InfoBaseString
{
    public string IconID;
    public override void Load(XmlNode node)
    {
        base.Load(node);
        var icon_attr = node.Attributes["icon_id"];
        if (icon_attr != null)
            IconID = icon_attr.Value;
        else
            IconID = string.Empty;
    }
}
public class HottimeEventInfoManager : InfoManager<HottimeEventInfo, HottimeEventInfo, HottimeEventInfoManager>
{

}