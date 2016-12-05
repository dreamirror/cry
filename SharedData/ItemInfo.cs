using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class ItemInfoManager : InfoManager<ItemInfo, ItemInfoBase, ItemInfoManager>
{
    public override bool ContainsKey(string key)
    {
        if (key.StartsWith("stuff_") == true)
            return StuffInfoManager.Instance.ContainsKey(key);

        if (key.StartsWith("soulstone_") == true)
            return SoulStoneInfoManager.Instance.ContainsKey(key);

        if (key.StartsWith("token_") == true)
            return TokenInfoManager.Instance.ContainsKey(key);

        if (key.StartsWith("rune_") == true)
            return RuneInfoManager.Instance.ContainsKey(key);

        return base.ContainsKey(key);
    }

    override public ItemInfoBase GetInfoByID(string key)
    {
        if (key.StartsWith("stuff_") == true)
            return StuffInfoManager.Instance.GetInfoByID(key);

        if (key.StartsWith("soulstone_") == true)
            return SoulStoneInfoManager.Instance.GetInfoByID(key);

        if (key.StartsWith("token_") == true)
            return TokenInfoManager.Instance.GetInfoByID(key);

        if (key.StartsWith("rune_") == true)
            return RuneInfoManager.Instance.GetInfoByID(key);

        return base.GetInfoByID(key);
    }

    override public ItemInfoBase GetInfoByIdn(long idn)
    {
        if (20000 <= idn && idn < 30000)
            return base.GetInfoByIdn(idn);

        if (30000 <= idn && idn < 40000)
            return StuffInfoManager.Instance.GetInfoByIdn(idn);

        if (40000 <= idn && idn < 50000)
            return TokenInfoManager.Instance.GetInfoByIdn(idn);

        if (50000 <= idn && idn < 60000)
            return RuneInfoManager.Instance.GetInfoByIdn(idn);

        if (100000 <= idn && idn < 110000)
            return SoulStoneInfoManager.Instance.GetInfoByIdn(idn);

        return null;
    }

    public bool ContainsIdn(int idn)
    {
        if (idn >= 20000 && idn < 30000)
            return base.ContainsIdn(idn);

        if (idn >= 30000 && idn < 40000)
            return StuffInfoManager.Instance.ContainsIdn(idn);

        if (idn >= 40000 && idn < 50000)
            return TokenInfoManager.Instance.ContainsIdn(idn);

        if (idn >= 50000 && idn < 60000)
            return RuneInfoManager.Instance.ContainsIdn(idn);

        if (100000 <= idn && idn < 110000)
            return SoulStoneInfoManager.Instance.ContainsIdn(idn);

        return false;
    }
}

public class ItemInfo : ItemInfoBase
{
    public override eItemType ItemType { get { return eItemType.Item; } }

    public int Value { get; private set; }
    override public void Load(XmlNode node)
    {
        base.Load(node);

        Value = int.Parse(node.Attributes["value"].Value);
    }

    public override string GetTooltip()
    {
        int count = 0;
        Item item = ItemManager.Instance.GetItemByID(ID);
        if (item != null)
            count = item.Count;
        return Localization.Format("ItemTooltipBase", Name, count);
    }
}
