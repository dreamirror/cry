using UnityEngine;
using System.Collections;
using PacketEnums;

public class HeroesInfoItem : MonoBehaviour
{
    public delegate bool OnToggleCharacterDelegate(Creature creature, bool bSelected);

    public UIToggleSprite character_border;

    public UISprite character;
    public UISprite character_type;
    public UIGrid gradeGrid;
    public GameObject[] stars;

    public UILabel m_LabelName, m_LabelLevel, m_LabelEnchant, m_LabelInTeam;

    public GameObject m_Notify;

    public UISprite m_SpriteSelected;

    public Creature Creature { get; private set; }

    public delegate void OnDeepTouchCharacterDelegate(HeroesInfoItem hero);
    public OnDeepTouchCharacterDelegate OnDeepTouchCharacter = null;

    // Use this for initialization
    void Start()
    {
    }

    //---------------------------------------------------------------------------
    public void Init(Creature creature, OnDeepTouchCharacterDelegate _deep = null)
    {
        gameObject.SetActive(true);
        if (creature == null)
        {
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = false);
            return;
        }
        else
        {
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);
        }
        Creature = creature;

        character_border.SetSpriteActive(Creature.Info.TeamSkill != null);

        character.spriteName = string.Format("cs_{0}", Creature.Info.ID);
        name = string.Format("hi_{0}", Creature.Info.ID);
        character_type.spriteName = string.Format("New_hero_info_hero_type_{0}", Creature.Info.ShowAttackType);
        for (int i = 0; i < stars.Length; ++i)
            stars[i].SetActive(i < Creature.Grade);
        gradeGrid.gameObject.SetActive(true);
        gradeGrid.Reposition();

        m_LabelName.text = creature.Info.Name;
        m_LabelLevel.text = creature.GetLevelText();
        m_LabelEnchant.text = creature.GetEnchantText();

        m_LabelInTeam.text = TeamDataManager.Instance.GetTeamString(creature);

        OnDeepTouchCharacter = _deep;

        SetSelect(false);
        m_Notify.SetActive(Creature.IsNotify);
    }
    //---------------------------------------------------------------------------
    public void SetCreature(Creature creature)
    {
        Creature = creature;
        m_Notify.SetActive(Creature.IsNotify);

    }
    public void SetSelect(bool is_selected)
    {
        m_SpriteSelected.gameObject.SetActive(is_selected);
    }

    //---------------------------------------------------------------------------

    public void OnClickHero()
    {
        if (Creature == null) return;
        if (OnDeepTouchCharacter != null)
            OnDeepTouchCharacter(this);
    }

    void OnDeepTouch()
    {
        if (Creature != null && OnDeepTouchCharacter != null)
        {
            OnDeepTouchCharacter(this);
        }
    }
}
