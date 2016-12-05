using UnityEngine;
using System.Collections;

public delegate void OnClickStoreTabDelegate(StoreInfo info);

public class StoreTabItem : MonoBehaviour
{
    public UISprite m_SpriteIcon;
    public UILabel m_LabelText;
    public UIToggle m_Toggle;

    StoreInfo m_Info;
    public StoreInfo Info { get { return m_Info; } }
    OnClickStoreTabDelegate OnClickStoreTab= null;
    // Use this for initialization
    void Start () {
    }

    //---------------------------------------------------------------------------
    public void Init(StoreInfo info, OnClickStoreTabDelegate _del = null)
    {
        m_Info = info;
        OnClickStoreTab = _del;

        gameObject.SetActive(true);
        m_SpriteIcon.spriteName = info.IconID;
        //m_SpriteIcon.MakePixelPerfect();
        m_LabelText.text = Localization.Get(string.Format("StoreTab_{0}",info.ID));
    }

    public void Select()
    {
        m_Toggle.value = true;
        OnClick();
    }

    void OnClick()
    {
        if(OnClickStoreTab != null)
        {
            OnClickStoreTab(m_Info);
        }
    }
    //---------------------------------------------------------------------------

}
