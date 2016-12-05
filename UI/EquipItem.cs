using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class EquipItem : MonoBehaviour
{
    public UISprite m_icon;
    public UILabel m_labelEnchant;
    public UILabel m_LabelGrade;

    // Use this for initialization
    void Start()
    {
    }

    Equip m_Equip;
    //---------------------------------------------------------------------------
    public void Init(Equip equip)
    {
        m_Equip = equip;
        m_icon.spriteName = equip.Info.IconID;
        if (equip.EnchantLevel > 0)
            m_labelEnchant.text = string.Format("[99FF99]+{0}", equip.EnchantLevel);
        m_labelEnchant.gameObject.SetActive(equip.EnchantLevel > 0);

        m_LabelGrade.text = equip.Info.Grade.ToString();

        gameObject.SetActive(true);
    }
    //---------------------------------------------------------------------------

    public void OnTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_Equip.GetTooltip(), tooltip);
    }
}
