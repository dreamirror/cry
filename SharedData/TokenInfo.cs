using MNS;
using System.Xml;
using System;
using System.Linq;
using PacketInfo;

public class TokenInfoManager : InfoManager<TokenInfo, ItemInfoBase, TokenInfoManager>
{
    public TokenInfo GetInfoByType(pe_GoodsType type)
    {
        return Values.Single(v => ((TokenInfo)v).TokenType == type) as TokenInfo;
    }
}

public class TokenInfo : ItemInfoBase
{
    public override eItemType ItemType { get { return eItemType.Token; } }
    public PacketInfo.pe_GoodsType TokenType { get; private set; }

    override public void Load(XmlNode node)
    {
        base.Load(node);
        TokenType = (PacketInfo.pe_GoodsType)Enum.Parse(typeof(PacketInfo.pe_GoodsType), ID);
    }

    public override string GetTooltip()
    {
        string tooltip = Localization.Format("ItemTooltipBase", Name, Network.PlayerInfo.GetGoodsValue(TokenType));
        if (string.IsNullOrEmpty(Description) == false || string.IsNullOrEmpty(DescriptionSub) == false)
        {
            tooltip += "\n\n[99ff99]";
            if (string.IsNullOrEmpty(Description) == false)
                tooltip += Description;
            if (string.IsNullOrEmpty(DescriptionSub) == false)
            {
                if (string.IsNullOrEmpty(Description) == false)
                    tooltip += "\n";
                tooltip += DescriptionSub;
            }
        }
        return tooltip;
    }
}
