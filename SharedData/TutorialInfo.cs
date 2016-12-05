using MNS;
using System.Xml;
using System;
using PacketInfo;
using UnityEngine;
using System.Collections.Generic;

public enum eTutorialType
{
    Dialog,
    Click,
    Drag,
    Indicator,
    CutScene,
}

public class TutorialInfoManager : InfoManager<TutorialInfo, InfoBaseString, TutorialInfoManager>
{
    int CutSceneStartState = 100000;
    public short CompletedState = 0;
    public List<InfoBaseString> CutScenes = new List<InfoBaseString>();

    public List<string> PreloadCharacters { get; private set; }

    protected override void PostLoadData(XmlNode node)
    {
        base.PostLoadData(node);

        CutScenes.AddRange(m_Infos.FindAll(e=>e.IDN >= CutSceneStartState));
        m_Infos.RemoveAll(e => e.IDN >= CutSceneStartState);

        if (m_Infos.Count == 0)
            CompletedState = 0;
        else
            CompletedState = (short)m_Infos[Count - 1].IDN;

        PreloadCharacters = new List<string>();
        XmlNode preloadNode = node.SelectSingleNode("Preload");
        if (preloadNode != null)
        {
            foreach (XmlNode preload_character_node in preloadNode.SelectNodes("PreloadCharacter"))
            {
                if (preload_character_node.NodeType == XmlNodeType.Comment)
                    continue;

                PreloadCharacters.Add(preload_character_node.Attributes["id"].Value);
            }
        }
    }

    public TutorialInfo GetNextTutorial(TutorialInfo tutorial)
    {
        if (tutorial == null) return null;
        for(int i=0; i<Count; ++i)
        {
            if (m_Infos[i].IDN > tutorial.IDN)
                return m_Infos[i] as TutorialInfo;
        }
        return null;
    }
    public int GetNextTutorialState(TutorialInfo tutorial)
    {
        if (tutorial == null) return CompletedState;
        for (int i = 0; i < Count; ++i)
        {
            if (m_Infos[i].IDN > tutorial.IDN)
                return (short)m_Infos[i].IDN;
        }
        return CompletedState;
    }
    

    public TutorialInfo GetNextTutorial(int state)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (m_Infos[i].IDN > state)
                return m_Infos[i] as TutorialInfo;
        }
        return null;
    }

    public TutorialInfo GetCutScene(eSceneType scene, MapStageDifficulty stage_info)
    {
        return CutScenes.Find(e => (e as TutorialInfo).CutSceneInfo != null && (e as TutorialInfo).CutSceneInfo.StageInfo == stage_info 
        && (e as TutorialInfo).CutSceneInfo.SceneType == scene) as TutorialInfo;
    }
}

public enum eSceneType
{
    None,
    PreAll,
    PreCharacter,
    Post,
    PreAll_Wave3,
}

public class CutSceneInfo
{
    public eSceneType SceneType;
    string  map_id;
    int stage_index;
    public MapStageDifficulty StageInfo { get; private set; }

    public CutSceneInfo(XmlNode node)
    {
        XmlAttribute scene_type_attr = node.Attributes["scene_type"];
        if (scene_type_attr != null)
        {
            SceneType = (eSceneType)Enum.Parse(typeof(eSceneType), scene_type_attr.Value);
            map_id = node.Attributes["map_id"].Value;
            stage_index = int.Parse(node.Attributes["stage_index"].Value);

            StageInfo = MapInfoManager.Instance.GetInfoByID(map_id).Stages[stage_index].Difficulty[0];
        }
        else
            SceneType = eSceneType.None;
    }

}

public enum eConditionType
{
    ManaFull,
    BattleEndPopup,
    BattleStart,
}
public class TutorialConditionBase
{
    public eConditionType Type;
    public string CreatureID;

    public bool IsConditionOK { get; set; }
    public TutorialConditionBase(XmlNode node)
    {
        IsConditionOK = false;
        Type = (eConditionType)Enum.Parse(typeof(eConditionType), node.Attributes["type"].Value);
        switch(Type)
        {
            case eConditionType.ManaFull:
                CreatureID = node.Attributes["creature"].Value;
                break;
            case eConditionType.BattleEndPopup:
                break;
            case eConditionType.BattleStart:
                break;
        }
    }
}
//public class TutorialManaCondition : TutorialConditionBase
//{
//    public TutorialManaCondition(XmlNode node) : base(node)
//    {
//        //CreatureID = node.Attributes["creature"].Value;
//        //bManaFull = bool.Parse(node.Attributes["is_mana_full"].Value);
//    }
//}
//public class TutorialBattleEndCondition : TutorialConditionBase
//{
//    public TutorialBattleEndCondition(XmlNode node) : base(node)
//    {
//    }
//}

public class TutorialInfo : InfoBaseString
{
    public List<TargetInfo> Targets = new List<TargetInfo>();
    public float delay = 0.5f;
    public bool AfterNetworking { get; private set; }
    public TutorialConditionBase Condition = null;
    public CutSceneInfo CutSceneInfo = null;
    public List<RewardBase> rewards = new List<RewardBase>();

    override public void Load(XmlNode node)
    {
        ID = node.Attributes["state"].Value;
        IDN = int.Parse(ID);

        XmlAttribute networking_attr = node.Attributes["networking"];
        if (networking_attr != null)
            AfterNetworking = bool.Parse(networking_attr.Value);

        XmlAttribute delay_attr = node.Attributes["delay"];
        if (delay_attr != null)
            delay = float.Parse(delay_attr.Value);

        foreach (XmlNode child in node.SelectNodes("Item"))
        {
            Targets.Add(new TargetInfo(child));
        }

        XmlNode condition_node = node.SelectSingleNode("Condition");
        if(condition_node != null)
            Condition = new TutorialConditionBase(condition_node);

        this.CutSceneInfo = new CutSceneInfo(node);

        foreach (XmlNode child in node.SelectNodes("Reward"))
        {
            rewards.Add(new RewardBase(child));
        }
    }
}

public class TargetInfo
{
    public GameMenu Menu;
    public eTutorialType type;
    public string Name;
    public string Desc;
    public Vector2 pos;
    public Vector2 size;
    public string confirm_tag = "";
    public string gameobject = "";

    public string creature_id;
    public string animation;
    public string position;

    public bool is_shadow = false;

    public float drag_x;
    public float drag_y;

    public TargetInfo() { }
    public TargetInfo(XmlNode node) { Load(node); }
    public void Load(XmlNode node)
    {
        XmlAttribute menuAttr = node.Attributes["menu"];
        if(menuAttr != null)
            Menu = (GameMenu)Enum.Parse(typeof(GameMenu), menuAttr.Value);

        XmlAttribute typeAttr = node.Attributes["type"];
        if (typeAttr != null)
        {
            type = (eTutorialType)Enum.Parse(typeof(eTutorialType), typeAttr.Value);
            switch (type)
            {
                case eTutorialType.CutScene:
                case eTutorialType.Dialog:
                    {
                        Name = node.Attributes["name"].Value;
                        Desc = node.Attributes["desc"].Value.Replace("\\n", "\n");
                        creature_id = node.Attributes["creature"].Value;
                        if (creature_id.EndsWith("@shadow") == true)
                        {
                            is_shadow = true;
                            creature_id = creature_id.Substring(0, creature_id.Length - 7);
                        }

                        int find_index = creature_id.LastIndexOf('_');
                        animation = creature_id.Substring(find_index+1);
                        creature_id = creature_id.Substring(0, find_index);
                        position = node.Attributes["position"].Value;
                    }
                    break;

                case eTutorialType.Click:
                case eTutorialType.Indicator:
                    {
                        pos.x = float.Parse(node.Attributes["x"].Value);
                        pos.y = float.Parse(node.Attributes["y"].Value);
                    }
                    break;

                case eTutorialType.Drag:
                    {
                        pos.x = float.Parse(node.Attributes["x"].Value);
                        pos.y = float.Parse(node.Attributes["y"].Value);
                        size.x = float.Parse(node.Attributes["width"].Value);
                        size.y = float.Parse(node.Attributes["height"].Value);

                        drag_x = float.Parse(node.Attributes["drag_x"].Value);
                        drag_y = float.Parse(node.Attributes["drag_y"].Value);
                    }
                    break;
            }
        }
        XmlAttribute gameobject_attr = node.Attributes["gameobject"];
        if (gameobject_attr != null)
            gameobject = gameobject_attr.Value;

        XmlAttribute confirm_tag_attr = node.Attributes["confirm_tag"];
        if (confirm_tag_attr != null)
            confirm_tag = confirm_tag_attr.Value;
    }
}
