using UnityEngine;
using System.Collections;

public class DungeonMonster : MonoBehaviour
{
    public UIGrid m_Grid;
    public UISprite[] m_Stars;
    public UISprite m_icon, m_type, m_Elite, m_Boss;
    public UILabel m_level;

    public UIToggleSprite character_border;

    MapCreatureInfo CreatureInfo { get; set; }
    // Use this for initialization
    void Start()
    {
    }

    //---------------------------------------------------------------------------
    public void Init(MapStageDifficulty stage_info, MapCreatureInfo info)
    {
        short level = info.Level;
        short grade = info.Grade;

        if (info.CreatureType == eMapCreatureType.Boss && stage_info.MapInfo.MapType == "boss")
        {
            level = Boss.CalculateLevel(level, stage_info);
            grade = Boss.CalculateGrade(level);
        }

        CreatureInfo = info;
        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].gameObject.SetActive(i<grade);
        }
        m_Grid.Reposition();

        InitCreatureInfo(info.CreatureInfo);

        m_level.text = level.ToString();
        m_Elite.gameObject.SetActive(info.CreatureType == eMapCreatureType.Elite);
        m_Boss.gameObject.SetActive(info.CreatureType == eMapCreatureType.Boss);
        gameObject.SetActive(true);

    }
    public void Init(Creature creature)
    {
        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].gameObject.SetActive(i < creature.Grade);
        }
        m_Grid.Reposition();

        InitCreatureInfo(creature.Info);

        m_level.text = creature.Level.ToString();
        m_Elite.gameObject.SetActive(false);
        m_Boss.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
    void InitCreatureInfo(CreatureInfo info)
    {
        gameObject.name = info.ID;
        string sprite_name = string.Format("cs_{0}", info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_icon.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_icon.spriteName = new_sprite_name;
        m_type.spriteName = string.Format("New_hero_info_hero_type_{0}", info.ShowAttackType);

        character_border.SetSpriteActive(info.TeamSkill != null);
    }
    //---------------------------------------------------------------------------
    public void OnShowTooltip(SHTooltip tooltip)
    {
        if(CreatureInfo != null)
            Tooltip.Instance.ShowTarget(CreatureInfo.CreatureInfo.GetTooltip(), tooltip);
    }

}
