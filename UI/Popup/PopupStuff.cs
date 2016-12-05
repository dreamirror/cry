using UnityEngine;
using System.Collections.Generic;
using System;
using PacketEnums;

//public enum PopupItemMode
//{
//    init,
//    Place,
//    Reserve,
//    Sale,
//}
public class PopupStuff: PopupBase
{
    public GameObject reserveItemPrefab, placeItemPrefab;
    public UIPlayTween m_PlayTween;

    //////////////////////////////////////////////////////////////////////////
    //main
    public GameObject m_MainObject, m_CloseObject;
    public UILabel m_LabelStuffName;
    public UISprite m_SpriteStuffIcon;
    public GameObject m_StuffPuzzle;
    public UILabel m_LabelStuffGrade;
    public UILabel m_LabelStuffCount;
    public UILabel m_LabelStuffPieceCount;

    
    public UILabel m_LabelStuffMakeDesc;
    public UILabel m_LabelStuffPurchaseValue;

    public UILabel m_LabelStuffSalePrice;

    //panel
    public GameObject m_Panel;
    public UILabel m_LabelPanelName;
    public UIGrid m_Grid;
    public GameObject m_GridFree;
    
    //////////////////////////////////////////////////////////////////////////
    
    StuffInfo m_Info = null;
    Item m_Item = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms == null || parms.Length != 1)
            throw new System.Exception("invalid parms");

        m_Info = (StuffInfo)parms[0];
        m_Item = ItemManager.Instance.GetItemByIdn(m_Info.IDN);
        
        if (is_new == false)
            Network.TargetItemInfo = null;

        Init();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        if(m_Panel.activeInHierarchy == true)
        {
            m_Grid.Reposition();

            UIScrollView scrollView = m_Grid.gameObject.GetComponentInParent<UIScrollView>();
            if (scrollView != null)
                scrollView.ResetPosition();
        }
    }
    void Update()
    {
    }

    public void OnShowPlace()
    {
        InitForPlace();
    }
    public void OnPurchaseStuff()
    {
        if (((m_Info.PieceCountMax - m_Item.PieceCount) * m_Info.StuffPurchaseValue) > Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gem))
        {
            Popup.Instance.Show(ePopupMode.MoveStore, PacketInfo.pe_GoodsType.token_gem);
            return;
        }
        PacketInfo.pd_StoreItem item = new PacketInfo.pd_StoreItem();
        item.item_idn = m_Item.Info.IDN;
        item.price = new PacketInfo.pd_GoodsData(PacketInfo.pe_GoodsType.token_gem, (m_Info.PieceCountMax - m_Item.PieceCount) * m_Info.StuffPurchaseValue);
        item.item_count = (short)(m_Info.PieceCountMax - m_Item.PieceCount);

        Popup.Instance.Show(ePopupMode.StoreConfirm, new StoreConfirmParam(item, OnPopupOk));
    }
    public void OnPopupOk(StoreConfirmParam parm)
    {
        C2G.StuffPurchase packet = new C2G.StuffPurchase();
        packet.stuff_idn = m_Item.Info.IDN;

        Network.GameServer.JsonAsync<C2G.StuffPurchase, C2G.StuffPurchaseAck>(packet, OnPurchaseStuffHandler);
    }
    public void OnPurchaseStuffHandler(C2G.StuffPurchase send, C2G.StuffPurchaseAck recv)
    {
        m_Item.AddPiece( (short)(m_Info.PieceCountMax - m_Item.PieceCount));
        m_LabelStuffPurchaseValue.text = ((m_Info.PieceCountMax - m_Item.PieceCount) * m_Info.StuffPurchaseValue).ToString();
        ItemManager.Instance.ItemMadeList.Clear();

        Network.PlayerInfo.UseGoods(new PacketInfo.pd_GoodsData(PacketInfo.pe_GoodsType.token_gem, m_Info.StuffPurchaseValue * (m_Info.PieceCountMax - m_Item.PieceCount)));

        GameMain.Instance.UpdateNotify(false);
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateMenu();

        Tooltip.Instance.ShowMessageKey("SuccessPurchased");
    }

    public void OnSale()
    {
        if (m_Item.Count <= 0)
        {
            Tooltip.Instance.ShowMessageKey("NotEnoughStuff");
            return;
        }
        Popup.Instance.Show(ePopupMode.StuffSale, m_Item);
    }

    public void OnFinishedPlayTween()
    {
    }

    void OnItemSale()
    {
        Init();
    }

    void InitForPlace()
    {
        m_Panel.SetActive(true);
        //m_ItemSale.gameObject.SetActive(false);

        m_LabelPanelName.text = Localization.Get("StuffPlace");
        Array.ForEach(m_Grid.GetComponentsInChildren(typeof(PopupStuffPlaceItem), true), i => DestroyImmediate(i.gameObject));
        for (int i = 0; i < m_Info.DropInfo.menus.Count; ++i)
        {
            var menu = m_Info.DropInfo.menus[i];
            if (menu.menu == GameMenu.Dungeon)
            {
                if (MapInfoManager.Instance.ContainsKey(menu.menu_parm_1) == true)
                {
                    var map_info = MapInfoManager.Instance.GetInfoByID(menu.menu_parm_1);
                    if (map_info.IDN > GameConfig.Get<int>("contents_open_main_map"))
                        continue;
                }
                else
                    continue;
            }

            GameObject obj = NGUITools.AddChild(m_Grid.gameObject, placeItemPrefab);
            var item = obj.GetComponent<PopupStuffPlaceItem>();
            item.Init(menu, OnClickItemLocation);
            obj.SetActive(true);
        }
        m_Grid.Reposition();

        UIScrollView scrollView = m_Grid.gameObject.GetComponentInParent<UIScrollView>();
        if (scrollView != null)
            scrollView.ResetPosition();
    }

    void OnClickItemLocation(MoveMenuInfo info)
    {
        switch (info.menu)
        {
            case GameMenu.Dungeon:
                pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), info.menu_parm_2);
                if (MapClearDataManager.Instance.AvailableMap(info.menu_parm_1, difficulty) == false)
                {
                    Tooltip.Instance.ShowMessageKey("NotAvailbleStage");
                    return;
                }
                Network.TargetItemInfo = m_Info;
                GameMain.Instance.StackPopup();

                MenuParams parm = new MenuParams();
                parm.AddParam("menu_parm_1", info.menu_parm_1);
                parm.AddParam("menu_parm_2", info.menu_parm_2);
                GameMain.Instance.ChangeMenu(info.menu, parm);
                break;
            case GameMenu.Store:
                GameMain.Instance.StackPopup();
                GameMain.MoveStore(info.menu_parm_1);
                break;
            default:
                GameMain.Instance.StackPopup();
                GameMain.Instance.ChangeMenu(info.menu);
                break;
        }
    }
    void TestPanelForReserve()
    {
        m_Panel.SetActive(true);
        //m_ItemSale.gameObject.SetActive(false);
        m_LabelPanelName.text = Localization.Get("StuffReserve");
        m_Grid.GetChildList().ForEach(a => a.parent = m_GridFree.transform);
        for (int i = 0; i < 5; ++i)
        {
            GameObject obj = NGUITools.AddChild(m_Grid.gameObject, reserveItemPrefab);
            obj.SetActive(true);
        }
        m_Grid.gameObject.GetComponentInParent<UIScrollView>().ResetPosition();
        m_Grid.Reposition();
    }
    //void TestInit()
    //{
    //    m_LabelStuffName.text = "스크롤";
    //    m_SpriteStuffIcon.spriteName = "item_01";
    //    m_LabelStuffCount.text = Localization.Format("ItemCount", 2);
    //    m_LabelStuffPieceCount.text = Localization.Format("ItemPieceCount", 10, 20);

    //    m_LabelStuffMakeDesc.text = "하위 재료로 만들수 있음";
    //    m_StuffMake1.Init("item_01");
    //    m_LabelStuffMakeCount.text = Localization.Format("StuffMakeCount", 1, 2);
    //    m_StuffMake2.Init("item_02"); ;

    //    m_LabelStuffReserveHeroes.text = Localization.Format("ReservedHeroes", 4);
    //    m_LabelStuffSalePrice.text = Localization.Format("GoodsFormat", 12345);

    //}

    void Init()
    {
        if (m_Info == null) return;

        m_LabelStuffName.text = m_Info.Name;
        m_SpriteStuffIcon.spriteName = m_Info.IconID;
        m_LabelStuffGrade.text = m_Info.Grade.ToString();

        m_StuffPuzzle.SetActive(false);// m_Info.PieceCountMax > 1);

        int item_count = 0;
        int piece_count = 0;
        if (m_Item != null)
        {
            item_count = m_Item.Count;
            piece_count = m_Item.PieceCount;
        }
        m_LabelStuffCount.text = Localization.Format("ItemCount", item_count);
        m_LabelStuffPieceCount.text = Localization.Format("ItemPieceCount", piece_count, m_Info.PieceCountMax);
        m_LabelStuffPieceCount.gameObject.SetActive(m_Info.PieceCountMax > 1);

        {
            if (m_Info.ID.Contains("stuff_recipe") == true)
                m_LabelStuffMakeDesc.text = Localization.Get("DescRecipe");
            else
                m_LabelStuffMakeDesc.text = Localization.Get("DescStuff");
            //m_MakeLayer.SetActive(false);
        }

        
        m_LabelStuffPurchaseValue.text = ((m_Info.PieceCountMax - piece_count) * m_Info.StuffPurchaseValue).ToString();

        m_LabelStuffSalePrice.text = Localization.Format("GoodsFormat", m_Info.SalePrice);
        //TODO: sale stuff
        if (item_count > 0)
        {

        }

        InitForPlace();
    }

    public void OnMakeStuff()
    {
        var make_item_info = ItemManager.Instance.GetItemByID(m_Info.MakeID);
        if(make_item_info.Count < m_Info.MakeCount)
        {
            return;
        }
        C2G.StuffMake packet = new C2G.StuffMake();
        packet.item_idn = m_Info.IDN;
        Network.GameServer.JsonAsync<C2G.StuffMake, C2G.StuffMakeAck>(packet, OnStuffMakeHandler);
    }

    void OnStuffMakeHandler(C2G.StuffMake packet, C2G.StuffMakeAck ack)
    {
        var make_item_info = ItemManager.Instance.GetItemByID(m_Info.MakeID);
        if(make_item_info == null)
        {
            make_item_info = new Item(ItemInfoManager.Instance.GetInfoByID(m_Info.MakeID));
        }

        if (m_Item == null)
        {
            m_Item = new Item(m_Info);
            ItemManager.Instance.Add(m_Item);
        }

        m_Item.AddCount(1);
        make_item_info.AddCount((short)-m_Info.MakeCount);
        GameMain.Instance.UpdateMenu();
        Init();
    }
}
