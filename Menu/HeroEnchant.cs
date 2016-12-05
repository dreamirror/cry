using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketInfo;
using SharedData;

public class HeroEnchant : MenuBase
{
    public PrefabManager heroItemPrefabManager, EnchantHeroPrefabManager, EnchantMaterialPrefabManager, DungeonHeroPrefabmanager;
    public GameObject m_EnchantHeroIndicator, m_OverEnchanteHeroIndicator, m_OverEnchantMaterialIndicator, m_OverEnchantResultIndicator;
    public UIToggle m_ToggleEnchant, m_ToggleOverEnchant; 
    public UIScrollView m_HeroScroll;
    public UIGrid m_HeroGrid, m_MaterialGrid;
    public HeroSortInfo m_SortInfo;

    public UIParticleContainer m_EnchantParticleContainer, m_OverEnchantParticleContainer;

    public UISlicedFilledSprite m_Progress;
    public UILabel m_ProgressText;

    public UILabel m_Price;
    public UILabel m_OverEnchantPrice;
    public UILabel m_HeroCount;

    public UILabel m_AttackDesc;

    public UILabel m_EquipGradeValue, m_EquipGradeValueNew;
    public UILabel m_MaxHPValue, m_MaxHPValueNew;
    public UILabel m_AttackValue, m_AttackValueNew;
    public UILabel m_PhysicDefense, m_PhysicDefenseNew;
    public UILabel m_MagicDefense, m_MagicDefenseNew;
    public UILabel m_CriticalPower, m_CriticalPowerNew;
    
    public UILabel m_HelpTitle, m_HelpDesc;

    Color32 normal_color = new Color32(66, 33, 0, 255);
    Color32 new_color = new Color32(162, 238, 30, 255);

    Creature m_Creature;
    EnchantHero m_Hero;
    List<EnchantMaterial> m_Materials;
    List<Creature> m_MaterialCreatures = null;

    EnchantHero m_OverEnchantBase, m_OverEnchantMaterial;
    DungeonHero m_OverEnchantResult;

    int m_EnchantGold = 0;
    int m_CurrentTotalEnchantPoint = 0, m_NewTotalEnchantPoint = 0, m_OldTotalEnchantPoint = 0;

    MenuParams m_parms;
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        m_parms = parms;
        //if(m_Creature == null || parms.bBack == false)
        m_Creature = parms.GetObject<Creature>();

        if (parms.bBack == true && CreatureManager.Instance.Contains(m_Creature) == false)
            return false;

        m_MaterialCreatures = parms.GetObject<List<Creature>>("MaterialCreatures");
        
        Init(parms.bBack);

        return true;
    }

    override public void UpdateMenu()
    {
        Init(false);
    }

    void Init(bool bBack)
    {
        m_Materials = new List<EnchantMaterial>();
        for (int i = 0; i < 5; ++i)
        {
            EnchantMaterial material = EnchantMaterialPrefabManager.GetNewObject<EnchantMaterial>(m_MaterialGrid.transform, Vector3.zero);
            m_Materials.Add(material);

            material.Init(null);
        }
        m_MaterialGrid.Reposition();

        if (bBack == false && (m_Creature.Grade >= 6 && m_Creature.Enchant >= 5) )
        {
            m_ToggleEnchant.Set(false);
            m_ToggleOverEnchant.Set(true);
            OverEnchantInit();
        }
        else if (bBack == true && m_ToggleOverEnchant.value == true)
            OverEnchantInit();
        else
            EnchantInit();
    }

    void EnchantInit()
    {   
        m_Hero = EnchantHeroPrefabManager.GetNewObject<EnchantHero>(m_EnchantHeroIndicator.transform, Vector3.zero);
        m_Hero.Init(m_Creature, null, null);

        m_SortInfo.Init(OnSorted, eCreatureSort.Grade, true);

        m_EnchantGold = m_Creature.EnchantGold;

        var event_info = EventHottimeManager.Instance.GetInfoByID("hero_enchant_discount");
        if (event_info != null)
            m_EnchantGold = (int)(m_EnchantGold * event_info.Percent);
        
        var material_creatures = m_MaterialCreatures != null ? m_MaterialCreatures.ToList() : null;

        InitHeroItem();

        if (material_creatures != null && material_creatures.Count > 0)
        {
            for (int i = 0; i < m_Materials.Count; ++i)
            {
                if (i < material_creatures.Count)
                {
                    Creature material_creature = material_creatures[i];
                    m_Heroes.Find(h => h.Creature == material_creature).m_toggle.value = true;
                    m_Materials[i].Init(material_creature, material_creature.CalculateEnchantPoint(m_Creature), new System.Action<Creature>(OnClickEnchantMaterial));
                }
                else
                {
                    m_Materials[i].Init(null);
                }
            }
        }

        m_HelpTitle.gameObject.SetActive(false);
        m_HelpDesc.gameObject.SetActive(false);

        InitInfo();
    }

    void OverEnchantInit()
    {
        if (m_Hero == null)
            m_Hero = EnchantHeroPrefabManager.GetNewObject<EnchantHero>(m_EnchantHeroIndicator.transform, Vector3.zero);
        m_Hero.Init(m_Creature, null, null);

        EnchantHeroPrefabManager.Clear();
        DungeonHeroPrefabmanager.Clear();

        m_OverEnchantBase = EnchantHeroPrefabManager.GetNewObject<EnchantHero>(m_OverEnchanteHeroIndicator.transform, Vector3.zero);
        m_OverEnchantBase.Init(m_Creature);

        m_OverEnchantMaterial = EnchantHeroPrefabManager.GetNewObject<EnchantHero>(m_OverEnchantMaterialIndicator.transform, Vector3.zero);
        m_OverEnchantMaterial.InitDummy(null, m_Creature.Grade, 1, m_Creature.Enchant);
        
        m_OverEnchantResult = DungeonHeroPrefabmanager.GetNewObject<DungeonHero>(m_OverEnchantResultIndicator.transform, Vector3.zero);

        m_OverEnchantPrice.text = "0";

        heroItemPrefabManager.Clear();

        if (IsPossibleOverEnchant() == true && m_Creature.Enchant < 10)
        {
            m_Heroes = new List<EnchantHero>();
            var materials = m_SortInfo.GetFilteredCreatures(c => c.Idx != m_Creature.Idx).Where(c => c.Grade >= 6 && c.Enchant >= m_Creature.Enchant);

            if (materials.Count() > 0)
            {
                foreach (Creature creature in materials)
                {
                    if (creature.Grade == 0)
                        continue;

                    EnchantHero item = heroItemPrefabManager.GetNewObject<EnchantHero>(m_HeroGrid.transform, Vector3.zero);
                    item.Init(creature, OnToggleOverEnchantMaterials, null);

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

                m_HelpTitle.gameObject.SetActive(false);
                m_HelpDesc.gameObject.SetActive(false);
                m_OverEnchantResult.InitDummy(m_Creature.Info, m_Creature.Grade, m_Creature.Level, (short)(m_Creature.Enchant + 1), m_Creature.Info.ShowAttackType);

                return;
            }
            else
            {
                m_HelpTitle.gameObject.SetActive(true);
                m_HelpDesc.gameObject.SetActive(true);
                m_HelpTitle.text = Localization.Get("OverEnchant_NoMatarials");
                m_HelpDesc.text = Localization.Get("Help_OverEnchant");
            }
        }
        else
        {
            m_HelpTitle.gameObject.SetActive(true);
            m_HelpDesc.gameObject.SetActive(true);
            m_HelpTitle.text = Localization.Get("OverEnchant_NotPrepare");
            m_HelpDesc.text = Localization.Get("Help_OverEnchant");

        }
        if(m_Creature.Enchant >= 10)
            m_OverEnchantResult.InitDummy(m_Creature.Info, m_Creature.Grade, m_Creature.Level, 10, m_Creature.Info.ShowAttackType);
        else
            m_OverEnchantResult.InitDummy(m_Creature.Info, m_Creature.Grade, m_Creature.Level, (short)(m_Creature.Enchant + 1), m_Creature.Info.ShowAttackType);
    }

    public void OnValueChange(UIToggle toggle)
    {
        if (toggle.instantTween == true)
            return;
        switch (toggle.name)
        {
            case "Enchant":
                EnchantInit();
                break;
            case "OverEnchant":
                OverEnchantInit();
                break;
        }
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
        EnchantMaterialPrefabManager.Clear();
        EnchantHeroPrefabManager.Clear();
        DungeonHeroPrefabmanager.Clear();
        return true;
    }

    int m_CreatureCount = 0;
    List<EnchantHero> m_Heroes = null;
    void InitHeroItem()
    {
        heroItemPrefabManager.Clear();

        m_Heroes = new List<EnchantHero>();
        var creatures = m_SortInfo.GetFilteredCreatures(c => c.Idx != m_Creature.Idx);
        m_CreatureCount = creatures.Count;

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
    }
    

    void OnSorted()
    {
        var creatures = m_SortInfo.GetFilteredCreatures(c => c.Idx != m_Creature.Idx);

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

    int GetTotalEnchantPoint()
    {
        return Math.Min(500, m_Creature.Enchant * 100 + m_Creature.EnchantPoint + m_Materials.Sum(m => m.EnchantPoint));
    }

    void InitInfo()
    {
        m_NewTotalEnchantPoint = m_OldTotalEnchantPoint = m_CurrentTotalEnchantPoint = m_Creature.Enchant * 100 + m_Creature.EnchantPoint;
        m_EquipGradeValue.text = Localization.Format("HeroEnchantPercent", m_Creature.Enchant, m_Creature.EnchantPoint);
        m_EquipGradeValueNew.text = m_EquipGradeValue.text;

        m_Progress.fillAmount = m_Creature.EnchantPoint / 100f;
        m_ProgressText.text = m_Creature.EnchantPoint + "%";

        switch(m_Creature.Info.AttackType)
        {
            case eAttackType.physic:
                m_AttackDesc.text = Localization.Get("StatType_" + eStatType.PhysicAttack);
                break;

            case eAttackType.magic:
                m_AttackDesc.text = Localization.Get("StatType_" + eStatType.MagicAttack);
                break;

            case eAttackType.heal:
                m_AttackDesc.text = Localization.Get("StatType_" + eStatType.Heal);
                break;
        }
        m_MaxHPValue.text = m_MaxHPValueNew.text = m_Creature.StatTotal.MaxHP.ToString();
        m_AttackValue.text = m_AttackValueNew.text = m_Creature.StatTotal.GetAttack().ToString();
        m_PhysicDefense.text = m_PhysicDefenseNew.text = m_Creature.StatTotal.PhysicDefense.ToString();
        m_MagicDefense.text = m_MagicDefenseNew.text = m_Creature.StatTotal.MagicDefense.ToString();
        m_CriticalPower.text = m_CriticalPowerNew.text = m_Creature.StatTotal.CriticalPower/100+"%";

        m_EquipGradeValueNew.color = m_MaxHPValueNew.color = m_AttackValueNew.color = m_PhysicDefenseNew.color = m_MagicDefenseNew.color = m_CriticalPowerNew.color = normal_color;
        m_EquipGradeValueNew.effectStyle = m_MaxHPValueNew.effectStyle = m_AttackValueNew.effectStyle = m_PhysicDefenseNew.effectStyle = m_MagicDefenseNew.effectStyle = m_CriticalPowerNew.effectStyle = UILabel.Effect.None;

        RefreshInfo();
    }

    void RefreshInfo()
    {
        m_HeroCount.text = Localization.Format("HeroCount", m_CreatureCount, Network.PlayerInfo.creature_count_max);

        m_NewTotalEnchantPoint = GetTotalEnchantPoint();
        m_Price.text = GetPrice().ToString();

        m_MaterialCreatures = m_Materials.Where(m => m.Creature != null).Select(m => m.Creature).ToList();
        m_parms.AddParam("MaterialCreatures", m_MaterialCreatures);
    }

    int GetPrice()
    {
        return m_Materials.Count(m => m.Creature != null) * m_EnchantGold;
    }

    void Update()
    {
        if (m_CurrentTotalEnchantPoint != m_NewTotalEnchantPoint)
        {
            int cur_enchant = Math.Min(5, m_CurrentTotalEnchantPoint / 100);

            float delta = Time.deltaTime * 200;

            m_CurrentTotalEnchantPoint = Mathf.RoundToInt(Mathf.Max(m_CurrentTotalEnchantPoint - delta, Mathf.Min(m_CurrentTotalEnchantPoint + delta, m_NewTotalEnchantPoint)));
            int new_enchant = Math.Min(5, m_CurrentTotalEnchantPoint / 100);
            int new_enchant_point = m_CurrentTotalEnchantPoint - new_enchant * 100;
            if (new_enchant == 5)
            {
                new_enchant_point = 0;
            }
            m_EquipGradeValueNew.text = Localization.Format("HeroEnchantPercent", new_enchant, new_enchant_point);
            m_Progress.fillAmount = new_enchant_point / 100f;
            m_ProgressText.text = new_enchant_point+"%";

            if (new_enchant != cur_enchant)
            {
                float grade_percent = CreatureInfoManager.Instance.Grades[m_Creature.Grade].enchants[new_enchant].stat_percent;

                var StatNew = m_Creature.CalculateBattleStat(grade_percent);

                m_MaxHPValueNew.text = StatNew.MaxHP.ToString();
                m_AttackValueNew.text = StatNew.GetAttack().ToString();
                m_PhysicDefenseNew.text = StatNew.PhysicDefense.ToString();
                m_MagicDefenseNew.text = StatNew.MagicDefense.ToString();
                m_CriticalPowerNew.text = StatNew.CriticalPower / 100 + "%";

                if (m_Creature.Enchant != new_enchant)
                {
                    m_MaxHPValueNew.color = m_AttackValueNew.color = m_PhysicDefenseNew.color = m_MagicDefenseNew.color = m_CriticalPowerNew.color = new_color;
                    m_MaxHPValueNew.effectStyle = m_AttackValueNew.effectStyle = m_PhysicDefenseNew.effectStyle = m_MagicDefenseNew.effectStyle = m_CriticalPowerNew.effectStyle = UILabel.Effect.Outline;
                }
                else
                {
                    m_MaxHPValueNew.color = m_AttackValueNew.color = m_PhysicDefenseNew.color = m_MagicDefenseNew.color = m_CriticalPowerNew.color = normal_color;
                    m_MaxHPValueNew.effectStyle = m_AttackValueNew.effectStyle = m_PhysicDefenseNew.effectStyle = m_MagicDefenseNew.effectStyle = m_CriticalPowerNew.effectStyle = UILabel.Effect.None;
                }
            }

            if (m_CurrentTotalEnchantPoint == m_OldTotalEnchantPoint)
            {
                m_EquipGradeValueNew.color = normal_color;
                m_EquipGradeValueNew.effectStyle = UILabel.Effect.None;
            }
            else
            {
                m_EquipGradeValueNew.color = new_color;
                m_EquipGradeValueNew.effectStyle = UILabel.Effect.Outline;
            }
        }
    }

    void ReorderMaterials()
    {
        var materials = m_Materials.Where(m => m.Creature != null).Select(m => m.Creature).ToList();
        for (int i = 0; i < 5; ++i)
        {
            if (i < materials.Count)
                m_Materials[i].Init(materials[i], materials[i].CalculateEnchantPoint(m_Creature), OnClickEnchantMaterial);
            else
                m_Materials[i].Init(null);
        }
    }

    bool OnToggleCharacter(EnchantHero hero, bool bSelected)
    {
        EnchantMaterial material = m_Materials.Find(m => m.Creature == hero.Creature);
        if (material != null)
        {
            // exists
            material.Init(null);
            ReorderMaterials();
            RefreshInfo();
            return true;
        }

        if (hero.Creature.IsLock == true)
        {
            Popup.Instance.ShowMessageKey("HeroEnchantConfirmLocked");
            return false;
        }


        if (GetTotalEnchantPoint() >= 500)
        {
            Tooltip.Instance.ShowMessageKey("HeroEnchantLimit");
            return false;
        }

        material = m_Materials.Find(m => m.Creature == null);
        if (material == null)
        {
            Tooltip.Instance.ShowMessageKey("HeroEnchantMaterialLimit");
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
            m_SelectEnchantHero = hero;
            Popup.Instance.ShowConfirmKey(TeamConfirm, "HeroEnchantConfirmTeam");
            return false;
        }

        material.Init(hero.Creature, hero.Creature.CalculateEnchantPoint(m_Creature), new System.Action<Creature>(OnClickEnchantMaterial));
        RefreshInfo();

        return true;
    }

    bool OnToggleOverEnchantMaterials(EnchantHero hero, bool bSelected)
    {
        if (bSelected == false)
        {
            m_OverEnchantPrice.text = "0";
            m_OverEnchantMaterial.InitDummy(null, m_Creature.Grade, 1, m_Creature.Enchant);
        }
        else
        {
            m_OverEnchantPrice.text = string.Format("{0:#,###}", CreatureInfoManager.Instance.Grades[m_Creature.Grade].enchants[m_Creature.Enchant + 1].over_enchant_gold);
            m_OverEnchantMaterial.Init(hero.Creature, OnToggleOverEnchantMaterials, null);
            
        }
        return true;
    }


    EnchantHero m_SelectEnchantHero = null;

    void OnClickEnchantMaterial(Creature creature)
    {
        m_Heroes.Find(h => h.Creature == creature).OnBtnCreatureClick();
    }

    void OnDeepTouchCharacter(EnchantHero hero)
    {
//        GameMain.Instance.BackMenu(false);
//        GameMain.Instance.BackMenu(false);

        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(hero.Creature);

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }

    public void OnBtnClickEnchant()
    {
        if (Network.Instance.CheckGoods(pe_GoodsType.token_gold, GetPrice()) == false)
            return;

        if (m_Materials.Any(m => m.Creature != null) == false)
        {
            Tooltip.Instance.ShowMessageKey("HeroEnchantMaterialNeed");
            return;
        }

        Popup.Instance.ShowConfirmKey(EnchantConfirm, "HeroEnchantConfirm");
    }

    public void OnBtnClickOverEnchant()
    {
        if (IsPossibleOverEnchant() == false)
        {
            Tooltip.Instance.ShowMessageKey("OverEnchant_NotPrepare");
            return;
        }

        if (m_OverEnchantMaterial.Creature == null)
        {
            Tooltip.Instance.ShowMessageKey("HeroEnchantMaterialNeed");
            return;
        }

        if (Network.Instance.CheckGoods(pe_GoodsType.token_gold, CreatureInfoManager.Instance.Grades[m_Creature.Grade].enchants[m_Creature.Enchant + 1].over_enchant_gold) == false)
            return;

        Popup.Instance.ShowConfirmKey(OverEnchantConfirm, "HeroEnchantConfirm");
    }

    void TeamConfirm(bool confirm)
    {
        if (confirm == false || m_SelectEnchantHero == null)
            return;

        var material = m_Materials.Find(m => m.Creature == null);
        m_Heroes.Find(h => h == m_SelectEnchantHero).m_toggle.value = true;

        material.Init(m_SelectEnchantHero.Creature, m_SelectEnchantHero.Creature.CalculateEnchantPoint(m_Creature), new System.Action<Creature>(OnClickEnchantMaterial));
        RefreshInfo();

        m_SelectEnchantHero = null;
    }

    void EnchantConfirm(bool confirm)
    {
        if (confirm == false)
            return;

        C2G.CreatureEnchant packet = new C2G.CreatureEnchant();
        packet.creature_idx = m_Creature.Idx;
        packet.creature_grade = m_Creature.Grade;
        packet.materials = new List<pd_CreatureEnchantInfo>();
        foreach (var material in m_Materials.Where(m => m.Creature != null).Select(m => m.Creature))
        {
            var data = new pd_CreatureEnchantInfo();
            data.creature_idx = material.Idx;
            data.creature_grade = material.Grade;
            packet.materials.Add(data);
        }

        if(Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
            tutorial_packet.creature_enchant = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialCreatureEnchant);
        }
        else
            Network.GameServer.JsonAsync<C2G.CreatureEnchant, C2G.CreatureEnchantAck>(packet, OnCreatureEnchant);
    }

    void OverEnchantConfirm(bool confirm)
    {
        if (confirm == false)
            return;

        C2G.CreatureOverEnchant packet = new C2G.CreatureOverEnchant();
        packet.creature_idx = m_Creature.Idx;
        packet.creature_grade = m_Creature.Grade;
        packet.creature_enchant = m_Creature.Enchant;

        packet.material_idx = m_OverEnchantMaterial.Creature.Idx;
        packet.material_grade = m_OverEnchantMaterial.Creature.Grade;
        packet.material_enchant = m_OverEnchantMaterial.Creature.Enchant;

        Network.GameServer.JsonAsync<C2G.CreatureOverEnchant, C2G.CreatureOverEnchantAck>(packet, OnCreatureOverEnchant);
    }

    void OnTutorialCreatureEnchant(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnCreatureEnchant(packet.creature_enchant, ack.creature_enchant);
        Tutorial.Instance.AfterNetworking();
    }
    void OnCreatureEnchant(C2G.CreatureEnchant packet, C2G.CreatureEnchantAck ack)
    {
        m_Creature.SetEnchant(ack.creature_enchant, ack.creature_enchant_point);
        m_Hero.Init(m_Creature, null, null);

        foreach (var material in m_Materials.Where(m => m.Creature != null))
        {
            EnchantHero enchant_hero = m_Heroes.Find(h => h.Creature == material.Creature);
            m_Heroes.Remove(enchant_hero);
            enchant_hero.Init(null);
            enchant_hero.transform.SetParent(null, false);
            enchant_hero.transform.SetParent(m_HeroGrid.transform, false);

            CreatureManager.Instance.Remove(material.Creature.Idx);
            material.Init(null);
            --m_CreatureCount;
        }
        InitInfo();

        m_HeroGrid.Reposition();

        Network.PlayerInfo.UseGoodsValue(PacketInfo.pe_GoodsType.token_gold, ack.use_gold);

        GameMain.Instance.UpdatePlayerInfo();

        m_EnchantParticleContainer.Play();
        Tooltip.Instance.ShowMessageKey("HeroEnchantSuccess");
    }

    void OnCreatureOverEnchant(C2G.CreatureOverEnchant packet, C2G.CreatureOverEnchantAck ack)
    {
        EnchantHero enchant_hero = m_Heroes.Find(h => h.Creature == m_OverEnchantMaterial.Creature);
        CreatureManager.Instance.Remove(m_OverEnchantMaterial.Creature.Idx);
        
        m_Creature.SetEnchant(ack.creature_enchant,0);
        m_Hero.Init(m_Creature, null, null);
        
        m_Heroes.Remove(enchant_hero);
        
        
        --m_CreatureCount;

        enchant_hero.Init(null);
        enchant_hero.transform.SetParent(null, false);
        enchant_hero.transform.SetParent(m_HeroGrid.transform, false);

        m_HeroGrid.Reposition();

        InitInfo();

        OverEnchantInit();

        Network.PlayerInfo.UseGoodsValue(PacketInfo.pe_GoodsType.token_gold, ack.use_gold);

        GameMain.Instance.UpdatePlayerInfo();

        m_OverEnchantParticleContainer.Play();
        Tooltip.Instance.ShowMessageKey("HeroEnchantSuccess");
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_HeroEnchant_Title"), Localization.Get("Help_HeroEnchant"));
    }

    public void OnClickOverEnchantHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_OverEnchant_Title"), Localization.Get("Help_OverEnchant"));
    }

    public void OnClickSlotBuy()
    {
        Popup.Instance.Show(ePopupMode.SlotBuy, pe_SlotBuy.Creature, (PopupSlotBuy.OnOkDeleage)OnSlotBuy);
    }

    void OnSlotBuy()
    {
        RefreshInfo();
    }

    bool IsPossibleOverEnchant()
    {
        return CreatureInfoManager.Instance.Grades[m_Creature.Grade].level_max == m_Creature.Level && m_Creature.Grade >= 6 && m_Creature.Enchant >= 5;
    }
}
