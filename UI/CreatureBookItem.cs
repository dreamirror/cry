using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureBookItem : MonoBehaviour {

    public UILabel m_CharacterLabel;
    public UISprite m_CharacterSprite;
    public UISprite m_CharacterType;
    public UIToggleSprite m_CharacterBoarder;

    public GameObject m_BlockObj;
    public GameObject m_MyselfObj;

    public UISprite m_NotifyIcon;

    public UIGrid m_GridStars;
    public List<UISprite> m_Stars;

    CreatureInfo info;
    short grade;

    public void Init()
    {
        m_MyselfObj.SetActive(false);
    }

    public void Init(CreatureInfo creature_info, short grade, bool is_block)
    {
        m_MyselfObj.SetActive(true);
        m_BlockObj.gameObject.SetActive(is_block);

        info = creature_info;
        this.grade = grade;
        if (info == null)
            return;

        gameObject.name = info.ID;
        string sprite_name = string.Format("cs_{0}", info.ID);
        if (m_CharacterSprite.atlas.Contains(sprite_name) == false)
            sprite_name = "cs_empty";

        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_CharacterSprite.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
        {
            if (sprite_name == "cs_empty")
                sp.y += (sp.height - sp.width)/2;
            sp.height = sp.width;
        }

        m_CharacterSprite.spriteName = new_sprite_name;
        m_CharacterType.spriteName = string.Format("New_hero_info_hero_type_{0}", info.ShowAttackType);

        m_CharacterBoarder.SetSpriteActive(info.TeamSkill != null);

        m_CharacterLabel.text = info.Name;

        for (int i = 0; i < m_Stars.Count; ++i)
            m_Stars[i].gameObject.SetActive(grade > i);
        m_GridStars.gameObject.SetActive(true);
        m_GridStars.Reposition();
    }

    public void OnClickCreature()
    {
        if (AssetManager.ContainsCharacterData(info.ID) == false)
        {
            Tooltip.Instance.ShowMessageKey("NotImplement");
            return;
        }
        Popup.Instance.Show(ePopupMode.BookCharacter, info.ID, grade);
        if (m_NotifyIcon.gameObject.activeSelf == true)
        {
            m_NotifyIcon.gameObject.SetActive(false);
        }
    }

    public void SetNotifyIcon(bool active)
    {
        m_NotifyIcon.gameObject.SetActive(active);
    }

}
