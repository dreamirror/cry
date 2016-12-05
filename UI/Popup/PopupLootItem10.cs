using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;
using System.Linq;

public class PopupLootItem10 : PopupBase
{
    public delegate void OnOkDeleage();

    public GameObject LootItemPrefab;
    public GameObject[] m_Items;
    public GameObject m_Bottom;

    StoreItem m_StoreItem;

    // Update is called once per frame
    void Update ()
    {
        if(index < m_Items.Length && showNextItemTime < Time.time)
        {
            showNextItemTime = Time.time + delay;
            LootItem item;
            if (m_LootItems.Count <= index)
            {
                item = NGUITools.AddChild(m_Items[index], LootItemPrefab).GetComponent<LootItem>();
                m_LootItems.Add(item);
            }
            else
                item = m_LootItems[index];
            item.gameObject.SetActive(true);

            item.Init(loot_item_infos[index]);

            ++index;
        }
        if (m_Bottom.activeSelf == false && index >= m_Items.Length)
            m_Bottom.SetActive(true);
    }
    List<LootItemInfo> loot_item_infos;

    float delay = 0.2f;
    float showNextItemTime = 0f;
    int index = 0;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        loot_item_infos = (List<LootItemInfo>)parms[0];
        if (parms.Length >= 2)
            m_StoreItem = parms[1] as StoreItem;
        else
            m_StoreItem = null;

        Init();
    }

    List<LootItem> m_LootItems = new List<LootItem>();
    public void Init()
    {
        index = 0;
        showNextItemTime = Time.time + delay;
        m_LootItems.ForEach(e => e.gameObject.SetActive(false));

        m_Bottom.SetActive(false);
    }

    public void OnPurchased()
    {
        parent.Close(true, true);
        m_StoreItem.OnLootMore();
    }
}
