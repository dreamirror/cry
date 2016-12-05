using UnityEngine;
using System.Collections.Generic;

public class Inventory : MenuBase
{

    public GameObject InventoryItemPrefab;
    public UIGrid m_Grid;

    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        InitItem();
        return true;
    }

    override public void UpdateMenu()
    {
        InitItem();
    }
    ////////////////////////////////////////////////////////////////

    void Start()
    {
        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();
    }

    List<InventoryItem> m_ListItem = new List<InventoryItem>();
    void InitItem()
    {
        if (m_Grid.gameObject.activeInHierarchy == false) return;

        m_Grid.GetChildList().ForEach(o => o.gameObject.SetActive(false));
        var items = ItemManager.Instance.AvailableItems;
        for (int i = 0; i < items.Count; ++i)
        {
            InventoryItem item;
            if (m_ListItem.Count > i)
                item = m_ListItem[i];
            else
            {
                item = NGUITools.AddChild(m_Grid.gameObject, InventoryItemPrefab).GetComponent<InventoryItem>();
                m_ListItem.Add(item);
            }
            item.Init(items[i], OnItemClick);
        }

        int count = m_ListItem.Count;
        while(count++ % 7 != 0)
        {
            InventoryItem item = NGUITools.AddChild(m_Grid.gameObject, InventoryItemPrefab).GetComponent<InventoryItem>();
            m_ListItem.Add(item);
            item.Init(null, null);
        }


        m_Grid.Reposition();

        UIScrollView scroll = m_Grid.GetComponentInParent<UIScrollView>();
        if (scroll != null)
        {
            scroll.ResetPosition();
//            Debug.LogFormat("{0}", scroll.contentPivot);
        }
    }

    void OnItemClick(Item item)
    {
        if(item.IsItem)
            Popup.Instance.Show(ePopupMode.Item, item.Info);
        else
            Popup.Instance.Show(ePopupMode.Stuff, item.Info);
    }

    public void OnValueChanged(UIToggle toggle)
    {
    }
}
