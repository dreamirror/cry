using UnityEngine;
using System.Collections.Generic;
using PacketEnums;
using System;

public enum eTooltipMode
{
    Message,
    TargetMessage,
    IconMessage,
    RewardItem,
    OpenContents,
    Energy,
    LeaderCharacter,
    Help,
    ScreenLock,
    Mission,
    MissionProgress,
    Character,
    Profile,
    ChatChannel,
    ChatFillter,
    TagCharacter,
    Reward,
}

public class Tooltip : MonoBehaviour
{
    static Tooltip m_Instance = null;
    static public Tooltip Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = ((GameObject)GameObject.Instantiate(Resources.Load("Prefab/Menu/Tooltip"))).GetComponent<Tooltip>();
                m_Instance.gameObject.SetActive(true);

                GameObject.DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    public GameObject contents;

    List<TooltipBase> tooltips = new List<TooltipBase>();
    List<TooltipBase> mission_tooltips = new List<TooltipBase>();

//     float showTime = 0f;
//     TooltipBase CurrentTooltip = null;
    void Update()
    {
    }
    GameObject Load(eTooltipMode mode)
    {
        GameObject result = null;

        string classname = string.Format("Tooltip{0}", mode.ToString());
        string path = string.Format("Prefab/Tooltip/{0}", classname);
        UnityEngine.Object loadPrefab = Resources.Load(path, typeof(GameObject));
        if (loadPrefab != null)
        {
            result = (Instantiate(loadPrefab) as GameObject);
            result.transform.parent = contents.transform;
            result.transform.localPosition = Vector3.zero;
            result.transform.localScale = Vector3.one;

            result.SetActive(false);
            return result;
        }
        else
            Debug.LogWarning("Resources.Load() failed. " + path);

        return result;
    }

    public void ShowTooltip(eTooltipMode mode, params object[] objs)
    {
        TooltipBase tooltip = Load(mode).GetComponent<TooltipBase>();
        tooltip.Init(objs);
        switch (mode)
        {
            case eTooltipMode.Message:
            case eTooltipMode.IconMessage:
                var find_tooltip = tooltips.Find(e => e.Compare(tooltip));
                if (find_tooltip != null && tooltips[0] == find_tooltip)
                {
                    tooltip.OnFinishedCallback = OnFinishedCallback;
                    tooltips.Insert(0, tooltip);
                    find_tooltip.OnFinished();
                }
                else
                {
                    tooltip.OnFinishedCallback = OnFinishedCallback;
                    if (tooltips.Count == 0)
                        tooltip.Play();

                    tooltips.Add(tooltip);
                }

                break;
            case eTooltipMode.MissionProgress:

                tooltip.OnFinishedCallback = OnFinishedCallbackForMission;
                mission_tooltips.Add(tooltip);
                if (mission_tooltips.Count == 1)
                    tooltip.Play();
                break;

            default:
                tooltip.Play();
                break;
        }
    }
    void OnFinishedCallback(TooltipBase tooltip)
    {
        tooltips.Remove(tooltip);
        if (tooltips.Count > 0)
            tooltips[0].Play();
    }
    void OnFinishedCallbackForMission(TooltipBase tooltip)
    {
        mission_tooltips.Remove(tooltip);
        if (mission_tooltips.Count > 0)
            mission_tooltips[0].Play();
    }
    public void ShowMessageKey(string key)
    {
        ShowTooltip(eTooltipMode.Message, Localization.Get(key));
    }

    public void ShowMessageKeyFormat(string key, params object[] param)
    {
        ShowTooltip(eTooltipMode.Message, Localization.Format(key, param));
    }

    public void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message) == false)
            ShowTooltip(eTooltipMode.Message, message);
    }

    public void ShowTarget(string message, object target)
    {
        ShowTooltip(eTooltipMode.TargetMessage, message, target);
    }

    public void ShowItemMade(ItemInfoBase item_info)
    {
        ShowTooltip(eTooltipMode.IconMessage, item_info.IconID, Localization.Format("MadeItem", item_info.Name));
    }

    public void ShowRewardItem(RewardItem item)
    {
        ShowTooltip(eTooltipMode.RewardItem, item);
//         switch (item.Info.ItemType)
//         {
//             case eItemType.Stuff:
//                 ShowTooltip(eTooltipMode.RewardItem, item);
//                 break;
// 
//             default:
//                 ShowTooltip(eTooltipMode.TargetMessage, item.GetTooltip(), item.GetComponent<SHTooltip>());
//                 break;
//         }
    }
    
    public void CheckOpenContentsMapStageClear(MapStageDifficulty map_stage_info)
    {
        if (Tutorial.Instance.Completed == false)
            return;

        List<ContentsOpenInfo> opens = new List<ContentsOpenInfo>();
        pe_Difficulty difficulty = map_stage_info.Difficulty;
        if (map_stage_info.MapInfo.Stages[map_stage_info.MapInfo.Stages.Count-1].Difficulty[(int)difficulty] == map_stage_info)
        {
            // map clear
            MapInfoManager.Instance.CheckOpenContents(ref opens, eMapCondition.MapClear, map_stage_info.MapInfo.ID, difficulty.ToString());
        }

        // map stage clear
        MapInfoManager.Instance.CheckOpenContents(ref opens, eMapCondition.MapStageClear, map_stage_info.ID, difficulty.ToString());

        if (opens.Count > 0)
            ShowTooltip(eTooltipMode.OpenContents, opens);
    }

    public void ShowMessageKey(string key, Vector3 pos)
    {
        ShowTooltip(eTooltipMode.Message, Localization.Get(key), pos);
    }

    public void ShowEnergy(object target)
    {
        ShowTooltip(eTooltipMode.Energy, target);
    }

    public void ShowLeaderCharacter(EventDelegate _del)
    {
        ShowTooltip(eTooltipMode.LeaderCharacter, _del);
    }

    public void ShowHelp(string title, string help)
    {
        ShowTooltip(eTooltipMode.Help, title, help);
    }

    public void ShowScreenLock()
    {
        ShowTooltip(eTooltipMode.ScreenLock);
    }

    public bool IsShowTooltip()
    {
        foreach (TooltipBase tooltip in contents.GetComponentsInChildren<TooltipBase>())
        {
            if (tooltip.ExternalClose && (tooltip as TooltipMission) == null)
                return true;
        }
        return false;
        //TooltipBase tooltip = contents.GetComponentInChildren<TooltipBase>();
        //return tooltip != null && (tooltip as TooltipMission) == null;
    }
    public void CloseAllTooltip()
    {
        foreach (TooltipBase tooltip in contents.GetComponentsInChildren<TooltipBase>())
        {
            if (tooltip.ExternalClose && (tooltip as TooltipMission) == null)
                tooltip.OnFinished();
        }
    }

    public void CloseTooltip(eTooltipMode mode)
    {
        foreach (TooltipBase tooltip in contents.GetComponentsInChildren<TooltipBase>())
        {
            if (tooltip.Mode == mode)
                tooltip.OnFinished();
        }
    }

    public bool IsShow(eTooltipMode mode)
    {
        foreach (TooltipBase tooltip in contents.GetComponentsInChildren<TooltipBase>())
        {
            if (tooltip.Mode == mode)
                return true;
        }
        return false;
    }
}


public class TooltipBase : MonoBehaviour
{
    public UIPlayTween m_Tween;
    protected object[] parms;

    public bool ExternalClose = true;

    public eTooltipMode Mode { get { return (eTooltipMode)Enum.Parse(typeof(eTooltipMode), GetType().Name.Substring(7)); } }
    public virtual string CompareValue { get { return "TooltipBase"; } }
    public Action<TooltipBase> OnFinishedCallback = null;
    protected EventDelegate finished = null;
    void Start()
    {
        if (m_Tween != null)
        {
            finished = new EventDelegate(OnFinished);
            m_Tween.onFinished.Add(finished);
        }
    }
    public virtual void Init(params object[] parms)
    {
        this.parms = parms;
        Debug.LogWarning("Not Implement Init");
    }
    public virtual bool Compare(TooltipBase tooltip)
    {
        return Mode == tooltip.Mode && CompareValue.Equals(tooltip.CompareValue);
    }
    public virtual void Play()
    {
        gameObject.SetActive(true);
        if (m_Tween != null)
            m_Tween.Play(true);
    }
    public virtual void OnFinished()
    {
        if(finished != null)
            m_Tween.onFinished.Remove(finished);

        if(OnFinishedCallback != null)
        {
            OnFinishedCallback(this);
        }
        Destroy(gameObject);
    }
}