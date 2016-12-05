using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Serialization;
using PacketInfo;

public class HeroInfoDetail : MenuBase
{
    public GameObject m_ButtonLeft, m_ButtonRight;
    public UISprite m_LeftIcon, m_RightIcon;
    //Left Menu
    ////////////////////////////////////////////////////////////////////
    public UIGrid m_GridStars;
    public UIToggleSprite[] m_Stars;
    public UILabel m_LabelName;
    public UILabel m_LeftLabelLevel, m_LabelLevel;
    public UICharacterContainer m_CharacterContainer;
    public UISprite m_CreatureType;
    public UISprite m_TeamSkill;
    public UIToggleSprite m_SpriteLocked;
    public UILabel m_LabelLock;
    //public SHTooltip m_TooltipLeaderSkill;
    //public SHTooltip m_TooltipType;
    ////////////////////////////////////////////////////////////////////

    //Right Menu
    ////////////////////////////////////////////////////////////////////
    //hero info
    public UIParticleContainer m_LevelupReady, m_EnchantReady, m_EvolveReady, m_EquipReady, m_SkillReady, m_RuneReady;
    public UIParticleContainer m_ParticleCreatureEvolve, m_ParticleEquipUpgrade, m_ParticleLevelUp;

    //btns
    public UILabel m_EnchantBtnLabel;
    ////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////
    //stat
    public UILabel m_LabelHeroStat;
    ////////////////////////////////////////////////////////////////////

    // equip
    public GameObject EquipItemPrefab;
    public GameObject m_WeaponIndicator, m_ArmorIndicator;
    EquipItem m_Weapon, m_Armor;
    public EquipParts m_WeaponParts, m_ArmorParts;

    public GameObject m_WeaponEnchantable;
    public GameObject m_ArmorEnchantable;

    // rune
    public UIGrid m_GridRune;
    public PrefabManager RuneItemPrefab;
    // skill
    public SkillItem[] m_ActiveSkills, m_PassiveSkills;

    public GameObject m_HeroEnchantEvent;
    public GameObject m_HeroEvolveEvent;
    public GameObject m_RuneEvent;
    public GameObject m_SkillEnchantEvent;


    Creature m_Creature = null;

    List<Creature> m_Creatures = null;

    MenuParams m_parms;
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        m_parms = parms;
        //if(m_Creature == null || parms.bBack == false)
            m_Creature = parms.GetObject<Creature>();

        if (parms.bBack == true && CreatureManager.Instance.Contains(m_Creature) == false)
            return false;

        m_Creatures = parms.GetObject<List<Creature>>("Creatures");
        if (m_Creatures != null)
            m_Creatures = m_Creatures.Where(c => CreatureManager.Instance.Contains(c)).ToList();
        parms.AddParam("Creatures", m_Creatures);

        Init();
        return true;
    }
    public override bool Uninit(bool bBack = true)
    {
        RuneItemPrefab.Clear();
        m_RuneList.Clear();
        return base.Uninit(bBack);
    }
    override public void UpdateMenu()
    {
        Init();
    }

    void Update()
    {
        if (IsDraggingCharacter)
            UpdateDragCharacter();
    }

    ////////////////////////////////////////////////////////////////
    void Start()
    {
        //m_TooltipLeaderSkill.OnShowTooltip = OnShowTooltip;
        //m_TooltipLeaderSkill.span_press_time = TimeSpan.FromMilliseconds(0);

        //m_TooltipType.OnShowTooltip = OnShowTooltipType;
        //m_TooltipType.span_press_time = TimeSpan.FromMilliseconds(0);
    }
    
    void UpdateLock()
    {
        m_SpriteLocked.SetSpriteActive(m_Creature.IsLock);
        m_LabelLock.text = Localization.Get(m_Creature.IsLock ? "UnLock" : "Lock");
    }
    void UpdateEventIcon()
    {
        m_HeroEnchantEvent.SetActive(EventHottimeManager.Instance.IsHeroEchantEvent);
        m_HeroEvolveEvent.SetActive(EventHottimeManager.Instance.IsHeroEvolveEvent);
        m_SkillEnchantEvent.SetActive(EventHottimeManager.Instance.IsSkillEvent);
        m_RuneEvent.SetActive(EventHottimeManager.Instance.IsRuneEvent);
    }
    public void Init()
    {
        UpdateEventIcon();
        m_EnchantBtnLabel.text = (m_Creature.Grade >= 6 && m_Creature.Enchant >= 5) ? Localization.Get("OverEnchant") : Localization.Get("Enchant");
        //m_CreatureType.spriteName = string.Format("hero_info_type_{0}", m_Creature.Info.ShowAttackType);
        m_CreatureType.spriteName = string.Format("New_hero_info_type_{0}", m_Creature.Info.ShowAttackType);
        m_LabelName.text = m_Creature.Info.Name;
        if (m_Creature.Enchant > 0)
            m_LabelName.text += " " + m_Creature.GetEnchantText();
        m_LeftLabelLevel.text = m_Creature.GetLevelText();
        m_LabelLevel.text = Localization.Format("HeroLevelDesc", m_Creature.Level, m_Creature.LevelLimit);

        UpdateLock();

        m_CharacterContainer.Init(AssetManager.GetCharacterAsset(m_Creature.Info.ID, m_Creature.SkinName), UICharacterContainer.Mode.UI_Normal, "win");
        m_CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        m_CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);

        if (m_Creature.Info.TeamSkill != null)
        {
            if (m_TeamSkill.atlas.Contains(m_Creature.TeamSkill.Info.ID) == true)
                m_TeamSkill.spriteName = m_Creature.TeamSkill.Info.ID;
            else
                m_TeamSkill.spriteName = "skill_default";
            m_TeamSkill.gameObject.transform.parent.parent.gameObject.SetActive(true);
        }
        else
            m_TeamSkill.gameObject.transform.parent.parent.gameObject.SetActive(false);

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].SetSpriteActive(m_Creature.Grade > i);
            m_Stars[i].gameObject.SetActive(m_Creature.Grade > i);
        }
        m_GridStars.Reposition();

        // equips
        if (m_Weapon == null) m_Weapon = NGUITools.AddChild(m_WeaponIndicator, EquipItemPrefab).GetComponent<EquipItem>();
        if (m_Armor == null) m_Armor = NGUITools.AddChild(m_ArmorIndicator, EquipItemPrefab).GetComponent<EquipItem>();
        m_Weapon.Init(m_Creature.Weapon);
        //m_WeaponParts.Init(m_Creature.Weapon);

        m_Armor.Init(m_Creature.Armor);
        //m_ArmorParts.Init(m_Creature.Armor);


        // skills
        List<Skill> active_skills = m_Creature.GetSkillsByType(eSkillType.active);
        for (int i = 0; i < m_ActiveSkills.Length; ++i)
        {
            bool active = i < active_skills.Count;
            m_ActiveSkills[i].gameObject.SetActive(active);
            if (active)
                m_ActiveSkills[i].Init(active_skills[i]);
        }

        List<Skill> passive_skills = m_Creature.GetSkillsByType(eSkillType.passive);
        for (int i = 0; i < m_PassiveSkills.Length; ++i)
        {
            bool active = i < passive_skills.Count;
            m_PassiveSkills[i].gameObject.SetActive(active);
            if (active)
                m_PassiveSkills[i].Init(passive_skills[i]);
        }


        bool move = m_Creatures != null && m_Creatures.Count > 0;
        m_ButtonLeft.SetActive(move);
        m_ButtonRight.SetActive(move);
        if(move)
            UpdateNextCreatureIcon();

        for (int i = 0; i < 10; ++i)
        {
            if (m_RuneList.Count <= i)
            {
                RuneItem item = RuneItemPrefab.GetNewObject<RuneItem>(m_GridRune.transform, Vector3.zero);
                m_RuneList.Add(item);
            }

            if (m_Creature.Runes.Count <= i)
                m_RuneList[i].Init(null, m_Creature.RuneSlotCount <= i, null, i);
            else
                m_RuneList[i].Init(m_Creature.Runes[i], false, null, i);
        }
        m_GridRune.Reposition();

        InitHeroInfo();
        InitSkillInfo();
        InitRuneInfo();
        InitEvolveInfo();
    }
    public List<RuneItem> m_RuneList = new List<RuneItem>();
    void InitHeroInfo()
    {
        m_LevelupReady.gameObject.SetActive(m_Creature.AvailableLevelup);

        m_LabelHeroStat.text = m_Creature.GetStatString(true);

        UpdateEquipNotify();
    }

    void InitSkillInfo()
    {
        m_SkillReady.gameObject.SetActive(m_Creature.AvailableSkillEnchant);
    }

    void InitRuneInfo()
    {
        m_RuneReady.gameObject.SetActive(m_Creature.RuneSlotCount > m_Creature.Runes.Count && RuneManager.Instance.Runes.Exists(r => r.CreatureIdx == 0));
    }

    void InitEvolveInfo()
    {
        m_EvolveReady.gameObject.SetActive(HeroEvolve.CanEvolve(m_Creature) == true && CreatureManager.Instance.Creatures.Exists(c => HeroEvolve.IsEvolveMaterial(m_Creature, c)));
    }

    public void OnClickLevelUP()
    {
        Popup.Instance.Show(ePopupMode.CharacterLevelup, m_Creature);
    }

    void OnSkillEnchantCallback()
    {
        m_LabelHeroStat.text = m_Creature.GetStatString(true);

        m_SkillReady.gameObject.SetActive(m_Creature.AvailableSkillEnchant);
        m_ParticleCreatureEvolve.Play();
    }

    void OnEquipEnchantCallback()
    {
        UpdateEquipNotify();

        m_ParticleEquipUpgrade.Play();
    }

    private void UpdateEquipNotify()
    {
        m_EquipReady.gameObject.SetActive(m_Creature.Weapon.IsNotify || m_Creature.Armor.IsNotify);
        m_WeaponEnchantable.SetActive(m_Creature.Weapon.IsNotify);
        m_ArmorEnchantable.SetActive(m_Creature.Armor.IsNotify);
        m_WeaponEnchantable.GetComponent<UIPlayTween>().Play(true);
    }

    public void OnClickEvolve()
    {
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(m_Creature);
        menu.AddParam("show_evolve", m_EvolveReady.gameObject.activeSelf);
        GameMain.Instance.ChangeMenu(GameMenu.HeroEvolve, menu);
    }

    void UpdateNextCreatureIcon()
    {
        //Left
        int cur_idx = m_Creatures.FindIndex(c => c == m_Creature);
        int next_idx = cur_idx == 0 ? -1 : m_Creatures.FindLastIndex(cur_idx - 1, c => c.Grade > 0);
        if (next_idx == -1)
        {
            next_idx = m_Creatures.FindLastIndex(c => c.Grade > 0);
            if (next_idx == -1)
                return;
        }
        SetCharacterIcon(m_LeftIcon, m_Creatures[next_idx]);

        //Right
        next_idx = m_Creatures.FindIndex(cur_idx + 1, c => c.Grade > 0);
        if (next_idx == -1)
        {
            next_idx = m_Creatures.FindIndex(c => c.Grade > 0);
            if (next_idx == -1)
                return;
        }

        SetCharacterIcon(m_RightIcon, m_Creatures[next_idx]);
    }

    void SetCharacterIcon(UISprite character, Creature creature)
    {
        string sprite_name = string.Format("cs_{0}", creature.Info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = character.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;
        character.spriteName = new_sprite_name;
    }
    public void OnLeft()
    {
        int cur_idx = m_Creatures.FindIndex(c => c == m_Creature);
        int next_idx = cur_idx==0?-1: m_Creatures.FindLastIndex(cur_idx - 1, c => c.Grade > 0);
        if (next_idx == -1)
        {
            next_idx = m_Creatures.FindLastIndex(c => c.Grade > 0);
            if (next_idx == -1)
                return;
        }

        m_Creature = m_Creatures[next_idx];
        m_parms.AddParam<Creature>(m_Creature);
        Init();
    }

    public void OnRight()
    {
        int cur_idx = m_Creatures.FindIndex(c => c == m_Creature);
        int next_idx = m_Creatures.FindIndex(cur_idx + 1, c => c.Grade > 0);
        if (next_idx == -1)
        {
            next_idx = m_Creatures.FindIndex(c => c.Grade > 0);
            if (next_idx == -1)
                return;
        }

        m_Creature = m_Creatures[next_idx];
        m_parms.AddParam<Creature>(m_Creature);

        Init();
    }

    public void OnClickClose()
    {
        GameMain.Instance.BackMenu();
    }

    public void OnCharacterPress()
    {
        m_FirstTouchPosition = m_TouchPosition = UICamera.lastTouchPosition;
        IsDraggingCharacter = true;
    }

    public void OnCharacterRelease()
    {
        if (m_FirstTouchPosition == UICamera.lastTouchPosition)
        {
            m_CharacterContainer.PlayRandomAction();
        }
        m_TouchPosition = Vector2.zero;
        IsDraggingCharacter = false;
    }

    bool IsDraggingCharacter = false;
    Vector2 m_TouchPosition = Vector2.zero, m_FirstTouchPosition = Vector2.zero;
    void UpdateDragCharacter()
    {
        Vector2 pos = UICamera.lastTouchPosition;
        float delta = m_TouchPosition.x - pos.x;
        float speed = 0.5f;
        m_TouchPosition = pos;

        m_CharacterContainer.transform.localRotation *= Quaternion.Euler(0f, delta * speed, 0f);

    }

    public void OnShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_Creature.TeamSkill.GetTooltip(), tooltip);
    }

    public void OnShowTooltipType(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(Localization.Get(string.Format("{0}", m_Creature.Info.ShowAttackType)), tooltip);
    }

    public void Levelup()
    {
        m_ParticleLevelUp.Play();
//         InitEvolveInfo();
//         InitSkillInfo();
    }

    public void OnClickEquipEnchant()
    {
        Popup.Instance.Show(ePopupMode.EnchantNew, m_Creature, new System.Action(OnEquipEnchantCallback));
        //Popup.Instance.Show(ePopupMode.EquipEnchant, m_Creature, new System.Action(OnEquipEnchantCallback));
    }

    public void OnClickSkillEnchant()
    {
        Popup.Instance.Show(ePopupMode.SkillEnchant, m_Creature, new System.Action(OnSkillEnchantCallback));
    }

    public void OnClickEnchant()
    {
        if(Tutorial.Instance.Completed == false)
        {
            foreach (var reward_base in Tutorial.Instance.CurrentInfo.rewards)
            {
                if (reward_base.CreatureInfo == null) continue;
                CreatureInfo info = reward_base.CreatureInfo;
                Creature enchant_creature = new Creature(-1, info.IDN, 0, (short)reward_base.Value, (short)reward_base.Value3, (short)reward_base.Value2);
                CreatureManager.Instance.AddTutorialCard(enchant_creature);
            }
        }
        if (m_Creature.Enchant >= 10)
        {
            Tooltip.Instance.ShowMessageKey("MaxEquipEnchant");
            return;
        }
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(m_Creature);
        GameMain.Instance.ChangeMenu(GameMenu.HeroEnchant, menu);
    }

    public void OnClickRuneEquip()
    {
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(m_Creature);
        GameMain.Instance.ChangeMenu(GameMenu.HeroRune, menu);
    }

    public void OnClickDetail()
    {
        Popup.Instance.Show(ePopupMode.HeroDetail, m_Creature);
    }

    public void OnClickHeroInfo()
    {
        GameMain.Instance.MoveEvalMenu(m_Creature.Info.ID);
    }

    public void OnClickLock()
    {
        C2G.CreatureLock _packet = new C2G.CreatureLock();
        _packet.creature_idx = m_Creature.Idx;
        _packet.is_lock = !m_Creature.IsLock;
        Network.GameServer.JsonAsync<C2G.CreatureLock, NetworkCore.AckDefault>(_packet, OnCreatureLock);
    }

    public void OnClickSale()
    {
        //var item = m_Creature;
        if (m_Creature.IsLock)
        {
            Popup.Instance.ShowMessageKey("HeroSaleConfirmTeamLocked");
            return;
        }
        if (TeamDataManager.Instance.CheckTeam(m_Creature.Idx, PacketEnums.pe_Team.PVP_Defense) == true)
        {
            Popup.Instance.ShowMessageKey("HeroSaleConfirmTeamPVPDefense");
            return;
        }
        if (TeamDataManager.Instance.CheckAdventureTeam(m_Creature.Idx) == true)
        {
            Popup.Instance.ShowMessageKey("HeroSaleConfirmTeamInAdventure");
            return ;
        }

        if (TeamDataManager.Instance.CheckTeam(m_Creature.Idx) != PacketEnums.pe_Team.Invalid)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "HeroSaleConfirmTeam");
            return;
        }
        if (m_Creature.Enchant > 0)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEnchanted");
            return;
        }
        if (m_Creature.Armor.EnchantLevel > 0 || m_Creature.Weapon.EnchantLevel > 0)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEquipEnchanted");
            return;
        }
        if (m_Creature.Runes.Count > 0)
        {
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SelectCreatureConfirmEquipRune");
            return;
        }

        Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnSelectSaleConfirm), "SaleConfirm");
    }

    void OnSelectSaleConfirm(bool is_confirm)
    {
        if (is_confirm)
        {
            C2G.CreatureSales packet = new C2G.CreatureSales();
            packet.creature_idxes = new List<long>() { m_Creature.Idx };
            packet.creature_grades = new List<long>() { m_Creature.Grade };
            Network.GameServer.JsonAsync<C2G.CreatureSales, C2G.CreatureSalesAck>(packet, OnCreatureSale);
        }
    }

    void OnCreatureLock(C2G.CreatureLock packet, NetworkCore.AckDefault ack)
    {
        m_Creature.IsLock = packet.is_lock;
        CreatureManager.Instance.Save();
        UpdateLock();
    }

    void OnCreatureSale(C2G.CreatureSales packet, C2G.CreatureSalesAck ack)
    {
        CreatureManager.Instance.Remove(m_Creature.Idx);
        Network.PlayerInfo.AddGoods(ack.add_goods);
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.BackMenu();
    }
}
