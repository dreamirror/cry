using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HeroSortInfo : MonoBehaviour
{
    public UILabel m_PopupLabel;
    public UIToggle m_PopupState;
    public UIToggleSprite m_SortDirection;

    public eCreatureSort SortType = eCreatureSort.Grade;
    public bool IsAscending = false;

    System.Action OnSortCallback = null;

    public List<Creature> GetFilteredCreatures(System.Func<Creature, bool> func)
    {
        if(IsSaleMode)
            return CreatureManager.Instance.GetSortedList(SortTypeSaleMode, IsAscendingSaleMode, CreatureManager.Instance.GetFilteredList(func));
        return CreatureManager.Instance.GetSortedList(SortType, IsAscending, CreatureManager.Instance.GetFilteredList(func));
    }

    public List<Creature> GetSortedCreatures()
    {
        if(IsSaleMode)
            return CreatureManager.Instance.GetSortedList(SortTypeSaleMode, IsAscendingSaleMode);
        return CreatureManager.Instance.GetSortedList(SortType, IsAscending);
    }

    public void Init(System.Action callback, eCreatureSort sortType = eCreatureSort.Grade, bool is_ascending = false)
    {
        OnSortCallback = callback;
        SortType = sortType;
        IsAscending = is_ascending;
        m_PopupLabel.text = Localization.Get("HeroSort" + SortType);
    }

    public void OnClickSort(GameObject obj)
    {
        m_PopupState.value = false;

        eCreatureSort sort_type = eCreatureSort.Grade;
        switch(obj.name)
        {
            case "btn_grade":       sort_type = eCreatureSort.Grade;    break;
            case "btn_level":       sort_type = eCreatureSort.Level;    break;
            case "btn_enchant":     sort_type = eCreatureSort.Enchant;  break;
            case "btn_idn":         sort_type = eCreatureSort.IDN;      break;
            case "btn_power":       sort_type = eCreatureSort.Power;    break;
            case "btn_hp":          sort_type = eCreatureSort.HP;       break;
            case "btn_attack":      sort_type = eCreatureSort.Attack;   break;
            case "btn_defense":     sort_type = eCreatureSort.Defense;  break;
        }

        if (IsSaleMode)
        {
            if (SortTypeSaleMode == sort_type)
                IsAscendingSaleMode = !IsAscendingSaleMode;
            else
                IsAscendingSaleMode = false;
            SortTypeSaleMode = sort_type;
            m_PopupLabel.text = Localization.Get("HeroSort" + SortTypeSaleMode);
            m_SortDirection.SetSpriteActive(IsAscendingSaleMode);
        }
        else
        {
            if (SortType == sort_type)
                IsAscending = !IsAscending;
            else
                IsAscending = false;
            SortType = sort_type;
            m_PopupLabel.text = Localization.Get("HeroSort" + SortType);
            m_SortDirection.SetSpriteActive(IsAscending);
        }

        OnSortCallback();
    }

    public bool IsSaleMode { get; private set; }
    eCreatureSort SortTypeSaleMode = eCreatureSort.Grade;
    bool IsAscendingSaleMode = true;
    public void SetSaleMode(bool is_sale)
    {
        IsSaleMode = is_sale;

        if (IsSaleMode)
        {
            m_SortDirection.SetSpriteActive(IsAscendingSaleMode);
            m_PopupLabel.text = Localization.Get("HeroSort" + SortTypeSaleMode);
        }
        else
        {
            m_SortDirection.SetSpriteActive(IsAscending);
            m_PopupLabel.text = Localization.Get("HeroSort" + SortType);
        }
    }
}
