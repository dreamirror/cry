using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RuneSortInfo : MonoBehaviour
{
    public UILabel m_PopupLabel;
    public UIToggle m_PopupState;
    public UIToggleSprite m_SortDirection;

    public eRuneSort SortType = eRuneSort.Grade;
    public bool IsAscending { get { return m_SortDirection.ActiveSprite; } }

    System.Action OnSortCallback = null;

    public List<Rune> GetSortedRunes()
    {
        return RuneManager.Instance.GetSortedList(SortType, IsAscending);
    }

    public void Init(System.Action callback)
    {
        OnSortCallback = callback;
    }

    public void OnClickSort(GameObject obj)
    {
        m_PopupState.value = false;

        eRuneSort sort_type = eRuneSort.Grade;
        switch(obj.name)
        {
            case "btn_grade":       sort_type = eRuneSort.Grade;    break;
            case "btn_level":       sort_type = eRuneSort.Level;    break;
            case "btn_idn":         sort_type = eRuneSort.IDN;      break;
        }

        if (SortType == sort_type)
            m_SortDirection.SetSpriteActive(!IsAscending);
        else
            m_SortDirection.SetSpriteActive(false);
        SortType = sort_type;
        m_PopupLabel.text = Localization.Get("HeroSort" + SortType);

        OnSortCallback();
    }
}
