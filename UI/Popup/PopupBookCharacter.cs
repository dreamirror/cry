using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System;

public class PopupBookCharacter : PopupBase
{
    public GameObject SkillItemPrefab;

    public UILabel m_LabelTitle, m_LabelDescription, m_TagLabel;
    public UICharacterContainer m_CharacterContainer;
    public UISprite m_CreatureType;
    
    public UIGrid m_GridStars;
    public UIToggleSprite[] m_Stars;
    public GameObject[] m_Skills;

    public UISprite m_TeamSkill;

    //    public TweenScale m_StarAni;

    public UIButton m_btnOK;

    public MeshRenderer m_BG;

    // Use this for initialization
    void Start()
    {
    }

    void OnEnable()
    {
        Network.HideIndicator();
        //m_NextUpdateCharacterTime = Time.time + 6f;
    }

    //float m_NextUpdateCharacterTime = 0f;
    // Update is called once per frame
    void Update()
    {
        UpdateDragCharacter();
    }
    
    CreatureInfo m_CreatureInfo = null;
    short creature_grade;
    List<SkillItem> m_ListSkill = new List<SkillItem>();
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        string creature_id = parms[0].ToString();
        creature_grade = (short)parms[1];

        string spriteName = "000_hero_book";
        Texture2D sp = AssetManager.LoadBG(spriteName);
        m_BG.material.mainTexture = sp;

        m_CreatureInfo = CreatureInfoManager.Instance.GetInfoByID(creature_id);
        m_CharacterContainer.Init(AssetManager.GetCharacterAsset(m_CreatureInfo.ID, "default"), UICharacterContainer.Mode.UI_Normal, "win");
        m_CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);
        m_CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        m_LabelTitle.text = m_CreatureInfo.Name;
        m_LabelDescription.text = m_CreatureInfo.Desc;
        m_CreatureType.spriteName = string.Format("New_hero_info_type_{0}", m_CreatureInfo.ShowAttackType);

        if (m_CreatureInfo.TeamSkill != null)
        {
            if (m_TeamSkill.atlas.Contains(m_CreatureInfo.TeamSkill.ID) == true)
                m_TeamSkill.spriteName = m_CreatureInfo.TeamSkill.ID;
            else
                m_TeamSkill.spriteName = "skill_default";
        }
        m_TeamSkill.transform.parent.parent.gameObject.SetActive(m_CreatureInfo.TeamSkill != null);

        for (int i = 1; i <= m_Skills.Length; ++i)
        {
            SkillItem skill;

            if (m_ListSkill.Count > i - 1)
                skill = m_ListSkill[i - 1];
            else
            {
                skill = NGUITools.AddChild(m_Skills[i - 1], SkillItemPrefab).GetComponent<SkillItem>();
                m_ListSkill.Add(skill);
            }

            if (m_CreatureInfo.Skills.Count > i)
                skill.Init(m_CreatureInfo.Skills[i]);
            else
                skill.gameObject.SetActive(false);
        }

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].SetSpriteActive(creature_grade > i);
            m_Stars[i].gameObject.SetActive(creature_grade > i);
        }
        m_GridStars.gameObject.SetActive(true);
        m_GridStars.Reposition();

        m_CharacterContainer.gameObject.SetActive(true);

        m_TagLabel.text = string.Empty;
        var tags = m_CreatureInfo.CreatureTags;
        foreach (string tag in tags)
            m_TagLabel.text += string.Format("[url={0}]{1}[/url] ", "Tag_" + tag, Localization.Get("Tag_" + tag));
    }
    /////////////////////////////////////////////////

    override public void OnClose()
    {
        m_CharacterContainer.gameObject.SetActive(false);
        parent.Close(true);
    }

    public void OnShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_CreatureInfo.TeamSkill.GetTooltip(), tooltip);
    }
    public void OnShowTooltipType(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(Localization.Get(string.Format("{0}", m_CreatureInfo.ShowAttackType)), tooltip);
    }

    public void OnCharacterPress()
    {
        m_FirstTouchPosition = m_TouchPosition = UICamera.lastTouchPosition;
        IsDraggingCharacter = true;
    }

    public void OnCharacterRelease()
    {
        if (m_FirstTouchPosition == UICamera.lastTouchPosition)
            m_CharacterContainer.PlayRandomAction();
        m_TouchPosition = Vector2.zero;
        IsDraggingCharacter = false;
    }

    public void OnClickEval()
    {
        GameMain.Instance.MoveEvalMenu(m_CreatureInfo.ID);
    }

    bool IsDraggingCharacter = false;
    Vector2 m_TouchPosition = Vector2.zero, m_FirstTouchPosition = Vector2.zero;
    void UpdateDragCharacter()
    {
        if (IsDraggingCharacter == false)
            return;

        Vector2 pos = UICamera.lastTouchPosition;
        float delta = m_TouchPosition.x - pos.x;
        float speed = 0.5f;
        m_TouchPosition = pos;

        m_CharacterContainer.transform.localRotation *= Quaternion.Euler(0f, delta * speed, 0f);

    }
}
