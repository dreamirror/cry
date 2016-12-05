using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BattleUICharacter : MonoBehaviour
{
    //public UIProgressBar m_HPBar;
    //public UISprite m_HP;
    //public UISprite m_HPDanger;
    //public UISprite m_Fever;
    public UISprite m_Character;
    public UISprite m_Skill1, m_Skill2;
    public UIParticleContainer m_SkillFever1, m_SkillFever2;
    public GameObject m_Skill1Full, m_Skill2Full;
    public GameObject m_Auto1, m_Auto2;
    public UIButton m_BtnSkill1, m_BtnSkill2;

    //public SHTooltip m_Tooltip;

    public BattleCreature m_Creature;

    //public GameObject m_Effect1, m_Effect2;
    //public UIPlayTween m_LowHP, m_TweenDefault;

    public UISprite[] m_SetDeadSprite;

    //TimeSpan span_press_time = TimeSpan.FromMilliseconds(500);

    // Use this for initialization
	void Start () {
        //m_Tooltip.OnShowTooltip = OnShowTooltip;
	}

    void UpdateHP()
    {
        if (m_Creature.IsDead)
            SetDead();
        //else
        //{
        //    if (m_HPBar.value > m_Creature.LowHP)
        //    {
        //        m_LowHP.Stop();
        //        m_TweenDefault.Play(true);
        //    }
        //    else if (m_HPBar.value > 0f && m_LowHP.IsPlaying() == false)
        //        m_LowHP.Play(true);
        //}

        //m_HPBar.value = m_Creature.HPBar.m_HP.fillAmount;
        //m_HPDanger.fillAmount = m_Creature.HPBar.m_HPDanger.fillAmount;
    }

    void UpdateMP()
    {
        bool is_dead = m_Creature.IsDead;
        bool bFeverEffect = m_Creature.IsMPFull == true && m_Creature.CanAction();

        if (is_dead)
        {
            m_Skill1Full.gameObject.SetActive(false);
            m_Skill2Full.gameObject.SetActive(false);
            m_Auto1.gameObject.SetActive(false);
            m_Auto2.gameObject.SetActive(false);
        }
        else
        {
            if (BattleBase.Instance.IsAuto == false)
            {
                m_Auto1.gameObject.SetActive(false);
                m_Auto2.gameObject.SetActive(false);

                if (bFeverEffect)
                {
                    if (m_Creature.Skills.Count > 1 && m_Skill1Full.gameObject.activeInHierarchy == false)
                        m_SkillFever1.Play();
                    if (m_Creature.Skills.Count > 2 && m_Skill2Full.gameObject.activeInHierarchy == false)
                        m_SkillFever2.Play();
                }
                m_Skill1Full.gameObject.SetActive(bFeverEffect && m_Creature.Skills.Count > 1);
                m_Skill2Full.gameObject.SetActive(bFeverEffect && m_Creature.Skills.Count > 2);
            }
            else
            {
                short auto_index = m_Creature.AutoSkillIndex;
                switch (auto_index)
                {
                    case 0:
                        if (bFeverEffect)
                        {
                            if (m_Skill1Full.gameObject.activeInHierarchy == false)
                                m_SkillFever1.Play();
                            if (m_Skill2Full.gameObject.activeInHierarchy == false)
                                m_SkillFever2.Play();
                        }
                        m_Skill1Full.gameObject.SetActive(bFeverEffect);
                        m_Skill2Full.gameObject.SetActive(bFeverEffect);
                        m_Auto1.gameObject.SetActive(true);
                        m_Auto2.gameObject.SetActive(true);
                        break;

                    case 1:
                        if (m_Creature.Skills.Count < 2) break;
                        if (bFeverEffect && m_Skill1Full.gameObject.activeInHierarchy == false)
                            m_SkillFever1.Play();

                        m_Skill1Full.gameObject.SetActive(bFeverEffect);
                        m_Auto1.gameObject.SetActive(true);

                        m_Skill2Full.gameObject.SetActive(false);
                        m_Auto2.gameObject.SetActive(false);
                        break;

                    case 2:
                        if (m_Creature.Skills.Count < 3) break;
                        if (bFeverEffect && m_Skill2Full.gameObject.activeInHierarchy == false)
                            m_SkillFever2.Play();

                        m_Auto1.gameObject.SetActive(false);
                        m_Skill1Full.gameObject.SetActive(false);

                        m_Auto2.gameObject.SetActive(true);
                        m_Skill2Full.gameObject.SetActive(false);
                        break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateHP();
        UpdateMP();
    }

    public void SetCharacter(BattleCreature creature)
    {
        m_Creature = creature;

        string sprite_name = string.Format("cs_{0}", m_Creature.Info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_Character.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;
        m_Character.spriteName = new_sprite_name;

        if (m_Creature.Skills.Count > 1)
            m_Skill1.spriteName = m_Creature.Skills[1].Info.IconID;
        else
            m_Skill1.gameObject.SetActive(false);

        if (m_Creature.Skills.Count > 2)
            m_Skill2.spriteName = m_Creature.Skills[2].Info.IconID;
        else
            m_Skill2.gameObject.SetActive(false);
    }

    public void OnSkill1()
    {
        if (BattlePVP.Instance != null)
        {
            Tooltip.Instance.ShowMessageKey("PVPOnlyAutoBattle");
            return;
        }

        if (bTooltipShowed == true)
        {
            bTooltipShowed = false;
            return;
        }

        if (m_Creature.Skills.Count <= 1)
            return;

        if (BattleBase.Instance.IsAuto)
            AutoSkillSelect(1);

        if (m_Creature.IsMPFull)
            m_Creature.DoAction(1, true, true);
    }

    public void OnSkill2()
    {
        if (BattlePVP.Instance != null)
        {
            Tooltip.Instance.ShowMessageKey("PVPOnlyAutoBattle");
            return;
        }

        if (bTooltipShowed == true)
        {
            bTooltipShowed = false;
            return;
        }

        if (m_Creature.Skills.Count <= 2)
            return;

        if (BattleBase.Instance.IsAuto)
            AutoSkillSelect(2);

        if (m_Creature.IsMPFull)
            m_Creature.DoAction(2, true, true);
    }

    public void AutoSkillSelect(short idx)
    {
        m_Creature.AutoSkillIndex = idx;
    }

    bool bTooltipShowed = false;
    public void OnShowTooltip1(SHTooltip tooltip)
    {
        if (m_Creature.Skills.Count <= 1)
            return;

        bTooltipShowed = true;
        Tooltip.Instance.ShowTarget(m_Creature.Skills[1].Info.GetTooltip(), tooltip);
    }

    public void OnShowTooltip2(SHTooltip tooltip)
    {
        if (m_Creature.Skills.Count <= 2)
            return;

        bTooltipShowed = true;
        Tooltip.Instance.ShowTarget(m_Creature.Skills[2].Info.GetTooltip(), tooltip);
    }

    public void SetBattleEnd()
    {
        //if (m_LowHP.IsPlaying())
        //    m_LowHP.Stop();
        m_Auto1.SetActive(false);
        m_Auto2.SetActive(false);

        //m_Stack1.SetActive(false);
        //m_Stack2.SetActive(false);
    }

    public void SetDead()
    {
        //if (m_LowHP.IsPlaying())
        //    m_LowHP.Stop();

        m_BtnSkill1.GetComponent<Collider2D>().enabled = false;
        m_BtnSkill2.GetComponent<Collider2D>().enabled = false;

        foreach (UISprite sprite in m_SetDeadSprite)
        {
            sprite.color = Color.grey;
            BoxCollider col = sprite.GetComponent<BoxCollider>();
            if (col != null)
                col.enabled = false;
            //m_Tooltip.gameObject.SetActive(false);
        }
        m_Auto1.SetActive(false);
        m_Auto2.SetActive(false);

        //m_Stack1.SetActive(false);
        //m_Stack2.SetActive(false);
    }
}
