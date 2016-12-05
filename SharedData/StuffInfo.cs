using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class StuffInfoManager : InfoManager<StuffInfo, ItemInfoBase, StuffInfoManager>
{
    public List<StuffDropInfo> DropInfos;
    protected override void PreLoadData(XmlNode node)
    {
        base.PreLoadData(node);

        DropInfos = new List<StuffDropInfo>();

        foreach(XmlNode child in node.SelectSingleNode("GetPlace_Preset").ChildNodes)
        {
            DropInfos.Add(new StuffDropInfo(child));
        }
    }
}
public class StuffDropInfo
{
    public string id;
    public List<MoveMenuInfo> menus;

    public StuffDropInfo(XmlNode node)
    {
        id = node.Attributes["get_place_id"].Value;

        menus = new List<MoveMenuInfo>();
        foreach(XmlNode child in node.ChildNodes)
        {
            menus.Add(new MoveMenuInfo(child));
        }
    }
}
public class MoveMenuInfo
{
    public string title;
    public string desc;
    public string icon_id;

    public GameMenu menu;
    public string menu_parm_1;
    public string menu_parm_2;

    public MoveMenuInfo (XmlNode node)
    {
        title = node.Attributes["title"].Value;
        desc = node.Attributes["description"].Value;
        icon_id = node.Attributes["icon_id"].Value;

        menu = (GameMenu)Enum.Parse(typeof(GameMenu), node.Attributes["move_menu"].Value);

        XmlAttribute parm1_attr = node.Attributes["move_value"];
        if (parm1_attr != null)
            menu_parm_1 = parm1_attr.Value;

        XmlAttribute parm2_attr = node.Attributes["move_value2"];
        if (parm2_attr != null)
            menu_parm_2 = parm2_attr.Value;
    }
}

public class StuffInfo : ItemInfoGradeBase
{
    public override eItemType ItemType { get { return eItemType.Stuff; } }

    public string MakeID;
    public int MakeCount;

    public StuffDropInfo DropInfo;

    public int StuffPurchaseValue;

    override public void Load(XmlNode node)
    {
        base.Load(node);

        XmlAttribute makeIDAttr = node.Attributes["make_id"];
        if (makeIDAttr != null)
            MakeID = makeIDAttr.Value;
        else
            MakeID = null;

        XmlAttribute makeCountAttr = node.Attributes["make_count"];
        if (makeCountAttr != null)
            MakeCount = int.Parse(makeCountAttr.Value);
        else
            MakeCount = 0;

        string preset_id = node.Attributes["get_place_id"].Value;
        DropInfo = StuffInfoManager.Instance.DropInfos.Find(e => e.id == preset_id);

        XmlAttribute pricegemAttr = node.Attributes["buy_price_gem"];
        if (pricegemAttr != null)
            StuffPurchaseValue = int.Parse(pricegemAttr.Value);
    }

    public override string GetTooltip()
    {
        int count = 0, piece_count = 0;
        Item item = ItemManager.Instance.GetItemByID(ID);
        if (item != null)
        {
            count = item.Count;
            piece_count = item.PieceCount;
        }
        string res = Localization.Format("ItemTooltipBase", Name, count);
        if (PieceCountMax > 1)
            res += string.Format("\n{0} : {1}/{2}", Localization.Get("Piece"), piece_count, PieceCountMax);
        return res;
    }
}
