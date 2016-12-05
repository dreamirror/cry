using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System;

public class PopupLootCharacter : PopupBase
{
    public GameObject SkillItemPrefab;

    public UILabel m_LabelTitle, m_LabelDescription, m_TagLabel;
    public UICharacterContainer m_CharacterContainer;
    public UISprite m_CreatureType;

    public UIDisableButton m_btnLoot;

    public UIGrid m_GridStars;
    public UIToggleSprite[] m_Stars;
    public GameObject[] m_Skills;

    public UISprite m_TeamSkill;

    public UIParticleContainer m_ParticleCreatureSummon;

    public UIButton m_btnOK;

    public MeshRenderer m_BG;
    
    Creature m_Creature = null;

    // Use this for initialization
    void OnEnable()
    {
        Network.HideIndicator();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDragCharacter();
    }

    StoreItem m_StoreItem;
    bool m_IsMoveDetail = false;
    List<SkillItem> m_ListSkill = new List<SkillItem>();
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        long creature_idx = (long)parms[0];

        bool show_effect = false;
        if (parms.Length >= 2)
            m_IsMoveDetail = (bool)parms[1];

        if (parms.Length >= 3)
            show_effect = (bool)parms[2];

        if (parms.Length >= 4)
            m_StoreItem = parms[3] as StoreItem;
        else
            m_StoreItem = null;

        m_btnLoot.gameObject.SetActive(m_StoreItem != null);

        m_Creature = CreatureManager.Instance.GetInfoByIdx(creature_idx);
        m_BG.material.mainTexture = AssetManager.LoadBG("000_hero_loot");

        m_CharacterContainer.Init(AssetManager.GetCharacterAsset(m_Creature.Info.ID, m_Creature.SkinName), UICharacterContainer.Mode.UI_Normal, "win");
        m_CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);
        m_CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        m_LabelTitle.text = m_Creature.Info.Name;
        m_LabelDescription.text = m_Creature.Info.Desc;
        m_CreatureType.spriteName = string.Format("New_hero_info_type_{0}", m_Creature.Info.ShowAttackType);

        if (m_Creature.TeamSkill != null)
        {
            if (m_TeamSkill.atlas.Contains(m_Creature.TeamSkill.Info.ID) == true)
                m_TeamSkill.spriteName = m_Creature.TeamSkill.Info.ID;
            else
                m_TeamSkill.spriteName = "skill_default";
        }
        m_TeamSkill.transform.parent.parent.gameObject.SetActive(m_Creature.TeamSkill != null);

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

            if (m_Creature.Skills.Count > i)
                skill.Init(m_Creature.Skills[i]);
            else
                skill.gameObject.SetActive(false);
        }

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].SetSpriteActive(m_Creature.Grade > i);
            m_Stars[i].gameObject.SetActive(m_Creature.Grade > i);
        }
        m_GridStars.gameObject.SetActive(true);
        m_GridStars.Reposition();

        m_CharacterContainer.gameObject.SetActive(true);
        if (show_effect == true)
            m_ParticleCreatureSummon.Play();

        m_TagLabel.text = string.Empty;
        var tags = m_Creature.Info.CreatureTags;
        foreach (string tag in tags)
            m_TagLabel.text += string.Format("[url={0}]{1}[/url] ","Tag_"+tag, Localization.Get("Tag_" + tag));
    }
    /////////////////////////////////////////////////

    override public void OnClose()
    {
        m_CharacterContainer.gameObject.SetActive(false);
        parent.Close(true);

        if (m_IsMoveDetail == true)
        {
            MenuParams menu = new MenuParams();
            menu.AddParam<Creature>(m_Creature);
            bool bShowChangeHeroButton = false;
            menu.AddParam("bShowChangeHeroButton", bShowChangeHeroButton);

            GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
        }
    }

    public void OnLoot()
    {
        parent.Close(true, true);
        m_StoreItem.OnLootMore();
    }

    public void OnShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_Creature.TeamSkill.GetTooltip(), tooltip);
    }
    public void OnShowTooltipType(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(Localization.Get(string.Format("{0}", m_Creature.Info.ShowAttackType)), tooltip);
    }
    public void OnClickUserEval()
    {
        GameMain.Instance.MoveEvalMenu(m_Creature.Info.ID);
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
