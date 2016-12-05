using UnityEngine;
using System;

public delegate void OnItemSaleDelegate();

public class ItemSale : MonoBehaviour {
    public UILabel m_LabelPrice;
    public UILabel m_LabelSelectedCount;
    public UILabel m_LabelTotalPrice;


    public OnItemSaleDelegate OnItemSale = null;
    // Update is called once per frame
    void Update () {
        UpdateBtn();
    }

    Item m_Item = null;
    short SelectedCount = 1;

    public void Init(Item item)
    {
        m_Item = item;
        SelectedCount = item.Count == 0 ? (short)0 : (short)1;
        UpdatePrice();
    }

    void UpdatePrice()
    {
#if DEBUG
        TestItemSet();
#endif
        m_LabelSelectedCount.text = SelectedCount.ToString();
        m_LabelPrice.text = Localization.Format("GoodsFormat", m_Item.Info.SalePrice);
        m_LabelTotalPrice.text = Localization.Format("TotalSalePriceFormat", m_Item.Info.SalePrice * SelectedCount);
    }

    private void TestItemSet()
    {
        if (m_Item == null || m_Item.Info == null)
        {
            PacketInfo.pd_ItemData itemData = new PacketInfo.pd_ItemData();
            itemData.item_idn = 20001;
            itemData.item_count = 108;
            itemData.item_piece_count = 2;
            m_Item = new Item(itemData);

        }
    }

    public void OnMaxCount()
    {
#if DEBUG
        TestItemSet();
#endif
        SelectedCount = m_Item.Count;
        UpdatePrice();
    }
    public void OnPlus()
    {
#if DEBUG
        TestItemSet();
#endif
        if (SelectedCount >= m_Item.Count) return;
        SelectedCount++;
        UpdatePrice();
    }

    public void OnMinus()
    {
#if DEBUG
        TestItemSet();
#endif
        int min_count = m_Item.Count == 0 ? 0 : 1;
        if (SelectedCount <= min_count) return;
        SelectedCount--;
        UpdatePrice();
    }

    UIButton m_PressedBtn = null;
    public void OnPress(UIButton btn)
    {
        m_PressedBtn = btn;
        delayed_time = Time.time + delay;
        //Debug.LogFormat("OnPress : {0}", btn.name);
    }

    public void OnRelease(UIButton btn)
    {
        m_PressedBtn = null;
        delay_delta = 0f;
        //Debug.LogFormat("OnRelease : {0}", btn.name);
    }

    float delay = 0.3f;
    float delay_min = 0.05f;
    float delay_delta = 0f;
    float delay_delta_increase = 0.03f;
    float delayed_time = 0f;
    public void UpdateBtn()
    {
        if (m_PressedBtn == null) return;
        if (Time.time > delayed_time)
        {
            delayed_time = Time.time + (delay - delay_delta);
            delay_delta = Math.Min(delay - delay_min, delay_delta + delay_delta_increase);
            m_PressedBtn.onClick.ForEach(btn => btn.Execute());
            //Debug.LogFormat("OnUpdate : {0}", m_PressedBtn.name);
        }

    }


    public void OnClickSale()
    {
        if(SelectedCount <= 0 || SelectedCount > m_Item.Count)
        {
            Popup.Instance.ShowMessageKey("NotEnoughStuff");
            return;
        }
        C2G.ItemSale packet = new C2G.ItemSale();
        packet.item_idn = m_Item.Info.IDN;
        packet.item_count = SelectedCount;
        Network.GameServer.JsonAsync<C2G.ItemSale, C2G.ItemSaleAck>(packet, OnSaleHandler);
    }

    public void OnSaleHandler(C2G.ItemSale packet, C2G.ItemSaleAck ack)
    {
        Network.PlayerInfo.AddGoods(ack.add_gold);
        m_Item.UseItem(SelectedCount);
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateMenu();

        if (OnItemSale != null)
        {
            OnItemSale();
        }
    }

}
