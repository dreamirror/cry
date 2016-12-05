using MNS;
using PacketEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using PacketInfo;

public class KingsScriptSet
{
    public int idn;
    public string kings_preset;
    public List<KingsScript> script_list;
}
public class KingsScript
{
    public eKingsState state;
    public string value;
}
public enum eKingsState
{
    IDLE,
    TAKEABLE,
}

public class KingsGiftInfo : InfoBaseString
{
    public pd_KingsGiftInfo gift_info { get; private set; }
    public int need_hour { get; private set; }
    public int script_set_idn { get; private set; }
    public string condition_value { get; private set; }
    public int chance { get; private set; }

    public MapInfo ConditionMapInfo { get { return MapInfoManager.Instance.GetInfoByID(condition_value); } }

    override public void Load(XmlNode node)
    {
        base.Load(node);

        gift_info = new pd_KingsGiftInfo();
        gift_info.reward_data = new pd_GoodsData((pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), node.Attributes["type"].Value), long.Parse(node.Attributes["value"].Value));
        need_hour = int.Parse(node.Attributes["need_hour"].Value);

        script_set_idn = int.Parse(node.Attributes["script_set_idn"].Value);
        condition_value = node.Attributes["condition_value"].Value;
        chance = int.Parse(node.Attributes["chance"].Value);
    }
}

public class KingsGiftInfoManager : InfoManager<KingsGiftInfo, KingsGiftInfo, KingsGiftInfoManager >
{
    List<KingsScriptSet> script_set = new List<KingsScriptSet>();

    public bool IsKingsGiftActive { get { return Tutorial.Instance.Completed == true && string.IsNullOrEmpty(MapClearDataManager.Instance.GetLastClearedMapID()) == false; } }

    protected override void PostLoadData(XmlNode node)
    {   
        foreach (XmlNode set_node in node.SelectSingleNode("ScriptList").ChildNodes)
        {
            KingsScriptSet set = new KingsScriptSet();
            set.idn = int.Parse(set_node.Attributes["idn"].Value);
            set.kings_preset = set_node.Attributes["kings_preset"].Value;

            set.script_list = new List<KingsScript>();
            foreach (XmlNode script_node in set_node.ChildNodes)
            {
                KingsScript script = new KingsScript();
                script.state = (eKingsState)Enum.Parse(typeof(eKingsState), script_node.Attributes["type"].Value);
                script.value = script_node.Attributes["value"].Value;

                set.script_list.Add(script);
            }
            script_set.Add(set);
        }
    }

    public string GetRandomScript(int kings_gift_idn, bool is_takeable)
    {   
        KingsScriptSet set = script_set.Find(s => s.idn == m_Infos.Find(g => g.IDN == kings_gift_idn).script_set_idn);
        if (set == null)
            return string.Empty;

        if (is_takeable)
            return set.script_list.Where(s => s.state == eKingsState.TAKEABLE).ToList()[MNS.Random.Instance.NextRange(0, set.script_list.Where(s => (s.state == eKingsState.TAKEABLE)).ToList().Count - 1)].value;
        else
            return set.script_list.Where(s => s.state == eKingsState.IDLE).ToList()[MNS.Random.Instance.NextRange(0, set.script_list.Where(s => (s.state == eKingsState.IDLE)).ToList().Count - 1)].value;
    }

    public KingsScriptSet GetScriptSetInfo(int kings_gift_idn)
    {
        KingsScriptSet set = script_set.Find(s => s.idn == m_Infos.Find(g => g.IDN == kings_gift_idn).script_set_idn);
        if (set != null)
            return set;
        else
            return script_set.First();
    }
}