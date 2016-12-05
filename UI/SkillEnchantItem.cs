using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using System.Collections.Generic;
using PacketInfo;

public class SkillEnchantItem : MonoBehaviour
{
    public UISprite m_SkillSprite;
    public UILabel m_labelName;
    public UILabel m_labelLevel;
    public UILabel m_labelGold;
    public UIParticleContainer m_ParticleContainer;
    public UIRepeatButton m_btnLevelUp;

    public System.Action _OnSkillEnchant = null;

    PrefabManager m_LevelupContainer;
    int gold_base, gold_per_level;

    // Use this for initialization
    void Start()
    {
        m_btnLevelUp._OnRepeat = OnRepeat;
        m_btnLevelUp._OnPressed = OnPressed;
    }

    Skill m_Skill = null;
    //---------------------------------------------------------------------------
    public void Init(Skill skill, PrefabManager levelupContainer)
    {
        gold_base = GameConfig.Get<int>("skill_enchant_gold_base");
        gold_per_level = GameConfig.Get<int>("skill_enchant_increase_gold_per_level");

        var event_info = EventHottimeManager.Instance.GetInfoByID("hero_skill_enchant_discount");
        if (event_info != null)
        {
            gold_base = (int)(gold_base * event_info.Percent);
            gold_per_level = (int)(gold_per_level * event_info.Percent);
        }
        m_LevelupContainer = levelupContainer;
        m_Skill = skill;
        m_SkillSprite.spriteName = m_Skill.Info.IconID;

        InitSkillEnchantValue();

        m_labelName.text = m_Skill.Info.Name;
        m_labelLevel.text = Localization.Format("Level", m_Skill.Level);
        UpdateEnchantCost();

        m_btnLevelUp.isEnabled = m_Skill.Level < m_Skill.Creature.Level;

        gameObject.SetActive(true);
    }

    public void UpdateEnchantCost()
    {
        m_labelGold.text = Localization.Format("GoodsFormat", current_enchant_gold);
        if (current_enchant_gold > Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gold))
            m_labelGold.color = Color.red;
        else
            m_labelGold.color = Color.white;
    }

    //---------------------------------------------------------------------------
    short add_level;
    int need_gold;
    long current_gold;
    int current_enchant_gold;
    void OnPressed()
    {
        InitSkillEnchantValue();
    }

    private void InitSkillEnchantValue()
    {
        add_level = 0;
        need_gold = 0;
        current_gold = Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gold);
        current_enchant_gold = gold_base + (m_Skill.Level - 1) * gold_per_level;
    }

    void OnRepeat()
    {
        if (m_btnLevelUp.isEnabled == false) return;
        if (m_Skill.Creature.Level <= m_Skill.Level + add_level)
        {
            OnClick();
            return;
        }
        int add_need_gold = current_enchant_gold + gold_per_level;
        if(current_gold < need_gold + add_need_gold)
        {
            OnClick();
            return;
        }
        need_gold += add_need_gold;

        current_enchant_gold += gold_per_level;
        add_level++;
        int current_level = m_Skill.Level + add_level;
        m_labelLevel.text = Localization.Format("Level", current_level);
        UpdateEnchantCost();
    }
    public void OnClick()
    {
        m_btnLevelUp.isEnabled = false;
        m_btnLevelUp.SetPressed(false);

        C2G.SkillEnchantLevel packet = new C2G.SkillEnchantLevel();
        packet.creature_idx = m_Skill.Creature.Idx;
        packet.creature_id = m_Skill.Creature.Info.ID;
        packet.skill_id = m_Skill.Info.ID;
        packet.skill_index = m_Skill.Index;
        packet.skill_level = m_Skill.Level;

        if (add_level > 1)
        {
            packet.add_level = add_level;
        }
        else
        {
            if (m_Skill.Creature.Level <= m_Skill.Level)
            {
                Tooltip.Instance.ShowMessageKey("NotAvailableSkillEnchantLevel");
                return;
            }
            if (current_enchant_gold > current_gold)
            {
                Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
                return;
            }

            add_level = 1;
            packet.add_level = add_level;

        }

        if (Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
            tutorial_packet.skill_enchant = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialSkillEnchant);
        }
        else
            Network.GameServer.JsonAsync<C2G.SkillEnchantLevel, C2G.SkillEnchantAck>(packet, OnSkillEnchant);
    }

    void OnTutorialSkillEnchant(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnSkillEnchant(packet.skill_enchant, ack.skill_enchant);
        Tutorial.Instance.AfterNetworking();
    }

    void OnSkillEnchant(C2G.SkillEnchantLevel packet, C2G.SkillEnchantAck ack)
    {
        if (Network.PlayerInfo.UseGoods(ack.use_gold) == false)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gold);
            return;
        }

        m_ParticleContainer.Play();

        string tooptip = m_Skill.GetLevelupTooltip(packet.add_level);
        string[] tooltips = tooptip.Split(new char[] { '\n' });

        float delay = 0f;
        foreach (string tip in tooltips)
        {
            if (string.IsNullOrEmpty(tip))
                continue;

            var levelup = m_LevelupContainer.GetNewObject<SkillEnchantItemLevelup>(transform, Vector3.zero);
            levelup.Init(delay, tip, OnFinishLevelup);
            delay += 0.3f;
        }

        CreatureManager.Instance.SkillLevelUP(m_Skill.Creature, packet.skill_index, packet.add_level);
        Init(m_Skill, m_LevelupContainer);
        GameMain.Instance.UpdatePlayerInfo();

        if (m_Skill.Creature.AvailableSkillEnchant == false)
            CreatureManager.Instance.UpdateNotify();

        if (_OnSkillEnchant != null)
            _OnSkillEnchant();

        //InitSkillEnchantValue();
    }
    public void PlayEffectSkillEnchantAll()
    {
        m_ParticleContainer.Play();

        string tooptip = m_Skill.GetLevelupTooltip(m_Skill.Creature.Level - m_Skill.Level);
        string[] tooltips = tooptip.Split(new char[] { '\n' });

        float delay = 0f;
        foreach (string tip in tooltips)
        {
            if (string.IsNullOrEmpty(tip))
                continue;

            var levelup = m_LevelupContainer.GetNewObject<SkillEnchantItemLevelup>(transform, Vector3.zero);
            levelup.Init(delay, tip, OnFinishLevelup);
            delay += 0.3f;
        }
    }
    public void OnShowTooltip(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowTarget(m_Skill.GetTooltip(), tooltip);
    }

    void OnFinishLevelup(GameObject obj)
    {
        m_LevelupContainer.Free(obj);
    }
}
