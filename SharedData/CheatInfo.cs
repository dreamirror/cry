using MNS;
using PacketInfo;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;

public class CheatInfoManager : InfoManager<CheatInfo, CheatInfo, CheatInfoManager>
{
    public IEnumerable<string> GetCategories()
    {
        return Values.GroupBy(v => v.Category).Select(g => g.Key);
    }

    public IEnumerable<string> GetCommands(string category)
    {
        return Values.Where(v => v.Category == category).Select(v => v.Command);
    }

    public CheatInfo GetInfo(string category, string command)
    {
        return Values.ToList().Find(v => v.Category == category && v.Command == command);
    }

    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);

        m_Infos.Add(new CheatInfo("Show", "FPS", "FPS 표시 On/Off"));
        m_Infos.Add(new CheatInfo("Show", "Memory", "Memory 표시 On/Off"));
        m_Infos.Add(new CheatInfo("Show", "DeviceInfo", "DeviceInfo 표시 On/Off"));

        List<string> texture_formats = new List<string>();
        foreach (var value in Enum.GetValues(typeof(TextureFormat)))
        {
            TextureFormat format = (TextureFormat)value;
            try
            {
                if (SystemInfo.SupportsTextureFormat(format))
                {
                    texture_formats.Add(format.ToString());
                }
            }
            catch (ArgumentException)
            {

            }
        }

        m_Infos.Add(new CheatInfo("Show", "TextureFormat", "TextureFormat 표시", texture_formats.ToArray()));

        m_Infos.Add(new CheatInfo("Set", "Quality", "Qulity 선택", QualitySettings.names));
    }
}

public class CheatInfo : InfoBaseString
{
    public string Category { get; private set; }
    public string Command { get; private set; }
    public string Description { get; private set; }
    public List<string> Params = new List<string>();
    public bool IsClient { get; private set; }

    public CheatInfo()
    {

    }

    public CheatInfo(string category, string command, string description, params string[] param)
    {
        Category = category;
        Command = command;
        Description = description;
        Params = param.ToList();
        IsClient = true;
    }

    override public void Load(XmlNode node)
    {
        base.Load(node);

        Category = node.Attributes["category"].Value;
        Command = node.Attributes["command"].Value;
        Description = node.Attributes["description"].Value;

        XmlAttribute paramAttr = node.Attributes["param"];
        if (paramAttr != null)
            Params = paramAttr.Value.Split(",".ToArray()).ToList();
    }
}
