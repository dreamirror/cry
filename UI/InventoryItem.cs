using UnityEngine;
using System.Collections;

public delegate void OnItemClickDelegate(Item item);

public class InventoryItem : MonoBehaviour
{
    public UISprite m_SpriteIcon;
    public UILabel m_Count, m_PieceCount;
    public GameObject m_Piece;
    public GameObject m_StuffGrade;
    public UILabel m_LabelGrade;

    OnItemClickDelegate OnItemClick = null;
    Item m_Item = null;

    float alpha = 1f;
    //---------------------------------------------------------------------------
    public void Init(Item item, OnItemClickDelegate _del)
    {
        gameObject.SetActive(true);

        if (item == null)
        {
            alpha = 0.001f;
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().alpha = alpha);
            return;
        }
        else if(alpha != 1f)
        {
            alpha = 1f;
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().alpha = 1f);
        }

        m_Item = item;
        OnItemClick = _del;
        m_Count.text = string.Format("{0}", item.Count);

        m_Piece.SetActive(item.Info.PieceCountMax > 1);
        m_PieceCount.text = string.Format("{0}", item.PieceCount);

        m_StuffGrade.SetActive(item.IsStuff);
        if (item.IsStuff)
        {
            m_LabelGrade.text = item.StuffInfo.Grade.ToString();
            m_SpriteIcon.spriteName = item.StuffInfo.IconID;
        }
        else
            m_SpriteIcon.spriteName = item.Info.ID;

    }
    //---------------------------------------------------------------------------

    public void OnClick()
    {
        if (m_Item == null) return;
        Debug.LogFormat("OnClick : {0}", m_Item.Info.ID);
        if (OnItemClick != null)
            OnItemClick(m_Item);
    }
}
