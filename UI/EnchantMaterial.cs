using UnityEngine;
using System.Collections;

public class EnchantMaterial : MonoBehaviour
{
    public UIToggle m_Info;

    public UIToggleSprite character_border;

    public UIGrid m_Grid;
    public UISprite[] m_Stars;
    public UISprite m_icon, m_type;
    public UILabel m_level, m_enchant, m_enchant_point;
    public SHTooltip m_Tooltip;

    public Creature Creature { get; private set; }
    System.Action<Creature> OnClickCallback = null;

    public int EnchantPoint { get; private set; }

    // Use this for initialization
    void Start()
    {
    }

    public void Init(Creature creature, short point = 0, System.Action<Creature> onClick = null)
    {
        Creature = creature;
        OnClickCallback = onClick;
        EnchantPoint = point;

        m_Info.value = Creature != null;
        if (Creature != null)
        {
            for (int i = 0; i < m_Stars.Length; ++i)
            {
                m_Stars[i].gameObject.SetActive(i < creature.Grade);
            }
            m_Grid.Reposition();

            string sprite_name = string.Format("cs_{0}", Creature.Info.ID);
            string new_sprite_name = "_cut_" + sprite_name;
            UISpriteData sp = m_icon.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
            if (sp != null)
                sp.height = sp.width;

            m_icon.spriteName = new_sprite_name;
            m_type.spriteName = string.Format("New_hero_info_hero_type_{0}", Creature.Info.ShowAttackType);

            m_level.text = creature.GetLevelText();
            m_enchant.text = creature.GetEnchantText();
            m_enchant_point.text = string.Format("+{0}%", point);
            m_enchant_point.gameObject.SetActive(point > 0);
        }
        else
        {
            m_icon.spriteName = "black";
        }

        character_border.SetSpriteActive(Creature != null && Creature.Info.TeamSkill != null);
        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        if (m_Tooltip.Showed == true)
            return;

        if (OnClickCallback != null)
            OnClickCallback(Creature);
    }

    //---------------------------------------------------------------------------
    public void OnShowTooltip(SHTooltip tooltip)
    {
        if (Creature != null)
            Tooltip.Instance.ShowTarget(Creature.GetTooltip(), tooltip);
    }

}
