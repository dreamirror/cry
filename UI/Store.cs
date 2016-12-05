using System;
using System.Collections.Generic;
using UnityEngine;
using LinqTools;
using PacketInfo;

public class Store : MenuBase
{
    public UIAtlas m_AtlasStore;

    public GameObject StoreItemPrefab, StoreTabItemPrefab, StoreStuffItemPrefab;
    public UIGrid m_Grid, m_GridTab, m_GridStuff;

    public UIToggle m_BottomToggle;

    public UISprite m_SpriteMileage;
    public UILabel m_LabelMileage;

    public UILabel m_LabelRefresh;

    public UILabel m_BottomDesc;
    public UILabel m_BottomStuffDesc;

    string init_tab = "";
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        object parm_obj = parms.GetObject("StoreTab");
        if (parm_obj != null)
        {
            init_tab = parm_obj as string;
        }

        if(parms.bBack == false)
            Init();

        return true;
    }


    override public void UpdateMenu()
    {
        UpdateMileage();
    }

    ////////////////////////////////////////////////////////////////
    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        if (m_AtlasStore.spriteMaterial == null)
            m_AtlasStore.replacement = AssetManager.LoadStoreAtlas();
#else
        m_AtlasStore.replacement = AssetManager.LoadStoreAtlas();
#endif
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnRefresh()
    {
        switch (CurrentStoreInfo.ID)
        {
            case "Items":
            case "Mileage":
            case "Boss":
            case "Arena":
                Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(CurrentStoreInfo.RefreshPrice, CurrentStoreInfo.IconID, OnRefreshPopupOk));
                break;

            default:
                Tooltip.Instance.ShowMessageKey("Invaild OnRefresh");
                break;
        }
    }

    void OnRefreshPopupOk(StoreConfirmParam parm)
    {
        if (Network.Instance.CheckGoods(parm.refresh_goods.goods_type, parm.refresh_goods.goods_value) == false)
            return;

        switch (CurrentStoreInfo.ID)
        {
            case "Items":
            case "Mileage":
            case "Boss":
            case "Arena":
                {
                    C2G.StoreItemsRefresh packet = new C2G.StoreItemsRefresh();
                    packet.clear_map_id = MapClearDataManager.Instance.GetLastClearedMapID();
                    packet.store_id = CurrentStoreInfo.ID;
                    packet.exclude_ids = ItemManager.Instance.NotInventoryItems.Select(i => i.Info.ID).ToList();
                    Network.GameServer.JsonAsync<C2G.StoreItemsRefresh, C2G.StoreItemsGetAck>(packet, OnStoreItemRefresh);
                }
                break;
            default:
                Tooltip.Instance.ShowMessageKey("Invaild OnRefresh");
                break;
        }
    }

    List<StoreTabItem> m_TabList = new List<StoreTabItem>();
    void Init()
    {
        for (int i = 0; i < StoreInfoManager.Instance.Count; ++i)
        {
            StoreInfo info = StoreInfoManager.Instance.GetAt(i);
            if (info.Enable == false) continue;
            StoreTabItem item;
            if (m_TabList.Count <= i)
                item = NGUITools.AddChild(m_GridTab.gameObject, StoreTabItemPrefab).GetComponent<StoreTabItem>();
            else
                item = m_TabList[i];
            item.Init(info, OnTab);

            m_TabList.Add(item);
            if (info.ID == init_tab)
                CurrentStoreInfo = info;
        }
        m_GridTab.Reposition();

        UIScrollView scroll = m_GridTab.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();

        if (CurrentStoreInfo == null)
            CurrentStoreInfo = m_TabList[0].Info;

        m_TabList.Find(e => e.Info == CurrentStoreInfo).Select();
    }

    StoreInfo CurrentStoreInfo = null;
    void OnTab(StoreInfo info)
    {
        Debug.LogFormat("OnTab : {0}", info.ID);
        CurrentStoreInfo = info;
        UpdateMileage();
        switch (info.ID)
        {
            case "Loot":
                {
                    C2G.StoreLootInfoGet packet = new C2G.StoreLootInfoGet();
                    packet.store_id = info.ID;
                    Network.GameServer.JsonAsync<C2G.StoreLootInfoGet, C2G.StoreLootInfoGetAck>(packet, OnStoreLootInfoGet);
                }
                break;
            case "Gem":
            case "Gold":
            case "Energy":
                {
                    C2G.StoreLimitInfoGet packet = new C2G.StoreLimitInfoGet();
                    packet.store_id = info.ID;
                    Network.GameServer.JsonAsync<C2G.StoreLimitInfoGet, C2G.StoreLimitInfoGetAck>(packet, OnStoreLimitInfoGet);
                }
                break;
            default:
                //case "Items":
                //case "Mileage":
                //case "Boss":
                {
                    C2G.StoreItemsGet packet = new C2G.StoreItemsGet();
                    packet.clear_map_id = MapClearDataManager.Instance.GetLastClearedMapID();
                    packet.store_id = info.ID;
                    packet.exclude_ids = ItemManager.Instance.NotInventoryItems.Select(i => i.Info.ID).ToList();
                    Network.GameServer.JsonAsync<C2G.StoreItemsGet, C2G.StoreItemsGetAck>(packet, OnStoreItemGet);
                }
                break;
        }
    }

    void OnStoreLootInfoGet(C2G.StoreLootInfoGet packet, C2G.StoreLootInfoGetAck ack)
    {
        InitStoreLootItem(CurrentStoreInfo, ack);
    }

    void OnStoreLimitInfoGet(C2G.StoreLimitInfoGet packet, C2G.StoreLimitInfoGetAck ack)
    {
        InitStoreItem(CurrentStoreInfo, ack);
    }

    void OnStoreItemGet(C2G.StoreItemsGet packet, C2G.StoreItemsGetAck ack)
    {
        InitStoreStuffItems(ack);
        DateTime refresh = CurrentStoreInfo.GetNextRefreshDate();
        m_LabelRefresh.text = Localization.Format("StoreItemRefreshFormat", refresh.Hour);
    }

    void OnStoreItemRefresh(C2G.StoreItemsRefresh packet, C2G.StoreItemsGetAck ack)
    {
        Network.PlayerInfo.UseGoods(ack.use_goods);
        GameMain.Instance.UpdatePlayerInfo();
        InitStoreStuffItems(ack);
    }

    void InitStoreStuffItems(C2G.StoreItemsGetAck ack)
    {
        m_BottomToggle.value = false;
        Array.ForEach(m_GridStuff.GetComponentsInChildren(typeof(StoreStuffItem), true), i => DestroyImmediate(i.gameObject));
        for (int i = 0; i < ack.store_items.Count; ++i)
        {
            StoreStuffItem item = NGUITools.AddChild(m_GridStuff.gameObject, StoreStuffItemPrefab).GetComponent<StoreStuffItem>();
            item.Init(ack.store_id, ack.store_items[i]);
        }
        m_GridStuff.Reposition();

        UIScrollView scroll = m_GridStuff.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
        UpdateBottomDesc();
    }

    void InitStoreItem(StoreInfo info, C2G.StoreLimitInfoGetAck ack)
    {
        m_BottomToggle.value = true;
        Array.ForEach(m_Grid.GetComponentsInChildren(typeof(StoreItem), true), i => DestroyImmediate(i.gameObject));
        for (int i = 0; i < info.m_GoodsItem.Count; ++i)
        {
            StoreItem item = NGUITools.AddChild(m_Grid.gameObject, StoreItemPrefab).GetComponent<StoreItem>();
            pd_StoreLimitInfo limit_info = null;
            if (ack.infos != null)
                limit_info = ack.infos.Find(e => e.item_id == info.m_GoodsItem[i].ID);

            item.Init(info.m_GoodsItem[i], limit_info);
        }
        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
        UpdateBottomDesc();
    }

    C2G.StoreLootInfoGetAck m_store_loot_info = null;
    void InitStoreLootItem(StoreInfo info, C2G.StoreLootInfoGetAck ack)
    {
        m_store_loot_info = ack;
        m_BottomToggle.value = true;
        Array.ForEach(m_Grid.GetComponentsInChildren(typeof(StoreItem), true), i => DestroyImmediate(i.gameObject));
        for (int i = 0; i < info.m_LootItem.Count; ++i)
        {
            StoreItem item = NGUITools.AddChild(m_Grid.gameObject, StoreItemPrefab).GetComponent<StoreItem>();
            pd_StoreLootInfo loot_info = null;
            if (ack.infos != null)
                loot_info = ack.infos.Find(e => e.loot_id == info.m_LootItem[i].ID);
            item.Init(info.m_LootItem[i], loot_info);
        }
        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();

        UpdateBottomDesc();
    }

    public void UpdateStoreLootFree()
    {
        if (m_store_loot_info == null || m_store_loot_info.infos == null) return;
        StoreInfo store_info = StoreInfoManager.Instance.GetInfoByID("Loot");
        foreach (var store_loot_info in m_store_loot_info.infos)
        {
            StoreLootItem item = store_info.m_LootItem.Find(l => l.ID == store_loot_info.loot_id);
            if (item != null && item.refresh_free > 0)
            {
                if (item.refresh_count == 0)
                    Network.Instance.NotifyMenu.is_store_free_loot = store_loot_info.available_time <= Network.Instance.ServerTime;
                else
                    Network.Instance.NotifyMenu.is_store_free_loot = Network.DailyIndex != store_loot_info.daily_index
                        || (store_loot_info.available_count > 0 && store_loot_info.available_time <= Network.Instance.ServerTime);
            }
            if (Network.Instance.NotifyMenu.is_store_free_loot) return;
        }
    }

    void UpdateBottomDesc()
    {
        if (m_BottomToggle.value == true)
            m_BottomDesc.text = Localization.Get(CurrentStoreInfo.DescID);
        else
            m_BottomStuffDesc.text = Localization.Get(CurrentStoreInfo.DescID);
    }

    void UpdateMileage()
    {
        pe_GoodsType type = pe_GoodsType.token_mileage;
        switch (CurrentStoreInfo.ID)
        {
            case "Boss":
                type = pe_GoodsType.token_boss;
                break;
            case "Arena":
                type = pe_GoodsType.token_arena;
                break;
            case "Energy":
                type = pe_GoodsType.token_friends;
                break;
        }
        m_SpriteMileage.spriteName = type.ToString();
        m_LabelMileage.text = Localization.Format("GoodsFormat", Network.PlayerInfo.GetGoodsValue(type));
    }

    static public string GetName(pd_StoreItem item)
    {
        if (item.item_type == pe_StoreItemType.SoulStone)
            return SoulStoneInfoManager.Instance.GetInfoByIdn(item.item_idn).Name;

        if (item.item_type == pe_StoreItemType.Creature)
            return CreatureInfoManager.Instance.GetInfoByIdn(item.item_idn).Name;

        return ItemInfoManager.Instance.GetInfoByIdn(item.item_idn).Name;
    }
}
