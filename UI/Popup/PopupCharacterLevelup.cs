using LinqTools;
using PacketEnums;
using System.Collections.Generic;
using UnityEngine;

public class PopupCharacterLevelup : PopupBase
{
    public PrefabManager DungeonHeroPrefabManager;

    public UILabel m_label_level, m_label_next_level, m_label_require_exp, m_label_exp_powder, m_label_level_limit;
    public UIParticleContainer m_LevelupParticleContainer;
    public GameObject m_Levelup, m_LevelLimit;

    Creature m_Creature;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        m_Creature = (Creature)parms[0];

        Init();
    }
    //////////////////////////////////////////////////////////////////////////////////////

    public void Init()
    {
        var hero = DungeonHeroPrefabManager.GetNewObject<DungeonHero>(DungeonHeroPrefabManager.transform, Vector3.zero);
        hero.Init(m_Creature, false, false);

        if (m_Creature.IsLevelLimit)
        {
            m_LevelLimit.gameObject.SetActive(true);
            m_Levelup.gameObject.SetActive(false);

            m_label_level_limit.text = Localization.Format("CharacterLevelLimit", m_Creature.Level);
        }
        else
        {
            m_LevelLimit.gameObject.SetActive(false);
            m_Levelup.gameObject.SetActive(true);

            m_label_level.text = m_Creature.Level.ToString();
            m_label_next_level.text = (m_Creature.Level + 1).ToString();

            int exp_max = LevelInfoManager.Instance.GetCharacterExpMax(m_Creature.Level);
            m_label_require_exp.text = (exp_max - m_Creature.Exp).ToString();
        }

        m_label_exp_powder.text = Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_exp_powder).ToString();
    }

    public void OnLevelup()
    {
        int exp_max = LevelInfoManager.Instance.GetCharacterExpMax(m_Creature.Level);
        if (exp_max == 0)
        {
            Tooltip.Instance.ShowMessageKeyFormat("CharacterLevelupLimit", m_Creature.Level);
            return;
        }

        int level_limit = m_Creature.LevelLimit;
        if (m_Creature.Level >= level_limit)
        {
            Tooltip.Instance.ShowMessageKeyFormat("CharacterLevelupLimit", level_limit);
            return;
        }

        if ((exp_max - m_Creature.Exp) > Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_exp_powder))
        {
            //Tooltip.Instance.ShowMessageKey("NotEnoughExpPowder");
            Popup.Instance.Show(ePopupMode.ExpPowderMove);
            return;
        }

        C2G.CreatureLevelup packet = new C2G.CreatureLevelup();
        packet.creature_idx = m_Creature.Idx;
        packet.level = m_Creature.Level;
        packet.grade = m_Creature.Grade;
        packet.exp = m_Creature.Exp;
        packet.add_level = 1;
        Network.GameServer.JsonAsync<C2G.CreatureLevelup, C2G.CreatureLevelupAck>(packet, OnLevelupAck);
    }

    public void OnLevelup10()
    {
        int require_exp = -m_Creature.Exp;

        int level_limit = m_Creature.LevelLimit;
        if (m_Creature.Level + 10 > level_limit)
        {
            Tooltip.Instance.ShowMessageKeyFormat("CharacterLevelupLimit", level_limit);
            return;
        }

        for (short level = m_Creature.Level; level < m_Creature.Level + 10; ++level)
        {
            int exp_max = LevelInfoManager.Instance.GetCharacterExpMax(level);
            if (exp_max == 0)
            {
                Tooltip.Instance.ShowMessageKeyFormat("CharacterLevelupLimit", level);
                return;
            }
            require_exp += exp_max;
        }


        if (require_exp > Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_exp_powder))
        {
            //Tooltip.Instance.ShowMessageKey("NotEnoughExpPowder");
            Popup.Instance.Show(ePopupMode.ExpPowderMove);
            return;
        }

        C2G.CreatureLevelup packet = new C2G.CreatureLevelup();
        packet.creature_idx = m_Creature.Idx;
        packet.level = m_Creature.Level;
        packet.grade = m_Creature.Grade;
        packet.exp = m_Creature.Exp;
        packet.add_level = 10;
        Network.GameServer.JsonAsync<C2G.CreatureLevelup, C2G.CreatureLevelupAck>(packet, OnLevelupAck);
    }

    void UpdateInfo()
    {

    }
    void OnLevelupAck(C2G.CreatureLevelup packet, C2G.CreatureLevelupAck ack)
    {
        m_LevelupParticleContainer.Play();
        m_Creature.UpdateExp(ack.creature_exp_add_info);
        m_Creature.CheckNotify();
        Network.PlayerInfo.UseGoodsValue(ack.use_goods.goods_type, ack.use_goods.goods_value);

        Init();

        var menu_info = GameMain.Instance.GetCurrentMenu();
        if (menu_info.obj != null)
        {
            var menu = menu_info.obj.GetComponent<HeroInfoDetail>();
            menu.UpdateMenu();
            menu.Levelup();
        }

        if (ack.maxlevel_reward_mail_idx > 0)
        {
            C2G.MailRewardDirect reward_mail = new C2G.MailRewardDirect();
            reward_mail.mail_idx = ack.maxlevel_reward_mail_idx;
            Network.GameServer.JsonAsync<C2G.MailRewardDirect, C2G.MailRewardDirectAck>(reward_mail, OnMailRewardAck);
        }
    }

    void OnMailRewardAck(C2G.MailRewardDirect packet, C2G.MailRewardDirectAck ack)
    {
        List<RewardBase> reward = ack.result_mail.rewards.Select(r => new RewardBase(r.reward_idn, r.reward_value)).ToList();
        Tooltip.Instance.ShowTooltip(eTooltipMode.Reward, reward, Localization.Get("PopupRewardTitle"), ack.result_mail.title);
    }

    public override void OnFinishedHide()
    {
        DungeonHeroPrefabManager.Clear();
        base.OnFinishedHide();
    }

    public void OnCancel()
    {
        DungeonHeroPrefabManager.Clear();
        parent.Close();
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_LevelUp_Title"), Localization.Get("Help_LevelUp"));
    }
}
