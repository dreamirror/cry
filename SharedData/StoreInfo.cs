using MNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PacketInfo;

public class StoreInfoManager : InfoManager<StoreInfo, StoreInfo, StoreInfoManager>
{
}

public class StoreItemBase
{
    public string StoreID;
}
public class StoreLootItem : StoreItemBase
{
    public string ID;
    public string Image;
    public string LootType;
    public string Name;
    public int LootCount;
    public pd_GoodsData Price;
    public short refresh_count;
    public short refresh_free;
    public string DescTop="";
    public string DescBottom="";

    public StoreLootItem() { }
    public StoreLootItem(XmlNode node, string store_id) { Load(node);  StoreID = store_id; }

    public void Load(XmlNode node)
    {
        ID = node.Attributes["id"].Value;
        Image = node.Attributes["image"].Value;
        LootType = node.Attributes["loot_type"].Value;
        Name = node.Attributes["name"].Value;
        LootCount = int.Parse(node.Attributes["loot_count"].Value);

        Price = new pd_GoodsData();
        Price.goods_type = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), node.Attributes["price_type"].Value);
        Price.goods_value = int.Parse(node.Attributes["price"].Value);

        refresh_free = short.Parse(node.Attributes["refresh_free"].Value);
        refresh_count = short.Parse(node.Attributes["refresh_count"].Value);

        XmlAttribute desc_top_attr = node.Attributes["desc_top"];
        if (desc_top_attr != null)
            DescTop = desc_top_attr.Value;

        XmlAttribute desc_bottom_attr = node.Attributes["desc_bottom"];
        if (desc_bottom_attr != null)
            DescBottom = desc_bottom_attr.Value;


    }

    //public string Name { get { return Localization.Format(string.Format("{0}Name", LootType), LootCount); } }
    public string IconID { get { return Price.goods_type.ToString(); } }
}

public class StoreGoodsItem : StoreItemBase
{
    public string ID;
    public string Image;
    public bool Event;
    public pd_GoodsData Target;
    public pd_GoodsData Price;
    public int refresh_count;
    public int refresh_free;
    public int mileage;
    public int bonus;
    public short limit;
    public ItemInfo NeedItem { get; private set; }

    public StoreGoodsItem() { }
    public StoreGoodsItem(XmlNode node, string store_id) { Load(node); StoreID = store_id; }

    public void Load(XmlNode node)
    {
        ID = node.Attributes["id"].Value;
        Image = node.Attributes["image"].Value;
        Event = bool.Parse(node.Attributes["event"].Value);
        Target = new pd_GoodsData();
        Target.goods_type = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), node.Attributes["goods_type"].Value);
        Target.goods_value = int.Parse(node.Attributes["goods_value"].Value);
        Price = new pd_GoodsData();
        Price.goods_type = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), node.Attributes["price_type"].Value);
        Price.goods_value = int.Parse(node.Attributes["price"].Value);

        bonus = int.Parse(node.Attributes["bonus"].Value);
        limit = short.Parse(node.Attributes["limit"].Value);
        string item_id = node.Attributes["need_ticket"].Value;
        if (string.IsNullOrEmpty(item_id) == false)
            NeedItem = ItemInfoManager.Instance.GetInfoByID(item_id) as ItemInfo;
        else
            NeedItem = null;
        mileage = int.Parse(node.Attributes["mileage"].Value);
        refresh_free = int.Parse(node.Attributes["refresh_free"].Value);
        refresh_count = int.Parse(node.Attributes["refresh_count"].Value);

    }

    public string Name { get { return string.Format("{0} {1}", Localization.Format("GoodsFormat", Target.goods_value), Localization.Get(Target.goods_type.ToString())); } }
    public string TagetIconID { get { return Target.goods_type.ToString(); } }
    public string PriceIconID { get { return Price.goods_type.ToString(); } }

}

public class StoreInfo : InfoBaseString
{
    public string IconID;
    public pe_StoreType StoreType;
    public bool Enable;

    public List<StoreGoodsItem> m_GoodsItem = null;
    public List<StoreLootItem> m_LootItem = null;

    public List<short> m_RefreshTimes = null;
    public pd_GoodsData RefreshPrice = null;
    public string DescID;

    override public void Load(XmlNode node)
    {
        base.Load(node);

        IconID = node.Attributes["icon_id"].Value;
        StoreType = (pe_StoreType)Enum.Parse(typeof(pe_StoreType), node.Attributes["store_type"].Value);
        Enable = bool.Parse(node.Attributes["enable"].Value);
        DescID = node.Attributes["desc_id"].Value;
        
        switch (StoreType)
        {
            case pe_StoreType.Loot:
                m_LootItem = new List<StoreLootItem>();
                foreach (XmlNode itemNode in node.SelectNodes("Item"))
                {
                    m_LootItem.Add(new StoreLootItem(itemNode, ID));
                }
                break;
            case pe_StoreType.Goods:
                m_GoodsItem = new List<StoreGoodsItem>();
                foreach(XmlNode itemNode in node.SelectNodes("Item"))
                {
                    m_GoodsItem.Add(new StoreGoodsItem(itemNode, ID));
                }
                break;
            default:
                break;
        }

        //refresh time item
        XmlNode refresh_node = node.SelectSingleNode("Refresh");
        if (refresh_node != null)
        {
            m_RefreshTimes = new List<short>();
            foreach (XmlNode time_node in refresh_node.ChildNodes)
            {
                m_RefreshTimes.Add(short.Parse(time_node.Attributes["hour"].Value));
            }
            RefreshPrice = new pd_GoodsData();
            RefreshPrice.goods_type = (pe_GoodsType)Enum.Parse(typeof(pe_GoodsType), refresh_node.Attributes["price_type"].Value);
            RefreshPrice.goods_value = int.Parse(refresh_node.Attributes["price_value"].Value);
        }

    }

    public DateTime GetNextRefreshDate()
    {
        if (m_RefreshTimes == null || m_RefreshTimes.Count == 0) return DateTime.MaxValue;
        DateTime now = Network.Instance.ServerTime;

        for (int i = 0; i < m_RefreshTimes.Count; ++i)
        {
            if (now.Hour < m_RefreshTimes[i])
            {
                return Network.Instance.ServerTime.Date.AddHours(m_RefreshTimes[i]);
            }
        }

        return Network.Instance.ServerTime.Date.AddHours(24 + m_RefreshTimes[0]);
    }
}
