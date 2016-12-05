using UnityEngine;
using System.Collections;

public class DungeonHero : MonoBehaviour
{
    public delegate bool OnToggleCharacterDelegate(DungeonHero hero, bool bSelected);
    public delegate void OnDeepTouchCharacterDelegate(Creature hero);

    public UIToggleSprite character_border;

    public UIGrid m_Grid;
    public UISprite[] m_Stars;
    public UISprite m_icon, m_type, m_Recommend;
    public UILabel m_level, m_enchant;
    public UIToggle m_toggle;
    public OnToggleCharacterDelegate OnToggleCharacter = null;
    public OnDeepTouchCharacterDelegate OnDeepTouchCharacter = null;
    public BoxCollider2D m_Collider;

    public Creature Creature { get; private set; }
    public CreatureInfo CreatureInfo { get; private set; }

    void OnEnable()
    {
        if(Creature != null)
            m_level.text = Creature.GetLevelText();
    }
    public void OnDeepTouch()
    {
        if (Creature != null && OnDeepTouchCharacter != null)
        {
            OnDeepTouchCharacter(Creature);
        }
    }
	
    void InitInternal(CreatureInfo creature_info, int grade, string level_text, string enchant_text, string showAttackType, bool border_bg)
    {
        if (m_Collider == null)
            m_Collider = gameObject.GetComponent<BoxCollider2D>();
        m_Collider.enabled = OnToggleCharacter != null;

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].gameObject.SetActive(i < grade);
        }

        m_Grid.Reposition();

        character_border.SetSpriteActive(creature_info != null && creature_info.TeamSkill != null);

        string sprite_id = creature_info != null?creature_info.ID:"";

        if (string.IsNullOrEmpty(sprite_id))
            m_icon.spriteName = "black";
        else
            m_icon.spriteName = string.Format("cs_{0}", sprite_id);
        m_icon.flip = UIBasicSprite.Flip.Nothing;

        gameObject.name = string.Format("dh_{0}",sprite_id);

        m_level.text = level_text;
        m_enchant.text = enchant_text;

        m_type.gameObject.SetActive(!string.IsNullOrEmpty(showAttackType));
        m_type.spriteName = string.Format("New_hero_info_hero_type_{0}", showAttackType);
    }

    //---------------------------------------------------------------------------
    public void Init(Creature creature, bool is_checked, bool recommend, OnToggleCharacterDelegate _del = null, OnDeepTouchCharacterDelegate _deep = null)
    {
        Creature = creature;
        CreatureInfo = creature.Info;

        gameObject.SetActive(true);

        OnToggleCharacter = _del;
        OnDeepTouchCharacter = _deep;

        InitInternal(creature.Info, creature.Grade, creature.GetLevelText(), creature.GetEnchantText(), creature.Info.ShowAttackType, true);

        m_toggle.value = is_checked;
        m_Recommend.gameObject.SetActive(recommend);
    }

    public void Init()
    {
        float alpha = 0.001f;
        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().alpha = alpha);
        gameObject.SetActive(true);
    }
    public void Init(CreatureInfo creature_info, bool is_checked = false, bool recommend = false, OnToggleCharacterDelegate _del = null, OnDeepTouchCharacterDelegate _deep = null)
    {
        Creature = null;
        CreatureInfo = creature_info;

        gameObject.SetActive(true);

        OnToggleCharacter = _del;
        OnDeepTouchCharacter = _deep;

        InitInternal(creature_info, 1, "", "", creature_info.ShowAttackType, true);

        m_toggle.value = is_checked;
        m_Recommend.gameObject.SetActive(recommend);
    }

    public void Init(BattleEndCreature creature)
    {
        Creature = creature.Creature;
        CreatureInfo = null;

        InitInternal(creature.Creature.Info, creature.Creature.Grade, creature.Creature.GetLevelText(), creature.Creature.GetEnchantText(), creature.Creature.Info.ShowAttackType, true);

        m_toggle.value = false;
        gameObject.SetActive(true);
        m_Recommend.gameObject.SetActive(false);
    }

    public void InitDummy(CreatureInfo creature_info, short grade, short level, short enchant, string show_attack_type)
    {
        Creature = null;
        CreatureInfo = creature_info;

        gameObject.SetActive(true);

        OnToggleCharacter = null;
        OnDeepTouchCharacter = null;

        InitInternal(creature_info, grade, Localization.Format("HeroLevel", level), Localization.Format("HeroEnchant", enchant), show_attack_type, true);

        m_toggle.value = false;
        m_Recommend.gameObject.SetActive(false);
    }

    //---------------------------------------------------------------------------
    public void OnBtnCreatureClick()
    {
        if (OnToggleCharacter != null)
        {
            m_toggle.value = !m_toggle.value;
            if (OnToggleCharacter(this, m_toggle.value) == false)
                m_toggle.value = false;
        }
    }

}
