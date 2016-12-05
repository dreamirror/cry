using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;
using System.Collections.Generic;

public class PopupLootCharacter10 : PopupBase
{
    public GameObject LootHeroItemPrefab;
    public GameObject[] m_Heroes;
    public GameObject m_Bottom;

    ///////////////////////
    bool ShowImmediately = false;

    StoreItem m_StoreItem;
    C2G.LootCreature10Ack _ack;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        _ack = parms[0] as C2G.LootCreature10Ack;
        if (parms.Length >= 2)
            m_StoreItem = parms[1] as StoreItem;
        else
            m_StoreItem = null;

        if (is_new)
        {
            ShowImmediately = false;
            Init();
        }
        else
        {
            ShowImmediately = true;
            showNextItemTime = Time.time + delay;
        }
    }
    override public bool bShowImmediately { get { return ShowImmediately; } }
    ///////////////////////

    float delay = 0.3f;
    float showNextItemTime = 0f;
    int index = 0;
    void Update()
    {
        if (index < m_Heroes.Length && showNextItemTime < Time.time)
        {
            showNextItemTime = Time.time + delay;
            pd_CreatureLootData loot = _ack.loots[index];
            LootHeroItem item;
            if (m_HeroItems.Count <= index)
            {
                item = NGUITools.AddChild(m_Heroes[index], LootHeroItemPrefab).GetComponent<LootHeroItem>();
                m_HeroItems.Add(item);
            }
            else
                item = m_HeroItems[index];

            item.gameObject.SetActive(true);
            item.Init(loot);
            index++;
            if(item.m_Creature.Grade >= 4)
                Popup.Instance.Show(ePopupMode.LootCharacter, item.m_Creature.Idx, false, false);
        }

        if (m_Bottom.activeSelf == false && index >= m_Heroes.Length)
        {
            m_Bottom.SetActive(true);
        }
    }
    List<LootHeroItem> m_HeroItems = new List<LootHeroItem>();
    public void Init()
    {
        index = 0;
        showNextItemTime = Time.time + delay;
        m_HeroItems.ForEach(e => e.gameObject.SetActive(false));

        m_Bottom.SetActive(false);
    }

    public void OnPurchased()
    {
        parent.Close(true, true);
        m_StoreItem.OnLootMore();
    }

}
