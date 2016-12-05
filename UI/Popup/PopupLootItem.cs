using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;

public class PopupLootItem : PopupBase
{
    public delegate void OnOkDeleage();

    public GameObject m_btnPurchase;

    public GameObject LootItemPrefab;
    public GameObject m_Item;

    StoreItem m_StoreItem;
    LootItemInfo loot_item_info;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        loot_item_info = (LootItemInfo)parms[0];
        if (parms.Length >= 2)
            m_StoreItem = parms[1] as StoreItem;
        else
            m_StoreItem = null;
        m_btnPurchase.gameObject.SetActive(m_StoreItem != null);

        Init();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        while (ItemManager.Instance.ItemMadeList.Count > 0)
        {
            Tooltip.Instance.ShowItemMade(ItemManager.Instance.ItemMadeList[0].item.Info);
            ItemManager.Instance.ItemMadeList.RemoveAt(0);
        }
    }
    LootItem m_LootItem = null;
    public void Init()
    {
        if (m_LootItem == null)
            m_LootItem = NGUITools.AddChild(m_Item, LootItemPrefab).GetComponent<LootItem>();

        m_LootItem.Init(loot_item_info);
    }

    public void OnLoot()
    {
        parent.Close(true, true);
        m_StoreItem.OnLootMore();
    }
}
