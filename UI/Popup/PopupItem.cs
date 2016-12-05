using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;

public class PopupItem: PopupBase
{
    public UIPlayTween m_PlayTween;

    //////////////////////////////////////////////////////////////////////////
    //main
    public UILabel m_LabelItemName;
    public UISprite m_SpriteItemIcon;
    public UILabel m_LabelItemCount;

    public UILabel m_LabelItemDesc;

    public UILabel m_LabelItemDescValue;

    public UILabel m_LabelItemSalePrice;
    //panel
    public GameObject m_Panel;
    public UILabel m_LabelPanelName;
    public UIGrid m_Grid;
    public GameObject m_GridFree;

    public ItemSale m_ItemSale;

    //////////////////////////////////////////////////////////////////////////


    // Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    ItemInfo m_Info = null;
    Item m_Item = null;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms == null || parms.Length != 1)
            throw new System.Exception("invalid parms");

        m_Info = (ItemInfo)parms[0];
        m_Item = ItemManager.Instance.GetItemByIdn(m_Info.IDN);
        Init();
    }

    override public void OnClose()
    {
        parent.Close(true);
    }

    public void OnUse()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
        return;
    }
    public void OnSale()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
        return;

        //if (m_eMode == PopupItemMode.Sale)
        //{
        //    m_PlayTween.Play(false);
        //    m_eMode = PopupItemMode.init;
        //}
        //else
        //{
        //    InitPanelForSale();
        //    if (m_eMode == PopupItemMode.init)
        //        m_PlayTween.Play(true);
        //    m_eMode = PopupItemMode.Sale;
        //}

    }
    void InitPanelForSale()
    {
        m_Panel.SetActive(true);
        m_LabelPanelName.text = Localization.Get("ItemSale");
        m_Grid.GetChildList().ForEach(a => a.parent = m_GridFree.transform);
    }
    void Init()
    {
        m_LabelItemName.text = m_Info.Name;
        m_SpriteItemIcon.spriteName = m_Info.ID;

        m_LabelItemDesc.text = m_Info.Description;

        m_LabelItemDescValue.text = m_Info.DescriptionSub;
        m_LabelItemSalePrice.text = Localization.Format("GoodsFormat", m_Info.SalePrice);

        //panel
        //m_LabelPanelName.text = Localization.Get("");
        //m_Grid.Reposition();
        int item_count = 0;
        if (m_Item != null)
        {
            m_ItemSale.Init(m_Item);
            m_ItemSale.OnItemSale = OnItemSale;
            item_count = m_Item.Count;
        }
        m_LabelItemCount.text = Localization.Format("ItemCount", item_count);
    }

    void OnItemSale()
    {
        Init();
    }
}
