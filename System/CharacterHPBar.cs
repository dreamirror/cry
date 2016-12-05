using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterHPBar : MonoBehaviour
{
    public GameObject BuffIndicator;
    public Vector3 position;
    BattleCreature m_Creature;

    public Image m_HP, m_HPDanger, m_MP, m_WarningImage, m_ManaFill;
    public float TimeScale = 1f, DangerScale = 1f, DangerDelay = 0.5f, WarningDuration = 1f, ManaFillRotate = 360f;
    public bool m_LowHPWarningEnable = false;

    public Color m_WarningDefaultColor = new Color(0.5f, 0f, 0f, 1f), m_WarningColor = new Color(1f, 0f, 0f, 1f);
    float hp_fill_time = 0f;
    float m_Scale = 1f;

    public void Init(BattleCreature creature, bool reverse, float scale)
    {
        m_Creature = creature;
        m_Scale = scale;
        m_MP.fillAmount = 0f;
        transform.localScale = Vector3.one * scale;
        if (reverse == true)
        {
            m_HP.fillOrigin = (int)Image.OriginHorizontal.Right;
            m_HPDanger.fillOrigin = (int)Image.OriginHorizontal.Right;
            m_MP.fillOrigin = (int)Image.OriginHorizontal.Right;

            m_ManaFill.transform.parent.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
            m_ManaFill.transform.parent.localScale = Vector3.one;
    }

    bool m_DangerAnimation = false;

    bool bWarningInc = false;
    void Update()
    {
        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        if (m_Creature == null || m_Creature.Character == null)
            return;

        transform.position = m_Creature.Character.transform.position + position * m_Scale;

        float delta = Time.deltaTime * TimeScale;
        float hp_percent = m_Creature.Stat.HPPercent;
        float time = Time.time;

//        if (m_ManaFill.gameObject.activeInHierarchy != m_Creature.IsManaFill)
        m_ManaFill.gameObject.SetActive(m_Creature.IsManaFill && BattleBase.Instance.IsBattleEnd == false && BattleBase.Instance.IsBattleStart == true);
        if (m_Creature.IsManaFill)
            m_ManaFill.transform.localRotation *= Quaternion.Euler(0f, 0f, -delta * ManaFillRotate);

        if (m_HP.fillAmount < hp_percent)
        {
            m_HP.fillAmount = Mathf.Min(m_HP.fillAmount + delta, hp_percent);
            m_HPDanger.fillAmount = Mathf.Max(m_HP.fillAmount, m_HPDanger.fillAmount);
        }
        else if (m_HP.fillAmount > hp_percent)
        {
            m_HP.fillAmount = Mathf.Max(m_HP.fillAmount - delta, hp_percent);
            if (hp_fill_time == 0f)
                hp_fill_time = time;
        }

        if (m_HPDanger.fillAmount == hp_percent)
        {
            m_DangerAnimation = false;
            hp_fill_time = 0f;
        }
        else if (m_HPDanger.fillAmount > hp_percent && (m_DangerAnimation == true || hp_fill_time != 0f && time - hp_fill_time > DangerDelay))
        {
            m_DangerAnimation = true;
            m_HPDanger.fillAmount = Mathf.Max(m_HPDanger.fillAmount - delta * DangerScale, hp_percent);
        }

        float mp_percent = m_Creature.Stat.MPPercent;
        if (m_MP.fillAmount < mp_percent)
        {
            m_MP.fillAmount = Mathf.Min(m_MP.fillAmount + delta, mp_percent);
        }
        else
        {
            m_MP.fillAmount = Mathf.Max(m_MP.fillAmount - delta, mp_percent);
        }

        if (m_Creature.IsDead && m_HP.fillAmount == 0f && m_HPDanger.fillAmount == 0f && m_MP.fillAmount == 0f)
        {
            gameObject.SetActive(false);
        }

        if (m_LowHPWarningEnable == true && m_HP.fillAmount < m_Creature.LowHP)
        {
            if (m_WarningImage.enabled == false)
            {
                m_WarningImage.enabled = true;
                m_WarningImage.color = m_WarningDefaultColor;
            }

            Color c = m_WarningImage.color;
            if (c.r == m_WarningDefaultColor.r)
                bWarningInc = true;
            else if (c.r == m_WarningColor.r)
                bWarningInc = false;

            delta = (m_WarningColor.r - m_WarningDefaultColor.r) / (WarningDuration / Time.deltaTime);
            if (bWarningInc)
            {
                c.r = Mathf.Min(c.r + delta, m_WarningColor.r);
                c.g = c.b = 0f;
                c.a = 1f;
            }
            else
            {
                c.r = Mathf.Max(c.r - delta, m_WarningDefaultColor.r);
                c.g = c.b = 0f;
                c.a = 1f;
            }

            m_WarningImage.color = c;
        }
        else
            m_WarningImage.enabled = false;
            
    }

    public void AddBuff(CharacterBuff character_buff)
    {
        character_buff.transform.SetParent(BuffIndicator.transform, false);
        SortBuffs(null);
    }

    public void RemoveBuff(CharacterBuff character_buff)
    {
        SortBuffs(character_buff);
    }

    void SortBuffs(CharacterBuff character_buff)
    {
        float Gap = 26f;
        Vector3 pos = Vector3.zero;

        for (int i = 0, index = 0; i < BuffIndicator.transform.childCount; ++i)
        {
            var go = BuffIndicator.transform.GetChild(i).gameObject;

            if (go.activeSelf && (character_buff == null || go != character_buff.gameObject))
            {
                pos.x = (index % 3) * Gap;
                pos.y = (index / 3) * Gap;
                go.transform.localPosition = pos;

                ++index;
            }
        }
    }
}
