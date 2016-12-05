using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

public class BattleRaid : Battle
{
    static new public BattleRaid Instance { get; protected set; }

    public UIAtlas m_AtlasCharacterShots, m_AtlasPF;

    // raid
    public PrefabManager m_RaidPlayerPrefabManager;
    public GameObject RaidCanvas;
    List<RaidPlayer> m_RaidPlayers;

    public PrefabManager m_RaidUIPlayerPrefabManager;
    public GameObject RaidUIContainer;
    RaidUIPlayer m_MineUI;

    public int column_count = 4;
    public int row_count = 10;
    public Vector3 StartPosition = Vector3.zero;
    public float RowGap = 50f, ColumnGap = 30f;

    override protected void Awake()
    {
        base.Awake();
        BattleRaid.Instance = this;
    }

    protected override void Start()
    {
        m_TeamData = TeamDataManager.Instance.GetTeam(Network.BattleStageInfo.TeamID);

        base.Start();
        m_BG.material.mainTexture = AssetManager.LoadBG(Network.BattleStageInfo.BG_ID);

        enemies = new List<ICreature>();
        List<MapCreatureInfo> enemy_infos = Network.BattleStageInfo.Waves[0].Creatures;

        for (int i = 0; i < enemy_infos.Count; ++i)
        {
            if (enemy_infos[i].CreatureInfo == null)
                continue;

            BattleCreature creature = new BattleCreature(enemy_infos[i], battle_layout.m_Enemy.m_Characters[i], 0f, character_hpbar_prefab, character_skill_prefab);
            enemies.Add(creature);
        }

        InitRaid();

        update_mana_creature_index = 0;
        update_mana_enemy_index = 0;

        battleEndType = pe_EndBattle.Invalid;
        IsBattleEnd = false;
        PlaybackTime = 0f;

        first_tick = 0;

        InitCreatures();

        SoundManager.Instance.PlayBGM("PVP");
    }

    protected override void Update()
    {
        if (BattleBase.CurrentBattleMode == eBattleMode.None)
            return;

        if (CheckFirstTick() == false)
            return;

        base.Update();

        if (Time.timeScale == 0f) return;
        if (IsPause == ePauseType.Pause)
            return;

        m_RaidPlayers.ForEach(p => p.UpdatePlayer());

        if (IsBattleEnd == true)
        {
            if (_battle_end_param != null && Time.time > ShowPopupTime && ShowPopupTime != 0f)
            {
                ShowPopupTime = 0f;

                OnFinishBattle(false);

                Popup.Instance.Show(ePopupMode.BattleEnd, _battle_end_param);
            }
        }
    }

    override protected void Clear()
    {
        base.Clear();
        m_RaidPlayerPrefabManager.Clear();
        m_RaidUIPlayerPrefabManager.Clear();
        m_RaidPlayers.Clear();
    }

    void InitRaid()
    {
        if (m_AtlasCharacterShots.spriteMaterial == null)
            m_AtlasCharacterShots.replacement = AssetManager.LoadCharacterShotsAtlas();

        m_RaidPlayers = new List<RaidPlayer>();
        Vector3 position = StartPosition;
        long account_idx = 1000000;
        for (int i = 0; i < row_count; ++i)
        {
            position.x = StartPosition.x;
            for (int j = 0; j < column_count; ++j)
            {
                if (i == 1 && j == 0)
                {
                    position.x -= ColumnGap;
                    continue;
                }

                var player = m_RaidPlayerPrefabManager.GetNewObject<RaidPlayer>(RaidCanvas.transform, position);

                string profile_name = m_AtlasPF.spriteList[MNS.Random.Instance.NextRange(0, m_AtlasPF.spriteList.Count - 1)].name;

                player.Init(account_idx, profile_name, "Player" + MNS.Random.Instance.NextRange(1000, 2000), MNS.Random.Instance.NextRange(5, 20));
                m_RaidPlayers.Add(player);
                position.x -= ColumnGap;
            }
            position.z -= RowGap;
        }

//         Vector3 ui_position = Vector3.zero;
//         {
//             string profile_name = string.Format("profile_{0}", Network.LeaderCreatureInfo.ID);
//             m_MineUI = m_RaidUIPlayerPrefabManager.GetNewObject<RaidUIPlayer>(RaidUIContainer.transform, ui_position);
//             m_MineUI.Init(true, true, true, profile_name, Network.PlayerInfo.nickname, Network.PlayerInfo.player_level, 0, 0, 0f);
//         }
    }

    public override void SetBattleEnd()
    {
        base.SetBattleEnd();

        C2G.EndBattle _packet = new C2G.EndBattle();
        _packet.battle_type = pe_Battle.Stage;
        _packet.end_type = pe_EndBattle.Win;
        _packet.difficulty = Network.BattleStageInfo.Difficulty;
        _packet.map_id = Network.BattleStageInfo.MapInfo.ID;
        _packet.stage_id = Network.BattleStageInfo.ID;

        _packet.creatures = new List<PacketInfo.pd_BattleEndCreatureInfo>();

        //List<int> skill_indice = new List<int>();
        for (int i = 0; i < m_TeamData.Creatures.Count; ++i)
        {
            Creature creature = m_TeamData.Creatures[i].creature;
            BattleCreature bc = characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            if (bc == null) bc = dead_characters.Find(c => c.Idx == creature.Idx) as BattleCreature;
            _packet.creatures.Add(new PacketInfo.pd_BattleEndCreatureInfo(bc.Idx, bc.IsDead));
            m_TeamData.Creatures[i].auto_skill_index = bc.AutoSkillIndex;
        }
        _packet.team_data = m_TeamData.CreateSaveData();

        Network.GameServer.JsonAsync<C2G.EndBattle, C2G.EndBattleAck>(_packet, OnEndBattleHandler);
    }

    protected EventParamBattleEnd _battle_end_param = null;
    void OnEndBattleHandler(C2G.EndBattle packet, C2G.EndBattleAck ack)
    {
        MapStageDifficulty stage_info = Network.BattleStageInfo;

        _battle_end_param = new EventParamBattleEnd();
        _battle_end_param.end_type = packet.end_type;
        _battle_end_param.is_boss = stage_info.MapInfo.MapType == "boss";
        _battle_end_param.maxlevel_reward_mail_idxs = ack.maxlevel_reward_mail_idx;
        if (packet.end_type != pe_EndBattle.Exit)
        {
            Network.PlayerInfo.UseEnergy(stage_info.Energy);

            _battle_end_param.player_levelup = Network.PlayerInfo.UpdateExp(ack.player_exp_add_info);
            _battle_end_param.creatures = new List<BattleEndCreature>();

            m_TeamData.Creatures.ForEach(c => _battle_end_param.creatures.Add(c.creature.UpdateExp(ack.creature_exp_add_infos.Find(i => i.creature_idx == c.creature.Idx))));
            _battle_end_param.add_goods = ack.add_goods;
            _battle_end_param.loot_items = ack.loot_items;
            _battle_end_param.loot_runes = ack.loot_runes;

            _battle_end_param.loot_creatures = ack.loot_creatures.Select(c => c.creature_idx).ToList();

            ack.loot_items.ForEach(i => ItemManager.Instance.Add(i));
            ack.loot_runes.ForEach(i => RuneManager.Instance.Add(i));
            ack.add_goods.ForEach(g => Network.PlayerInfo.AddGoods(g));
            Network.Instance.LootCreatures(ack.loot_creatures, ack.loot_creatures_equip);

            //int dead_count = dead_characters.Count;
            _battle_end_param.clear_rate = 3;

            MapClearDataManager.Instance.SetClearRate(stage_info, _battle_end_param.clear_rate);
        }
        GameMain.SetBattleMode(eBattleMode.None);
    }
}
