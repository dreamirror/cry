using UnityEngine;
using System.Collections;
using System;

public class WorldBossCharacter : MonoBehaviour
{
	public TweenScale m_TweenSkill, m_TweenScale;
	public TweenPosition m_TweenMelee;
	public AtlasSprite m_Sprite, m_Shadow, m_Melee, m_Skill;

	float m_NextSkillTime = 0f;
	//float m_Seed = 0f;
	int HP, Attack;
	
	// Use this for initialization
	void Start () {

		m_TweenScale.tweenFactor = UnityEngine.Random.Range(0f, 1f);
		m_TweenScale.duration = 0.5f + UnityEngine.Random.Range(-0.1f, 0.1f);

		//m_Seed = Mathf.Repeat(MNS.Random.Instance.NextFloat(), 1f);
	}

	public void Init(Creature creature, bool is_melee, Vector3 worldboss_position)
	{
		m_Skill.gameObject.SetActive(false);
		m_Melee.gameObject.SetActive(false);

		m_NextSkillTime = Time.time + Battle.play_start_delay + UnityEngine.Random.Range(0f, BattleConfig.Instance.AttackCoolTimeMax * 2);
		IsMelee = is_melee;

		m_Shadow.spriteName = m_Sprite.spriteName = creature.Info.ID+"_battle";
		HP = creature.StatTotal.MaxHP/5;
		switch (creature.Info.AttackType)
		{
			case SharedData.eAttackType.physic:
				Attack = creature.StatTotal.PhysicAttack/5;
				break;

			case SharedData.eAttackType.magic:
				Attack = creature.StatTotal.MagicAttack/5;
				break;

			case SharedData.eAttackType.heal:
				Attack = creature.StatTotal.Heal/5;
				break;
		}

		m_TweenMelee.from = transform.localPosition;
		m_TweenMelee.to = worldboss_position;
		float row_range = 8f, col_range = 8f;
		m_TweenMelee.to.x += MNS.Random.Instance.NextRange(-row_range, row_range)-4f;
		m_TweenMelee.to.z += MNS.Random.Instance.NextRange(-col_range, col_range)-4f;
		//		m_TweenMelee.to.x += (12f - transform.localPosition.x);

		m_TweenSkill.enabled = false;
		m_TweenMelee.enabled = false;
	}

	public bool IsMelee = false;
	
	// Update is called once per frame
	public void UpdateCharacter () {
		float time = Time.time;
		if (m_NextSkillTime < time)
		{
			m_NextSkillTime = time + UnityEngine.Random.Range(BattleConfig.Instance.AttackCoolTimeMin*2, BattleConfig.Instance.AttackCoolTimeMax*2);
			PlaySkill();
		}
	}

	void PlayAttack()
	{
		if (IsMelee)
		{
			m_TweenMelee.enabled = true;
			m_TweenMelee.ResetToBeginning();
			m_TweenMelee.PlayForward();
		}
		m_Melee.gameObject.SetActive(true);
		BattleWorldboss.Instance.m_BossHP.Creature.SetDamage(-Attack, true);
	}

	void PlaySkill()
	{
		m_Skill.gameObject.SetActive(true);

		m_TweenSkill.enabled = true;
		m_TweenSkill.ResetToBeginning();
		m_TweenSkill.PlayForward();

		PlayAttack();
	}
}
