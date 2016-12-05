using UnityEngine;
using System.Collections;
using PacketEnums;
using System;

public delegate void OnItemLocationDelegate(MoveMenuInfo info);

public class PopupStuffPlaceItem : MonoBehaviour
{
    public UILabel m_LabelName, m_LabelChapter;
    public UISprite m_SpritePlaceIcon;
    public UIButton m_ButtonMove;
    public UILabel m_LabelMove;

    OnItemLocationDelegate OnItemLocation = null;
    MoveMenuInfo MenuInfo { get; set; }

    //---------------------------------------------------------------------------
    public void Init(MoveMenuInfo info, OnItemLocationDelegate _del)
    {
        MenuInfo = info;
        OnItemLocation = _del;
        m_LabelChapter.text = info.title;
        m_LabelName.text = info.desc;
        m_SpritePlaceIcon.spriteName = info.icon_id;

        if (MenuInfo.menu == GameMenu.Dungeon)
        {
            pe_Difficulty difficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), MenuInfo.menu_parm_2);

            if (MapClearDataManager.Instance.AvailableMap(MenuInfo.menu_parm_1, difficulty) == false)
            {
                m_ButtonMove.SetState(UIButtonColor.State.Disabled, true);
                m_ButtonMove.GetComponent<BoxCollider2D>().enabled = false;
                m_LabelMove.color = Color.grey;
                return;
            }
        }
        m_ButtonMove.SetState(UIButtonColor.State.Normal, true);
        m_ButtonMove.GetComponent<BoxCollider2D>().enabled = true;
        m_LabelMove.color = Color.white;
    }
    //---------------------------------------------------------------------------
    public void OnClick()
    {
        if (OnItemLocation != null)
            OnItemLocation(MenuInfo);
    }
}
