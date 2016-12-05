using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class CharacterBuffContainer : AssetContainer<CharacterBuff>
{
    static AssetData s_Data;
    static AssetData static_Data
    {
        get
        {
            if (s_Data == null)
                s_Data = new AssetData(Resources.Load<CharacterBuff>("Prefab/Battle/CharacterBuff").gameObject);
            return s_Data;
        }
    }

    public CharacterBuffContainer()
        : base(static_Data)
    {
    }
}

public enum eBuffColorType
{
    None,
    Buff,
    DeBuff,
    Shield,
    Immune,
    Aggro,
    Mana,
    Stun,
}

public class CharacterBuff : MonoBehaviour, IAssetObject
{
    public Image m_BackLine, m_BackLineDanger;
    public Image m_Cool;
    public UGUISprite m_Icon;

    static public Color GetColor(eBuffColorType buff_color_type)
    {
        switch (buff_color_type)
        {
            case eBuffColorType.Buff:
                return BattleBase.Instance.color_container.GetColor("ui_buff");

            case eBuffColorType.DeBuff:
                return BattleBase.Instance.color_container.GetColor("ui_debuff");

            case eBuffColorType.Shield:
                return BattleBase.Instance.color_container.GetColor("ui_shield");

            case eBuffColorType.Immune:
                return BattleBase.Instance.color_container.GetColor("ui_immune");

            case eBuffColorType.Aggro:
                return BattleBase.Instance.color_container.GetColor("ui_aggro");

            case eBuffColorType.Mana:
                return BattleBase.Instance.color_container.GetColor("ui_mana");

            case eBuffColorType.Stun:
                return BattleBase.Instance.color_container.GetColor("ui_stun");

            default:
                return Color.white;
        }
    }

    public void Init(string icon_id, eBuffColorType buff_color_type, bool show_cool)
    {
        m_Icon.spriteName = icon_id;
        m_Cool.fillAmount = 0f;
        m_BackLine.color = GetColor(buff_color_type);
        m_BackLine.fillAmount = 1f;
        m_BackLineDanger.fillAmount = 1f;
        m_Cool.gameObject.SetActive(show_cool);
    }

    public void OnAlloc()
    {
    }

    public void OnFree()
    {
    }

    public void InitPrefab()
    {
    }

    public void OnUpdate(float percent, float affect_percent)
    {
        if (percent < 0f)
            m_Cool.gameObject.SetActive(false);
        else
        {
            m_Cool.gameObject.SetActive(true);
            m_Cool.fillAmount = percent;
        }
        m_BackLine.fillAmount = affect_percent;

        float delta = Time.deltaTime*2f;

        if (m_BackLineDanger.fillAmount < affect_percent)
        {
            m_BackLineDanger.fillAmount = Mathf.Min(m_BackLineDanger.fillAmount + delta, affect_percent);
        }
        else
        {
            m_BackLineDanger.fillAmount = Mathf.Max(m_BackLineDanger.fillAmount - delta, affect_percent);
        }
    }
}
