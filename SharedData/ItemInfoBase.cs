using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public enum eItemType : short
{
    SoulStone,
    Equip,
    Item,
    Stuff,
    Token,
    Rune,
}

abstract public class ItemInfoBase : InfoBaseString
{
    public string IconID { get; private set; }
    public string Name { get; private set; }
    public short PieceCountMax { get; private set; }
    public short ItemCountMax { get; private set; }
    public int SalePrice { get; private set; }
    public bool InvetoryShow { get; protected set; }
    public string Description { get; private set; }
    public string DescriptionSub { get; private set; }

    abstract public eItemType ItemType { get; }

    public bool IsLootItem
    {
        get
        {
            switch (ItemType)
            {
                case eItemType.Item:
                case eItemType.Stuff:
                case eItemType.SoulStone:
                    return true;
            }
            return false;
        }
    }

    override public void Load(XmlNode node)
    {
        base.Load(node);
        Name = node.Attributes["name"].Value;

        XmlAttribute iconIDAttr = node.Attributes["icon_id"];
        if (iconIDAttr != null)
            IconID = iconIDAttr.Value;
        else
            IconID = ID;

        XmlAttribute pieceCountMaxAttr = node.Attributes["piece_count_max"];
        if (pieceCountMaxAttr != null)
            PieceCountMax = short.Parse(pieceCountMaxAttr.Value);
        else
            PieceCountMax = 1;

        XmlAttribute itemCountMaxAttr = node.Attributes["item_count_max"];
        if (itemCountMaxAttr != null)
            ItemCountMax = short.Parse(itemCountMaxAttr.Value);
        else
            ItemCountMax = 999;

        XmlAttribute salePriceAttr = node.Attributes["sale_price"];
        if (salePriceAttr != null)
            SalePrice = int.Parse(salePriceAttr.Value);

        XmlAttribute invetory_show_attr = node.Attributes["invetory_show"];
        if (invetory_show_attr != null)
            InvetoryShow = bool.Parse(invetory_show_attr.Value);
        else
            InvetoryShow = true;

        XmlAttribute descAttr = node.Attributes["desc_1"];
        if (descAttr != null)
            Description = descAttr.Value;

        XmlAttribute desc2Attr = node.Attributes["desc_2"];
        if (desc2Attr != null)
            DescriptionSub = desc2Attr.Value;
    }

    virtual public string GetTooltip()
    {
        return Name;
    }
}

abstract public class ItemInfoGradeBase : ItemInfoBase
{
    public short Grade;

    override public void Load(XmlNode node)
    {
        base.Load(node);

        Grade = short.Parse(node.Attributes["grade"].Value);
    }
}
