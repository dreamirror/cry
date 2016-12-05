using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketInfo;
using PacketEnums;

public class HeroEvolve : MenuBase
{
    public PrefabManager heroItemPrefabManager, DungeonHeroPrefabManager;
    public UIScrollView m_HeroScroll;
    public UIGrid m_HeroGrid;
    public HeroSortInfo m_SortInfo;

    public GameObject EnchantBaseIndicator, EnchantMaterialIndicator;

    public UIToggle m_ToggleMix, m_ToggleEvolve;

    public UILabel m_Price;
    public UILabel m_HeroCount;

    public UILabel m_LabelEnchantButton, m_LabelEnchantMenu;
    public UISprite m_Question;

    public GameObject m_Help;
    public UILabel m_LabelHelp, m_LabelHelpDesc;

    public GameObject m_Event;
    public GameObject m_EventEvolve;
    public GameObject m_EventMix;

    Creature m_Creature, m_EnchantCreature = null;
    DungeonHero m_Hero;
    EnchantHero m_EnchantBase = null, m_EnchantMaterial = null;

    MenuParams m_parms;
    ////////////////////////////////////////////////////////////////

    override public bool Init(MenuParams parms)
    {
        m_parms = parms;
        //if(m_Creature == null || parms.bBack == false)
        m_Creature = parms.GetObject<Creature>();
        object show_evolve_param = parms.GetObject("show_evolve");
        bool show_evolve = false;
        if (show_evolve_param != null)
        {
            show_evolve = (bool)show_evolve_param;
        }

        if (parms.bBack == true && CreatureManager.Instance.Contains(m_Creature) == false)
            return false;

        m_EnchantCreature = parms.GetObject<Creature>("EnchantCreature");

        Init(parms.bBack, show_evolve);
        return true;
    }

    override public void UpdateMenu()
    {
//        Init(false);
    }

    short m_MixLevelLimit = 0, m_EvolveLevelLimit = 0;
    short m_MixBaseLevelLimit = 0, m_EvolveBaseLevelLimit = 0;

    void Init(bool bBack, bool show_evolve)
    {
        m_Hero = DungeonHeroPrefabManager.GetNewObject<DungeonHero>(DungeonHeroPrefabManager.transform, Vector3.zero);

        m_SortInfo.Init(OnSorted);

        m_MixLevelLimit = CreatureInfoManager.Instance.MixLevelLimit(m_Creature.Grade);
        m_MixBaseLevelLimit = CreatureInfoManager.Instance.MixBaseLevelLimit(m_Creature.Grade);
        m_EvolveLevelLimit = CreatureInfoManager.Instance.EvolveLevelLimit(m_Creature.Grade);
        m_EvolveBaseLevelLimit = CreatureInfoManager.Instance.EvolveBaseLevelLimit(m_Creature.Grade);

        if (bBack == true)
        {
            Creature enchant_creature = m_EnchantCreature;
            if (m_ToggleMix.value == true)
                OnValueChanged(m_ToggleMix);
            else
                OnValueChanged(m_ToggleEvolve);
            if (enchant_creature != null)
            {
                m_Heroes.Find(h => h.Creature == enchant_creature).m_toggle.value = true;
                m_EnchantMaterial.Init(enchant_creature, OnClickEnchantMaterial);
                m_EnchantCreature = enchant_creature;
                m_parms.AddParam("EnchantCreature", m_EnchantCreature);
            }
        }
        else
        {
            if (show_evolve)
            {
                m_ToggleEvolve.value = true;
                OnValueChanged(m_ToggleEvolve);
            }
            else
            {
                m_ToggleMix.value = true;
                OnValueChanged(m_ToggleEvolve);
            }
            //OnValueChanged(m_ToggleMix);
        }

        UpdateEventMark();
    }
    void UpdateEventMark()
    {
        if(m_ToggleEvolve.value)
            m_Event.SetActive(EventHottimeManager.Instance.IsHeroEvolveEvent);
        else
            m_Event.SetActive(EventHottimeManager.Instance.IsHeroMixEvent);

        m_EventEvolve.SetActive(EventHottimeManager.Instance.IsHeroEvolveEvent);
        m_EventMix.SetActive(EventHottimeManager.Instance.IsHeroMixEvent);
    }
    ////////////////////////////////////////////////////////////////

    void Start()
    {
        if(GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();
        Localization.language = ConfigData.Instance.Language;
    }

    public override bool Uninit(bool bBack)
    {
        heroItemPrefabManager.Clear();
        DungeonHeroPrefabManager.Clear();
        return true;
    }

    public static bool IsEvolveMaterial(Creature base_creature, Creature material)
    {
        int evolve_level_limit = CreatureInfoManager.Instance.EvolveLevelLimit(base_creature.Grade);
        return material.Idx != base_creature.Idx && material.Info.IDN == base_creature.Info.IDN && material.Grade == base_creature.Grade && material.Level >= evolve_level_limit && material.Enchant >= 5;
    }

    public static bool IsMixMaterial(Creature base_creature, Creature material)
    {
        int mix_level_limit = CreatureInfoManager.Instance.MixLevelLimit(base_creature.Grade);
        return material.Idx != base_creature.Idx && material.Grade == base_creature.Grade && material.Level >= mix_level_limit && material.Enchant >= 5;
    }

    int m_CreatureCount = 0;
    List<EnchantHero> m_Heroes = null;
    bool InitHeroItem()
    {
        heroItemPrefabManager.Clear();

        m_Heroes = new List<EnchantHero>();
        List<Creature> creatures = null;
        
        if (m_ToggleMix.value)
            creatures = m_SortInfo.GetFilteredCreatures(c => IsMixMaterial(m_Creature, c));
        else
            creatures = m_SortInfo.GetFilteredCreatures(c => IsEvolveMaterial(m_Creature, c));
        m_CreatureCount = creatures.Count;

        if (creatures.Count == 0)
            return false;

        foreach (Creature creature in creatures)
        {
            if (creature.Grade == 0)
                continue;

            EnchantHero item = heroItemPrefabManager.GetNewObject<EnchantHero>(m_HeroGrid.transform, Vector3.zero);
            item.Init(creature, OnToggleCharacter, OnDeepTouchCharacter);

            m_Heroes.Add(item);
        }

        for (int i = 0; i < 5; ++i)
        {
            EnchantHero item = heroItemPrefabManager.GetNewObject<EnchantHero>(m_HeroGrid.transform, Vector3.zero);
            item.Init(null);
        }

        m_HeroGrid.Reposition();
        m_HeroScroll.ResetPosition();

        RefreshInfo();

        return true;
    }

    void OnSorted()
    {
        List<Creature> creatures = null;
        if (m_ToggleMix.value)
            creatures = m_SortInfo.GetFilteredCreatures(c => IsMixMaterial(m_Creature, c));
        else
            creatures = m_SortInfo.GetFilteredCreatures(c => IsEvolveMaterial(m_Creature, c));

        List<EnchantHero> items = new List<EnchantHero>();
        for (int i = 0; i < m_HeroGrid.transform.childCount; ++i)
            items.Add(m_HeroGrid.transform.GetChild(i).gameObject.GetComponent<EnchantHero>());
        m_HeroGrid.transform.DetachChildren();

        foreach (Creature creature in creatures)
        {
            if (creature.Grade == 0)
                continue;

            EnchantHero item = items.Find(i => i.Creature == creature);
            m_HeroGrid.AddChild(item.transform, false);
            items.Remove(item);
        }

        foreach (var item in items)
        {
            m_HeroGrid.AddChild(item.transform, false);
        }

        m_HeroGrid.Reposition();
        m_HeroScroll.ResetPosition();
    }

    void InitInfo()
    {
        RefreshInfo();
    }

    void RefreshInfo()
    {
        m_HeroCount.text = Localization.Format("HeroCount", m_CreatureCount, Network.PlayerInfo.creature_count_max);
        m_Price.text = GetPrice().ToString();
    }

    int GetPrice()
    {
        int gold = 0;
        if (m_ToggleMix.value == true)
        {
            gold = m_Creature.MixGold;
            var event_info = EventHottimeManager.Instance.GetInfoByID("hero_mix_discount");
            if (event_info != null)
                gold = (int)(gold * event_info.Percent);
        }
        else
        {
            gold = m_Creature.EvolveGold;
            var event_info = EventHottimeManager.Instance.GetInfoByID("hero_evolve_discount");
            if (event_info != null)
                gold = (int)(gold * event_info.Percent);
        }

        return gold;
    }

    bool OnToggleCharacter(EnchantHero hero, bool bSelected)
    {
        if (bSelected == true)
        {
            if(hero.Creature.IsLock)
            {
                Popup.Instance.ShowMessageKey("HeroEnchantConfirmLocked");
                return false;
            }
            PacketEnums.pe_Team team_type = TeamDataManager.Instance.CheckTeam(hero.Creature.Idx);
            if (team_type != PacketEnums.pe_Team.Invalid)
            {
                if (TeamDataManager.Instance.CheckTeam(hero.Creature.Idx, PacketEnums.pe_Team.PVP_Defense) == true)
                {
                    Popup.Instance.ShowMessageKey("HeroEnchantConfirmTeamPVPDefense");
                    return false;
                }
                if (TeamDataManager.Instance.CheckAdventureTeam(hero.Creature.Idx) == true)
                {
                    Popup.Instance.ShowMessageKey("HeroEnchantConfirmTeamInAdventure");
                    return false;
                }

                m_SelectEnchantHero = hero.Creature;
                Popup.Instance.ShowConfirmKey(TeamConfirm, "HeroEnchantConfirmTeam");
                return false;
            }
        }

        if (m_EnchantMaterial.Creature != null)
        {
            if (m_EnchantMaterial.Creature == hero.Creature)
            {
                InitMaterialDummy();
                return true;
            }
            m_Heroes.Find(h => h.Creature == m_EnchantMaterial.Creature).OnBtnCreatureClick();
        }
        m_EnchantMaterial.Init(hero.Creature, OnClickEnchantMaterial);
        m_EnchantCreature = hero.Creature;
        m_parms.AddParam("EnchantCreature", m_EnchantCreature);

        return true;
    }

    bool OnClickEnchantMaterial(EnchantHero hero, bool bSelected)
    {
        m_Heroes.Find(h => h.Creature == hero.Creature).OnBtnCreatureClick();
        return false;
    }

    Creature m_SelectEnchantHero = null;

    void OnDeepTouchCharacter(EnchantHero hero)
    {
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(hero.Creature);
        bool bShowChangeHeroButton = true;
        menu.AddParam("bShowChangeHeroButton", bShowChangeHeroButton);

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }

    public void OnBtnClickEnchant()
    {
        if (Network.Instance.CheckGoods(pe_GoodsType.token_gold, GetPrice()) == false)
            return;

        if (m_EnchantMaterial.Creature == null)
        {
            if (m_ToggleMix.value)
                Tooltip.Instance.ShowMessageKey("HeroMixMaterialNeed");
            else
                Tooltip.Instance.ShowMessageKey("HeroEvolveMaterialNeed");
            return;
        }

        if (m_ToggleMix.value)
        {
            if (m_Creature.IsLock)
            {
                Popup.Instance.ShowMessageKey("HeroMixConfirmTeamLocked");
                return;
            }
            if (TeamDataManager.Instance.CheckTeam(m_Creature.Idx, PacketEnums.pe_Team.PVP_Defense) == true)
            {
                Popup.Instance.ShowMessageKey("HeroMixConfirmTeamPVPDefense");
                return;
            }

            Popup.Instance.ShowConfirmKey(EnchantConfirm, "HeroMixConfirm");
        }
        else
            Popup.Instance.ShowConfirmKey(EnchantConfirm, "HeroEvolveConfirm");
    }

    void TeamConfirm(bool confirm)
    {
        if (confirm == false || m_SelectEnchantHero == null)
            return;

        if (m_EnchantMaterial.Creature != null)
        {
            m_Heroes.Find(h => h.Creature == m_EnchantMaterial.Creature).OnBtnCreatureClick();
        }

        m_Heroes.Find(h => h.Creature == m_SelectEnchantHero).m_toggle.value = true;
        m_EnchantMaterial.Init(m_SelectEnchantHero, OnClickEnchantMaterial);
        m_EnchantCreature = m_SelectEnchantHero;
        m_parms.AddParam("EnchantCreature", m_EnchantCreature);

        RefreshInfo();

        m_SelectEnchantHero = null;
    }

    void EnchantConfirm(bool confirm)
    {
        if (confirm == false)
            return;

        if (m_ToggleMix.value)
        {
            C2G.CreatureMix packet = new C2G.CreatureMix();
            packet.creature_idx = m_EnchantBase.Creature.Idx;
            packet.creature_grade = m_EnchantBase.Creature.Grade;
            packet.material_creature_idx = m_EnchantMaterial.Creature.Idx;

            if(Tutorial.Instance.Completed == false)
            {
                C2G.TutorialState tutorial_packet = new C2G.TutorialState();
                tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
                tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
                tutorial_packet.creature_mix = packet;
                Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialCreatureMix);
            }
            else
                Network.GameServer.JsonAsync<C2G.CreatureMix, C2G.CreatureMixAck>(packet, OnCreatureMix);
        }
        else
        {
            C2G.CreatureEvolve packet = new C2G.CreatureEvolve();
            packet.creature_idx = m_EnchantBase.Creature.Idx;
            packet.creature_grade = m_EnchantBase.Creature.Grade;
            packet.material_creature_idx = m_EnchantMaterial.Creature.Idx;
            Network.GameServer.JsonAsync<C2G.CreatureEvolve, C2G.CreatureEvolveAck>(packet, OnCreatureEvolve);
        }
    }

    void OnTutorialCreatureMix(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnCreatureMix(packet.creature_mix, ack.creature_mix);
        Tutorial.Instance.AfterNetworking();
    }
    void OnCreatureMix(C2G.CreatureMix packet, C2G.CreatureMixAck ack)
    {
        CreatureManager.Instance.Remove(packet.creature_idx);
        CreatureManager.Instance.Remove(packet.material_creature_idx);

        Network.PlayerInfo.UseGoodsValue(pe_GoodsType.token_gold, ack.use_gold);

        EquipManager.Instance.Add(ack.creature_loot_data.equip[0]);
        EquipManager.Instance.Add(ack.creature_loot_data.equip[1]);
        CreatureManager.Instance.Add(ack.creature_loot_data.creature);

        GameMain.Instance.BackMenu(false);
        GameMain.Instance.BackMenu(false);

        Popup.Instance.Show(ePopupMode.LootCharacter, ack.creature_loot_data.creature.creature_idx, true, true);

        GameMain.Instance.UpdatePlayerInfo();
        CreatureManager.Instance.SetUpdateNotify();
    }

    void OnCreatureEvolve(C2G.CreatureEvolve packet, C2G.CreatureEvolveAck ack)
    {
        if(m_EnchantMaterial != null)
            m_EnchantMaterial.m_toggle_dummy.value = false;

        CreatureManager.Instance.Remove(packet.material_creature_idx);
        CreatureManager.Instance.Update(ack.creature_data);

        Network.PlayerInfo.UseGoodsValue(pe_GoodsType.token_gold, ack.use_gold);

        GameMain.Instance.BackMenu(false);
        GameMain.Instance.BackMenu(false);

        Popup.Instance.Show(ePopupMode.LootCharacter, packet.creature_idx, true, true);

        GameMain.Instance.UpdatePlayerInfo();
    }

    public void OnClickHelp()
    {
        if (m_ToggleMix.value)
            Tooltip.Instance.ShowHelp(Localization.Get("Help_HeroMix_Title"), Localization.Format("Help_HeroMix", m_MixBaseLevelLimit, m_MixLevelLimit));
        else
            Tooltip.Instance.ShowHelp(Localization.Get("Help_HeroEvolve_Title"), Localization.Format("Help_HeroEvolve", m_EvolveBaseLevelLimit, m_EvolveLevelLimit));
    }

    public void OnValueChanged(UIToggle toggle)
    {
        if (toggle.value == true)
        {
            switch (toggle.name)
            {
                case "Mix":
                    InitMix();
                    break;

                case "Evolve":
                    InitEvolve();
                    break;
            }
        }
        UpdateEventMark();
    }

    void InitMix()
    {
        heroItemPrefabManager.Clear();
        m_CreatureCount = 0;

        TeamData team = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);

        bool level_limit = IsLevelLimit(m_Creature, m_MixBaseLevelLimit);
        bool enchant_limit = IsEnchantLimit(m_Creature);
        bool grade_max = IsGradeLimit(m_Creature);
        bool pvp_defense = team != null && team.Contains(m_Creature.Idx) == true;

        if (level_limit || enchant_limit || grade_max || pvp_defense)
        {
            m_LabelHelp.text = Localization.Get("HeroMixInvalidState");

            if (grade_max) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateGradeMax");
            else if (enchant_limit) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateEnchant");
            else if (level_limit) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateLevel");
            else if (pvp_defense) m_LabelHelp.text += "\n" + Localization.Get("InvalidStatePVPDefenseTeam");

            m_LabelHelpDesc.text = Localization.Format("Help_HeroMix", m_MixBaseLevelLimit, m_MixLevelLimit);
            m_Help.gameObject.SetActive(true);
        }
        else
        {
            if (InitHeroItem() == false)
            {
                m_LabelHelp.text = Localization.Get("HeroMixNotExistsMaterial");
                m_LabelHelpDesc.text = Localization.Format("Help_HeroMix", m_MixBaseLevelLimit, m_MixLevelLimit);
                m_Help.gameObject.SetActive(true);
            }
            else
                m_Help.gameObject.SetActive(false);
        }

        m_EnchantBase = heroItemPrefabManager.GetNewObject<EnchantHero>(EnchantBaseIndicator.transform, Vector3.zero);
        m_EnchantBase.Init(m_Creature);

        m_EnchantMaterial = heroItemPrefabManager.GetNewObject<EnchantHero>(EnchantMaterialIndicator.transform, Vector3.zero);
        InitMixMaterialDummy();

        m_LabelEnchantButton.text = m_LabelEnchantMenu.text = Localization.Get("Mix");
        m_Hero.InitDummy(null, (short)(Math.Min(6, m_Creature.Grade + 1)), 1, 0, "");
        m_Question.gameObject.SetActive(true);
        InitInfo();
    }

    void InitMixMaterialDummy()
    {
        m_EnchantMaterial.InitDummy(null, m_Creature.Grade, m_MixLevelLimit, 5);
        m_EnchantCreature = null;
        m_parms.AddParam("EnchantCreature", m_EnchantCreature);
    }

    static public bool IsLevelLimit(Creature creature, int level_limit)
    {
        return creature.Level < level_limit;
    }

    static public bool IsEnchantLimit(Creature creature)
    {
        return creature.Enchant < 5;
    }

    static public bool IsGradeLimit(Creature creature)
    {
        return creature.Grade >= 6;
    }

    static public bool CanEvolve(Creature creature)
    {
        return IsLevelLimit(creature, CreatureInfoManager.Instance.EvolveBaseLevelLimit(creature.Grade)) == false && IsEnchantLimit(creature) == false && IsGradeLimit(creature) == false;
    }

    void InitEvolve()
    {
        heroItemPrefabManager.Clear();
        m_CreatureCount = 0;

        //TeamData team = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);

        bool level_limit = IsLevelLimit(m_Creature, m_EvolveBaseLevelLimit);
        bool enchant_limit = IsEnchantLimit(m_Creature);
        bool grade_max = IsGradeLimit(m_Creature);
        //bool pvp_defense = team != null && team.Contains(m_Creature.Idx) == true;

        if (level_limit || enchant_limit || grade_max)
        {
            m_LabelHelp.text = Localization.Get("HeroEvolveInvalidState");

            if (grade_max) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateGradeMax");
            else if (enchant_limit) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateEnchant");
            else if (level_limit) m_LabelHelp.text += "\n" + Localization.Get("InvalidStateLevel");
            //else if (pvp_defense) m_LabelHelp.text += "\n" + Localization.Get("InvalidStatePVPDefenseTeam");

            m_LabelHelpDesc.text = Localization.Format("Help_HeroEvolve", m_EvolveBaseLevelLimit, m_EvolveLevelLimit);
            m_Help.gameObject.SetActive(true);
        }
        else
        {
            if (InitHeroItem() == false)
            {
                m_LabelHelp.text = Localization.Get("HeroEvolveNotExistsMaterial");
                m_LabelHelpDesc.text = Localization.Format("Help_HeroEvolve", m_EvolveBaseLevelLimit, m_EvolveLevelLimit);
                m_Help.gameObject.SetActive(true);
            }
            else
                m_Help.gameObject.SetActive(false);
        }

        m_EnchantBase = heroItemPrefabManager.GetNewObject<EnchantHero>(EnchantBaseIndicator.transform, Vector3.zero);
        m_EnchantBase.Init(m_Creature);

        m_EnchantMaterial = heroItemPrefabManager.GetNewObject<EnchantHero>(EnchantMaterialIndicator.transform, Vector3.zero);
        InitEvolveMaterialDummy();

        m_LabelEnchantButton.text = m_LabelEnchantMenu.text = Localization.Get("Evolve");
        m_Hero.InitDummy(m_Creature.Info, (short)(Math.Min(6, m_Creature.Grade + 1)), m_Creature.Level, 0, m_Creature.Info.ShowAttackType);
        m_Question.gameObject.SetActive(false);
        InitInfo();
    }

    void InitEvolveMaterialDummy()
    {
        m_EnchantMaterial.m_icon.spriteName = string.Format("_cut_cs_{0}", m_Creature.Info.ID);
        m_EnchantMaterial.InitDummy(m_Creature.Info, m_Creature.Grade, m_EvolveLevelLimit, 5, m_Creature.Info.ShowAttackType);
        m_EnchantCreature = null;
        m_parms.AddParam("EnchantCreature", m_EnchantCreature);
    }

    void InitMaterialDummy()
    {
        if (m_ToggleMix.value)
            InitMixMaterialDummy();
        else
            InitEvolveMaterialDummy();
    }

    public void OnClickSlotBuy()
    {
        Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Creature, (PopupSlotBuy.OnOkDeleage)OnSlotBuy);
    }

    void OnSlotBuy()
    {
        RefreshInfo();
    }
}
