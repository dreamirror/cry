using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class CreatureBookInfo
{
    public CreatureInfo Creature { get; private set; }
    public short Grade { get; private set; }

    public CreatureBookInfo(XmlNode node, short grade)
    {
        Creature = CreatureInfoManager.Instance.GetInfoByID(node.Attributes["id"].Value);
        XmlAttribute grade_attr = node.Attributes["grade"];
        if (grade_attr != null)
            grade = short.Parse(grade_attr.Value);

        Grade = grade;
    }
}

public class CreatureBookGroupInfo
{
    public List<CreatureBookInfo> Books { get; private set; }
    public string Name { get; private set; }

    public CreatureBookGroupInfo(XmlNode node, string root_name, short grade)
    {
        Books = new List<CreatureBookInfo>();
        Name = node.Attributes["name"].Value;

        XmlAttribute grade_attr = node.Attributes["grade"];
        if (grade_attr != null)
            grade = short.Parse(grade_attr.Value);

        foreach (XmlNode child_node in node.ChildNodes)
        {
            Books.Add(new CreatureBookInfo(child_node, grade));
        }
    }
}

public class CreatureBookList : InfoBaseString
{
    public List<CreatureBookGroupInfo> Groups { get; private set; }
    public string Name { get; private set; }
    override public void Load(XmlNode node)
    {
        base.Load(node);

        Groups = new List<CreatureBookGroupInfo>();
        Name = node.Attributes["name"].Value;

        short grade = -1;
        XmlAttribute grade_attr = node.Attributes["grade"];
        if (grade_attr != null)
            grade = short.Parse(grade_attr.Value);

        foreach (XmlNode BookNode in node.ChildNodes)
        {
            Groups.Add(new CreatureBookGroupInfo(BookNode, ID, grade));
        }
    }
}

public class CreatureBookInfoManager : InfoManager<CreatureBookList, CreatureBookList, CreatureBookInfoManager>
{
    Dictionary<int, string> ListIDs;
    public string GetListIDByCreatureIdn(int creature_idn)
    {
        string list_id = null;
        ListIDs.TryGetValue(creature_idn, out list_id);
        return list_id;
    }

    protected override void PostLoadData(XmlNode node)
    {
        ListIDs = new Dictionary<int, string>();
        m_Infos.ForEach(l => l.Groups.ForEach(g => g.Books.ForEach(b => ListIDs.Add(b.Creature.IDN, l.ID))));
    }
}
