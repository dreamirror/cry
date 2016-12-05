using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class CharacterSkill : MonoBehaviour
{
    public TextAnimation.AnimationUnit Position, Scale, Alpha;

    [Serializable]
    public class _SkillName
    {
        public GameObject obj;
        public Image bg;
        public Text text;
    }
    public _SkillName[] m_SkillNames;

    public Transform AttachTransform { get; private set; }

    public bool IsPlaying { get; private set; }
    public float PlaybackTime { get; set; }
    public float Depth = -1f;
    float m_Scale = 1f;
    Vector3 m_DefaultPosition;

    _SkillName m_CurrentSkill;

    public float CurrentPosition
    {
        get
        {
            return Position.Evaluate(PlaybackTime);
        }
    }

    public void Init(Transform attach_transform, bool is_team)
    {
        AttachTransform = attach_transform;
        transform.localScale = Vector3.one;

        m_DefaultPosition = transform.localPosition;
        gameObject.SetActive(false);
        if (is_team == false)
            m_CurrentSkill = m_SkillNames[0];
        else
            m_CurrentSkill = m_SkillNames[1];
        m_CurrentSkill.obj.SetActive(true);
    }

    void Update()
    {
        if (IsPlaying == false)
            return;

        PlaybackTime += Time.deltaTime;

        Sample();
    }

    public void Sample()
    {
        if (IsPlaying == false)
            return;

        transform.position = new Vector3(0f, CurrentPosition - Depth*0.15f, Depth) + AttachTransform.position + m_DefaultPosition * m_Scale;

        Vector3 scale = Vector3.one * Scale.Evaluate(PlaybackTime);
        scale.z = 1f;
        transform.localScale = scale;

        byte alpha = (byte)(255 * Alpha.Evaluate(PlaybackTime));

        Color32 color = m_CurrentSkill.text.color;
        color.a = alpha;
        m_CurrentSkill.text.color = color;

        color = m_CurrentSkill.bg.color;
        color.a = alpha;
        m_CurrentSkill.bg.color = color;

        IsPlaying = PlaybackTime < Position.Length || PlaybackTime < Scale.Length || PlaybackTime < Alpha.Length;

        if (IsPlaying == false)
        {
            gameObject.SetActive(false);
        }
    }

    public void Show(string skill_name, float scale)
    {
        PlaybackTime = 0f;
        m_CurrentSkill.text.text = skill_name;
        IsPlaying = true;
        m_Scale = scale;
        gameObject.SetActive(true);
        Sample();
    }
}
