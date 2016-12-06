using UnityEngine;
using System.Collections;
using SharedData;

//这个类主要用来维护主界面角色的信息
public class UICharacterInfo : MonoBehaviour //角色的信息的类
{
    Color[] m_level_color = //不同等级的颜色数组
    {
        new Color(70f/255f,200f/255f,1f)
        ,new Color(233f/255f,200f/255f,109f/255f)
    };
    public GameObject m_Warning;
    public GameObject m_Notify;
    public UILabel m_LabelLevel, m_LabelEnchant;
    public UIGrid m_GridStars;
    public GameObject[] m_Stars;

    bool bActiveGrade = true;
    public bool IsWarning { get; private set; }

    void Update()
    {
        if (bActiveGrade == false)
        {
            m_GridStars.gameObject.SetActive(true);
            bActiveGrade = true;
            m_GridStars.Reposition();
        }

    }

    public void UpdateInfo(Creature creature, int index)
    {
        if(creature == null)
        {
            gameObject.SetActive(false); //激活
            return;
        }
        gameObject.SetActive(true);
        for (int i = 0; i < m_Stars.Length; ++i)
            m_Stars[i].SetActive(i < creature.Grade);
        m_GridStars.gameObject.SetActive(false);
        bActiveGrade = false;

        if (creature.TeamSkill != null)
            m_LabelLevel.color = m_level_color[0];
        else
            m_LabelLevel.color = m_level_color[1];

        m_LabelLevel.text = creature.GetLevelText();
        m_LabelEnchant.text = creature.GetEnchantText();

        m_Notify.SetActive(creature.IsNotify);

        IsWarning = index == 0 && creature.Info.Position != eCreaturePosition.front;
        m_Warning.SetActive(IsWarning);
    }

}
