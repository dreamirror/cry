using PacketInfo;
using UnityEngine;
using System.Collections.Generic;
using PacketEnums;

public class StoreConfirmParam
{
    public delegate void OnOkDeleage(StoreConfirmParam param);

    public string title;
    public string message;
    public string icon_id;
    public long price;

    public pd_StoreItem stuff_item;
    public StoreLootItem loot_item;
    public StoreGoodsItem goods_item;
    public Rune rune_item;
    public string refresh_icon;
    public pd_GoodsData refresh_goods;

    public bool is_event = false;

    public OnOkDeleage OnOk = null;
    public StoreConfirmParam() { }
    public StoreConfirmParam(StoreLootItem item, OnOkDeleage _del = null, bool is_free = false)
    {
        title = item.Name;
        icon_id = item.IconID;
        if (is_free)
            price = 0;
        else
            price = item.Price.goods_value;
        message = Localization.Get("StorePurchaseMessage");

        loot_item = item;

        OnOk = _del;
    }
    public StoreConfirmParam(StoreGoodsItem item, OnOkDeleage _del = null)
    {
        title = item.Name;
        icon_id = item.PriceIconID;
        price = item.Price.goods_value;
        message = Localization.Get("StorePurchaseMessage");
        goods_item = item;
        OnOk = _del;
    }
    public StoreConfirmParam(pd_StoreItem item, OnOkDeleage _del = null)
    {
        title = Store.GetName(item);
        icon_id = item.price.goods_type.ToString();
        price = item.price.goods_value;
        message = Localization.Get("StorePurchaseMessage");
        stuff_item = item;
        OnOk = _del;
    }

    public StoreConfirmParam(pd_GoodsData goods, string refresh_icon, OnOkDeleage _del = null)
    {
        title = Localization.Get("StoreRefreshTitle");
        refresh_goods = goods;
        this.icon_id = goods.goods_type.ToString();
        this.price = goods.goods_value;
        message = Localization.Get("StoreRefreshMessage");
        this.refresh_icon = refresh_icon;
        OnOk = _del;
    }

    public enum RuneType
    {
        Sale,
        Unequip,
    }
    public StoreConfirmParam(Rune rune, RuneType type, pd_GoodsData goods, OnOkDeleage _del, bool is_event)
    {
        title = rune.GetName();
        this.icon_id = goods.goods_type.ToString();
        this.price = goods.goods_value;
        rune_item = rune;
        message = Localization.Get(type+"Confirm");
        OnOk = _del;
        this.is_event = is_event;
    }
}

public class PopupStoreConfirm: PopupBase
{
    public UILabel m_title;
    public UILabel m_message;

    public UISprite m_icon;
    public UILabel m_price;

    public UIButton m_btnOK;
    public Transform m_RewardItemIndicator;
    public GameObject m_RewardItemPrefab;
    public GameObject m_Event;

    RewardItem m_RewardItem;
    StoreConfirmParam parm;    
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        parm = parms[0] as StoreConfirmParam;
        Init();
    }

    void Init()
    {
        m_title.text = parm.title;
        m_icon.spriteName = parm.icon_id;
        m_price.text = Localization.Format("GoodsFormat", parm.price);

        if (m_RewardItem == null)
            m_RewardItem = NGUITools.AddChild(m_RewardItemIndicator.gameObject, m_RewardItemPrefab).GetComponent<RewardItem>();

        m_RewardItem.gameObject.SetActive(false);
        m_message.gameObject.SetActive(false);

        m_message.text = parm.message;
        if (parm.stuff_item != null)
            m_RewardItem.InitStoreItem(parm.stuff_item);
        else if (parm.loot_item != null)
            m_RewardItem.InitStoreItem(parm.loot_item);
        else if (parm.goods_item != null)
            m_RewardItem.InitStoreItem(parm.goods_item);
        else if (parm.rune_item != null)
            m_RewardItem.InitRune(parm.rune_item);
        else
            m_RewardItem.InitRefreshItem(parm.refresh_icon);

        m_RewardItem.gameObject.SetActive(true);
        m_message.gameObject.SetActive(true);
        m_Event.SetActive(parm.is_event);
    }

    public void OnCancel()
    {
        //parent.Close();
        base.OnClose();
    }

    public void OnClickOK()
    {
        parent.Close(true, true);
        if (parm.OnOk != null)
        {
            parm.OnOk(parm);
        }
    }
}
