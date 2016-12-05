using UnityEngine;
using System.Collections.Generic;


public class TeamLevelUp : MonoBehaviour
{
    static TeamLevelUp m_Instance = null;
    static public TeamLevelUp Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = ((GameObject)GameObject.Instantiate(Resources.Load("Prefab/Menu/TeamLevelUp"))).GetComponent<TeamLevelUp>();
                GameObject.DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    public GameObject contents;

    public UILabel[] m_LabelCurrent;
    public UILabel[] m_LabelChanged;

    string whiteFormat = "{0}";
    string greenFormat = "[A5EE22]{0}[-]";
    public void Show(EventParamLevelUp param)
    {
        gameObject.SetActive(true);

        m_LabelCurrent[0].text = string.Format(whiteFormat, param.old_level);
        m_LabelCurrent[1].text = string.Format(whiteFormat, LevelInfoManager.Instance.GetEnergyMax(param.old_level));
        m_LabelCurrent[2].text = string.Format(whiteFormat, param.old_energy);


        m_LabelChanged[0].text = string.Format(greenFormat, param.new_level);

        if (LevelInfoManager.Instance.GetEnergyMax(param.old_level) == LevelInfoManager.Instance.GetEnergyMax(param.new_level))
            m_LabelChanged[1].text = string.Format(whiteFormat, LevelInfoManager.Instance.GetEnergyMax(param.new_level));
        else
            m_LabelChanged[1].text = string.Format(greenFormat, LevelInfoManager.Instance.GetEnergyMax(param.new_level));

        //if (LevelInfoManager.Instance.GetEnergyBonus(param.old_level) == LevelInfoManager.Instance.GetEnergyBonus(param.new_level))
        //    m_LabelChanged[2].text = string.Format(whiteFormat, LevelInfoManager.Instance.GetEnergyBonus(param.new_level));
        //else
            m_LabelChanged[2].text = string.Format(greenFormat, param.new_energy);

        GameMain.Instance.UpdatePlayerInfo();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
