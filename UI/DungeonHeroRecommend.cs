using UnityEngine;
using System.Collections;

public delegate bool OnSelectDelegate(CreatureInfo hero);

public class DungeonHeroRecommend : MonoBehaviour
{
    public UISprite[] m_Stars;
    public UISprite m_icon, m_type;
    public SHTooltip m_tooltip;
    public UIToggleSprite character_border;

    OnSelectDelegate OnSelect = null;
    public CreatureInfo CreatureInfo { get; private set; }
    // Use this for initialization
    void Start()
    {
    }

    //---------------------------------------------------------------------------
    public void Init(CreatureInfo info, OnSelectDelegate _del = null)
    {
        CreatureInfo = info;
        if (CreatureInfo == null)
        {
            gameObject.SetActive(true);
            m_icon.spriteName = "";
            m_type.spriteName = "";
            character_border.spriteName = "";

            return;
        }
        gameObject.name = info.ID;

        string sprite_name = string.Format("cs_{0}", info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_icon.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_icon.spriteName = new_sprite_name;

        m_type.spriteName = string.Format("New_hero_info_hero_type_{0}", info.ShowAttackType);
        gameObject.SetActive(true);

        character_border.SetSpriteActive(info.TeamSkill != null);

        OnSelect = _del;
        //if (OnSelect != null)
        //    m_tooltip.span_press_time = 0.2f;
        //else
        //    m_tooltip.span_press_time = 0f;

    }
    //---------------------------------------------------------------------------
    public void OnShowTooltip(SHTooltip tooltip)
    {
        if (CreatureInfo == null) return;
        if (OnSelect == null)
            Tooltip.Instance.ShowTarget(CreatureInfo.GetTooltip(), tooltip);
    }

    public void OnClickSelect()
    {
        if (CreatureInfo == null) return;
        if(OnSelect != null)
        {
            OnSelect(CreatureInfo);
        }
    }
}
