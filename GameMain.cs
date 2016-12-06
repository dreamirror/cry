using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketEnums;
using System;

public enum GameMenu //游戏正处于哪个状态中
{
    IDLE,
    MainMenu,
    Dungeon,
    HeroesInfo,
    HeroInfoDetail,
    Inventory,
    DungeonInfo,
    Store,
    Boss,
    Mission,
    Friends,
    PVP,
    PVPDeckInfo,
    HeroEnchant,
    HeroEvolve,
    HeroRune,
    WorldBossInfo,
    Battle,
    Training,
    HeroesEval,
    Community,
    Guild,
    //GuildCreate,
}

public class MenuParams //这是一个管理字典的类
{
    Dictionary<string, object> parms = new Dictionary<string, object>();

    const string keyAdditive = "bAdditive";
    const string keyBack = "bBack";
    const string keyStack = "bStack";
    public const string keyPopup = "popup"; //这个表示 弹出框的Container

    public bool bAdditive
    {
        get //当 parms 字典中没有keyAdditive的时候返回的值是false
        {//default false
            if (parms.ContainsKey(keyAdditive) == false)
                return false;

            return GetObject<bool>(keyAdditive); //这是一个泛型表达式用来 在parms字典中查找value
        }
        set
        {
            if (parms.ContainsKey(keyAdditive) == false) //如果parms字典中没有 这个key就忘里面加入这个key并且出事话value
                AddParam(keyAdditive, value); //初始化key 和value
            else
                parms[keyAdditive] = value; //如果有这个key就直接返回这个key对应的value
        }
    }
    public bool bStack //没看懂
    {
        get
        {//default true
            if (parms.ContainsKey(keyStack) == false)
                return true;
            return GetObject<bool>(keyStack);
        }
        set
        {
            if (parms.ContainsKey(keyStack) == false)
                AddParam(keyStack, value);
            else
                parms[keyStack] = value;
        }
    }
    public bool bBack //没看懂
    {
        get
        {//default false
            if (parms.ContainsKey(keyBack) == false)
                return false;
            return GetObject<bool>(keyBack);
        }
        set
        {
            if (parms.ContainsKey(keyBack) == false)
                AddParam(keyBack, value);
            else
                parms[keyBack] = value;
        }
    }
    public GameObject GetGameObject() //得到gameobj
    {
        return GetObject<GameObject>();
    }

    public T GetObject<T>(string key) //泛型方法在pams中查找对应的value
    {
        if (parms.ContainsKey(key) == false)
        {
            //Debug.LogErrorFormat("{0} is not exists.", key);
            return default(T);
        }
        return (T)parms[key];
    }


    public T GetObject<T>()
    {
        return GetObject<T>(typeof(T).ToString());
    }

    public void AddParam(string key, object obj) //忘parms字典中加入新的k-v
    {
        if (parms.ContainsKey(key) == true)
        {
            //Debug.LogErrorFormat("{0} is already exists.", key);
            parms[key] = obj;
            return;
        }

        parms.Add(key, obj);
    }
    public void AddParam<T>(object obj) //通过类型往parms添加k-v
    {
        string key = typeof(T).ToString();
        AddParam(key, obj);
    }

    public void RemoveParam(string key) //移除k-v
    {
        if (parms.ContainsKey(key) == false)
        {
            Debug.LogErrorFormat("{0} is not exists.", key);
            return;
        }

        parms.Remove(key);
    }
    public void RemoveParam<T>()
    {
        string key = typeof(T).ToString();
        RemoveParam(key);
    }

    public void RemoveGameObject() //移除parms里面的一个gameObj
    {
        string key = typeof(GameObject).ToString();
        if (parms.ContainsKey(key) == true)
        {
            parms.Remove(key);
        }
    }
    public object GetObject(string key)
    {
        if (parms.ContainsKey(key) == false)
            return null;
        return parms[key];
    }


    //////////////////////////////////////////////////////////
    //for Popup
    public void AddPopup(PopupContainer obj) //添加 弹出框
    {
        AddParam(keyPopup, obj);
    }
    public PopupContainer GetPopup() //得到弹出框
    {
        return GetObject<PopupContainer>(keyPopup);
    }
    public void RemovePopup() //移除弹出框
    {
        if (parms.ContainsKey(keyPopup) == true)
            parms.Remove(keyPopup);
    }
    //////////////////////////////////////////////////////////
}

public class MenuBase : MonoBehaviour
{
    virtual public bool Init(MenuParams parms) //初始化
    {
        throw new System.Exception("not implement"); //作为没有实现虚方法的 异常抛出
    }
    //if return true, destroy gameobject
    virtual public bool Uninit(bool bBack=true)//析构函数
    {
        return true;
    }
    virtual public void UpdateMenu() //更新预设体
    {
        throw new System.Exception("not implement");
    }
    virtual public bool CheckBackMenuAvailable() //检查当前的预设体 是不是激活的状态
    {
        return true;
    }
}

public class GameMain : MonoBehaviour
{
    //     public Character[] m_MyCharacters;
    //     public Character[] m_Enemies;

    public string m_StartScene;
    public GameObject m_UIRoot, m_TopIndicator; //UI的根节点

    public GameObject m_TopFramePrefab;

    public MeshRenderer m_BG, m_BGAdd; //bg 和bgadd 是，东西

    TopFrame m_TopFrame = null;

    static public GameMain Instance { get; private set; }

    static public List<MenuInfo> m_MenuStack = new List<MenuInfo>();

    public GameMenu CurrentGameMenu //返回 gamemenu 通过条件 battlebase.currentbattlemode  和ebatlemode.none的值
    {
        get
        {
            if (BattleBase.CurrentBattleMode != eBattleMode.None) //有战斗场景
                return GameMenu.Battle;

            MenuInfo menu = GetCurrentMenu();
            return menu == null ? GameMenu.IDLE : menu.menu;
        }
    }

    public class MenuInfo //这是一个管理预设体 也就是当前状态场景的类
    {
        public GameMenu menu { get; private set; } //游戏的状态的枚举
        public GameObject obj { get; private set; }
        MenuParams parms; //GameMenu字典的管理类

        public bool IsBack { get { return parms.bBack; } set { parms.bBack = true; } }

        public MenuInfo(GameMenu _menu, MenuParams _parms) //构造函数
        {
            menu = _menu;
            parms = _parms;
            obj = parms.GetGameObject();
        }
        public T GetComponent<T>() //得到T这个类型的组件
        {
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return default(T);
        }
        public void SetActive(bool active) //把当前的 预设体 激活
        {
            if (obj == null) //如果预设体 不存在 则 加载这个预设体
            {
                obj = GameMain.Instance.LoadMenu(menu, parms, false).obj; //得到当前的预设体
            }

            obj.SetActive(active);
        }
        public bool Init() //初始化
        {
            if (obj == null)
            {
                obj = GameMain.Instance.LoadMenu(menu, parms, false).obj; //加载新的预设体
            }
            MenuBase iMenu = obj.GetComponent<MenuBase>();
            if (iMenu != null)
            {
                if (iMenu.Init(parms) == false)
                    return false;
                var popup = parms.GetPopup();
                if (popup != null)
                {
                    Popup.Instance.PushStacks(popup); 
                    parms.RemovePopup();
                }
            }
            else
            {
                Debug.LogWarningFormat("not found IMenu componet.({0})", obj);
                return false;
            }
            return true;
        }

        public void Uninit(bool destroy, bool bBack=true) //类似于析构函数 bBack 默认的是真
        {
            if (obj == null)
                return;
            MenuBase iMenu = obj.GetComponent<MenuBase>(); //得到预设体绑定的脚本
            if (iMenu != null) //如果有脚本
            {
                if (iMenu.Uninit(bBack) == true) 
                {
                    if (destroy == true)//如果是要销毁掉
                    {
                        GameObject.Destroy(obj); //销毁当前的预设体实例
                        obj = null;
                    }
                }
            }
            else
            {
                Debug.LogWarningFormat("not found IMenu componet.({0})", obj);
            }
        }

        public void UpdateMenu() //更新预设体实例
        {
            if (obj == null)
                return;
            MenuBase iMenu = obj.GetComponent<MenuBase>();
            if (iMenu != null)
            {
                iMenu.UpdateMenu();
            }
            else
            {
                Debug.LogWarningFormat("not found IMenu componet.({0})", obj);
            }
        }

        public void AddMenuParam(string key, object obj) //往字典中添加
        {
            parms.AddParam(key, obj);
        }

        public void SetParm(MenuParams parm) //设置字典
        {
            parms = parm;
        }

        public void AddPopup(PopupContainer obj)
        {
            parms.AddPopup(obj);
        }

        public bool CheckBackMenuAvailable() //检查是否激活
        {
            if (obj == null)
                return true;
            MenuBase iMenu = obj.GetComponent<MenuBase>();
            if (iMenu != null)
            {
                return iMenu.CheckBackMenuAvailable();
            }
            else
            {
                Debug.LogWarningFormat("not found IMenu componet.({0})", obj);
            }
            return true;
        }
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        destory = false;
    }

    // Use this for initialization
    void Start()
    {
        if (string.IsNullOrEmpty(m_StartScene) == false)
            UnityEngine.SceneManagement.SceneManager.LoadScene(m_StartScene, UnityEngine.SceneManagement.LoadSceneMode.Single);

//         if (Application.isPlaying == true && Network.PlayerInfo.can_cheat == true)
//         {
//             SRDebug.Instance.IsTriggerEnabled = true;
//         }

        UpdateNotify(false); //更新最新的信息
        if (CurrentGameMenu == GameMenu.IDLE) //空闲状态
        {
            MenuInfo menu = LoadMenu(GameMenu.MainMenu, new MenuParams()); //初始化主场景
            ChangeBG(menu); //切换背景
            menu.SetActive(true); //激活主场景
            StackMenu(menu); //将主场景入栈
        }
        else //非空闲状态
        {
            MenuInfo menu = GetCurrentMenu(); //获取当前的场景
            ChangeBG(menu); //切换背景
            menu.Init(); //初始化当前的场景
        }

        PushManager.Instance.ReloadDefaulLocalNotifiation();
    }

    public void Logout(bool clear_account_idx) //登出
    {
        m_MenuStack.Clear(); //清空当前的存档场景的栈

        SoundManager.Instance.StopSound(); //结束声音

        if (BattleBase.CurrentBattleMode != eBattleMode.None) //如果当前是某个战斗场景
        {
            UnityEngine.SceneManagement.SceneManager.UnloadScene(GetBattleSceneName(BattleBase.CurrentBattleMode)); //卸载到当前的战斗场景
            BattleBase.CurrentBattleMode = eBattleMode.None;//讲记录的战斗场景记为None
        }
        if (ChattingMain.IsInstance) //如果没有关闭聊天功能
            ChattingMain.Clear(); //关闭聊天功能

        ExceptionHandler.Instance.Reset(); 
        //关闭下面的游戏体
        Destroy(gameObject);
        Destroy(Tutorial.Instance.gameObject);
        Destroy(Popup.Instance.gameObject);
        Destroy(Tooltip.Instance.gameObject);
        Destroy(CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.gameObject);
        Destroy(Networking.Instance.gameObject);

        if (MetapsAnalyticsScript.Instance != null)
            MetapsAnalyticsScript.Clear();
        if (ExceptionHandler.IsInit)
            Destroy(GameObject.Find("ExceptionHandler"));

        //析构网络相关
        Network.Uninit();
        Network.ClearInstance();
        Network.GameServer.ClearSession();


        //清楚记录的数据
        SaveDataManger.Instance.Clear();

        SHSavedData.AccessToken = 0;

        if (clear_account_idx == true && SHSavedData.LoginPlatform != LoginPlatform.Guest && SHSavedData.LoginPlatform != LoginPlatform.Betakey)
        {
            SHSavedData.AccountIdx = -1;
            SHSavedData.LoginPlatform = PacketEnums.LoginPlatform.Invalid;
        }

        //         if (SHSavedData.LoginPlatform == LoginPlatform.Facebook)// && Facebook.Unity.FB.IsLoggedIn)
        //             return;
        //             //Facebook.Unity.FB.LogOut();

        if (SHSavedData.LoginPlatform == LoginPlatform.GooglePlay && UM_GameServiceManager.Instance.IsConnected)
            UM_GameServiceManager.Instance.Disconnect();

        Network.ShowIndicator();
#if UNITY_EDITOR
        UnityEngine.SceneManagement.SceneManager.LoadScene("title", UnityEngine.SceneManagement.LoadSceneMode.Single);
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene("Splash", UnityEngine.SceneManagement.LoadSceneMode.Single);
#endif
        //UnityEngine.SceneManagement.SceneManager.UnloadScene("Main");

    }
    public void InitTopFrame()
    {
        if (m_TopFrame == null)
            m_TopFrame = NGUITools.AddChild(GameMain.Instance.m_TopIndicator, m_TopFramePrefab).GetComponent<TopFrame>();
        m_TopFrame.Init();
        CheckNotify();
        m_TopFrame.UpdateNotify();
    }


    public MenuInfo GetCurrentMenu() //得到当前的场景
    {
        MenuInfo result = null;
        if (m_MenuStack.Count > 0)
        {
            result = m_MenuStack[0]; //栈的最上面的就是第一个场景
        }
        return result;
    }

    public MenuInfo GetParentMenu(int up_count = 1) //没看明白
    {
        MenuInfo result = null;
        if (m_MenuStack.Count > up_count)
        {
            result = m_MenuStack[up_count];
        }
        return result;
    }

    MenuInfo PopMenu() //弹出最上面的那个场景
    {
        MenuInfo result = null;
        if (m_MenuStack.Count > 0)
        {
            result = m_MenuStack[0];
            m_MenuStack.RemoveAt(0);
        }
        return result;
    }

    void StackMenu(MenuInfo menu) //将某个场景入栈 到栈顶
    {
        m_MenuStack.Insert(0, menu);
    }

    //两个颜色
    public static readonly Color colorZero = new Color32(0, 0, 0, 0);
    public static readonly Color colorHard = new Color32(0, 0, 120, 80);

    static string m_BGMName = ""; //背景因为的名字
    void ChangeBG(MenuInfo menu, int up_count = 0) //切换背景
    {
        string spriteName = "";

        switch (menu.menu)
        {
            case GameMenu.MainMenu:
                m_BGMName = "Main";
                break;

            case GameMenu.Boss:
            case GameMenu.Dungeon:
            case GameMenu.PVP:
                m_BGMName = "Adventure";
                break;
        }

        SoundManager.Instance.PlayBGM(m_BGMName); //播放背景音乐

        switch (menu.menu)
        {
            case GameMenu.MainMenu:
                spriteName = "000_main";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.HeroInfoDetail:
                spriteName = "000_heroinfo";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.Dungeon:
                spriteName = menu.GetComponent<Dungeon>().MapInfo.ID + "_map";
                break;

            case GameMenu.DungeonInfo:
                {
                    MapStageDifficulty stage_info = menu.GetComponent<DungeonInfoMenu>().StageInfo;
                    if (stage_info.MapInfo.MapType == "boss")
                        spriteName = stage_info.BG_ID + "_D";
                    else
                        ChangeBG(GetParentMenu(up_count+1), up_count + 1);
                }
                break;

            case GameMenu.WorldBossInfo:
                ChangeBG(GetParentMenu(up_count + 1), up_count + 1);
                break;

            case GameMenu.PVP:
            case GameMenu.PVPDeckInfo:
                spriteName = "000_pvp";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.HeroesEval:
                spriteName = "000_hero_loot";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.Inventory:
            case GameMenu.HeroesInfo:
            case GameMenu.Mission:
            case GameMenu.Store:
                ChangeBG(GetParentMenu(up_count + 1), up_count + 1);
                return;

            case GameMenu.Boss:
                spriteName = "000_boss_map";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.Training:
                spriteName = "000_training";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;

            case GameMenu.Community:
            case GameMenu.Friends:
                spriteName = "000_community";
                m_BG.material.SetColor("_GrayColor", colorZero);
                break;
        }

        if (string.IsNullOrEmpty(spriteName) == false)
        {
            Texture2D sp = AssetManager.LoadBG(spriteName);
            m_BG.material.mainTexture = sp;
            if (sp != null)
            {
                if (spriteName == "000_main")
                {
                    if (m_BGAdd.material.mainTexture == null)
                        m_BGAdd.material.mainTexture = AssetManager.LoadBG(spriteName + "_add");
                    m_BGAdd.gameObject.SetActive(true);
                }
                else
                    m_BGAdd.gameObject.SetActive(false);
            }
        }
    }
    public bool BackMenu(bool bInit = true, bool check = true) //退格到上个场景
    {
        while (m_MenuStack.Count > 1)
        {
            if (check == true && GetCurrentMenu().CheckBackMenuAvailable() == false)
                return false;

            MenuInfo currentMenu = PopMenu(); //取出最栈中最上面的预设体并且删除

            MenuInfo newMenu = GetCurrentMenu(); //得到当前的场景就是退格前的场景的前一个 在这里完成了 退格的操作

            RemoveMenu(currentMenu); //移除掉当前的这个场景

            newMenu.IsBack = true; //就是一个状态为
            if (bInit)
            {
                //newMenu init 
                newMenu.SetActive(true);
                if (newMenu.Init() == false)
                    continue;

                ChangeBG(newMenu);
                if (m_TopFrame != null)
                    m_TopFrame.Init();
            }

            return true;
        }
        return false;
    }

    void RemoveMenu(MenuInfo menuInfo) //移除场景
    {
        if (m_MenuStack.Exists(e => e.menu == menuInfo.menu) == false)
        {
            cachedMenu.Remove(menuInfo.menu);
            menuInfo.Uninit(true);
        }
        else
            menuInfo.Uninit(false);
        menuInfo.SetActive(false);
    }

    GameMenu[] m_ShortCut = { GameMenu.HeroesInfo, GameMenu.Inventory };
    bool FindShortCutMenu() //看看当前的栈中有没有上面的两种场景
    {
        for (int i = 0; i < m_MenuStack.Count; ++i)
        {
            GameMenu menu = m_MenuStack[i].menu;
            for (int j = 0; j < m_ShortCut.Length; ++j)
            {
                if (menu == m_ShortCut[j]) return true;
            }
        }
            return false;
    }

    public void PopupToShortCutMenu() //弹出上面的两种场景 并且移除掉
    {
        if (FindShortCutMenu() == false) return;
        bool loop = true;
        do
        {
            MenuInfo menu = PopMenu();
            GameMenu gamemenu = menu.menu;
            RemoveMenu(menu);
            for (int j = 0; j < m_ShortCut.Length; ++j)
            {
                if (gamemenu == m_ShortCut[j]) loop = false;
            }

        } while (loop);
    }


    public void AddMenuParams(string key, object obj) //向栈中添加场景
    {
        MenuInfo info = GetCurrentMenu();
        info.AddMenuParam(key, obj);
    }

    public void StackPopup() //没明白
    {
        MenuInfo info = GetCurrentMenu(); //得到当前的场景
        if(info.menu == GameMenu.HeroInfoDetail) //如果当枪的场景是 HeroInfoDetail
        {
            List<MenuInfo> stacked_menus = m_MenuStack.GetRange(1, m_MenuStack.Count - 1);
            MenuInfo stacked = stacked_menus.Find(m=>m.menu == GameMenu.HeroInfoDetail);
            if (stacked_menus.Count > 0 && stacked != null)
            {
                for(int i=0; stacked_menus.Count > i && stacked_menus[i] != stacked; ++i) m_MenuStack.Remove(stacked_menus[i]);
                m_MenuStack.Remove(stacked);
            }
        }

        info.AddPopup(Popup.Instance.PopStacks());
    }

    public void ChangeMenu(GameMenu gameMenu) //切换场景
    {
        if(gameMenu == GameMenu.MainMenu) //如果是切换到主界面
        {
            while (m_MenuStack.Count > 2) BackMenu(false); //如果 栈中的场景数量大于2 那么就退格直到退到主界面
            BackMenu();//退到主界面的前一个就在退一个就是主界面了
            return;
        }
        ChangeMenu(gameMenu, new MenuParams()); //如果不是切换到主界面那么就按常规的切换
    }

    public void ChangeMenu(GameMenu gameMenu, MenuParams parms) //切换场景
    {
        MenuInfo currentMenu = GetCurrentMenu(); //得到当前的场景
        if (gameMenu == currentMenu.menu) //如果就是切换到当前的场景那么就把当前的场景激活
        {
            if (parms != null)
            {
                currentMenu.SetParm(parms); //重新的设置参数
                currentMenu.SetActive(true); //激活
                currentMenu.Init(); //初始化
            }
            else
                currentMenu.UpdateMenu(); //跟新状态
            ChangeBG(currentMenu); //切换背景
            return;
        }

        if (currentMenu != null)
        {
            if (parms.bAdditive == false)
            {
                if (parms.bStack == false) //表示上面三种长存的界面是长存在栈中的
                {
                    PopMenu();
                    RemoveMenu(currentMenu);
                }
                else
                {
                    currentMenu.Uninit(false, false);
                    currentMenu.SetActive(false);
                }
            }
        }

        switch (gameMenu)
        {
            case GameMenu.Dungeon:
            case GameMenu.DungeonInfo:
                m_MenuStack.RemoveAll(e => e.menu == gameMenu);
                break;
        }

        MenuInfo newMenu = LoadMenu(gameMenu, parms);

        newMenu.IsBack = false;

        StackMenu(newMenu);
        ChangeBG(newMenu);
        if (m_TopFrame != null)
            m_TopFrame.Init();//会在这里面来判断该显示top操作界面上面的东西 
    }

    Dictionary<GameMenu, MenuInfo> cachedMenu = new Dictionary<GameMenu, MenuInfo>(); //讲预设体 和他的管理者 存起来的 字典
    MenuInfo LoadMenu(GameMenu gameMenu, MenuParams parms, bool bInit = true) //通过当前所在的gameMenu 的位置来加载预设体
    {
        MenuInfo menuInfo;
        GameObject menu = null;
        if (cachedMenu.TryGetValue(gameMenu, out menuInfo) == false) //如果cacheMenu中没有这个预设体
        {
            GameObject obj = Resources.Load(string.Format("Prefab/Menu/{0}", gameMenu.ToString())) as GameObject; //通过 当前gameMenu 的状态来获取 预设体
            menu = Instantiate(obj); //实例化这个预设体
            menu.transform.SetParent(m_UIRoot.transform, false); //在这里就添加上去了
            menu.transform.localScale = Vector3.one;
            parms.RemoveGameObject(); //从字典中移除所有的预设体
            parms.AddParam<GameObject>(menu); //把这个预设体加入到字典中
            menuInfo = new MenuInfo(gameMenu, parms); //实例化一个预设体的管理类
            cachedMenu.Add(gameMenu, menuInfo); //讲当前的预设体 和 他的管理类 添加打cachedMenu中
        }
        else //cacheMenu里面已经有这个预设体了
        {
            parms.RemoveGameObject();
            parms.AddParam<GameObject>(menuInfo.obj);
            menuInfo = new MenuInfo(gameMenu, parms);
        }
        menuInfo.SetActive(true); //激活这儿预设体
        if (bInit == true)
            menuInfo.Init();

        return menuInfo;
    }

    public void Update() //基本上没有看懂
    {
        UpdateNotify(false); //更新讣告 什么鬼

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Tutorial.Instance.Completed == false) return;
            Popup.PopupInfo popup = Popup.Instance.GetCurrentPopup(); //弹出一个对话框
            if (Tooltip.Instance.IsShowTooltip())
                Tooltip.Instance.CloseAllTooltip();
            else if (popup != null) 
                popup.Obj.OnClose();
            else if (BackMenu() == false)
                Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnExit), "ConfirmQuit");
        }
    }

    void OnExit(bool is_confirm)
    {
        if (is_confirm)
        {
            SaveDataManger.Instance.Save(); //把当前的数据存起来
            Application.Quit();
        }
    }

    public void UpdateNotify(bool force) //更新各个类
    {
        bool update = false;
        if (CreatureManager.Instance.IsUpdateNotify) //更新所有的角色
        {
            Debug.Log("CreatureManager.UpdateNotify");
            CreatureManager.Instance.UpdateNotify();
            update = true;
        }
        if (TeamDataManager.Instance.IsUpdateNotify) //更新队伍
        {
            Debug.Log("TeamDataManager.UpdateNotify");
            TeamDataManager.Instance.UpdateNotify();
            update = true;
        }
        if (QuestManager.Instance.IsUpdateNotify)
        {
            Debug.Log("QuestManager.UpdateNotify()");
            QuestManager.Instance.UpdateNotify();
            update = true;
        }

        if (m_TopFrame == null)
            return;

        if (Network.Instance.UpdateMail == true)
            update = true;

        if (update || force)
        {
            Debug.Log("m_TopFrame.UpdateNotify()");
            m_TopFrame.UpdateNotify();
        }
    }

    public void CheckNotify() //更新
    {
        UpdateNotify(true);
    }

    static public string GetBattleSceneName(eBattleMode mode) //根据现在的战斗 返回字符串
    {
        switch (mode)
        {
            case eBattleMode.Battle:
                return "battle";

            case eBattleMode.BattleWorldboss:
                return "battle_worldboss";

            case eBattleMode.PVP:
                return "battle_pvp";

            case eBattleMode.RVR:
                return "battle_rvr";
        }
        return "";
    }

    static public void SetBattleMode(eBattleMode mode) //设置现在的战斗类型
    {
        Debug.LogWarningFormat("SetBattleMode : {0}", mode);

        var old_battle_mode = BattleBase.CurrentBattleMode; //之前的战斗类型
        BattleBase.CurrentBattleMode = mode; //设置现在的战斗状态

        switch (mode)
        {
            case eBattleMode.Battle:
            case eBattleMode.BattleWorldboss:
            case eBattleMode.PVP:
            case eBattleMode.RVR: 
                {
                    Network.ShowIndicator();
                    Battle.scene_name = GetBattleSceneName(BattleBase.CurrentBattleMode); //得到当前场景的名字
                    UnityEngine.SceneManagement.SceneManager.LoadScene(Battle.scene_name, UnityEngine.SceneManagement.LoadSceneMode.Additive); //加载场景
                }

                break;

            default: //默认的场景
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadScene(GetBattleSceneName(old_battle_mode));
                    SoundManager.Instance.StopSound();

                    //Resources.UnloadUnusedAssets();

                    Instance.gameObject.SetActive(true); //不明白
                    if (Network.NewStageInfo != null && Network.TargetItemInfo == null || old_battle_mode == eBattleMode.BattleWorldboss)
                    {
                        Instance.BackMenu();
                    }
                    else if (Network.PVPBattleInfo != null)
                    {
                        Network.PVPBattleInfo = null;
                        GameMain.Instance.BackMenu();
                    }
                    else
                    {
                        Instance.UpdateMenu();
                    }
                    SoundManager.Instance.PlayBGM(m_BGMName);
                }
                break;
        }

        TimeManager.Instance.ResetTimeScale();
        TimeManager.Instance.SetPause(false);
        ConfigData.Instance.RefreshSleep();
    }

    public void UpdatePlayerInfo() //更新玩家的信息
    {
        m_TopFrame.UpdatePlayerInfo();
        if (GetCurrentMenu().menu == GameMenu.MainMenu)
        {
            UpdateMenu();
        }
    }

    public void UpdateMenu() //更新现在场景
    {
        GetCurrentMenu().UpdateMenu();
    }
    
    public void MoveEvalMenu(string creature_id)
    {   
        C2G.CreatureEvalInitInfo packet = new C2G.CreatureEvalInitInfo();
        packet.creature_id = creature_id;

        Network.GameServer.JsonAsync<C2G.CreatureEvalInitInfo, C2G.CreatureEvalInitInfoAck>(packet, EvalInitInfoHandler);
    }

    void EvalInitInfoHandler(C2G.CreatureEvalInitInfo send, C2G.CreatureEvalInitInfoAck recv)
    {
        MenuParams parms = new MenuParams();
        parms.AddParam("InitInfo", recv);
        parms.AddParam("CreatureID", send.creature_id);

        if(Popup.Instance.GetCurrentPopup() != null)
            StackPopup();
        
        ChangeMenu(GameMenu.HeroesEval, parms);
    }

    static public void MoveStore(string store_id)
    {
        MenuParams parm = new MenuParams();
        parm.AddParam("StoreTab", store_id);
        //parm.AddPopup
        if (Popup.Instance.GetCurrentPopup() != null)
        {
            GameMain.Instance.StackPopup();
        }
        Instance.ChangeMenu(GameMenu.Store, parm);
    }

    static public void MoveShortCut(GameMenu menu)
    {
        if (GameMain.Instance.CurrentGameMenu != menu)
        {
            GameMain.Instance.PopupToShortCutMenu();
            MenuParams parms = new MenuParams();
            parms.bAdditive = false;
            parms.bStack = GameMain.Instance.IsMenuStack();
            GameMain.Instance.ChangeMenu(menu, parms);
        }
    }
    public bool IsMenuStack() //如果是下面三种界面就返回false
    {
        bool bStack = true;
        switch (GameMain.Instance.CurrentGameMenu)
        {
            case GameMenu.Inventory:
            case GameMenu.HeroesInfo:
            case GameMenu.Mission:
                bStack = false;
                break;
        }
        return bStack;
    }

    public void OnApplicationPause(bool pause)
    {
        Debug.LogFormat("OnApplicationPause : {0}", pause);
        if (pause == false)
        {
            PushManager.Instance.ReloadDefaulLocalNotifiation();
            PushManager.Instance.ResetBadgeNumber();
            if (Network.IsRetry == false)
            {
                if (Network.GameServer.IsConnected == true)
                {
                    Network.GameServer.JsonAsync<C2G.Reconnect, C2G.ReconnectAck>(new C2G.Reconnect(), OnReconnect);
                }
            }
//             if ((DateTime.Now - SHSavedData.PauseTime).TotalMinutes > 10)
//             {
//                 Logout(false);
//                 return;
//             }
        }
        else
        {
            SHSavedData.PauseTime = DateTime.Now;
            SaveDataManger.Instance.Save();
        }
    }

    void OnReconnect(C2G.Reconnect packet, C2G.ReconnectAck ack)
    {

    }

    public static bool destory = false;
    public void OnDestroy()
    {
        destory = true;
        Debug.LogWarning("GameMain:OnDestroy");
    }

//     void OnApplicationFocus(bool focus)
//     {
//         Debug.LogWarningFormat("OnApplicationFocus : {0}", focus);
//     }
}