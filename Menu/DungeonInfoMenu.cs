using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PacketEnums;
using PacketInfo;

public class DungeonInfoMenu : MenuBase
{
    public enum Mode
    {
        Info,
        Hero,
        Hire,
        Position,
    }

    Mode m_Mode = Mode.Info;
    TeamData m_TeamData = null, m_TeamDataBackup = null;

    public GameObject DungeonMonsterPrefab, RewardItemPrefab;

    public GameObject DungeonHeroPrefab, CharacterInfoPrefab, DungeonHeroRecommendPrefab;
    public UIGrid m_GridHeroes;

    public MainLayout m_MainLayout;
    public GameObject[] m_CharacterInfoIndicator;

    public UILabel m_DungeonName, m_DungeonDesc, m_DungeonInfo;
    public UIToggleSprite[] m_ClearRating;
    public UIGrid m_MonsterGrid, m_RewardGrid, m_RecommendGrid;

    public UILabel m_Energy;

    public GameObject m_StageMonster, m_StageRecommend;
    public GameObject m_SelectCharacter;

    public UILeaderSkill m_LeaderSkill;

    public UIToggle[] toggle_menus;

    public UILabel m_TeamPower, m_label_InfoTitle;

    public UIToggle m_ToggleNormal;
    public BoxCollider2D m_HeroDetailRange;

    public UIToggle m_ToggleStage;
    public UILabel m_LabelStartTraining;
    public UISprite m_HeroActive;

    public GameObject m_EventEnergyZero;

    MapStageDifficulty m_StageInfo = null;

    public MapInfo MapInfo { get { return m_StageInfo.MapInfo; } }
    public MapStageDifficulty StageInfo { get { return m_StageInfo; } }

    int m_BattleAvailableCount = 0;
    pd_MapClearData m_map_clear_data;

    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        m_StageInfo = parms.GetObject<MapStageDifficulty>();
        if (m_Heroes != null)
        {
            m_Heroes.ForEach(e => DestroyImmediate(e.gameObject));
            m_Heroes = null;
        }
        toggle_menus[0].value = true;
        Init();

        return true;
    }

    override public bool Uninit(bool bBack)
    {
        if (bBack)
        {
            m_Heroes = null;
            Array.ForEach(m_MonsterGrid.GetComponentsInChildren(typeof(DungeonMonster), true), i => Destroy(i.gameObject));
            Array.ForEach(m_RecommendGrid.GetComponentsInChildren(typeof(DungeonHeroRecommend), true), i => Destroy(i.gameObject));
            Array.ForEach(m_RewardGrid.GetComponentsInChildren(typeof(RewardItem), true), i => Destroy(i.gameObject));
        }
        return true;
    }
    override public void UpdateMenu()
    {
        toggle_menus[0].value = true;
        Init();
    }

    ////////////////////////////////////////////////////////////////

    void Start()
    {
        Localization.language = ConfigData.Instance.Language;

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

//             bool hero_active = false;
//             if (m_MainLayout.IsDragOut == true)
//             {
//                 Vector3 pos = CoreUtility.WorldPositionToUIPosition(UICamera.lastWorldPosition);
// 
//                 if (IsRectContainsPoint(CoreUtility.WorldPositionToUIPosition(m_HeroDetailRange.transform.position), m_HeroDetailRange.size, pos) == true)
//                 {
//                     hero_active = true;
//                 }
//             }
//            m_HeroActive.gameObject.SetActive(hero_active);
        }
    }

    void Init()
    {
        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();

        m_ToggleNormal.value = false;
        if (m_StageInfo != null)
        {
//             if (m_StageInfo.MapInfo.MapType == "main")
//                 m_DungeonInfo.text = Localization.Format("DungeonInfoDesc", Mathf.CeilToInt(m_StageInfo.Power * 0.7f));
//             else
                m_DungeonInfo.text = Localization.Get("DungeonInfo");

            if (m_StageInfo.MapInfo.MapType == "weekly")
            {
                m_DungeonDesc.text = m_StageInfo.MapInfo.Description + "\n";
                m_DungeonDesc.text += Localization.Get("WeeklyAvailable");
                foreach (var tag in m_StageInfo.MapInfo.AvailableTags)
                {
                    m_DungeonDesc.text += string.Format("[url={0}]{1}[/url] ", "Tag_" + tag, Localization.Get("Tag_" + tag));
                }
            }
            else
                m_DungeonDesc.text = m_StageInfo.Description;

            m_map_clear_data = MapClearDataManager.Instance.GetData(m_StageInfo);
            int rate = m_map_clear_data != null ? m_map_clear_data.clear_rate : (short)0;
            for (int i = 0; i < 3; ++i)
            {
                m_ClearRating[i].SetSpriteActive(i < rate);
            }

            if (MapInfo.MapType == "boss")
            {
                m_StageRecommend.SetActive(true);
                m_StageMonster.SetActive(false);

                var boss_info = m_StageInfo.Waves[0].Creatures.Find(e => e.CreatureType == eMapCreatureType.Boss);
                m_DungeonName.text = Localization.Format("HeroName", "Lv." + Boss.CalculateLevel(boss_info.Level, m_StageInfo), boss_info.CreatureInfo.Name);

                //set recommend
                //List<MapCreatureInfo> show_recommends = new List<MapCreatureInfo>();

                Array.ForEach(m_RecommendGrid.GetComponentsInChildren(typeof(DungeonHeroRecommend), true), i => DestroyImmediate(i.gameObject));
                foreach (var recommend in m_StageInfo.Recommends)
                {
                    GameObject obj = NGUITools.AddChild(m_RecommendGrid.gameObject, DungeonHeroRecommendPrefab);
                    obj.GetComponent<DungeonHeroRecommend>().Init(recommend);
                }

                m_RecommendGrid.Reposition();

            }
            else
            {
                m_StageRecommend.SetActive(false);
                m_StageMonster.SetActive(true);

                m_DungeonName.text = m_StageInfo.ShowName;

                //set monster
                List<MapCreatureInfo> show_monsters = new List<MapCreatureInfo>();

                for (int i = 0; i < m_StageInfo.Waves.Count; ++i)
                {
                    MapWaveInfo wave = m_StageInfo.Waves[i];
                    //Debug.LogFormat("wave.Creatures.Count : {0}", wave.Creatures.Count);
                    for (int j = 0; j < wave.Creatures.Count; ++j)
                    {
                        var map_creature = wave.Creatures[j];
                        if (map_creature.IsShow == true && show_monsters.Exists(m => m.CreatureType == map_creature.CreatureType && m.Grade == map_creature.Grade && m.Level == map_creature.Level && m.CreatureInfo.ID == map_creature.CreatureInfo.ID) == false)
                        {
                            show_monsters.Add(map_creature);
                        }
                    }
                }

                Array.ForEach(m_MonsterGrid.GetComponentsInChildren(typeof(DungeonMonster), true), i => DestroyImmediate(i.gameObject));
                foreach (var map_creature in show_monsters)
                {
                    GameObject obj = NGUITools.AddChild(m_MonsterGrid.gameObject, DungeonMonsterPrefab);
                    obj.GetComponent<DungeonMonster>().Init(m_StageInfo, map_creature);
                }

                m_MonsterGrid.Reposition();

            }

            if (MapInfo.MapType == "main")
            {
                m_ToggleStage.value = true;
            }
            else
            {
                m_ToggleStage.value = false;

                int try_count = 0;
                if(MapInfo.MapType == "weekly")
                    try_count = MapClearDataManager.Instance.GetMapDailyClearCount(m_StageInfo.MapInfo.IDN);
                else
                    try_count = MapClearDataManager.Instance.GetMapDailyClearCount(m_StageInfo.MapInfo.IDN, PacketEnums.pe_Difficulty.Normal);
                m_LabelStartTraining.text = Localization.Format("LeftTryCount", m_StageInfo.MapInfo.TryLimit - try_count, m_StageInfo.MapInfo.TryLimit);
            }
            m_BattleAvailableCount = CalculateBattleAvailableCount(m_StageInfo);

            var energy_event = EventHottimeManager.Instance.GetInfoByID("dungeon_energy_zero");
            if (energy_event != null)
                m_Energy.text = ((short)(m_StageInfo.Energy * energy_event.Percent)).ToString();
            else
                m_Energy.text = m_StageInfo.Energy.ToString();

            m_EventEnergyZero.SetActive(energy_event != null);
        }

        InitCharacter();

        if (BattleContinue.Instance.CheckFinish() == false)
        {
            if (BattleContinue.Instance.IsRetry == true)
            {
                BattleContinue.Instance.Clear();
                OnStart();
            }
            else if (BattleContinue.Instance.IsPlaying)
            {
                if (CheckConditionBattleStart() == false)
                    return;

                BattleContinue.Instance.IncreaseBattle();
                if (BattleContinue.Instance.IsPlaying == false)
                {
                    BattleContinue.Instance.Finish(eBattleContinueFinish.Count);
                    return;
                }
                else
                    EnterBattle();
            }
        }
        //m_HeroActive.gameObject.SetActive(false);
    }

    public static int CalculateBattleAvailableCount(MapStageDifficulty stage_info)
    {
        int battle_available_count = stage_info.TryLimit;
        switch(stage_info.MapInfo.MapType)
        {
            case "event":
                battle_available_count -= MapClearDataManager.Instance.GetMapDailyClearCount(stage_info.MapInfo.IDN, pe_Difficulty.Normal);
                break;

            case "boss":
                battle_available_count -= MapClearDataManager.Instance.GetMapDailyClearCount(stage_info.MapInfo.IDN, pe_Difficulty.Normal);
                break;

            case "worldboss":
                battle_available_count -= MapClearDataManager.Instance.GetMapDailyClearCount(stage_info.MapInfo.IDN, pe_Difficulty.Normal);
                break;

            case "weekly":
                battle_available_count -= MapClearDataManager.Instance.GetMapDailyClearCount(stage_info.MapInfo.IDN);
                break;
            default:
                battle_available_count = 10;
                break;
        }
        return battle_available_count;
    }

    void CheckSelectCharacter()
    {
        m_SelectCharacter.SetActive(m_TeamData.Creatures.Count == 0);
    }

    void InitCharacter()
    {
        pe_Team team_id = m_StageInfo.TeamID;
        m_TeamData = TeamDataManager.Instance.GetTeam(team_id);

        if (m_TeamData == null)
        {
            m_TeamData = new TeamData(m_StageInfo.TeamID, null);
            TeamDataManager.Instance.AddTeam(m_TeamData, Tutorial.Instance.Completed);
            m_TeamDataBackup = null;
        }
        else
            m_TeamDataBackup = m_TeamData.Clone();

        m_MainLayout.Init(m_TeamData);

        m_LeaderSkill.Init(m_TeamData.LeaderCreature, m_TeamData.UseLeaderSkillType, OnLeaderSkill);
        //OnChangedLeaderSkillChanged(m_TeamData!=null?m_TeamData.LeaderCreature:null);


        UpdateCharacterInfo(false);
    }

    void OnChangedLeaderSkillChanged(Creature creature)
    {
        m_LeaderSkill.Init(creature, m_LeaderSkill.UseLeaderSkillType, OnLeaderSkill);
        SaveTeamData();
    }

    void OnLeaderSkillConditionChanged(PacketEnums.pe_UseLeaderSkillType condition)
    {
        m_LeaderSkill.Init(m_LeaderSkill.LeaderCreature, condition, OnLeaderSkill);
        SaveTeamData();
    }


    public void OnValueChanged(UIToggle obj)
    {
//        Debug.LogFormat("OnValueChanged({0}:{1})", toggle.name, toggle.value);
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

        if (m_Mode == Mode.Hero)
            InitHeroesItem();
    }

    List<DungeonHero> m_Heroes = null;
    void InitHeroesItem()
    {
        if (m_Heroes != null)
        {
            m_GridHeroes.Reposition();
            return;
        }
        m_Heroes = new List<DungeonHero>();
        CreatureManager.Instance.Sort();

        List<Creature> creatures = CreatureManager.Instance.GetFilteredList(c => c.Grade > 0); 
        if (m_StageInfo.MapInfo.AvailableTags.Count > 0)
            creatures = creatures.Where(c => c.Info.ContainsTags(m_StageInfo.MapInfo.AvailableTags)).ToList();

        if (m_StageInfo.MapInfo.MapType.Equals("boss") || m_StageInfo.MapInfo.MapType.Equals("worldboss"))
        {
            foreach (Creature creature in creatures.Where(c => m_StageInfo.Recommends.Exists(r => r.ID == c.Info.ID)))
            {
                DungeonHero item = NGUITools.AddChild(m_GridHeroes.gameObject, DungeonHeroPrefab).GetComponent<DungeonHero>();
                item.Init(creature, m_TeamData != null && m_TeamData.Contains(creature.Idx), true, OnToggleCharacter, OnDeepTouchListCharacter);
                m_Heroes.Add(item);
            }
            creatures.RemoveAll(c => m_StageInfo.Recommends.Exists(r => r.ID == c.Info.ID));
        }

        creatures = CreatureManager.Instance.GetSortedList(eCreatureSort.Power, false, creatures);
       
        foreach (Creature creature in creatures.Where(e=>TeamDataManager.Instance.CheckAdventureTeam(e.Idx) == false))
        {
            DungeonHero item = NGUITools.AddChild(m_GridHeroes.gameObject, DungeonHeroPrefab).GetComponent<DungeonHero>();
            item.Init(creature, m_TeamData != null && m_TeamData.Contains(creature.Idx), false, OnToggleCharacter, OnDeepTouchListCharacter);
            m_Heroes.Add(item);
        }

        foreach (Creature creature in creatures.Where(e => TeamDataManager.Instance.CheckAdventureTeam(e.Idx) == true))
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


        m_GridHeroes.Reposition();

        UIScrollView scroll = m_GridHeroes.GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();
    }

    bool CheckConditionBattleStart()
    {
        short energy = m_StageInfo.Energy;
        var energy_event = EventHottimeManager.Instance.GetInfoByID("dungeon_energy_zero");
        if (energy_event != null)
            energy = (short)(energy * energy_event.Percent);

        if (Network.PlayerInfo.GetEnergy() < energy)
        {
            if (BattleContinue.Instance.Finish(eBattleContinueFinish.NotEnoughEnergy) == false)
                Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_energy);
            return false;
        }

        if (m_TeamData.Creatures.Count == 0)
        {
            Array.Find(toggle_menus, m => m.name == "toggleMenu_hero").value = true;
            return false;
        }

        if (m_TeamData.LeaderCreature == null && m_TeamData.Creatures.Any(c => c.creature.TeamSkill != null && c.creature.TeamSkill.Info.IsEnabled == true))
        {
            OnLeaderSkill();
            return false;
        }

        if (m_BattleAvailableCount == 0)
        {
            if (BattleContinue.Instance.Finish(eBattleContinueFinish.None) == false)
                Popup.Instance.ShowMessageKey("NotEnoughTryCount");
            return false;
        }

        if (Network.Instance.CheckCreatureSlotCount(m_StageInfo.MapInfo.CheckCreature, false, BattleContinue.Instance.IsPlaying == false, null) == false)
        {
            BattleContinue.Instance.Finish(eBattleContinueFinish.NotEnoughCreatureSlot);
            return false;
        }

        if (Network.Instance.CheckRuneSlotCount(m_StageInfo.MapInfo.CheckRune, false, BattleContinue.Instance.IsPlaying == false, null) == false)
        {
            BattleContinue.Instance.Finish(eBattleContinueFinish.NotEnoughCreatureSlot);
            return false;
        }
        return true;
    }
    public void OnStart()
    {
        if (CheckConditionBattleStart() == false) return;

        EnterBattle();
    }

    private void EnterBattle()
    {
        C2G.EnterBattle packet = new C2G.EnterBattle();
        packet.map_id = m_StageInfo.MapInfo.ID;
        packet.stage_id = m_StageInfo.ID;
        packet.difficulty = m_StageInfo.Difficulty;

        if (m_StageInfo.MapInfo.AvailableTags.Count > 0)
        {
            packet.creature_ids = m_TeamData.Creatures.Select(c => c.creature.Info.ID).ToList();
        }

        if (Tutorial.Instance.Completed == true)
        {
            if (m_TeamData.IsEqual(m_TeamDataBackup) == false)
                packet.team_data = m_TeamData.CreateSaveData();
            Network.GameServer.JsonAsync<C2G.EnterBattle, C2G.EnterBattleAck>(packet, OnEnterBattle);
        }
        else
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.enter_battle = packet;
            tutorial_packet.tutorial_state = Network.PlayerInfo.tutorial_state;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.CurrentState;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialEnterBattle);
        }
    }

    void OnTutorialEnterBattle(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnEnterBattle(packet.enter_battle, null);
        Tutorial.Instance.AfterNetworking();
    }

    void OnEnterBattle(C2G.EnterBattle packet, C2G.EnterBattleAck ack)
    {
        MapClearDataManager.Instance.SetTry(m_StageInfo);

        short energy = m_StageInfo.Energy;
        var energy_event = EventHottimeManager.Instance.GetInfoByID("dungeon_energy_zero");
        if (energy_event != null)
            energy = (short)(energy * energy_event.Percent);

        Network.PlayerInfo.UseEnergy(energy);
        Network.BattleStageInfo = m_StageInfo;
        if (m_StageInfo.MapInfo.MapType == "worldboss")
            GameMain.SetBattleMode(eBattleMode.BattleWorldboss);
        else
            GameMain.SetBattleMode(eBattleMode.Battle);
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

            if (m_TeamData != null && i < m_TeamData.Creatures.Count)
            {
                bool is_warning = info.IsWarning;
                info.UpdateInfo(m_TeamData.Creatures[i].creature, i);
                if (show_warning == true && is_warning == false && info.IsWarning == true)
                    Tooltip.Instance.ShowMessage(Localization.Get("WarningFront"));
            }
            else
                info.UpdateInfo(null, i);
        }
        CheckSelectCharacter();
        m_TeamPower.text = Localization.Format("PowerValue", m_TeamData.Power);
    }


    public void OnLeaderSkill()
    {
        if (m_TeamData.Creatures.Any(c => c.creature.TeamSkill != null))
            Popup.Instance.Show(ePopupMode.LeaderSkillSelect, m_TeamData, (LeaderSkillSelectItem.OnChangedLeaderSkillDelegate)OnChangedLeaderSkillChanged, (LeaderSkillConditionInfo.OnChangedLeaderSkillConditionDelegate)OnLeaderSkillConditionChanged);
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
//        Debug.LogFormat("{0} , {1}", creature.Info.ID, bSelected);
        if (bSelected == true)
        {
            if (m_TeamData.ContainsIDN(hero.Creature.Info.IDN) == true)
            {
                Tooltip.Instance.ShowMessageKey("CreatureNotUseSame");
                return false;
            }
            else if(m_TeamData.Creatures.Count == 5)
            {
                Tooltip.Instance.ShowMessageKey("NoMoreSelect");
                return false;
            }

            for (int i = 0; i < m_MainLayout.m_Characters.Length; ++i)
            {
                UICharacterContainer container = m_MainLayout.m_Characters[i];
                if (container.Character == null)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (m_MainLayout.m_Characters[j].Character.Creature.Info.Position > hero.Creature.Info.Position)
                        {
                            m_MainLayout.Reposition(i, j);
                            m_MainLayout.Batch(j);
                            break;
                        }
                    }
                    container.Init(AssetManager.GetCharacterAsset(hero.Creature.Info.ID, hero.Creature.SkinName), UICharacterContainer.Mode.UI_Normal, "social");
                    if (container.Character != null)
                    {
                        container.Character.Creature = hero.Creature;
                        container.CharacterAsset.Asset.name = string.Format("asset_{0}", hero.Creature.Info.ID);
                    }
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
            UICharacterContainer container = Array.Find(m_MainLayout.m_Characters, c => c.IsInit == true && c.CharacterAsset.Asset.Creature.Idx == hero.Creature.Idx);
            container.Uninit();
            m_MainLayout.Rebatch();

            SaveTeamData();
            if (m_TeamData.LeaderCreatureIdx == hero.Creature.Idx)
                OnChangedLeaderSkillChanged(null);
            UpdateCharacterInfo(true);
        }
        return true;
    }

    void OnDeepTouchDrag()
    {
        if (m_MainLayout.DragContainer != null)
            OnMainCharacter(CreatureManager.Instance.GetInfoByIdx(m_MainLayout.DragContainer.CharacterAsset.Asset.Creature.Idx), true);
        else
        {
            var container = GetPressCharacter();
            OnMainCharacter(container.Character.Creature as Creature, false);
        }
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

    void OnDeepTouchListCharacter(Creature hero)
    {
        if (Tutorial.Instance.Completed == false) return;

        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(hero);
        menu.AddParam("Creatures", m_Heroes.Where(h => h.Creature != null).Select(h => h.Creature).ToList());

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }

    static public bool IsRectContainsPoint(Vector2 center, Vector2 size, Vector2 pos)
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
        //m_HeroActive.gameObject.SetActive(false);
        if (m_MainLayout.DragContainer != null)
        {
            //m_ToggleNormal.value = true;

            UICharacterContainer container = m_MainLayout.DragContainer;
            m_MainLayout.DragContainer = null;

//             if (m_MainLayout.IsDragOut == true)
//             {
//                 Vector3 pos = CoreUtility.WorldPositionToUIPosition(UICamera.lastWorldPosition);
// 
//                 if (IsRectContainsPoint(CoreUtility.WorldPositionToUIPosition(m_HeroDetailRange.transform.position), m_HeroDetailRange.size, pos) == true)
//                 {
//                     OnMainCharacter(container.Character.Creature as Creature, false);
//                     return;
//                 }
// 
//                 DungeonHero hero = m_Heroes.Find(h => h.Creature.Idx == container.CharacterAsset.Asset.Creature.Idx);
//                 if (hero != null && is_forced == false)
//                 {
//                     hero.OnBtnCreatureClick();
//                     return;
//                 }
//             }
            m_MainLayout.Rebatch();
            SaveTeamData();
        }
        UpdateCharacterInfo(true);
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
                case Mode.Info:
                    OnMainCharacter(container.Character.Creature as Creature, false);
                    break;

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
            TeamCreature team_creature = m_TeamData.Creatures.Find(c => c.creature == creature);
            if (team_creature != null)
                creatures.Add(team_creature);
            else
                creatures.Add(new TeamCreature(creature, 1));
        }
        m_TeamData.SetCreatures(creatures, Tutorial.Instance.Completed);
        m_TeamData.SetLeaderCreature(m_LeaderSkill.LeaderCreature, m_LeaderSkill.UseLeaderSkillType, Tutorial.Instance.Completed);
        GameMain.Instance.UpdateNotify(false);
    }

    public void OnClickSelectCharacter()
    {
        Array.Find(toggle_menus, m => m.name == "toggleMenu_hero").value = true;
    }

    public void OnDeepTouchHeroInfo()
    {
        Tooltip.Instance.ShowMessageKey("HelpHeroInfo3D");
    }
}
