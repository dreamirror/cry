using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LinqTools;

public enum eGuildTabMode
{
    GuildInfo,
    GuildBuff,
    GuildJoin,
    RequestList,
    GuildCreate,
    GuildRank,
    GuildManagement,
}

public class Guild : MenuBase
{
    public GameObject GuildJoinPrefab;
    public GameObject RequestListPrefab;
    public GameObject GuildCreatePrefab;
    public GameObject GuildInfoPrefab;
    public GameObject GuildBuffPrefab;
    public GameObject GuildRankPrefab;
    public GameObject GuildManagementPrefab;
    public PrefabManager GuildTabPrefabManager;

    public UIGrid m_GridTab;
    public UIScrollView m_ScrollTab;

    public GameObject m_Contents;

    Dictionary<eGuildTabMode, GuildContentsBase> Contents = new Dictionary<eGuildTabMode, GuildContentsBase>();
    //GuildJoin m_GuildJoin;

    Dictionary<eGuildTabMode, GuildTab> Tabs = new Dictionary<eGuildTabMode, GuildTab>();

    eGuildTabMode m_CurrentTab;
    override public bool Init(MenuParams parms)
    {
        GuildTabPrefabManager.Clear();
        if (parms.bBack == true)
        {
            var tabs = Tabs.Keys.ToList();
            Tabs.Clear();
            InitTabs(tabs);
            SetTab(m_CurrentTab);
            return true;
        }
        Tabs.Clear();
        if (GuildManager.Instance.IsGuildJoined)
        {
            m_CurrentTab = eGuildTabMode.GuildInfo;
            List<eGuildTabMode> tabs = new List<eGuildTabMode>() { m_CurrentTab, eGuildTabMode.GuildBuff, eGuildTabMode.GuildRank};
            if (GuildManager.Instance.AvailableGuildManagement)
                tabs.Add(eGuildTabMode.GuildManagement);
            InitTabs(tabs);
        }
        else
        {
            m_CurrentTab = eGuildTabMode.GuildJoin;

            List<eGuildTabMode> tabs = new List<eGuildTabMode>() { m_CurrentTab };

            if (Network.PlayerInfo.player_level >= GuildInfoManager.Config.AtLeastPlayerLevel)
            {
                tabs.Add(eGuildTabMode.RequestList);
                tabs.Add(eGuildTabMode.GuildCreate);
            }
            InitTabs(tabs);
        }
        SetTab(m_CurrentTab);

        return true;
    }

    private void InitTabs(List<eGuildTabMode> tabs)
    {
        foreach (var mode in tabs)
        {
            var item = GuildTabPrefabManager.GetNewObject<GuildTab>(m_GridTab.transform, Vector3.zero);
            item.Init(Localization.Get("GuildTab_" + mode.ToString()), mode, OnTabClick);
            Tabs.Add(mode, item);
        }
        m_GridTab.Reposition();
        m_ScrollTab.ResetPosition();
    }

    override public void UpdateMenu()
    {
        switch(m_CurrentTab)
        {
            case eGuildTabMode.GuildInfo:
            case eGuildTabMode.GuildBuff:
            case eGuildTabMode.GuildManagement:
                GuildContentsBase content = null;
                if (Contents.TryGetValue(m_CurrentTab, out content) == true)
                {
                    content.UpdateInfo();
                }
                break;
            case eGuildTabMode.GuildJoin:
            case eGuildTabMode.RequestList:
                GameMain.Instance.ChangeMenu(GameMenu.Guild);
                break;
        }
    }

    public override bool Uninit(bool bBack = true)
    {
        if(bBack)
        {
            GuildManager.Instance.SetGuildMembers(null);
        }
        GuildTabPrefabManager.Clear();
        AllContentsClear();
        return base.Uninit(bBack);
    }

    public void AddManagementTab()
    {
        if (GuildManager.Instance.AvailableGuildManagement)
            InitTabs(new List<eGuildTabMode>() { eGuildTabMode.GuildManagement });
    }
    public void SetTab(eGuildTabMode mode)
    {
        GuildTab tab = null;
        if (Tabs.TryGetValue(mode, out tab) == true)
        {
            m_CurrentTab = mode;
            tab.OnTabClick();
        }
    }
    void OnTabClick(eGuildTabMode mode)
    {
        m_CurrentTab = mode;
        AllContentsDiable();
        GuildContentsBase content = null;
        if (Contents.TryGetValue(mode, out content) == false)
        {
            switch (mode)
            {
                case eGuildTabMode.GuildJoin:
                    content = GameObject.Instantiate(GuildJoinPrefab).GetComponent<GuildJoin>();
                    break;
                case eGuildTabMode.GuildCreate:
                    content = GameObject.Instantiate(GuildCreatePrefab).GetComponent<GuildCreate>();
                    break;
                case eGuildTabMode.GuildInfo:
                    content = GameObject.Instantiate(GuildInfoPrefab).GetComponent<UIGuildInfo>();
                    break;
                case eGuildTabMode.GuildBuff:
                    content = GameObject.Instantiate(GuildBuffPrefab).GetComponent<GuildBuff>();
                    break;
                case eGuildTabMode.RequestList:
                    content = GameObject.Instantiate(RequestListPrefab).GetComponent<GuildRequestList>();
                    break;
                case eGuildTabMode.GuildRank:
                    content = GameObject.Instantiate(GuildRankPrefab).GetComponent<GuildRank>();
                    break;
                case eGuildTabMode.GuildManagement:
                    content = GameObject.Instantiate(GuildManagementPrefab).GetComponent<GuildManagement>();
                    break;
                default:
                    Tooltip.Instance.ShowMessageKey("NotImplement");
                    return;
            }
            content.transform.SetParent(m_Contents.transform, false);
            content.transform.localPosition = Vector3.zero;
            content.transform.localScale = Vector3.one;
            Contents.Add(mode, content);
        }
        content.Init(this);
    }

    void AllContentsDiable()
    {
        foreach(var content in Contents)
        {
            content.Value.Uninit();
        }
    }

    void AllContentsClear()
    {
        AllContentsDiable();
        Contents.Clear();
    }
}

public class GuildContentsBase : MonoBehaviour
{
    protected Guild parent;
    virtual public void Init(Guild _parent)
    {
        parent = _parent;
        gameObject.SetActive(true);
    }
    virtual public void Uninit()
    {
        gameObject.SetActive(false);
    }

    virtual public void UpdateInfo()
    {

    }
}
