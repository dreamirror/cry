using UnityEngine;
using System.Collections;


public class BossSpot : MonoBehaviour {

    public UIToggleSprite m_ToggleAvailable;

    public MapStageDifficulty StageInfo { get; private set; }

    public UISprite[] m_DisableSprite;

    public GameObject Profile;
    public UILabel m_LevelText;
    public UISprite m_ProfileImage;

    public GameObject m_Level, m_Lock;

    readonly Color32 m_DisableColor = new Color32(150, 150, 150, 255);

    static public short CalculateLevel(short boss_level, MapStageDifficulty stage_info)
    {
        var clear_data = MapClearDataManager.Instance.GetData(stage_info);
        if (clear_data != null)
            boss_level = (short)(boss_level + clear_data.clear_count);
        return boss_level;
    }

    static public short CalculateGrade(short boss_level)
    {
        return (short)System.Math.Min(6, boss_level / 20 + 1);
    }

    static public short CalculateEnchant(short boss_level)
    {
        switch (boss_level % 10)
        {
            case 0:
                return 0;

            case 1:
            case 2:
                return 1;

            case 3:
            case 4:
                return 2;

            case 5:
            case 6:
                return 3;

            case 7:
            case 8:
                return 4;

            case 9:
                return 5;
        }
        return 0;
    }

    public bool Availble { get { return m_ToggleAvailable.ActiveSprite; } }
	// Use this for initialization
	void Start () {
	
	}

    MapCondition m_Condition = null;
    public void Init(MapStageDifficulty info)
    {
        StageInfo = info;

        MapCreatureInfo creature = StageInfo.Waves[0].Creatures.Find(c => c.CreatureType == eMapCreatureType.Boss);

        short level = CalculateLevel(creature.Level, StageInfo);
        m_LevelText.text = level.ToString();
        m_ProfileImage.spriteName = string.Format("profile_{0}", creature.CreatureInfo.ID);

        m_Condition = StageInfo.CheckCondition;
        bool is_lock = m_Condition != null;
        m_Level.gameObject.SetActive(!is_lock);
        m_Lock.gameObject.SetActive(is_lock);

        m_ToggleAvailable.SetSpriteActive(!is_lock);

        foreach (UISprite sprite in m_DisableSprite)
        {
            if (is_lock)
                sprite.color = m_DisableColor;
            else
                sprite.color = Color.white;
        }

        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        if (m_Condition != null)
        {
            Tooltip.Instance.ShowMessage(m_Condition.Condition);
            return;
        }
        if (StageInfo.Waves.Count > 0)
        {
            Popup.Instance.Show(ePopupMode.BossInfo,StageInfo);
        }
        else
            Tooltip.Instance.ShowMessageKey("NotImplement");
    }
}
