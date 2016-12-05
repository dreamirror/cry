using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class SoulStoneInfoManager : InfoManager<SoulStoneInfo, ItemInfoBase, SoulStoneInfoManager>
{
    static public bool IsSoulStone(long idn)
    {
        return 100000 <= idn && idn < 110000;
    }
}

public class SoulStoneInfo : ItemInfoBase
{
    public CreatureInfo Creature { get; private set; }

    public override eItemType ItemType { get { return eItemType.SoulStone; } }
    public short Grade, LootCount;

    override public void Load(XmlNode node)
    {
        base.Load(node);

        InvetoryShow = false;

        string creature_id = node.Attributes["creature_id"].Value;
        Creature = CreatureInfoManager.Instance.GetInfoByID(creature_id);

        Grade = short.Parse(node.Attributes["grade"].Value);
        LootCount = short.Parse(node.Attributes["loot_count"].Value);
    }

    public override string GetTooltip()
    {
        int count = 0;
        Item item = ItemManager.Instance.GetItemByID(ID);
        if (item != null)
            count = item.Count;
        return Localization.Format("SoulStoneTooltip", Name, count, LootCount);
    }
}
