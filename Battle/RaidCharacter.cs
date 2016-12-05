using UnityEngine;
using System.Collections;
using System;

public class RaidCharacter : MonoBehaviour
{
	public TweenScale m_TweenSkill, m_TweenScale;
	public TweenPosition m_TweenMelee;
	public AtlasSprite m_Sprite, m_Shadow, m_Melee, m_Skill;

	float m_NextTweenTime = 0f, m_NextSkillTime = 0f;
	readonly float TweenTimeGap = 10f, SkillTimeGap = 30f;
	//float m_Seed = 0f;
	
	// Use this for initialization
	void Start () {
		do
		{
			m_Shadow.spriteName = m_Sprite.spriteName = m_Sprite.atlas.Atlas.spriteList[MNS.Random.Instance.NextRange(0, m_Sprite.atlas.Atlas.spriteList.Count - 1)].name;
		} while (m_Sprite.spriteName.Contains("_battle") == false);

		m_NextTweenTime = Time.time + UnityEngine.Random.Range(0f, TweenTimeGap);
		m_NextSkillTime = Time.time + UnityEngine.Random.Range(0f, SkillTimeGap);

		m_TweenScale.tweenFactor = UnityEngine.Random.Range(0f, 1f);
		m_TweenScale.duration = 0.5f + UnityEngine.Random.Range(-0.1f, 0.1f);

		//m_Seed = Mathf.Repeat(MNS.Random.Instance.NextFloat(), 1f);
	}

	public void Init(Vector3 position, bool is_melee)
	{
		transform.localPosition = position;
		IsMelee = is_melee;

		m_TweenMelee.from = position;
		m_TweenMelee.to = position;
		m_TweenMelee.to.x += (12f - position.x);
	}

	public bool IsMelee = false;
	
	// Update is called once per frame
	public void UpdateCharacter () {
		float time = Time.time;
		if (m_NextTweenTime < time)
		{
			PlayAttack();
			m_NextTweenTime = time + TweenTimeGap;
		}
		if (m_NextSkillTime < time)
		{
			m_NextSkillTime = time + SkillTimeGap;
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
