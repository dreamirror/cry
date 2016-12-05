using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PacketInfo;
using PacketEnums;
using System;

public class Dungeon : MenuBase
{
	public static readonly Color hardColor = new Color32(180, 100, 100, 120);

	public GameObject DungeonSpotPrefab;
	public UICharacterContainer m_CharacterContainer;

	public GameObject DungeonLocation;

	public UIButton m_Left, m_Right;
	public UILabel m_LabelTitle;

	public GameObject m_ClearReward;
	public UILabel m_LabelClearTotal;
	public UILabel [] m_LabelCondition;
	public GameObject[] m_Boxes;
	public GameObject[] m_BoxEffects;
	public UIProgressBar m_RewardProgress;

	public UIToggle m_DifficultyToggle;

	public UIToggle m_ToggleDifficultyNormal;
	public UIToggle m_ToggleDifficultyHard;
	public GameObject m_HardModeLock;

    public UIEventTrigger m_EventTrigger;
	public MapInfo MapInfo
	{
		get
		{
			return m_SelectedMapInfo;
		}
	}

	MapStageDifficulty m_SelectStageInfo = null;
	MapInfo m_SelectedMapInfo = null;
	pe_Difficulty CurrentDifficulty;

    Vector2 m_DragStart;
    public void _DragStart()
    {
        m_DragStart = UICamera.lastTouchPosition;
    }
    public void _DragEnd()
    {
        if (UICamera.lastTouchPosition.x - m_DragStart.x > 50)
            OnLeftClick();
        else if (m_DragStart.x  - UICamera.lastTouchPosition.x > 50)
            OnRightClick();

        m_DragStart = Vector2.zero;
    }
    ////////////////////////////////////////////////////////////////
    override public bool Init(MenuParams parms)
	{
		string menu_parm_1 = parms.GetObject<string>("menu_parm_1");
		string menu_parm_2 = parms.GetObject<string>("menu_parm_2");

		if (string.IsNullOrEmpty(menu_parm_2) == false && string.IsNullOrEmpty(menu_parm_1) == false)
		{
			CurrentDifficulty = (pe_Difficulty)Enum.Parse(typeof(pe_Difficulty), menu_parm_2);

			m_SelectedMapInfo = MapInfoManager.Instance.GetInfoByID(menu_parm_1);
			m_SelectStageInfo = m_SelectedMapInfo.Stages[0].Difficulty[(int)CurrentDifficulty];
		}

		if (Network.LastOpenContentsStageInfo != null)
		{
			Tooltip.Instance.CheckOpenContentsMapStageClear(Network.LastOpenContentsStageInfo);
			Network.LastOpenContentsStageInfo = null;
		}

		if (Network.NewStageInfo != null)
		{
			m_SelectStageInfo = Network.NewStageInfo;
			m_SelectedMapInfo = m_SelectStageInfo.MapInfo;

			Network.NewStageInfo = null;
			CurrentDifficulty = m_SelectStageInfo.Difficulty;
		}
		else if (m_SelectStageInfo == null)
		{
			var last_main_stage = MapClearDataManager.Instance.GetLastMainStage();
			if (last_main_stage == null)
			{
				CurrentDifficulty = pe_Difficulty.Normal;
				m_SelectedMapInfo = MapInfoManager.Instance.Values.First();
				m_SelectStageInfo = m_SelectedMapInfo.Stages[0].Difficulty[(int)pe_Difficulty.Normal];
			}
			else
			{
				m_SelectedMapInfo = MapInfoManager.Instance.GetInfoByIdn(last_main_stage.map_idn);
				m_SelectStageInfo = m_SelectedMapInfo.Stages[last_main_stage.stage_index].Difficulty[(int)last_main_stage.difficulty];
				CurrentDifficulty = m_SelectStageInfo.Difficulty;
			}
		}

		m_ToggleDifficultyNormal.value = CurrentDifficulty == pe_Difficulty.Normal;
		m_ToggleDifficultyHard.value = CurrentDifficulty == pe_Difficulty.Hard;
		m_DifficultyToggle.value = CurrentDifficulty == pe_Difficulty.Hard;
		Init();

		return true;
	}
	override public bool Uninit(bool bBack)
	{
		base.Uninit();

		if (bBack)
		{
			m_SelectedMapInfo = null;
			m_SelectStageInfo = null;
		}
		return true;
	}
	override public void UpdateMenu()
	{
		Init();
	}

	////////////////////////////////////////////////////////////////

	// Use this for initialization
	void Start () {
		if (GameMain.Instance != null)
			GameMain.Instance.InitTopFrame();

        m_EventTrigger.onDragStart.Add(new EventDelegate(_DragStart));
        m_EventTrigger.onDragEnd.Add(new EventDelegate(_DragEnd));
    }

	void Update()
	{
	}

	List<DungeonSpot> m_Spots = new List<DungeonSpot>();
	void Init()
	{
		MapInfo map_info = MapInfo;
		List<MapStageDifficulty> stages = map_info.Stages.Select(e=>e.Difficulty[(int)CurrentDifficulty]).ToList();

		pd_MapClearData last_stage_info = null;
		if (map_info != m_SelectStageInfo.MapInfo)
			last_stage_info = MapClearDataManager.Instance.GetLastStage(map_info.IDN, CurrentDifficulty);

		bool is_set_character_position = false;
		for (int i = 0; i < stages.Count; ++i)
		{
			MapStageDifficulty stage_info = stages[i];
			DungeonSpot stage;
			if (m_Spots.Count < i + 1)
			{
				GameObject dungeon = NGUITools.AddChild(DungeonLocation, DungeonSpotPrefab);
				stage = dungeon.GetComponent<DungeonSpot>();
				m_Spots.Add(stage);
			}
			else
				stage = m_Spots[i];

			stage.transform.localPosition = stage_info.MapPos;
			stage.gameObject.name = stage_info.ID;

			pd_MapClearData clear_data = MapClearDataManager.Instance.GetData(stage_info);
			short clear_rate = clear_data == null ? (short)0 : clear_data.clear_rate;
			stage.Init(stage_info, clear_rate);

			if (last_stage_info != null && stage_info.StageIndex == last_stage_info.stage_index || stage_info == m_SelectStageInfo || is_set_character_position == false && clear_rate == 0)
			{
				SetCharacterPosition(stage.transform.position);
				is_set_character_position = true;
			}

			stage.OnClickStage = OnClickStage;
		}
		while (m_Spots.Count > stages.Count)
		{
			DungeonSpot spot = m_Spots[m_Spots.Count - 1];
			m_Spots.Remove(spot);
			spot.gameObject.SetActive(false);
			Destroy(spot);
		}
		m_LabelTitle.text = map_info.GetShowName(CurrentDifficulty);
		if (CurrentDifficulty == pe_Difficulty.Hard)
		{
			GameMain.Instance.m_BG.material.SetColor("_GrayColor", GameMain.colorHard);
		}
		else
		{
			GameMain.Instance.m_BG.material.SetColor("_GrayColor", GameMain.colorZero);
		}

		//clear reward
		MapClearRewardInfo reward_info = MapClearRewardInfoManager.Instance.GetInfoByIdn(map_info.IDN);
		if (reward_info.conditions(CurrentDifficulty).Count > 0)
		{
			m_ClearReward.SetActive(true);
			int total_clear = MapClearDataManager.Instance.GetTotalClearRate(map_info.IDN, CurrentDifficulty);
			m_RewardProgress.value = (float)total_clear / reward_info.Total;
			m_LabelClearTotal.text = total_clear.ToString();

			pd_MapClearReward rewarded_info = MapClearRewardManager.Instance.GetRewardedData(m_SelectedMapInfo.IDN, CurrentDifficulty);

			for (int i = 0; i < reward_info.conditions(CurrentDifficulty).Count; ++i)
			{
				m_LabelCondition[i].text = reward_info.conditions(CurrentDifficulty)[i].condition.ToString();

				if (rewarded_info == null || rewarded_info.GetAt(i) == false)
				{
					m_Boxes[i].GetComponentInChildren<UIToggleSprite>().SetSpriteActive(false);
					m_Boxes[i].GetComponent<BoxCollider2D>().enabled = true;
					m_BoxEffects[i].SetActive(GetClearRewardAvailable(i));
				}
				else
				{//rewarded
					m_Boxes[i].GetComponentInChildren<UIToggleSprite>().SetSpriteActive(true);
					m_Boxes[i].GetComponent<BoxCollider2D>().enabled = false;
					m_BoxEffects[i].SetActive(false);
				}
			}
		}
		else
			m_ClearReward.SetActive(false);


		m_Left.gameObject.SetActive(MapInfo.IDN > 1);
		m_Right.gameObject.SetActive(MapInfoManager.Instance.ContainsIdn(MapInfo.IDN + 1) && MapInfo.IDN+1 <= GameConfig.Get<int>("contents_open_main_map"));

		m_HardModeLock.SetActive(MapInfo.IDN ==1 && MapInfo.CheckCondition(pe_Difficulty.Hard) != null);
	}

	public void OnLeftClick()
	{
		m_SelectedMapInfo = MapInfoManager.Instance.GetInfoByIdn(MapInfo.IDN - 1);
		GameMain.Instance.ChangeMenu(GameMenu.Dungeon, null);
	}

	public void OnRightClick()
	{
		var next_map = MapInfoManager.Instance.GetInfoByIdn(MapInfo.IDN + 1);

		var condition = next_map.CheckCondition(CurrentDifficulty);
		if (condition != null)
		{
			Tooltip.Instance.ShowMessage(condition.Condition);
			return;
		}

		m_SelectedMapInfo = MapInfoManager.Instance.GetInfoByIdn(MapInfo.IDN + 1);
		GameMain.Instance.ChangeMenu(GameMenu.Dungeon, null);
	}

	public void OnDifficultChanged(GameObject obj)
	{
		UIToggle toggle = obj.GetComponentInChildren<UIToggle>();
		//Debug.LogFormat("OnDifficultChanged({0}:{1})", obj.name, toggle.value);
		if(toggle.value == true)
		{
			switch (obj.name)
			{
				case "NormalMap":
					{
						if (CurrentDifficulty == pe_Difficulty.Normal) return;
						CurrentDifficulty = pe_Difficulty.Normal;
						m_DifficultyToggle.value = false;
					}
					break;

				case "HardMap":
					{
						if (CurrentDifficulty == pe_Difficulty.Hard) return;

						if (MapInfo.IDN == 1)
						{
							var condition = MapInfo.CheckCondition(pe_Difficulty.Hard);
							if (condition != null)
							{
								Tooltip.Instance.ShowMessage(condition.Condition);
								m_ToggleDifficultyNormal.value = true;
								return;
							}
						}

						CurrentDifficulty = pe_Difficulty.Hard;
						m_DifficultyToggle.value = true;
					}
					break;
			}
			var last_main_stage = MapClearDataManager.Instance.GetLastMainStage(CurrentDifficulty);
			if (last_main_stage == null)
			{
				m_SelectedMapInfo = MapInfoManager.Instance.Values.First();
				m_SelectStageInfo = m_SelectedMapInfo.Stages[0].Difficulty[(int)CurrentDifficulty];
			}
			else
			{
				m_SelectedMapInfo = MapInfoManager.Instance.GetInfoByIdn(last_main_stage.map_idn);
				m_SelectStageInfo = m_SelectedMapInfo.Stages[last_main_stage.stage_index].Difficulty[(int)CurrentDifficulty];
			}
			GameMain.Instance.ChangeMenu(GameMenu.Dungeon, null);
		}
	}

	void SetCharacterPosition(Vector3 position)
	{
		if (m_CharacterContainer.IsInit == false)
		{
            var leader_creature = Network.PlayerInfo.leader_creature;
            var creature_info = leader_creature.GetCreatureInfo();

            m_CharacterContainer.Init(AssetManager.GetCharacterAsset(creature_info.ID, creature_info.GetSkinName(Network.PlayerInfo.leader_creature.leader_creature_skin_index)), UICharacterContainer.Mode.UI_Battle);
		}
		m_CharacterContainer.transform.parent.position = position;
	}

	void OnClickStage(DungeonSpot spot)
	{
		//        SetCharacterPosition(spot.transform.position);
		m_SelectStageInfo = spot.StageInfo;

		MenuParams parms = new MenuParams();
		parms.AddParam<MapStageDifficulty>(spot.StageInfo);
		GameMain.Instance.ChangeMenu(GameMenu.DungeonInfo, parms);
	}

	GameObject m_SelectedBtn = null;
	public void OnClickBox(GameObject btn)
	{
		short reward_index = 0;
		switch (btn.name)
		{
			case "Box_1":
				reward_index = 0;
				break;
			case "Box_2":
				reward_index = 1;
				break;
			case "Box_3":
				reward_index = 2;
				break;
		}
		if (GetClearRewardAvailable(reward_index) == true)
		{
			m_SelectedBtn = btn;
			C2G.MapClearReward packet = new C2G.MapClearReward();
			packet.map_id = m_SelectedMapInfo.ID;
			packet.index = reward_index;
			packet.difficulty = CurrentDifficulty;
			Network.GameServer.JsonAsync<C2G.MapClearReward, C2G.MapClearRewardAck>(packet, OnMapClearRewardHandler);
		}
		else
		{
			Popup.Instance.Show(ePopupMode.Reward, MapClearRewardInfoManager.Instance.GetInfoByIdn(m_SelectedMapInfo.IDN).conditions(CurrentDifficulty)[reward_index].rewards, Localization.Get("PopupRewardTitle"), Localization.Get("MapClearReward_Available"));
		}
	}

	public void OnFinishTween(UIToggleSprite sprite)
	{
		sprite.SetSpriteActive(true);
		BoxCollider2D parent = sprite.GetComponentInParent<BoxCollider2D>();

		string reward_desc = "";
		short reward_index = 0;

		switch (parent.name)
		{
			case "Box_1":
				reward_desc = Localization.Get("MapClearReward_1");
				reward_index = 0;
				break;

			case "Box_2":
				reward_desc = Localization.Get("MapClearReward_2");
				reward_index = 1;
				break;

			case "Box_3":
				reward_desc = Localization.Get("MapClearReward_3");
				reward_index = 2;
				break;
		}

		parent.enabled = false;
		m_BoxEffects[reward_index].SetActive(false);

		GameMain.Instance.UpdatePlayerInfo();

		List<RewardBase> rewards = MapClearRewardInfoManager.Instance.GetInfoByIdn(m_SelectedMapInfo.IDN).conditions(CurrentDifficulty)[reward_index].rewards;
		Popup.Instance.Show(ePopupMode.Reward, rewards, Localization.Get("PopupRewardTitle"), reward_desc, m_temp_map_clear_reward_ack);

		m_temp_map_clear_reward_ack = null;
	}

	public bool GetClearRewardAvailable(int idx)
	{
		int total_clear = MapClearDataManager.Instance.GetTotalClearRate(m_SelectedMapInfo.IDN, CurrentDifficulty);
		MapClearRewardInfo map_clear_reward_info = MapClearRewardInfoManager.Instance.GetInfoByIdn(m_SelectedMapInfo.IDN);
		pd_MapClearReward rewarded_info = MapClearRewardManager.Instance.GetRewardedData(m_SelectedMapInfo.IDN, CurrentDifficulty);
		return (rewarded_info == null || rewarded_info.GetAt(idx) == false) && map_clear_reward_info.CheckCondition(idx, total_clear, CurrentDifficulty);
	}

	C2G.Reward3Ack m_temp_map_clear_reward_ack = null;
	public void OnMapClearRewardHandler(C2G.MapClearReward packet, C2G.MapClearRewardAck ack)
	{
		m_temp_map_clear_reward_ack = ack.reward_ack;

		Network.Instance.ProcessReward3Ack(ack.reward_ack);
		MapClearRewardManager.Instance.SetReward(packet.map_id, packet.index, packet.difficulty);

		m_SelectedBtn.GetComponent<BoxCollider2D>().enabled = false;

		UIPlayTween tween = m_SelectedBtn.GetComponentInChildren<UIPlayTween>();
		tween.Play(true);
	}

}
