using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketEnums;
using PacketInfo;

public class PVPDeckInfo : MenuBase
{
    enum eDeckType
    {
        Defense,
        Offense,
    }
    enum Mode
    {
        Hero,
        Position,
        Skill,
    }
    Mode m_Mode = Mode.Hero;
    eDeckType m_DeckType = eDeckType.Defense;

    public GameObject DungeonHeroPrefab, CharacterInfoPrefab;
    public PrefabManager DungeonMonsterManager;
    public UIGrid m_GridHeroes;
    public UIGrid m_GridEnemyHeroes;
    public UIToggle m_ToggleOffenseTeam;

    public MainLayout m_MainLayout;
    public GameObject[] m_CharacterInfoIndicator;
    public BoxCollider2D m_HeroDetailRange;

    public GameObject m_SelectCharacter;

    public UILeaderSkill m_LeaderSkill;

    public UIToggle[] toggle_menus;

    public UILabel m_TeamPower, m_label_InfoTitle;

    public GameObject PrefabSelectSkillAuto;
    public GameObject[] m_SelectSkillAutoIndicator;
    List<SelectSkillAuto> m_SelectSkillAuto = null;

    public UISprite m_HeroActive;

    bool is_regist = false;

    TeamData m_CurrentTeam = null;

    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        is_regist = parms.GetObject<bool>("is_regist");

        string deck_type = parms.GetObject("deck_type") as string;
        if (deck_type != null && deck_type == "defense")
        {
            m_DeckType = eDeckType.Defense;
            var team_data = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);
            if (team_data == null)
                m_CurrentTeam = new TeamData(pe_Team.PVP_Defense, null);
            else
                m_CurrentTeam = team_data.Clone();
        }
        else
        {
            m_DeckType = eDeckType.Offense;
            var team_data = TeamDataManager.Instance.GetTeam(pe_Team.PVP);
            if (team_data == null)
                m_CurrentTeam = new TeamData(pe_Team.PVP, null);
            else
                m_CurrentTeam = team_data.Clone();

            foreach (var team_creature in Network.PVPBattleInfo.enemy_team_data.Creatures)
            {
                DungeonMonster enemy = DungeonMonsterManager.GetNewObject<DungeonMonster>(m_GridEnemyHeroes.transform, Vector3.zero);
                enemy.Init(team_creature.creature);
            }
            m_GridEnemyHeroes.Reposition();
        }

        m_ToggleOffenseTeam.value = m_DeckType == eDeckType.Offense;

        toggle_menus[0].value = true;
        InitHeroesItem();
        Init();
        return true;
    }

    override public bool Uninit(bool bBack)
    {
        DungeonMonsterManager.Clear();
        m_Heroes.ForEach(e => DestroyImmediate(e.gameObject));
        m_Heroes = null;
        if (bBack == true)
        {
        }
        return true;
    }

    override public void UpdateMenu()
    {
        Init();
    }

    void OnDisable()
    {
        DungeonMonsterManager.Clear();
    }

    override public bool CheckBackMenuAvailable()
    {
        TeamData teamData = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);
        if(teamData == null)
        {
            GameMain.Instance.BackMenu(false, false);
        }

        return true;
    }
    ////////////////////////////////////////////////////////////////

    void Start()
    {
        m_label_InfoTitle.text = Localization.Get("HeroInfo");
        if (Input.touchPressureSupported)
            m_label_InfoTitle.text += Localization.Get("Support3DTouch");

        m_MainLayout._OnClick = OnCharacterClick;
        m_MainLayout._OnRelease = OnCharacterRelease;
        m_MainLayout._OnPress = OnCharacterPress;
        m_MainLayout._OnDeepTouch = OnDeepTouchDrag;
  //      m_MainLayout._OnDragOver = OnCharacterDragOver;
  //        m_MainLayout._OnDragOut = OnCharacterDragOut;
    }

    void Update()
    {
        if (m_MainLayout.DragContainer != null)
        {
            m_MainLayout.UpdateDrag();
        }
    }

    void Init()
    {
        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();

        InitCharacter();
        m_HeroActive.gameObject.SetActive(false);
    }


    void CheckSelectCharacter()
    {
        m_SelectCharacter.SetActive(m_CurrentTeam == null || m_CurrentTeam.Creatures.Count == 0);
    }

    void InitCharacter()
    {
        m_MainLayout.Init(m_CurrentTeam);

        m_LeaderSkill.Init(m_CurrentTeam.LeaderCreature, m_CurrentTeam.UseLeaderSkillType, OnLeaderSkill);

        m_TeamPower.text = Localization.Format("PowerValue", m_CurrentTeam == null ? 0 : m_CurrentTeam.Power);

        UpdateCharacterInfo(false);
    }

    void OnChangedLeaderSkillChanged(Creature creature)
    {
        m_LeaderSkill.Init(creature, m_LeaderSkill.UseLeaderSkillType, OnLeaderSkill);
        if (m_CurrentTeam != null)
            m_CurrentTeam.SetLeaderCreature(m_LeaderSkill.LeaderCreature, m_LeaderSkill.UseLeaderSkillType);
    }

    void OnLeaderSkillConditionChanged(PacketEnums.pe_UseLeaderSkillType condition)
    {
        m_LeaderSkill.Init(m_LeaderSkill.LeaderCreature, condition, OnLeaderSkill);
        if (m_CurrentTeam != null)
            m_CurrentTeam.SetLeaderCreature(m_LeaderSkill.LeaderCreature, m_LeaderSkill.UseLeaderSkillType);
    }

    public void OnValueChanged(UIToggle obj)
    {
        if (obj.value == true)
        {
            SetMode((Mode)Enum.Parse(typeof(Mode), obj.name.Substring(11), true));
        }
    }

    void SetMode(Mode mode)
    {
        if (m_Mode == mode)
            return;
        m_Mode = mode;

        switch (mode)
        {
            case Mode.Skill:
                InitHeroesSkill();
                return;
        }

        InitHeroesItem();
    }

    List<DungeonHero> m_Heroes = null;
    void InitHeroesItem()
    {
        UIScrollView scroll = m_GridHeroes.GetComponentInParent<UIScrollView>();
        if (m_Heroes != null)
        {
            m_GridHeroes.repositionNow = true;
            return;
        }
        m_Heroes = new List<DungeonHero>();
        CreatureManager.Instance.Sort();

        var creatures = CreatureManager.Instance.GetFilteredList(c => TeamDataManager.Instance.CheckAdventureTeam(c.Idx) == false);
        foreach (Creature creature in CreatureManager.Instance.GetSortedList(eCreatureSort.Power, false, creatures))
        {
            DungeonHero item = NGUITools.AddChild(m_GridHeroes.gameObject, DungeonHeroPrefab).GetComponent<DungeonHero>();
            item.Init(creature, m_CurrentTeam != null && m_CurrentTeam.Contains(creature.Idx), false, OnToggleCharacter, OnDeepTouchListCharacter);
            m_Heroes.Add(item);
        }

        
        foreach (Creature creature in TeamDataManager.Instance.GetCreaturesInAdventure())
        {
            DungeonHero item = NGUITools.AddChild(m_GridHeroes.gameObject, DungeonHeroPrefab).GetComponent<DungeonHero>();
            item.Init(creature, false, false, OnToggleCharacterInAdventure, OnDeepTouchListCharacter);
            m_Heroes.Add(item);
        }

        int count = m_Heroes.Count;
        while (count++ % m_GridHeroes.maxPerLine != 0)
        {
            DungeonHero item = NGUITools.AddChild(m_GridHeroes.gameObject, DungeonHeroPrefab).GetComponent<DungeonHero>();
            item.Init();
            m_Heroes.Add(item);
        }

        m_GridHeroes.repositionNow = true;

        if (scroll != null)
            scroll.ResetPosition();
    }

    void InitHeroesSkill()
    {

        if (m_SelectSkillAuto == null)
        {
            m_SelectSkillAuto = new List<SelectSkillAuto>();
        }
        else
        {
            m_SelectSkillAuto.ForEach(c => DestroyImmediate(c.gameObject));
            m_SelectSkillAuto.Clear();
        }

        for (int i = 0; i < m_SelectSkillAutoIndicator.Length; ++i)
        {
            var item = NGUITools.AddChild(m_SelectSkillAutoIndicator[i], PrefabSelectSkillAuto).GetComponent<SelectSkillAuto>();
            m_SelectSkillAuto.Add(item);
        }

        for (int i=0; i < m_SelectSkillAuto.Count; ++i)
        {
            if (i < m_CurrentTeam.Creatures.Count)
                m_SelectSkillAuto[i].Init(m_CurrentTeam.Creatures[i], OnSelectSkillAutoCallback);
            else
                m_SelectSkillAuto[i].gameObject.SetActive(false);
        }
    }

    void OnSelectSkillAutoCallback(Creature creature, int index)
    {
        TeamCreature team_creature = m_CurrentTeam.Creatures.Find(c => c.creature.Idx == creature.Idx);
        if (team_creature != null)
            team_creature.auto_skill_index = (short)index;
    }
    List<UICharacterInfo> m_ListCharacterInfos = new List<UICharacterInfo>();
    public void UpdateCharacterInfo(bool show_warning)
    {
        for (int i = 0; i < m_MainLayout.m_Characters.Length; i++)
        {
            UICharacterInfo info = null;
            if (m_ListCharacterInfos.Count <= i)
            {
                info = NGUITools.AddChild(m_CharacterInfoIndicator[i], CharacterInfoPrefab).GetComponent<UICharacterInfo>();
                m_ListCharacterInfos.Add(info);
            }
            else
                info = m_MainLayout.m_Characters[i].Info.GetComponentsInChildren<UICharacterInfo>(true)[0];

            if (m_CurrentTeam != null && i < m_CurrentTeam.Creatures.Count)
            {
                bool is_warning = info.IsWarning;
                info.UpdateInfo(m_CurrentTeam.Creatures[i].creature, i);
                if (show_warning == true && is_warning == false && info.IsWarning == true)
                    Tooltip.Instance.ShowMessage(Localization.Get("WarningFront"));
            }
            else
                info.UpdateInfo(null, i);
        }
        CheckSelectCharacter();
    }


    public void OnLeaderSkill()
    {
        if (m_CurrentTeam.Creatures.Any(c => c.creature.TeamSkill != null))
            Popup.Instance.Show(ePopupMode.LeaderSkillSelect, m_CurrentTeam, (LeaderSkillSelectItem.OnChangedLeaderSkillDelegate)OnChangedLeaderSkillChanged, (LeaderSkillConditionInfo.OnChangedLeaderSkillConditionDelegate)OnLeaderSkillConditionChanged);
        else
            Tooltip.Instance.ShowMessageKey("NotExistsLeaderSkill");
    }

    bool OnToggleCharacterInAdventure(DungeonHero hero, bool bSelected)
    {
        Tooltip.Instance.ShowMessageKey("CreatureInAdventure");
        return false;
    }

    bool OnToggleCharacter(DungeonHero hero, bool bSelected)
    {
        if (m_MainLayout.DragContainer != null)
            OnCharacterRelease(true);

        if (bSelected == true)
        {
            if (m_CurrentTeam.ContainsIDN(hero.Creature.Info.IDN) == true)
            {
                Tooltip.Instance.ShowMessageKey("CreatureNotUseSame");
                return false;
            }

            for (int i = 0; i < m_MainLayout.m_Characters.Length; ++i)
            {
                UICharacterContainer container = m_MainLayout.m_Characters[i];
                if (container.Character == null)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (m_MainLayout.m_Characters[j].Character.Creature.Info.Position > hero.CreatureInfo.Position)
                        {
                            m_MainLayout.Reposition(i, j);
                            m_MainLayout.Batch(j);
                            break;
                        }
                    }
                    container.Init(AssetManager.GetCharacterAsset(hero.Creature.Info.ID, hero.Creature.SkinName), UICharacterContainer.Mode.UI_Normal, "social");
                    if (container.Character != null)
                        container.Character.Creature = hero.Creature;

                    m_MainLayout.UpdateBatch();
                    SaveTeamData();
                    UpdateCharacterInfo(true);
                    return true;
                }
            }
            return false;
        }
        else
        {
            UICharacterContainer container = Array.Find(m_MainLayout.m_Characters, c => c.IsInit == true && c.Character.Creature.Idx == hero.Creature.Idx);
            container.Uninit();
            m_MainLayout.Rebatch();

            if (m_CurrentTeam.LeaderCreatureIdx == hero.Creature.Idx)
                OnChangedLeaderSkillChanged(null);
            SaveTeamData();
            UpdateCharacterInfo(true);
        }
        return true;
    }

    void OnDeepTouchListCharacter(Creature hero)
    {
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(hero);
        menu.AddParam("Creatures", m_Heroes.Where(h => h.Creature != null).Select(h => h.Creature).ToList());

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }

    void OnDeepTouchDrag()
    {
        if (m_MainLayout.DragContainer != null)
            OnMainCharacter(CreatureManager.Instance.GetInfoByIdx(m_MainLayout.DragContainer.CharacterAsset.Asset.Creature.Idx), true);
    }

    void OnMainCharacter(Creature hero, bool deep_touch)
    {
        if (deep_touch == true && Tutorial.Instance.Completed == false) return;

        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(hero);
        menu.AddParam("Creatures", m_MainLayout.Creatures);

        m_MainLayout.DragContainer = null;

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }

    bool IsRectContainsPoint(Vector2 center, Vector2 size, Vector2 pos)
    {
        //Debug.LogFormat("{0}\n{1}-{2}", pos, center, size);
        return center.x - size.x / 2 <= pos.x && pos.x <= center.x + size.x / 2
            && center.y - size.y / 2 <= pos.y && pos.y <= center.y + size.y / 2;
    }
    void OnCharacterRelease()
    {
        OnCharacterRelease(false);
    }
    void OnCharacterRelease(bool is_forced)
    {
        if (m_MainLayout.DragContainer != null)
        {
            UICharacterContainer container = m_MainLayout.DragContainer;
            m_MainLayout.DragContainer = null;
            
            m_MainLayout.Rebatch();
            SaveTeamData();
            UpdateCharacterInfo(true);
        }
        m_HeroActive.gameObject.SetActive(false);
    }

    public void OnCharacterClick()
    {
        if (m_Mode == Mode.Position)
            return;

        var container = GetPressCharacter();
        if (container != null)
        {
            switch (m_Mode)
            {
                case Mode.Hero:
                    DungeonHero hero = m_Heroes.Find(h => h.Creature.Idx == container.CharacterAsset.Asset.Creature.Idx);
                    if (hero != null)
                    {
                        hero.OnBtnCreatureClick();
                        return;
                    }
                    break;
            }
        }
    }

    UICharacterContainer GetPressCharacter()
    {
        Ray main_ray = UICamera.currentRay;

        int mask = Camera.main.cullingMask;
        float dist = Camera.main.farClipPlane - Camera.main.nearClipPlane;

        RaycastHit hitInfo;

        if (Physics.Raycast(main_ray, out hitInfo, dist, mask))
        {
            UICharacterContainer selected_character = CoreUtility.GetParentComponent<UICharacterContainer>(hitInfo.collider.transform);
            if (selected_character)
            {
                return selected_character;
            }
        }
        return null;
    }

    void OnCharacterPress()
    {
        if (m_Mode != Mode.Position)
            return;

        if (m_MainLayout.DragContainer != null)
            return;

        m_MainLayout.DragContainer = GetPressCharacter();
    }

    void SaveTeamData()
    {
        List<TeamCreature> creatures = null;
        creatures = new List<TeamCreature>();
        foreach (var character in m_MainLayout.m_Characters.Where(c => c.IsInit))
        {
            Creature creature = CreatureManager.Instance.GetInfoByIdx(character.CharacterAsset.Asset.Creature.Idx);
            TeamCreature team_creature = m_CurrentTeam.Creatures.Find(c => c.creature.Idx == creature.Idx);
            if (team_creature != null)
                creatures.Add(team_creature);
            else
                creatures.Add(new TeamCreature(creature, -1));
        }
        //List<TeamCreature> creatures = m_MainLayout.m_Characters.Where(c => c.IsInit).Select(c => new TeamCreature(CreatureManager.Instance.GetInfoByIdx(c.CharacterAsset.Asset.Creature.Idx), 1)).ToList();
        m_CurrentTeam.SetCreatures(creatures, m_DeckType == eDeckType.Offense);
        m_TeamPower.text = Localization.Format("PowerValue", m_CurrentTeam == null ? 0 : m_CurrentTeam.Power);

        GameMain.Instance.UpdateNotify(false);
    }

    bool CheckSkill()
    {
        if (m_CurrentTeam.Creatures.Any(c => c.auto_skill_index == -1))
        {
            var toggle = Array.Find(toggle_menus, m => m.name == "toggleMenu_skill");
            if (toggle.value == true)
                Tooltip.Instance.ShowMessageKey("PVPDeckSkillSelect");
            else
                toggle.value = true;
            return false;
        }
        return true;
    }

    public void OnClickSelectCharacter()
    {
        Array.Find(toggle_menus, m => m.name == "toggleMenu_hero").value = true;
    }

    public void OnClickSaveDefenseTeam()
    {
        if (m_CurrentTeam.Creatures.Count == 0)
        {
            Tooltip.Instance.ShowMessageKey("PVPTeamCountZero");
            return;
        }

        if (CheckSkill() == false)
            return;

        if (m_CurrentTeam.LeaderCreature == null && m_CurrentTeam.Creatures.Any(c => c.creature.TeamSkill != null))
        {
            OnLeaderSkill();
            return;
        }

        if (is_regist == true)
        {
            C2G.PVPRegistDefense packet = new C2G.PVPRegistDefense();
            packet.leader_creature = Network.PlayerInfo.leader_creature;
            packet.team_power = m_CurrentTeam.Power;
            packet.team_data = m_CurrentTeam.CreateSaveData();
            packet.message = Localization.Get("PVPMessageDefault");
            Network.GameServer.JsonAsync<C2G.PVPRegistDefense, C2G.PVPRegistDefenseAck>(packet, OnPvpRegistDefense);
        }
        else 
        {
            TeamData defense_team = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);
            if (defense_team == null || defense_team.IsEqual(m_CurrentTeam) == false)
            {
                C2G.PvpUpdateDefense packet = new C2G.PvpUpdateDefense();
                packet.leader_creature = Network.PlayerInfo.leader_creature;
                packet.team_power = m_CurrentTeam.Power;
                packet.team_data = m_CurrentTeam.CreateSaveData();
                Network.GameServer.JsonAsync<C2G.PvpUpdateDefense, C2G.PVPRegistDefenseAck>(packet, OnPvpUpdateDefense);
            }
            else
                PvpDefenseTeamUpdated();
        }
    }

    void OnPvpUpdateDefense(C2G.PvpUpdateDefense packet, C2G.PVPRegistDefenseAck ack)
    {
        OnPvpDefense();
    }

    void OnPvpRegistDefense(C2G.PVPRegistDefense packet, C2G.PVPRegistDefenseAck ack)
    {
        OnPvpDefense();
    }

    void OnPvpDefense()
    {
        if (TeamDataManager.Instance.Contains(pe_Team.PVP_Defense) == true)
        {
            TeamData defense_team = TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense);
            defense_team.Set(m_CurrentTeam);
        }
        else
            TeamDataManager.Instance.AddTeam(m_CurrentTeam, true);
        PvpDefenseTeamUpdated();
    }

    private static void PvpDefenseTeamUpdated()
    {
        Tooltip.Instance.ShowMessageKey("SetPVPDefenseTeam");
        GameMain.Instance.UpdateNotify(false);

        //if (GameMain.Instance.GetParentMenu().menu == GameMenu.MainMenu)
        //{
        //    MenuParams parm = new MenuParams();
        //    parm.bStack = false;
        //    GameMain.Instance.ChangeMenu(GameMenu.PVP, parm);
        //}
        //else
            GameMain.Instance.BackMenu();
    }

    public void OnClickBattleStart()
    {
        if (m_CurrentTeam.Creatures.Count == 0)
        {
            Tooltip.Instance.ShowMessageKey("PVPTeamCountZero");
            return;
        }

        if (CheckSkill() == false)
            return;

        if (m_CurrentTeam.LeaderCreature == null && m_CurrentTeam.Creatures.Any(c => c.creature.TeamSkill != null))
        {
            OnLeaderSkill();
            return;
        }

        bool save = false;

        TeamData pvp_team = TeamDataManager.Instance.GetTeam(pe_Team.PVP);
        if (pvp_team == null)
        {
            TeamDataManager.Instance.AddTeam(m_CurrentTeam, true);
            save = true;
        }
        else if (pvp_team.IsEqual(m_CurrentTeam) == false)
        {
            pvp_team.Set(m_CurrentTeam);
            save = true;
        }

        C2G.PvpEnterBattle packet = new C2G.PvpEnterBattle();
        packet.enemy_account_idx = Network.PVPBattleInfo.enemy_info.account_idx;
        if (save)
            packet.team_data = m_CurrentTeam.CreateSaveData();
        Network.GameServer.JsonAsync<C2G.PvpEnterBattle, C2G.PvpEnterBattleAck>(packet, OnPvpEnterBattle);
    }

    void OnPvpEnterBattle(C2G.PvpEnterBattle packet, C2G.PvpEnterBattleAck ack)
    {
        DungeonMonsterManager.Clear();
        GameMain.SetBattleMode(eBattleMode.PVP);
    }

    public void OnDeepTouchHeroInfo()
    {
        Tooltip.Instance.ShowMessageKey("HelpHeroInfo3D");
    }
}
