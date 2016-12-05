using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public enum eTextPushType
{
	Default,
	Normal,
	Critical,
	Dot,
	CriticalDot,
}

public class TextManager : MonoBehaviour
{
	static public TextManager Instance { get; private set; }

	[Serializable]
	public class TextPrefab
	{
		public TextAnimation prefab;
		public int prewarm = 5;

		public int PlayCount { get { return m_PlayingList.Sum(l => l.Value.Count); } }
		public int FreeCount { get { return m_FreeList.Count; } }

		public class AnimationList
		{
			List<TextAnimation> m_List = new List<TextAnimation>();
			public int Count { get { return m_List.Count; } }

			public bool Update(Stack<TextAnimation> free_list)
			{
				while (m_List.Count > 0 && m_List[0].IsPlaying == false)
				{
					m_List[0].gameObject.SetActive(false);
					free_list.Push(m_List[0]);
					m_List.RemoveAt(0);
				}
				return m_List.Count > 0;
			}

			public void Clear(Stack<TextAnimation> free_list)
			{
				while (m_List.Count > 0)
				{
					m_List[0].gameObject.SetActive(false);
					free_list.Push(m_List[0]);
					m_List.RemoveAt(0);
				}
			}

			public void Add(TextAnimation animation)
			{
				m_List.Add(animation);
				Optimize();
			}

			void Optimize()
			{
				if (m_List.Count <= 1)
					return;

				float push_time = m_List[0].PushTime;
				float push_position_normal = m_List[0].PushPosition;
				float push_position_critical = m_List[0].PushPositionCritical;
				float push_position_critical_dot = m_List[0].PushPositionCriticalDot;
				float push_position_dot = m_List[0].PushPositionDot;

				float last_time = -10f;
				float last_position = 0f;
				eTextPushType last_push_type = eTextPushType.Normal;
				for (int i = m_List.Count-1; i >= 0; --i)
				{
					TextAnimation animation = m_List[i];
					if (animation.IsPlaying == false)
						continue;

					bool is_optimize = false;
					if (push_time > 0f)
					{
						float time = animation.PlaybackTime;
						if (time - last_time < push_time)
						{
							animation.PlaybackTime = last_time + push_time;
							is_optimize = true;
						}
						last_time = animation.PlaybackTime;
					}

					float local_push_position = 0f;
					if (i != m_List.Count-1)
					{
						switch(last_push_type)
						{
							case eTextPushType.Dot:         local_push_position += push_position_dot; break;
							case eTextPushType.Critical:    local_push_position += push_position_critical; break;
							case eTextPushType.CriticalDot:    local_push_position += push_position_critical_dot; break;
							default:                        local_push_position += push_position_normal; break;
						}
					}
					switch (animation.PushType)
					{
						case eTextPushType.Dot: local_push_position += push_position_dot; break;
						case eTextPushType.Critical: local_push_position += push_position_critical; break;
						case eTextPushType.CriticalDot: local_push_position += push_position_critical_dot; break;
						default: local_push_position += push_position_normal; break;
					}

					if (local_push_position > 0f)
					{
						float pos = animation.CurrentPosition;
						if (pos - last_position < local_push_position)
						{
							animation.AddPosition += last_position + local_push_position - pos;
							last_position = animation.CurrentPosition;
							is_optimize = true;
						}
						else
							last_position = pos;
					}
					last_push_type = animation.PushType;

					if (is_optimize == true)
						animation.Sample();
				}
			}
		}

		Stack<TextAnimation> m_FreeList = new Stack<TextAnimation>();
		Dictionary<Transform, AnimationList> m_PlayingList = new Dictionary<Transform, AnimationList>();

		public void Init()
		{
			for (int i = 0; i < prewarm; ++i)
			{
				TextAnimation new_animation = NewAnimation();
				m_FreeList.Push(new_animation);
			}
		}

		public void Update()
		{
			foreach (var v in m_PlayingList.Values)
			{
				v.Update(m_FreeList);
			}
		}

		public void Clear()
		{
			foreach (var v in m_PlayingList.Values)
			{
				v.Clear(m_FreeList);
			}
		}

		TextAnimation NewAnimation()
		{
			TextAnimation new_animation = Instantiate(prefab);
			new_animation.gameObject.hideFlags = HideFlags.DontSave;
			new_animation.transform.SetParent(TextManager.Instance.transform, false);
			new_animation.gameObject.SetActive(false);
			return new_animation;
		}

		TextAnimation GetFree(Transform attach_transform)
		{
			TextAnimation animation = null;
			if (m_FreeList.Count > 0)
			{
				animation = m_FreeList.Pop();
			}
			else
			{
				animation = NewAnimation();
			}
			animation.gameObject.SetActive(true);

			return animation;
		}

		public TextAnimation Push(Transform attach_transform, float text_offset, string text, eTextPushType push_type, float scale, float add_position)
		{
			switch (push_type)
			{
				case eTextPushType.Critical:
				case eTextPushType.CriticalDot:
					text = "CRITICAL\n" + text;
					break;
			}

			TextAnimation new_obj = GetFree(attach_transform);
			new_obj.Init(attach_transform, text_offset, text, push_type, scale, add_position);
			AddList(attach_transform, new_obj);

			return new_obj;
		}

		void AddList(Transform attach_transform, TextAnimation animation)
		{
			AnimationList list;
			if (m_PlayingList.TryGetValue(attach_transform, out list) == false)
			{
				list = new AnimationList();
				m_PlayingList.Add(attach_transform, list);
			}
			list.Add(animation);
		}
	}

	public TextPrefab DamagePhysic, DamageMagic, Heal, Mana, Message;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		DamagePhysic.Init();
		DamageMagic.Init();
		Heal.Init();
		Mana.Init();
		Message.Init();
	}
	
	// Update is called once per frame
	void Update ()
	{
		DamagePhysic.Update();
		DamageMagic.Update();
		Heal.Update();
		Mana.Update();
		Message.Update();
	}

	public void Clear()
	{
		DamagePhysic.Clear();
		DamageMagic.Clear();
		Heal.Clear();
		Mana.Clear();
		Message.Clear();
	}

	string GetNumberText(int number)
	{
		if (number > 0)
			return "+" + number.ToString();
		return number.ToString();
	}

	public void PushDamage(bool is_physic, ICreature creature, int damage, eTextPushType push_type, float add_position = 0f)
	{
		if (creature.IsShowText == false)
			return;

		Transform attach_transform = creature.Character.transform;
		float scale = attach_transform.GetComponent<Character>().Creature.Scale;
		if (is_physic)
			DamagePhysic.Push(attach_transform, creature.TextOffset, GetNumberText(damage), push_type, scale, add_position);
		else
			DamageMagic.Push(attach_transform, creature.TextOffset, GetNumberText(damage), push_type, scale, add_position);
	}

	public void PushHeal(ICreature creature, int heal, eTextPushType push_type, float add_position = 0f)
	{
		if (creature.IsShowText == false)
			return;

		Transform attach_transform = creature.Character.transform;
		float scale = attach_transform.GetComponent<Character>().Creature.Scale;
		Heal.Push(attach_transform, creature.TextOffset, GetNumberText(heal), push_type, scale, add_position);
	}

	public void PushMana(ICreature creature, int mp, eTextPushType push_type, float add_position = 0f)
	{
		if (creature.IsShowText == false)
			return;

		Transform attach_transform = creature.Character.transform;
		float scale = attach_transform.GetComponent<Character>().Creature.Scale;

		string text = string.Format("{0:0.##}%", mp * 0.01f);
		if (mp > 0)
			text = "+" + text;

		Mana.Push(attach_transform, creature.TextOffset, text, push_type, scale, add_position);
	}

#if !SH_ASSETBUNDLE
	public void PushMessage(ICreature creature, string message, eBuffColorType color_type, eTextPushType push_type, float add_position = 0f)
	{
		if (creature.IsShowText == false)
			return;

		Transform attach_transform = creature.Character.transform;
		float scale = attach_transform.GetComponent<Character>().Creature.Scale;
		TextAnimation ta = Message.Push(attach_transform, creature.TextOffset, message, push_type, scale, add_position);
		ta.Text.color = CharacterBuff.GetColor(color_type);
	}

	public void PushMessagePosition(Transform attach_transform, ICreature creature, string message, eBuffColorType color_type, eTextPushType push_type, float add_position = 0f)
	{
		if (creature.IsShowText == false)
			return;

		TextAnimation ta = Message.Push(attach_transform, creature.TextOffset, message, push_type, 1f, add_position);
		ta.Text.color = CharacterBuff.GetColor(color_type);
	}
#endif
}
