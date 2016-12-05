
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using PacketEnums;

public class Boss : MenuBase
{
    public GameObject m_btnGuildWar;
    public GameObject BossSpotPrefab;
    public GameObject BossLocation;
    public UILabel m_LabelBossDesc;

    //dummy
    public UICharacterContainer WorldBossContainer;

    static public short CalculateLevel(short boss_level, MapStageDifficulty stage_info)
    {
        var clear_data = MapClearDataManager.Instance.GetData(stage_info);
        if (clear_data != null)
            boss_level = (short)(boss_level + clear_data.clear_count);
        return boss_level;
    }

    static public short CalculateGrade(short boss_level)
    {
        return (short)System.Math.Min(6, boss_level / 20 + 1);
    }

    static public short CalculateEnchant(short boss_level)
    {
        switch (boss_level % 10)
        {
            case 0:
                return 0;

            case 1:
            case 2:
                return 1;

            case 3:
            case 4:
                return 2;

            case 5:
            case 6:
                return 3;

            case 7:
            case 8:
                return 4;

            case 9:
                return 5;
        }
        return 0;
    }

    void Start()
    {
    }

    void OnEnable()
    {
    }

    void OnHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_ChapterBoss_Title"),Localization.Get("Help_ChapterBoss"));
    }

    override public bool Init(MenuParams parms)
    {
        Init(parms.bBack == false);

        InitWorldBoss();

        m_btnGuildWar.gameObject.SetActive(true);

        return true;
    }

    public void InitWorldBoss()
    {
        var event_info = EventHottimeManager.Instance.GetInfoByID("worldboss", true);
        if (event_info != null && (event_info.state == PacketInfo.pe_EventHottimeState.Hottime || event_info.state == PacketInfo.pe_EventHottimeState.WaitingHottime))
        {
            var map_info = MapInfoManager.Instance.GetInfoByIdn(event_info.Value);
            if (map_info != null)
            {
                var world_boss_info = map_info.Stages[0].Difficulty[0].Waves[0].Creatures.Find(c => c.CreatureType == eMapCreatureType.WorldBoss);
                if (world_boss_info != null)
                {
                    WorldBossContainer.transform.parent.gameObject.SetActive(true);

                    WorldBossContainer.Init(AssetManager.GetCharacterAsset(world_boss_info.CreatureInfo.ID, world_boss_info.SkinName), UICharacterContainer.Mode.UI_Normal, event_info.state == PacketInfo.pe_EventHottimeState.Hottime?"idle":"disabled");
                    return;
                }
            }
        }
        WorldBossContainer.transform.parent.gameObject.SetActive(false);
    }

    public void OnCharacterClick()
    {
        MapInfo map_info = MapInfoManager.Instance.GetInfoByID("2001_worldboss_golem");
        var condition = map_info.CheckCondition(pe_Difficulty.Normal);
        if (condition != null)
        {
            Tooltip.Instance.ShowMessage(condition.Condition);
            return;
        }

        GameMain.Instance.ChangeMenu(GameMenu.WorldBossInfo);
    }

    List<BossSpot> m_Spots = new List<BossSpot>();
    void Init(bool is_new)
    {
        MapInfo map_info = MapInfoManager.Instance.GetInfoByID("10001_boss");

        m_LabelBossDesc.text = Localization.Format("LeftTryCount", (map_info.TryLimit - MapClearDataManager.Instance.GetMapDailyClearCount(map_info.IDN, pe_Difficulty.Normal)), map_info.TryLimit);

        List<MapStageDifficulty> stages = map_info.Stages.Select(e=>e.Difficulty[0]).ToList();
        for (int i = 0; i < stages.Count; ++i)
        {
            MapStageDifficulty stage_info = stages[i];
            BossSpot stage;
            if (m_Spots.Count < i + 1)
            {
                GameObject dungeon = NGUITools.AddChild(BossLocation, BossSpotPrefab);
                stage = dungeon.GetComponent<BossSpot>();
                m_Spots.Add(stage);
            }
            else
                stage = m_Spots[i];

            stage.transform.localPosition = stage_info.MapPos;
            stage.gameObject.name = stage_info.ID;

            stage.Init(stage_info);
        }
    }

    override public void UpdateMenu()
    {
        Init(false);
    }

    public void OnClickBossStore()
    {
        GameMain.MoveStore("Boss");
    }

    public void OnGuildWar()
    {
        GameMain.SetBattleMode(eBattleMode.RVR);
    }
}
