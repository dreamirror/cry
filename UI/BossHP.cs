using UnityEngine;
using System.Collections;

public class BossHP : MonoBehaviour {

	public UISlicedFilledSprite m_HPDanger, m_HP, m_HPBack;
	public UILabel m_Label;
	public UISprite m_Icon;
	public UITweener[] m_DangerTweens;

	public float TimeScale = 1f, DangerScale = 1f, DangerDelay = 0.5f;
	float hp_fill_time = 0f;
	bool m_DangerAnimation = false;
	bool use_hp_back = false;
	int die_count = 0;

	string[] hpbars = {
			"New_boss_battle_boss_hp"
			, "New_boss_battle_boss_hp_2"
			, "New_boss_battle_boss_hp_3"
			, "New_boss_battle_boss_hp_4"
			, "New_boss_battle_boss_hp_5"
			, "New_boss_battle_boss_hp_6" };

	public BattleCreature Creature { get; private set; }

	// Use this for initialization
	void Start () {
	}

	void Update()
	{
		if (Creature == null)
			return;

		m_Label.text = string.Format("{0:n0} / {1:n0}", Creature.Stat.HP, Creature.Stat.MaxHP);

		float delta = Time.deltaTime * TimeScale;
		float hp_percent = Creature.Stat.HPPercent + (die_count - Creature.Stat.DieCount);
		float time = Time.time;

		if (m_HP.fillAmount < hp_percent)
		{
			m_HP.fillAmount = Mathf.Min(m_HP.fillAmount + delta, hp_percent);
			m_HPDanger.fillAmount = Mathf.Max(m_HP.fillAmount, m_HPDanger.fillAmount);
		}
		else if (hp_percent < m_HP.fillAmount)
		{
			float set_amount = m_HP.fillAmount-delta;
			if (use_hp_back == true && set_amount <= 0)
			{
				++die_count;
				SetHPBack();
				m_HP.fillAmount = 1f;
			}
			else
			{
				m_HP.fillAmount = Mathf.Max(set_amount, hp_percent);
			}
			foreach (var tween in m_DangerTweens)
			{
				tween.ResetToBeginning();
				tween.PlayForward();
			}
			if (hp_fill_time == 0f)
				hp_fill_time = time;
		}

		if (m_HPDanger.fillAmount < hp_percent)
		{
			m_HPDanger.fillAmount = hp_percent;
		}
		if (m_HPDanger.fillAmount == hp_percent)
		{
		m_DangerAnimation = false;
		hp_fill_time = 0f;
		}
		else if (m_HPDanger.fillAmount > hp_percent && (m_DangerAnimation == true || hp_fill_time != 0f && time - hp_fill_time > DangerDelay))
		{
			m_DangerAnimation = true;
			float set_amount = m_HPDanger.fillAmount - delta * DangerScale;
			m_HPDanger.fillAmount = Mathf.Max(set_amount, hp_percent);
		}
	}

	public void Init(BattleCreature creature, bool use_hp_back = false)
	{
		Creature = creature;
		this.use_hp_back = use_hp_back;

		string sprite_name = string.Format("profile_{0}", Creature.Info.ID);
		m_Icon.spriteName = sprite_name;
		die_count = 0;

		SetHPBack();
		gameObject.SetActive(true);
	}

	void SetHPBack()
	{
		if (use_hp_back == false)
		{
			m_HPBack.gameObject.SetActive(false);
		}
		else
		{
			m_HP.spriteName = hpbars[die_count % hpbars.Length];
			m_HPBack.spriteName = hpbars[(die_count+1) % hpbars.Length];

			m_HPBack.gameObject.SetActive(true);
		}
	}
}
