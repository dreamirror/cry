using UnityEngine;
using System.Collections;
using PacketEnums;
using PacketInfo;
using System;
using System.Linq;
using System.Collections.Generic;

public class StoreItem : MonoBehaviour
{
    public UILabel m_ItemName;
    public GameObject m_Event;

    public UISprite m_SpriteStoreItem;

    public GameObject m_Bonus;
    public UISprite m_BonusIcon;
    public UILabel m_BonusText;

    public GameObject m_NeedTicket;
    public UILabel m_LabelNeedTicket;

    public UISprite m_UseIcon;
    public UILabel m_Price;
    
    public UIButton m_BtnPurchase;

    public GameObject m_Normal;
    public GameObject m_Free;

    public UILabel m_LabelDescTop;

    StoreGoodsItem m_ItemGoods;
    StoreLootItem m_ItemLoot;
    // Use this for initialization
    void Start () {
    }
    void Update()
    {
        if (m_Free.activeSelf == false 
            && m_ItemLoot != null && m_ItemLoot.refresh_free > 0 
            && m_StoreLootInfo != null && m_StoreLootInfo.available_time <= Network.Instance.ServerTime && (m_StoreLootInfo.available_count > 0 || m_ItemLoot.refresh_count == 0))
        {
            m_Free.SetActive(true);
            m_Normal.SetActive(false);
            Network.Instance.NotifyMenu.is_store_free_loot = true;
        }

        if (m_ItemLoot != null && m_ItemLoot.refresh_free > 0)
        {
            if (m_StoreLootInfo != null && m_StoreLootInfo.available_time > Network.Instance.ServerTime && (m_ItemLoot.refresh_count == 0 || m_StoreLootInfo.available_count > 0))
            {
                int seconds = (int)(m_StoreLootInfo.available_time - Network.Instance.ServerTime).TotalSeconds;
                string time = "";
                if (seconds >= 3600)
                    time = Localization.Format("HourMinute", seconds / 3600, seconds % 3600 / 60);
                else if (seconds >= 60)
                    time = Localization.Format("MinuteSeconds", seconds / 60, seconds % 60);
                else
                    time = Localization.Format("Seconds", seconds);

                m_LabelNeedTicket.text = Localization.Format("Free_Remains", time);
            }
            else
            {
                if (m_StoreLootInfo != null && m_ItemLoot.refresh_count > 0)
                    m_LabelNeedTicket.text = Localization.Format("StoreFreeLimit", m_ItemLoot.refresh_count, m_StoreLootInfo.available_count);

                else if (m_StoreLootInfo != null && m_ItemLoot.refresh_count == 0 && m_ItemLoot.refresh_free > 0)
                {
                    m_NeedTicket.SetActive(false);
                    m_Free.SetActive(true);
                    m_Normal.SetActive(false);
                }
                //m_LabelNeedTicket.text = Localization.Format("StoreFreeLimit", m_ItemLoot.refresh_count, m_ItemLoot.refresh_count);
            }
        }
    }
    //---------------------------------------------------------------------------
    pd_StoreLimitInfo m_StoreLimitInfo = null;
    public void Init(StoreGoodsItem item, pd_StoreLimitInfo limit_info)
    {
        m_ItemGoods = item;
        gameObject.SetActive(true);

        m_ItemName.text = m_ItemGoods.Name;

        m_SpriteStoreItem.spriteName = m_ItemGoods.Image;

        m_Event.SetActive(m_ItemGoods.Event);

        m_Bonus.SetActive(false);

        if (m_ItemGoods.bonus > 0)
        {
            m_Bonus.SetActive(true);
            m_BonusIcon.spriteName = m_ItemGoods.TagetIconID;
            //m_BonusIcon.MakePixelPerfect();
            m_BonusText.text = Localization.Format("GoodsFormat", m_ItemGoods.bonus);
        }
        else if(m_ItemGoods.mileage > 0)
        {
            m_Bonus.SetActive(true);
            m_BonusIcon.spriteName = pe_GoodsType.token_mileage.ToString();
            m_BonusText.text = Localization.Format("GoodsFormat", m_ItemGoods.mileage);
        }

        m_StoreLimitInfo = limit_info;
        if(m_StoreLimitInfo == null)
            m_StoreLimitInfo = new pd_StoreLimitInfo();

        if (m_StoreLimitInfo.daily_index != Network.DailyIndex || m_StoreLimitInfo.weekly_index != Network.WeeklyIndex)
        {
            if(m_ItemGoods.limit > 0)
                m_StoreLimitInfo.available_count = m_ItemGoods.limit;
            else
                m_StoreLimitInfo.available_count = short.MaxValue;
            m_StoreLimitInfo.daily_index = Network.DailyIndex;
            m_StoreLimitInfo.weekly_index = Network.WeeklyIndex;
        }

        if (m_ItemGoods.NeedItem != null && ItemManager.Instance.GetItemByIdn(m_ItemGoods.NeedItem.IDN) == null) 
        {
            m_NeedTicket.SetActive(true);
            m_LabelNeedTicket.text = Localization.Get("NeedTicket");
        }
        else
        {
            m_NeedTicket.SetActive(m_ItemGoods.limit > 0);
            //m_LabelNeedTicket.text = Localization.Format("StoreLimit", m_ItemGoods.limit, m_StoreLimitInfo.available_count);
        }

        m_UseIcon.spriteName = m_ItemGoods.PriceIconID;

        m_Price.text = Localization.Format("GoodsFormat", m_ItemGoods.Price.goods_value);

        m_Free.SetActive(false);
        m_Normal.SetActive(true);

        m_LabelDescTop.gameObject.SetActive(false);
    }

    pd_StoreLootInfo m_StoreLootInfo = null;
    public void Init(StoreLootItem item, pd_StoreLootInfo loot_info)
    {
        m_ItemLoot = item;
        gameObject.SetActive(true);
        m_Event.SetActive(false);
        m_Bonus.SetActive(false);

        m_SpriteStoreItem.spriteName = item.Image;

        m_ItemName.text = item.Name;

        m_NeedTicket.SetActive(false);

        m_UseIcon.spriteName = item.Price.goods_type.ToString();
        m_Price.text = Localization.Format("GoodsFormat", item.Price.goods_value);
                
        bool bFree = false;

        if (item.refresh_count > 0)
        {
            m_StoreLootInfo = loot_info;
            if(m_StoreLootInfo == null)
            {
                m_StoreLootInfo = new pd_StoreLootInfo();
                m_StoreLootInfo.daily_index = Network.DailyIndex;
                m_StoreLootInfo.weekly_index = Network.WeeklyIndex;
                m_StoreLootInfo.available_count = item.refresh_count;
                m_StoreLootInfo.available_time = DateTime.MinValue;
            }

            if (m_StoreLootInfo.daily_index != Network.DailyIndex || m_StoreLootInfo.available_count > 0 && m_StoreLootInfo.available_time <= Network.Instance.ServerTime)
            {
                bFree = true;
                if (m_StoreLootInfo.daily_index != Network.DailyIndex)
                {
                    if (m_StoreLootInfo.available_time > Network.Instance.ServerTime)
                        bFree = false;
                    else
                    {
                        m_StoreLootInfo.daily_index = Network.DailyIndex;
                        m_StoreLootInfo.weekly_index = Network.WeeklyIndex;
                        m_StoreLootInfo.available_count = item.refresh_count;
                        m_StoreLootInfo.available_time = DateTime.MinValue;
                    }
                }
                m_LabelNeedTicket.text = Localization.Format("StoreFreeLimit", item.refresh_count, m_StoreLootInfo.available_count);
            }

            m_NeedTicket.SetActive(true);
        }
        else if(item.refresh_free > 0)
        {
            m_StoreLootInfo = loot_info;
            if (m_StoreLootInfo == null)
            {
                m_StoreLootInfo = new pd_StoreLootInfo();
                m_StoreLootInfo.daily_index = Network.DailyIndex;
                m_StoreLootInfo.weekly_index = Network.WeeklyIndex;
                m_StoreLootInfo.available_count = item.refresh_count;
                m_StoreLootInfo.available_time = DateTime.MinValue;
            }
            if (m_StoreLootInfo.available_time <= Network.Instance.ServerTime)
            {
                bFree = true;
            }
            m_NeedTicket.SetActive(!bFree);
        }

        m_LabelDescTop.text = item.DescTop;
        m_LabelDescTop.gameObject.SetActive(item.DescTop != "");

        if(item.DescBottom != "")
        {
            m_LabelNeedTicket.text = item.DescBottom;
            m_NeedTicket.SetActive(true);
        }

        m_Free.SetActive(bFree);
        m_Normal.SetActive(!bFree);

        Network.Instance.NotifyMenu.is_store_free_loot |= bFree;
    }

    public void OnLootMore()
    {
        //long price = Network.PlayerInfo.GetGoodsValue(m_ItemLoot.Price.goods_type);
        if (Network.Instance.CheckGoods(m_ItemLoot.Price.goods_type, m_ItemLoot.Price.goods_value) == false)
            return;

        Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(m_ItemLoot, OnPopupOk));
    }

    public void OnClickPurchase()
    {
        if (m_ItemLoot != null)
        {
            long price = Network.PlayerInfo.GetGoodsValue(m_ItemLoot.Price.goods_type);
            if (m_Free.activeSelf == false && m_ItemLoot.Price.goods_value > price)
            {
                Popup.Instance.Show(ePopupMode.MoveStore, m_ItemLoot.Price.goods_type);
                return;
            }
            Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(m_ItemLoot, OnPopupOk, m_Free.activeInHierarchy));
        }
        else if (m_ItemGoods != null)
        {
            if (m_ItemGoods.StoreID != "Gem")
            {
                long price = Network.PlayerInfo.GetGoodsValue(m_ItemGoods.Price.goods_type);
                if (m_ItemGoods.Price.goods_value > price)
                {
                    if (m_ItemGoods.Price.goods_type == pe_GoodsType.token_friends)
                        Tooltip.Instance.ShowMessageKey("NotEnoughtoken_friends");
                    else
                        Popup.Instance.Show(ePopupMode.MoveStore, m_ItemGoods.Price.goods_type);
                    return;
                }
            }

            Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(m_ItemGoods, OnPopupOk));
            return;
        }
    }

    void OnPopupOkLoot()
    {
        OnPopupOk(null);
    }

    void OnPopupOk(StoreConfirmParam parm)
    {
        if (m_ItemLoot != null)
        {
            long price = Network.PlayerInfo.GetGoodsValue(m_ItemLoot.Price.goods_type);
            if (m_Free.activeInHierarchy == false && m_ItemLoot.Price.goods_value > price)
            {
                Popup.Instance.Show(ePopupMode.MoveStore, m_ItemLoot.Price.goods_type);
                return;
            }
            switch (m_ItemLoot.LootType)
            {
                case "LootHero":
                {
                    if (Network.Instance.CheckCreatureSlotCount(m_ItemLoot.LootCount, true, true, OnLootMore) == false)
                        return;

                    if (m_ItemLoot.LootCount == 1)
                    {
                        C2G.LootCreature packet = new C2G.LootCreature();
                        packet.loot_id = m_ItemLoot.ID;
                        packet.is_free = m_Free.activeSelf;
                        if (Tutorial.Instance.Completed == false)
                        {
                            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
                            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
                            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
                            tutorial_packet.loot_creature = packet;
                            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialLootCreature);
                        }
                        else
                            Network.GameServer.JsonAsync<C2G.LootCreature, C2G.LootCreatureAck>(packet, OnLootCreature);
                        return;
                    }
                    else if (m_ItemLoot.LootCount == 10)
                    {
                        C2G.LootCreature10 packet = new C2G.LootCreature10();
                        packet.loot_id = m_ItemLoot.ID;
                        Network.GameServer.JsonAsync<C2G.LootCreature10, C2G.LootCreature10Ack>(packet, OnLootCreature10);
                        return;
                    }
                }
                break;

                case "LootRune":
                case "LootItem":
                {
                    if (m_ItemLoot.LootType == "LootRune" && Network.Instance.CheckRuneSlotCount(m_ItemLoot.LootCount, true, true, OnLootMore) == false)
                        return;

                    if (m_ItemLoot.LootCount == 1)
                    {
                        C2G.StoreLootItem packet = new C2G.StoreLootItem();
                        packet.loot_id = m_ItemLoot.ID;
                        packet.is_free = m_Free.activeSelf;
                        Network.GameServer.JsonAsync<C2G.StoreLootItem, C2G.StoreLootItemAck>(packet, OnLootItem);
                        return;
                    }
                    else if (m_ItemLoot.LootCount == 10)
                    {
                        C2G.StoreLootItem10 packet = new C2G.StoreLootItem10();
                        packet.loot_id = m_ItemLoot.ID;
                        Network.GameServer.JsonAsync<C2G.StoreLootItem10, C2G.StoreLootItem10Ack>(packet, OnLootItem10);
                        return;
                    }
                }
                break;
            }
        }
        else if (m_ItemGoods != null)
        {
            if (m_ItemGoods.StoreID != "Gem")
            {
                long price = Network.PlayerInfo.GetGoodsValue(m_ItemGoods.Price.goods_type);
                if (m_ItemGoods.Price.goods_value > price)
                {
                Popup.Instance.Show(ePopupMode.MoveStore, m_ItemGoods.Price.goods_type);
                    return;
                }
            }

            C2G.StoreGoodsBuy packet = new C2G.StoreGoodsBuy();

            packet.store_id = m_ItemGoods.StoreID;
            packet.item_id = m_ItemGoods.ID;

            Network.GameServer.JsonAsync<C2G.StoreGoodsBuy, C2G.StoreGoodsBuyAck>(packet, OnStoreItemBuy);
            return;
        }

        Tooltip.Instance.ShowMessageKey("NotImplement");
    }
    void OnStoreItemBuy(C2G.StoreGoodsBuy packet, C2G.StoreGoodsBuyAck ack)
    {
        switch (m_ItemGoods.StoreID)
        {
            case "Gem":
                {
                    Network.PlayerInfo.AddGoodsValue(m_ItemGoods.Target.goods_type, m_ItemGoods.Target.goods_value + m_ItemGoods.bonus);
                    Network.PlayerInfo.AddGoodsValue(PacketInfo.pe_GoodsType.token_mileage, m_ItemGoods.mileage);
                    MetapsAnalyticsScript.TrackPurchase(packet.item_id, m_ItemGoods.Price.goods_value, "WON");
                }
                break;

            case "Gold":
                {
                    m_StoreLimitInfo.available_count--;
                    m_LabelNeedTicket.text = Localization.Format("StoreLimit", m_ItemGoods.limit, m_StoreLimitInfo.available_count);
                    Network.PlayerInfo.AddGoodsValue(m_ItemGoods.Target.goods_type, m_ItemGoods.Target.goods_value + m_ItemGoods.bonus);
                    Network.PlayerInfo.UseGoods(m_ItemGoods.Price);
                }
                break;

            case "Energy":
                {
                    m_StoreLimitInfo.available_count--;
                    m_LabelNeedTicket.text = Localization.Format("StoreLimit", m_ItemGoods.limit, m_StoreLimitInfo.available_count);
                    Network.PlayerInfo.AddEnergy((int)(m_ItemGoods.Target.goods_value + m_ItemGoods.bonus));
                    Network.PlayerInfo.UseGoods(m_ItemGoods.Price);
                }
                break;

        }
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateMenu();
        Tooltip.Instance.ShowMessageKey("SuccessPurchased");
    }

    void OnTutorialLootCreature(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnLootCreature(packet.loot_creature, ack.loot_creature);
        Tutorial.Instance.AfterNetworking();
    }

    void OnLootCreature(C2G.LootCreature packet, C2G.LootCreatureAck ack)
    {
        if (packet.is_free == true)
        {
            if (m_StoreLootInfo == null)
            {
                m_StoreLootInfo = new pd_StoreLootInfo();
                m_StoreLootInfo.loot_id = m_ItemLoot.ID;
                m_StoreLootInfo.available_count = m_ItemLoot.refresh_count;
                m_StoreLootInfo.daily_index = Network.DailyIndex;
                m_StoreLootInfo.weekly_index = Network.WeeklyIndex;
            }
            m_StoreLootInfo.available_count--;
            if(m_StoreLootInfo.available_count > 0 || (m_ItemLoot.refresh_count == 0 && m_ItemLoot.refresh_free > 0))
                m_StoreLootInfo.available_time = Network.Instance.ServerTime.AddMinutes(m_ItemLoot.refresh_free);

            m_Free.SetActive(false);
            m_Normal.SetActive(true);

            m_NeedTicket.SetActive(m_ItemLoot.refresh_free > 0);
        }
        else
            Network.PlayerInfo.UseGoods(m_ItemLoot.Price);

        Network.Instance.LootCreature(ack.creature_loot_data);

        Popup.Instance.Show(ePopupMode.LootCharacter, ack.creature_loot_data.creature.creature_idx, false, true, this);

        GameMain.Instance.UpdatePlayerInfo();

        GameMain.Instance.GetCurrentMenu().GetComponent<Store>().UpdateStoreLootFree();
    }

    void OnLootCreature10(C2G.LootCreature10 packet, C2G.LootCreature10Ack ack)
    {
        Debug.Log("OnLootCreature10");
        Network.PlayerInfo.UseGoods(m_ItemLoot.Price);
        ack.loots.ForEach(c => Network.Instance.LootCreature(c));
        Popup.Instance.Show(ePopupMode.LootCharacter10, ack, this);
        GameMain.Instance.UpdatePlayerInfo();
    }

    void OnLootItem(C2G.StoreLootItem packet, C2G.StoreLootItemAck ack)
    {
        if(packet.is_free == true)
        {
            if (m_StoreLootInfo == null)
            {
                m_StoreLootInfo = new pd_StoreLootInfo();
                m_StoreLootInfo.loot_id = m_ItemLoot.ID;
                m_StoreLootInfo.available_count = m_ItemLoot.refresh_count;
                m_StoreLootInfo.daily_index = Network.DailyIndex;
                m_StoreLootInfo.weekly_index = Network.WeeklyIndex;
            }
            m_StoreLootInfo.available_count--;
            m_StoreLootInfo.available_time = Network.Instance.ServerTime.AddMinutes(m_ItemLoot.refresh_free);
            m_Free.SetActive(false);
            m_Normal.SetActive(true);
        }
        else
            Network.PlayerInfo.UseGoods(m_ItemLoot.Price);

        GameMain.Instance.UpdatePlayerInfo();

        LootItemInfo loot_item_info = null;

        if (ack.loot_item != null)
        {
            ItemManager.Instance.Add(ack.loot_item);
            loot_item_info = new LootItemInfo(ack.loot_item.item_idn, ack.loot_item.add_piece_count);
        }
        else
        {
            RuneManager.Instance.Add(ack.loot_rune);
            loot_item_info = new LootItemInfo(ack.loot_rune.rune_idn, 0);
        }
        Popup.Instance.Show(ePopupMode.LootItem, loot_item_info, this);

        GameMain.Instance.GetCurrentMenu().GetComponent<Store>().UpdateStoreLootFree();
    }

    void OnLootItem10(C2G.StoreLootItem10 packet, C2G.StoreLootItem10Ack ack)
    {
        Network.PlayerInfo.UseGoods(m_ItemLoot.Price);
        List<LootItemInfo> loot_item_infos = new List<LootItemInfo>();
        for (int i = 0; i < ack.loot_items.Count; ++i)
        {
            ItemManager.Instance.Add(ack.loot_items[i]);
            loot_item_infos.Add(new LootItemInfo(ack.loot_items[i].item_idn, ack.loot_items[i].add_piece_count));
        }
        for (int i = 0; i < ack.loot_runes.Count; ++i)
        {
            RuneManager.Instance.Add(ack.loot_runes[i]);
            RuneInfo rune_info = RuneInfoManager.Instance.GetRuneInfoByIdn(ack.loot_runes[i].rune_idn);
            loot_item_infos.Add(new LootItemInfo(ack.loot_runes[i].rune_idn, 0, rune_info.Grade > 3));
        }
        loot_item_infos = loot_item_infos.OrderBy(i => MNS.Random.Instance.Next()).ToList();
        //ItemManager.Instance.ItemMadeList.Clear();

        GameMain.Instance.UpdatePlayerInfo();

        Popup.Instance.Show(ePopupMode.LootItem10, loot_item_infos, this);
    }
    //---------------------------------------------------------------------------
}
