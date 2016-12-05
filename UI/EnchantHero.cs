using UnityEngine;
using System.Collections;
using PacketEnums;

public class EnchantHero : MonoBehaviour
{
    public delegate bool OnToggleCharacterDelegate(EnchantHero hero, bool bSelected);
    public delegate void OnDeepTouchCharacterDelegate(EnchantHero hero);

    public UIToggleSprite character_border;

    public UIGrid m_Grid;
    public UISprite[] m_Stars;
    public UISprite m_icon, m_type;
    public UILabel m_level, m_enchant, m_label_in_team;
    public UIToggle m_toggle, m_toggle_dummy;
    public OnToggleCharacterDelegate OnToggleCharacter = null;
    public OnDeepTouchCharacterDelegate OnDeepTouchCharacter = null;

    public Creature Creature { get; private set; }

    public void OnDeepTouch()
    {
        if (OnDeepTouchCharacter != null)
            OnDeepTouchCharacter(this);
    }

    void InitInternal(CreatureInfo creature_info, int grade, string level_text, string enchant_text, string showAttackType, bool border_bg)
    {
        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].gameObject.SetActive(i < grade);
        }

        m_Grid.Reposition();

        character_border.SetSpriteActive(creature_info != null && creature_info.TeamSkill != null);
        string sprite_id = creature_info != null ? creature_info.ID : "";
        string sprite_name = string.Format("cs_{0}", sprite_id);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_icon.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_icon.spriteName = new_sprite_name;

        gameObject.name = string.Format("eh_{0}", sprite_id);

        m_level.text = level_text;
        m_enchant.text = enchant_text;

        m_label_in_team.text = TeamDataManager.Instance.GetTeamString(Creature);

        m_type.spriteName = string.Format("New_hero_info_hero_type_{0}", showAttackType);
    }

    //---------------------------------------------------------------------------
    public void Init(Creature creature, OnToggleCharacterDelegate _del = null, OnDeepTouchCharacterDelegate _deep = null)
    {
        OnToggleCharacter = _del;
        OnDeepTouchCharacter = _deep;

        if (creature == null)
        {
            name = "dummy";
            GetComponent<BoxCollider2D>().enabled = true;

            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = false);
            return;
        }
        else
        {
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);
        }

        Creature = creature;

        m_toggle.gameObject.SetActive(true);
        gameObject.SetActive(true);
        m_toggle.value = false;
        m_toggle_dummy.value = false;

        //PacketEnums.pe_Team team_type = TeamDataManager.Instance.CheckTeam(creature.Idx);

        InitInternal(creature.Info, creature.Grade, creature.GetLevelText(), creature.GetEnchantText(), creature.Info.ShowAttackType, true);

        var collider = GetComponent<BoxCollider2D>();
        collider.enabled = _del != null;
    }

    public void InitSoulStone(SoulStoneInfo info)
    {
        m_toggle.value = false;
        m_toggle_dummy.value = false;
        Creature = null;

        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);

        gameObject.SetActive(true);
        InitInternal(info.Creature, info.Grade, "", "", info.Creature.ShowAttackType, true);

        OnToggleCharacter = null;
        OnDeepTouchCharacter = null;

        var collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
    }

    public void InitDummy(CreatureInfo creature_info, short grade, short level, short enchant, string showAttackType = "")
    {
        gameObject.SetActive(true);
        m_toggle.value = false;
        m_toggle_dummy.value = true;
        Creature = null;

        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);

        InitInternal(creature_info, grade, Localization.Format("HeroLevel", level), Localization.Format("HeroEnchant", enchant), showAttackType, true);
        m_level.gameObject.SetActive(level > 0);
        m_enchant.gameObject.SetActive(enchant > 0);

        OnToggleCharacter = null;
        OnDeepTouchCharacter = null;

        var collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
    }

    public void Init()
    {
        float alpha = 0.001f;
        System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().alpha = alpha);

        m_toggle.value = false;
        m_toggle_dummy.value = false;

        var collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
        gameObject.SetActive(true);
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
