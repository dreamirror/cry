using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PacketEnums;

public delegate void OnPopupCloseDelegate();

public class MainMenu : MenuBase
{
    public UIAtlas m_AtlasMainMenu;
    public UIAtlas m_AtlasCreature;
    public UIAtlas m_AtlasSkill;
    public UIAtlas m_AtlasEquip;
    public UIAtlas m_AtlasStuff;
    public UIAtlas m_AtlasStuffGray;
    public UIAtlas m_AtlasProfile;
    public UIAtlas m_AtlasMission;
    public UIAtlas m_AtlasUI_Sub1;
    public UIAtlas m_AtlasBattle;
    public UIAtlas m_AtlasGuildEmblem;

    public GameObject m_CharacterInfoPrefab;
    public GameObject []m_CharacterInfoIndicator;
    public MainLayout m_MainLayout;
    public UILabel m_LabelPower;

    public GameObject m_ProfileIndicator;
    public GameObject m_ProfilePrefab;

    public GameObject m_AttendNotify;

    public GameObject[] m_MenuNotify;

    public KingsGift m_KingsGift;

    public HottimeEventIconContainer m_Hottime;

    PlayerProfile m_Profile;

    // Use this for initialization
    void Start () {

#if UNITY_EDITOR
        if (m_AtlasMainMenu.spriteMaterial == null)
            m_AtlasMainMenu.replacement = AssetManager.LoadMainMenuAtlas();

        if (m_AtlasCreature.spriteMaterial == null)
            m_AtlasCreature.replacement = AssetManager.LoadCreatureAtlas();

        if (m_AtlasSkill.spriteMaterial == null)
            m_AtlasSkill.replacement = AssetManager.LoadSkillAtlas();

        if (m_AtlasEquip.spriteMaterial == null)
            m_AtlasEquip.replacement = AssetManager.LoadEquipAtlas();

        if (m_AtlasStuff.spriteMaterial == null)
            m_AtlasStuff.replacement = AssetManager.LoadStuffAtlas();

        if (m_AtlasStuffGray.spriteMaterial == null)
            m_AtlasStuffGray.replacement = AssetManager.LoadStuffAtlas(true);

        if (m_AtlasProfile.spriteMaterial == null)
            m_AtlasProfile.replacement = AssetManager.LoadProfileAtlas();

        if (m_AtlasMission.spriteMaterial == null)
            m_AtlasMission.replacement = AssetManager.LoadMissionAtlas();

        if (m_AtlasUI_Sub1.spriteMaterial == null)
            m_AtlasUI_Sub1.replacement = AssetManager.LoadUIAtlas_Sub1();

        if (m_AtlasBattle.spriteMaterial == null)
            m_AtlasBattle.replacement = AssetManager.LoadUIBattleAtlas();

        if (m_AtlasGuildEmblem.spriteMaterial == null)
            m_AtlasGuildEmblem.replacement = AssetManager.LoadGuildEmblem();

#else
        m_AtlasMainMenu.replacement = AssetManager.LoadMainMenuAtlas();
        m_AtlasCreature.replacement = AssetManager.LoadCreatureAtlas();
        m_AtlasSkill.replacement = AssetManager.LoadSkillAtlas();
        m_AtlasEquip.replacement = AssetManager.LoadEquipAtlas();
        m_AtlasStuff.replacement = AssetManager.LoadStuffAtlas();
        m_AtlasStuffGray.replacement = AssetManager.LoadStuffAtlas(true);
        m_AtlasProfile.replacement = AssetManager.LoadProfileAtlas();
        m_AtlasMission.replacement = AssetManager.LoadMissionAtlas();
        m_AtlasUI_Sub1.replacement = AssetManager.LoadUIAtlas_Sub1();
        m_AtlasBattle.replacement = AssetManager.LoadUIBattleAtlas();
        m_AtlasGuildEmblem.replacement = AssetManager.LoadGuildEmblem();
#endif

        if (GameMain.Instance != null)
            GameMain.Instance.InitTopFrame();

        UpdatePlayerInfo();
        m_MainLayout._OnClick = OnCharacter;
        m_MainLayout._OnDeepTouch = OpenCharacterDetail;
        m_MainLayout._OnPress = OnPressCharacter;
        m_MainLayout._OnRelease = OnReleaseCharacter;

        Tutorial.Instance.Check();
    }

    void OnEnable()
    {
        Network.HideIndicator();
    }

    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
    {
        if (m_Profile == null)
            m_Profile = NGUITools.AddChild(m_ProfileIndicator, m_ProfilePrefab).GetComponent<PlayerProfile>();

        InitCharacter();
        UpdatePlayerInfo();
        UpdateMenuNotify();

        AttendCheckedCallback = new OnPopupCloseDelegate(NotifyMailCheck);
        AttendCheck();
        //NotifyMailCheck();

        Quest quest = QuestManager.Instance.GetShowQuest();
        if(quest != null)
            Tooltip.Instance.ShowTooltip(eTooltipMode.Mission, quest);
        return true;
    }

    override public void UpdateMenu()
    {
        UpdatePlayerInfo();
        InitCharacter();
        UpdateMenuNotify();
    }

    public override bool Uninit(bool bBack = true)
    {
        m_Hottime.Clear();
        return base.Uninit(bBack);
    }

    ////////////////////////////////////////////////////////////////
    void UpdatePlayerInfo()
    {
        if (m_Profile == null)
            m_Profile = NGUITools.AddChild<PlayerProfile>(m_ProfileIndicator, m_ProfilePrefab);

        var player_info = Network.PlayerInfo;
        if (player_info != null && m_Profile != null)
            m_Profile.UpdateProfile(Network.PlayerInfo.leader_creature, player_info.nickname, player_info.player_level, OnProfile);
    }

    void UpdateMenuNotify()
    {
        m_MenuNotify[0].SetActive(Network.Instance.NotifyMenu.is_friends_requested);
        m_MenuNotify[1].SetActive(Network.Instance.NotifyMenu.is_store_free_loot);
        m_MenuNotify[2].SetActive(Network.Instance.NotifyMenu.is_pvp_rank_changed);

        bool is_new_event_map = false;
        List<MapInfo> event_map_infos = MapInfoManager.Instance.Values.Where(m => m.MapType == "event").ToList();
        foreach (var map_info in event_map_infos)
        {
            is_new_event_map = map_info.CheckCondition() == null && MapClearDataManager.Instance.GetTotalClearRate(map_info.IDN, pe_Difficulty.Normal) == 0;
            if (is_new_event_map) break;
        }
        m_MenuNotify[3].SetActive(is_new_event_map);
    }
    void InitCharacter()
    {
        if(GameMain.Instance == null || Network.ConnectState != eConnectState.connected)
        {
            SaveDataManger.Instance.InitFromFile();
        }

        TeamData team_data = TeamDataManager.Instance.GetTeam(pe_Team.Main);
        m_MainLayout.Init(team_data);

        UpdateCharacterInfo();
        
        m_LabelPower.text = Localization.Format("PowerValue", team_data == null ? 0 : team_data.Power);
    }

    List<UICharacterInfo> m_ListCharacterInfos = new List<UICharacterInfo>();
    public void UpdateCharacterInfo()
    {
        TeamData team_data = TeamDataManager.Instance.GetTeam(pe_Team.Main);
        for (int i = 0; i < m_MainLayout.m_Characters.Length; i++)
        {
            if (m_ListCharacterInfos.Count <= i)
            {
                UICharacterInfo info = NGUITools.AddChild(m_CharacterInfoIndicator[i], m_CharacterInfoPrefab).GetComponent<UICharacterInfo>();
                m_ListCharacterInfos.Add(info);
            }
            if (team_data != null && i < team_data.Creatures.Count)
            {
                m_ListCharacterInfos[i].UpdateInfo(team_data.Creatures[i].creature, 5);
            }
            else
                m_ListCharacterInfos[i].UpdateInfo(null, 5);
        }
    }

    void Update()
    {
        UpdateCharacters();
        
        if (AttendManager.Instance.IsInit == true)
            AttendNotifyCheck();

        if (is_kingsGiftInit == false && KingsGiftInfoManager.Instance.IsKingsGiftActive == true)
            InitKingsGift();
        else if (is_kingsGiftInit == true)
            m_KingsGift.KingsGiftUpdate();
    }

    

    void InitKingsGift()
    {
        is_kingsGiftInit = true;
        m_KingsGift.Init();
    }

    bool is_kingsGiftInit = false;
        

    void OnCharacter()
    {
        Ray main_ray = UICamera.currentRay;

        int mask = Camera.main.cullingMask;
        float dist = Camera.main.farClipPlane - Camera.main.nearClipPlane;

        RaycastHit hitInfo;

        if (Physics.Raycast(main_ray, out hitInfo, dist, mask))
        {
            selected_character = CoreUtility.GetParentComponent<UICharacterContainer>(hitInfo.collider.transform);
            if (selected_character)
            {
                OpenCharacterDetail();
            }
        }
    }

    void OpenCharacterDetail()
    {
        if (selected_character == null)
            return;

        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(CreatureManager.Instance.GetInfoByIdx(selected_character.CharacterAsset.Asset.Creature.Idx));
        menu.AddParam("Creatures", m_MainLayout.Creatures);

        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
        selected_character = null;
    }

    UICharacterContainer selected_character = null;
    void OnPressCharacter()
    {
        Ray main_ray = UICamera.currentRay;

        int mask = Camera.main.cullingMask;
        float dist = Camera.main.farClipPlane - Camera.main.nearClipPlane;

        RaycastHit hitInfo;

        if (Physics.Raycast(main_ray, out hitInfo, dist, mask))
        {
            selected_character = CoreUtility.GetParentComponent<UICharacterContainer>(hitInfo.collider.transform);
        }
    }

    void OnReleaseCharacter()
    {
        selected_character = null;
    }

    float m_NextUpdateCharacterTime = 0f;
    List<UICharacterContainer> m_NextUpdateCharacters = null;
    void UpdateCharacters()
    {
        if (m_NextUpdateCharacters == null || m_NextUpdateCharacters.Count == 0)
        {
            m_NextUpdateCharacters = m_MainLayout.m_Characters.Where(c => c.IsInit == true).OrderBy(c => MNS.Random.Instance.NextFloat()).ToList();
        }
        if (m_NextUpdateCharacters == null || m_NextUpdateCharacters.Count == 0)
            return;

        float time = Time.time;
        if (m_NextUpdateCharacterTime < time)
        {
            m_NextUpdateCharacterTime = time + MNS.Random.Instance.NextRange(1f, 8f);

            UICharacterContainer container = m_NextUpdateCharacters[0];
            if (container.Character == null) return;
            container.PlaySocialAction();
            m_NextUpdateCharacters.RemoveAt(0);
        }
    }

    public void OnClickMenu(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Argument is null");
            return;
        }
        Debug.LogFormat("OnClickMenu :{0}", obj.name);
        switch (obj.name)
        {
            case "btn_community":
                GameMain.Instance.ChangeMenu(GameMenu.Community);
//                GameMain.Instance.ChangeMenu(GameMenu.Friends);
                break;

            case "btn_store":
                GameMain.Instance.ChangeMenu(GameMenu.Store);
                break;

            case "btn_colosseum":
                {
                    int pvp_level = GameConfig.Get<int>("contents_open_pvp");
                    if (pvp_level <= 0)
                    {
                        Tooltip.Instance.ShowMessageKey("NotImplement");
                    }
                    else
                    {
                        if (Network.PlayerInfo.player_level >= pvp_level)
                            GameMain.Instance.ChangeMenu(GameMenu.PVP);
                        else
                            Tooltip.Instance.ShowMessageKeyFormat("ContentsPvpOpenLimitFormat", pvp_level);
                    }
                }
                break;

            case "btn_adventure":
                OnAdventure();
                break;

            case "btn_training":
                GameMain.Instance.ChangeMenu(GameMenu.Training);
                //Popup.Instance.Show(ePopupMode.Training);
                break;

            case "btn_devil":
                GameMain.Instance.ChangeMenu(GameMenu.Boss);
//                Popup.Instance.Show(ePopupMode.Devil);
                break;

            default:
                Tooltip.Instance.ShowMessageKey("NotImplement");
                break;
        }
    }

    void OnAdventure()
    {
        //        if (Network.)
        if (CreatureManager.Instance.Creatures.Count(c => c.Grade > 0) == 0)
        {
            GameMain.Instance.ChangeMenu(GameMenu.Store);
//            Popup.Instance.ShowMessageKey("NoCreature");
            return;
        }
        GameMain.Instance.ChangeMenu(GameMenu.Dungeon);
    }

    public void OnEvent()
    {
        if(EventHottimeManager.Instance.Events.Exists(e=>e.show_state))
            Popup.Instance.Show(ePopupMode.HottimeEvent);
        else
            Tooltip.Instance.ShowMessageKey("NotExistHottimeEvent");
    }

    public void OnProfile()
    {
        //throw new System.Exception("aaa");
        Popup.Instance.Show(ePopupMode.Profile);
    }

    public void OnAttend()
    {
        if (AttendManager.Instance.IsInit == false)
        {
            force_attend = true;
            AttendCheckedCallback = null;
            Network.GameServer.JsonAsync<C2G.AttendInfoRequest, C2G.AttendInfoRequestAck>(new C2G.AttendInfoRequest(), AttendRequestInfoHandler);
        }
        else if(AttendManager.Instance.OngoingAttendCount > 0)
            Popup.Instance.Show(ePopupMode.Attend, true);
    }

    public void OnSurvey()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    bool force_attend = false;
    OnPopupCloseDelegate AttendCheckedCallback = null;
    void AttendCheck()
    {
        if (Tutorial.Instance.Completed == false)
            return;
        
        if (AttendManager.Instance.Attends == null || AttendManager.Instance.LastRequestDailyIndex < Network.DailyIndex)
            Network.GameServer.JsonAsync<C2G.AttendInfoRequest, C2G.AttendInfoRequestAck>(new C2G.AttendInfoRequest(), AttendRequestInfoHandler);
        else if (AttendInfoManager.Instance.Values.Any(a => a.start_at < Network.Instance.ServerTime && Network.Instance.ServerTime < a.end_at) && AttendManager.Instance.isAvailableReward)
            Popup.Instance.Show(ePopupMode.Attend, true, AttendCheckedCallback);
        else
            AttendCheckedCallback();
    }

    void AttendRequestInfoHandler(C2G.AttendInfoRequest send, C2G.AttendInfoRequestAck recv)
    {
        AttendManager.Instance.Init(recv.attends);

        int last_daily_idx = 0;
        foreach (var attend in recv.attends)
        {
            if (last_daily_idx < attend.last_daily_index)
                last_daily_idx = attend.last_daily_index;
        }

        if (last_daily_idx > 0)
            SHSavedData.Instance.LastAttendCheckDailyIndex = last_daily_idx;

        if (AttendManager.Instance.OngoingAttendCount == 0)
        { }
        else if (force_attend)
        {
            Popup.Instance.Show(ePopupMode.Attend, true, AttendCheckedCallback);
            force_attend = false;
            return;
        }
        else if (AttendManager.Instance.IsNewReward)
            Popup.Instance.Show(ePopupMode.Attend, false, AttendCheckedCallback);
        else
            AttendCheckedCallback();        
    }
    
    void AttendNotifyCheck()
    {
        m_AttendNotify.SetActive(AttendManager.Instance.isAvailableReward);
    }

    void NotifyMailCheck()
    {
        if (Network.Instance.UnreadMailState != pe_UnreadMailState.MainMenuOpen)
            return;
        
        Network.GameServer.JsonAsync<C2G.NotifyMailGet, C2G.NotifyMailGetAck>(new C2G.NotifyMailGet(), NotifyMailPacketHandler);
    }

    void NotifyMailPacketHandler(C2G.NotifyMailGet send, C2G.NotifyMailGetAck recv)
    {
        MailManager.Instance.SetNotifyMail(recv);
        RecursiveNotifyMailChecker();
    }

    void RecursiveNotifyMailChecker()
    {
        if (MailManager.Instance.GetNotifyMail().Count == 0)
            return;
        var detail_mail = MailManager.Instance.GetNotifyMail().First();
        MailManager.Instance.ReadNotifyMail(detail_mail);
        Popup.Instance.Show(ePopupMode.MailDetail, detail_mail, new OnPopupCloseDelegate(RecursiveNotifyMailChecker));
    }
}
