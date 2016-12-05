using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("SmallHeroes/UI/UICharacterContainer")]
public class UICharacterContainer : UIWidget
{
	override public eWidgetDepth DepthType { get { return eWidgetDepth.Character; } }

	public enum Mode
	{
		UI_Normal,
		UI_Battle,
		Normal,
		Battle,
	}

	public string mDepthGroupID;
	public override string DepthID
	{
		get
		{
			return mDepthGroupID;
		}
	}

	Character m_Character = null;
	public Character Character
	{
		get
		{
			if (CharacterAsset != null)
				return CharacterAsset.Asset;
			if (m_Character == null)
				m_Character = transform.GetComponentInChildren<Character>();
			return m_Character;
		}
	}

	public enum ePlayType
	{
		None,
		Social,
		Random,
	}
	ePlayType m_PlayType = ePlayType.None;

	public bool IsActive { get { return CharacterAsset != null && CharacterAsset.Asset != null; } }

	public GameObject Info;

	public CharacterAssetContainer CharacterAsset { get; private set; }
	public bool IsInit { get { return CharacterAsset != null; } }

	string m_ActionName;
	bool m_SetDefaultAction = false;
	Mode m_Mode = Mode.Battle;

	public bool IsDrag { get; set; }

	public void Init(CharacterAssetContainer character, Mode mode, string action_name=null, bool set_default_action = false)
	{
		if (IsInit == true)
			Uninit();

		CharacterAsset = character;
		m_Mode = mode;
		m_ActionName = action_name;
		m_SetDefaultAction = set_default_action;
		m_PlaybackTime = 0f;

		SetActive(false);
		if (gameObject.activeInHierarchy == true)
			SetActive(true);
		m_PlayType = ePlayType.None;
	}

	public void Uninit()
	{
		SetActive(false);
		if (CharacterAsset != null && CharacterAsset.Asset != null && CharacterAsset.Asset.Creature != null)
			CharacterAsset.Asset.Creature.Container = null;
		CharacterAsset = null;
	}

	float m_Rotation = 0f;
	public void SetRotation(float target_rotation)
	{
		target_rotation *= -1f;
		float rotation_ratio = Mathf.Clamp(Time.deltaTime*5f, 0f, 0.5f);
		float rotation = m_Rotation * (1f - rotation_ratio) + target_rotation * rotation_ratio;

		float limit_rotation = 30f;
		rotation = Mathf.Clamp(rotation, -limit_rotation, limit_rotation);

		transform.localRotation = Quaternion.Euler(-Mathf.Abs(rotation)*0.5f, rotation*1.5f, rotation*0.7f);

		m_Rotation = rotation;
	}

	void SetActive(bool isActive)
	{
		if (CharacterAsset == null)
			return;

		if (isActive == true)
		{
			Character character = CharacterAsset.Alloc();
			CoreUtility.SetRecursiveLayer(character.gameObject, "Character");
			character.transform.SetParent(transform, false);
			character.transform.localPosition = Vector3.zero;
			character.IsPause = false;

			switch(m_Mode)
			{
				case Mode.Battle:
					character.CharacterAnimation.SetBattleMode();
					character.SetUIMode(false);
					character.CharacterAnimation.SetRenderQueue(2999);
					break;

				case Mode.Normal:
					character.CharacterAnimation.SetIdleMode();
					character.SetUIMode(false);
					character.CharacterAnimation.SetRenderQueue(2999);
					break;

				case Mode.UI_Battle:
					character.SetUIMode(true);
					character.CharacterAnimation.SetBattleMode();
					break;

				case Mode.UI_Normal:
					character.SetUIMode(true);
					character.CharacterAnimation.SetIdleMode();
					break;
			}

			if (string.IsNullOrEmpty(m_ActionName) == false)
			{
				character.PlayAction(m_ActionName);
				if (m_SetDefaultAction == true)
					character.CharacterAnimation.DefaultState = character.CharacterAnimation.CurrentState;
				character.CharacterAnimation.PrevState = null;
				Character.UpdatePlay(m_PlaybackTime);
				last_action = m_ActionName;
			}

			LateUpdate();
			//character.CharacterAnimation.Play(m_Pause);
		}
		else if (CharacterAsset != null && CharacterAsset.Asset != null)
		{
			m_PlaybackTime = Character.PlaybackTime;
			Character.Reset();
			CharacterAsset.Asset.Creature = null;
			CharacterAsset.Free();
		}
			
	}

	float m_PlaybackTime = 0f;

	override protected void OnEnable()
	{
		SetActive(true);
		base.OnEnable();
	}

	override protected void OnDisable()
	{
		base.OnDisable();
		SetActive(false);
	}

	void OnDestroy()
	{
		SetActive(false);
	}

	override protected void OnUpdate()
	{
		base.OnUpdate();
		if (IsDrag == false && m_Rotation != 0f)
		{
			SetRotation(0f);
			Info.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		if (Character == null)
			return;

		if (IsDrag)
		{
			Info.SetActive(false);
			Character.CharacterAnimation.SetRenderQueue(3020);
			return;
		}

		switch (m_Mode)
		{
			case Mode.UI_Battle:
			case Mode.UI_Normal:
				{
					if (drawCall != null)
						Character.CharacterAnimation.SetRenderQueue(drawCall.renderQueue);

					if (m_PlayType != ePlayType.None)
					{
						float time = Time.time;
						if (Character.IsPlayingAction || Character.IsPlayingActionAnimation || Character.IsPlaying)
						{
							m_Playing = time;
						}
						else if (m_Playing + 2f < time)
						{
							m_Playing = time;
							if (m_PlayType == ePlayType.Random)
								PlayRandomAction();
							else
								PlaySocialAction();
						}
					}
				}
				break;
		}
	}

	public void Batch(Vector3 pos)
	{
		transform.localPosition = pos;
		if (Info != null)
		{
			Vector3 info_local_pos = Info.transform.localPosition;
			info_local_pos.x = pos.x;
			Info.transform.localPosition = info_local_pos;
		}
		Character character = Character;
		if (character != null)
			character.transform.localPosition = Vector3.zero;
	}

	public void PlayWinAction()
	{
		Character.PlayAction("win");
		last_action = "win";
	}

	public void PlaySocialAction()
	{
		Character.PlayAction("social");
		last_action = "social";
	}

	readonly string[] action_list = { "social", "win", "attack", "skill1", "skill2", "skill_leader" };
	List<string> random_actions = new List<string>();
	string last_action = "";
	public void PlayRandomAction()
	{
		if (random_actions.Count == 0)
		{
			random_actions.AddRange(action_list.Where(a => Character.ContainsAction(a)));
			if (random_actions.Count == 0)
				return;

			random_actions = random_actions.OrderBy<string, int>((item) => MNS.Random.Instance.Next()).ToList();
			if (last_action == random_actions[0])
			{
				random_actions.RemoveAt(0);
				random_actions.Add(last_action);
			}
		}

		Character.CancelAction(false);
		Character.PlayAction(random_actions[0]);
		last_action = random_actions[0];
		random_actions.RemoveAt(0);
	}

	public void SetPlay(ePlayType play_type)
	{
		m_PlayType = play_type;
	}
	float m_Playing = 0f;
}
