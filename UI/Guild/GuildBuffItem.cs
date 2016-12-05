using UnityEngine;

public class GuildBuffItem : MonoBehaviour
{
    public UILabel m_LabelLevel;
    public UILabel m_LabelExp;
    public UILabel m_LabelBuff;
    public GameObject m_Disable;

    public void Init(short level, bool isActive = false)
    {
        m_LabelLevel.text = level.ToString();
        m_LabelExp.text = Localization.Format("GuildNextRequiredExp", GuildInfoManager.Config.RequiredExp(level));
        m_LabelBuff.text = GuildInfoManager.Config.GuildBuffString(1, level);
        var buff2 = GuildInfoManager.Config.GuildBuffString(2, level);
        if (string.IsNullOrEmpty(buff2) == false)
            m_LabelBuff.text += string.Format(" / {0}",buff2);

        m_Disable.SetActive(!isActive);
    }
}
