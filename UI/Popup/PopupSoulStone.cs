using UnityEngine;
using System.Collections;
using PacketEnums;
using System.Collections.Generic;
using System;

public class PopupSoulStone : PopupBase
{
    public GameObject placeItemPrefab;

    public UIPlayTween m_PlayTween;
    //main
    public UILabel m_LabelHeroName;
    public UISprite m_SpriteHeroIcon;
    public UILabel m_LabelHeroCount;

    public UILabel m_LabelHeroDesc;

    public UILabel m_LabelHeroDescValue;

    public UILabel m_LabelHeroSalePrice;
    //panel
    public UILabel m_LabelPanelName;
    public UIGrid m_Grid;
    public GameObject m_GridFree;
    //////////////////////////////////////////////////////////////////////////

    // Use this for initialization
	void Start ()
    {
#if DEBUG
        TestInit();
#endif
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
    }

    void TestInit()
    {
        m_LabelHeroName.text = "소라게";
        string sprite_name = "cs_001_sora";
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_SpriteHeroIcon.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;
        m_SpriteHeroIcon.spriteName = new_sprite_name;

        m_LabelHeroCount.text = Localization.Format("ItemCount", 6);

        m_LabelHeroDesc.text = "영웅을 소환하거나, 진화시킬 수 있습니다.";

        m_LabelHeroDescValue.text = Localization.Format("ReservedHeroes", 4);
        m_LabelHeroSalePrice.text = Localization.Format("GoodsFormat", 12345);

        //panel
        //m_LabelPanelName.text = Localization.Get("");
        //m_Grid.Reposition();
    }

    public void OnShowPlace()
    {
        //if (m_eMode == PopupItemMode.Place)
        //{
        //    m_PlayTween.Play(false);
        //    m_eMode = PopupItemMode.init;
        //}
        //else
        //{
        //    TestPanelForPlace();
        //    if (m_eMode == PopupItemMode.init)
        //        m_PlayTween.Play(true);
        //    m_eMode = PopupItemMode.Place;
        //}
    }

    public void OnSale()
    {
        //if (m_eMode == PopupItemMode.Sale)
        //{
        //    m_PlayTween.Play(false);
        //    m_eMode = PopupItemMode.init;
        //}
        //else
        //{
        //    TestPanelForSale();
        //    if (m_eMode == PopupItemMode.init)
        //        m_PlayTween.Play(true);
        //    m_eMode = PopupItemMode.Sale;
        //}
    }
    void TestPanelForSale()
    {
        m_LabelPanelName.text = Localization.Get("ItemSale");
        m_Grid.GetChildList().ForEach(a => a.parent = m_GridFree.transform);
    }
    void TestPanelForPlace()
    {
        m_LabelPanelName.text = Localization.Get("StuffPlace");
        m_Grid.GetChildList().ForEach(a => a.parent = m_GridFree.transform);
        for (int i = 0; i < 5; ++i)
        {
            GameObject obj = NGUITools.AddChild(m_Grid.gameObject, placeItemPrefab);
            obj.SetActive(true);
        }
        m_Grid.gameObject.GetComponentInParent<UIScrollView>().ResetPosition();
        m_Grid.Reposition();
    }
}
