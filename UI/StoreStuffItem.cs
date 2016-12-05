using UnityEngine;
using System.Collections;
using PacketInfo;

public class StoreStuffItem : MonoBehaviour
{
    public GameObject RewardItemPrefab;
    public UILabel m_ItemName;
    public GameObject m_ItemIndicator;
    public UILabel m_Price;
    public UISprite m_PriceIcon;
    public GameObject m_Purchased;

    pd_StoreItem m_StoreItem;
    string m_StoreID;
    // Use this for initialization
    void Start () {
    }

    //---------------------------------------------------------------------------
    public void Init(string store_id, pd_StoreItem item)
    {
        m_StoreItem = item;
        m_StoreID = store_id;

        gameObject.SetActive(true);

        m_ItemName.text = Store.GetName(item);

        RewardItem reward = NGUITools.AddChild(m_ItemIndicator, RewardItemPrefab).GetComponent<RewardItem>();
        reward.InitStoreItem(item);
        reward.OnClickItem = OnClickItem;

        m_Price.text = Localization.Format("GoodsFormat", item.price.goods_value);
        m_PriceIcon.spriteName = item.price.goods_type.ToString();

        m_Purchased.SetActive(item.buying_state > 0);
    }

    void OnClick()
    {
        if (m_StoreItem.buying_state > 0) return;

        long price = Network.PlayerInfo.GetGoodsValue(m_StoreItem.price.goods_type);
        if (m_StoreItem.price.goods_value > price)
        {
            switch(m_StoreItem.price.goods_type)
            {
                case pe_GoodsType.token_boss:
                case pe_GoodsType.token_arena:
                case pe_GoodsType.token_raid:
                    Tooltip.Instance.ShowMessageKey(string.Format("NotEnough{0}", m_StoreItem.price.goods_type));
                    return;
                case pe_GoodsType.token_cash:
                    return;
            }
            Popup.Instance.Show(ePopupMode.MoveStore, m_StoreItem.price.goods_type);
            return;
        }

        Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(m_StoreItem, OnPopupOk));
    }

    void OnClickItem(ItemInfoBase info)
    {
        OnClick();
    }

    void OnPopupOk(StoreConfirmParam parm)
    {
        C2G.StoreItemBuy packet = new C2G.StoreItemBuy();

        packet.store_id = m_StoreID;
        packet.store_idx = m_StoreItem.store_idx;

        switch (m_StoreItem.item_type)
        {
            case pe_StoreItemType.Rune:
                RuneInfo rune_info = ItemInfoManager.Instance.GetInfoByIdn(m_StoreItem.item_idn) as RuneInfo;
                packet.rune_id = rune_info.ID;
                break;
            case pe_StoreItemType.Token:
                packet.goods = new pd_GoodsData((ItemInfoManager.Instance.GetInfoByIdn(m_StoreItem.item_idn) as TokenInfo).TokenType, m_StoreItem.item_count);
                break;
            case pe_StoreItemType.Creature:
                packet.creature_id = CreatureInfoManager.Instance.GetInfoByIdn(m_StoreItem.item_idn).ID;
                packet.creature_grade = m_StoreItem.item_count;
                break;
            default:
                break;
        }

        Network.GameServer.JsonAsync<C2G.StoreItemBuy, C2G.StoreItemBuyAck>(packet, OnStoreItemBuy);
    }

    void OnStoreItemBuy(C2G.StoreItemBuy packet, C2G.StoreItemBuyAck ack)
    {
        m_StoreItem.buying_state = 1;
        m_Purchased.SetActive(m_StoreItem.buying_state > 0);

        if (ack.loot_rune != null)
            RuneManager.Instance.Add(ack.loot_rune);
        else if (packet.goods != null)
        {
            Network.PlayerInfo.AddGoods(packet.goods);
        }
        else if (ack.loot_creature != null)
        {
            Network.Instance.LootCreature(ack.loot_creature);
            Popup.Instance.Show(ePopupMode.LootCharacter, ack.loot_creature.creature.creature_idx, false, true);
        }
        else
        {
            ItemManager.Instance.Add(m_StoreItem);

            //while (ItemManager.Instance.ItemMadeList.Count > 0)
            //{
            //    Tooltip.Instance.ShowItemMade(ItemManager.Instance.ItemMadeList[0].item.Info);
            //    ItemManager.Instance.ItemMadeList.RemoveAt(0);
            //}
            ItemManager.Instance.ItemMadeList.Clear();
        }

        Network.PlayerInfo.UseGoods(m_StoreItem.price);
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateMenu();

        Tooltip.Instance.ShowMessageKey("SuccessPurchased");
    }

    public void OnClickPurchased()
    {

    }
    //---------------------------------------------------------------------------
}
