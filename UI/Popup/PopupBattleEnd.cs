using UnityEngine;
using System.Collections.Generic;
using PacketEnums;
using LinqTools;

public class PopupBattleEnd: PopupBase
{
    public UIToggleSprite [] m_Star;

    public UILabel m_LabelTeamLevel;
    public UILabel m_LabelTeamExp;
    public UILabel m_LabelGold;
    public UILabel m_LabelContinue;

    public GameObject m_BattleEndHeroPrefab;
    public GameObject[] m_Heroes;

    public GameObject m_RewardItemPrefab;
    public UIGrid m_GridReward;

    public UIPlayTween m_PlayTween;
    public GameObject m_Star1;
    public GameObject m_Star2;
    public GameObject m_Star3;

    public UIButton m_BtnConfirm, m_BtnRetry;

    List<BattleEndCreature> m_Creatures = null;
    float m_ContinueTime = 0f;

    List<long> m_MaxLevelMailIdxs = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
        {
            EventParamBattleEnd _param = (EventParamBattleEnd)parms[0];

            if (_param.end_type == pe_EndBattle.Win)
            {
                m_Creatures = _param.creatures;

                m_MaxLevelMailIdxs = _param.maxlevel_reward_mail_idxs;

                m_Star1.SetActive(_param.clear_rate >= 1);
                m_Star2.SetActive(_param.clear_rate >= 2);
                m_Star3.SetActive(_param.clear_rate == 3);

                m_LabelTeamLevel.text = Localization.Format("TeamLevelFormat", _param.player_levelup.new_level);
                if(_param.player_levelup.old_level < _param.player_levelup.new_level)
                {
                    TeamLevelUp.Instance.Show(_param.player_levelup);
                }

                m_LabelTeamExp.text = Localization.Format("AddTeamExp", _param.player_levelup.add_exp);

                for (int i = 0; i < m_Heroes.Length; ++i)
                {
                    BattleEndHero[] heroes = m_Heroes[i].GetComponentsInChildren<BattleEndHero>(true);
                    BattleEndHero hero;
                    if (heroes != null && heroes.Length > 0)
                        hero = heroes[0];
                    else
                        hero = NGUITools.AddChild(m_Heroes[i], m_BattleEndHeroPrefab).GetComponent<BattleEndHero>();

                    if (m_Creatures.Count > i)
                        hero.Init(m_Creatures[i]);
                    else
                        hero.gameObject.SetActive(false);

                }
                m_GridReward.GetChildList().ForEach(ch => DestroyImmediate(ch.gameObject));
                for (int i = 0; i < _param.add_goods.Count; ++i)
                {
                    RewardItem reward = NGUITools.AddChild(m_GridReward.gameObject, m_RewardItemPrefab).GetComponent<RewardItem>();
                    reward.InitReward(40000 + (int)_param.add_goods[i].goods_type, (int)_param.add_goods[i].goods_value);
                }
                for (int i = 0; i < _param.loot_items.Count; ++i)
                {
                    RewardItem reward = NGUITools.AddChild(m_GridReward.gameObject, m_RewardItemPrefab).GetComponent<RewardItem>();
                    reward.InitReward(_param.loot_items[i].item_idn, _param.loot_items[i].add_piece_count);
                }
                for (int i = 0; i < _param.loot_runes.Count; ++i)
                {
                    RewardItem reward = NGUITools.AddChild(m_GridReward.gameObject, m_RewardItemPrefab).GetComponent<RewardItem>();
                    reward.InitReward(_param.loot_runes[i].rune_idn, 0);
                }
                if (_param.loot_creatures != null)
                {
                    foreach (var loot_creature in _param.loot_creatures)
                    {
                        RewardItem reward = NGUITools.AddChild(m_GridReward.gameObject, m_RewardItemPrefab).GetComponent<RewardItem>();
                        reward.InitCreature(CreatureManager.Instance.GetInfoByIdx(loot_creature));
                    }
                }
                m_GridReward.Reposition();
            }
        }
        else
            throw new System.Exception("invalid parms");

        m_MadeList = new List<EventParamItemMade>();
        //m_MadeList = new List<EventParamItemMade>(ItemManager.Instance.ItemMadeList);
        ItemManager.Instance.ItemMadeList.Clear();
        if (m_MadeList.Count > 0)
        {
            showMadeItemTooltip = Time.time + 2f;
            m_ContinueTime = showMadeItemTooltip+1f;
        }
        else
            m_ContinueTime = Time.time + 3f;

        m_LabelContinue.gameObject.SetActive(false);
        if (BattleStage.Instance != null && Network.NewStageInfo == null)
            m_BtnRetry.gameObject.SetActive(true);
        else
            m_BtnRetry.gameObject.SetActive(false);

        if (BattleContinue.Instance.IsPlaying)
        {
            m_LabelContinue.gameObject.SetActive(true);
            m_LabelContinue.text = Localization.Format("BattleContinueDesc", BattleContinue.Instance.BattleCount, BattleContinue.Instance.RequestCount);
        }
    }

    List<EventParamItemMade> m_MadeList = null;
    float showMadeItemTooltip;
    void Update()
    {
        float time = Time.time;

        if (showMadeItemTooltip < time)
        {
            if (m_MadeList.Count > 0)
            {
                Tooltip.Instance.ShowItemMade(m_MadeList[0].item.Info);
                m_MadeList.RemoveAt(0);
                showMadeItemTooltip = time + 1f;
                m_ContinueTime = showMadeItemTooltip+1f;
            }
            else
                showMadeItemTooltip = float.MaxValue;
        }

        if (BattleContinue.Instance.IsPlaying && m_ContinueTime < time)
        {
            TeamLevelUp.Instance.Close();
            Tooltip.Instance.CloseAllTooltip();
            OnExit();
        }
    }

    void OnEnable()
    {
        m_PlayTween.Play(true);
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        if(Tutorial.Instance.CheckConditionBattleEndPopup() == true)
            Tutorial.Instance.SetConditionOK();
    }
    public void OnFinishedTweenStar()
    {
        OnCheckMaxLevelReward();
    }

    public void OnExit()
    {
        parent.Close(true, true);
        GameMain.SetBattleMode(eBattleMode.None);
    }

    public void OnClickBattleInfo()
    {
//        parent.Close(true);
    }

    public void OnRetry()
    {
        parent.Close(true, true);
        if (BattleContinue.Instance.IsPlaying == false)
            BattleContinue.Instance.SetRetry();
        GameMain.SetBattleMode(eBattleMode.None);
    }

    void OnCheckMaxLevelReward()
    {
        if (m_MaxLevelMailIdxs == null || m_MaxLevelMailIdxs.Count == 0)
            return;

        C2G.MailRewardDirect packet = new C2G.MailRewardDirect();
        packet.mail_idx = m_MaxLevelMailIdxs.First();

        Network.GameServer.JsonAsync<C2G.MailRewardDirect, C2G.MailRewardDirectAck>(packet, OnMailRewardDirectAckHandler);
    }

    void OnMailRewardDirectAckHandler(C2G.MailRewardDirect packet, C2G.MailRewardDirectAck ack)
    {   
        m_MaxLevelMailIdxs.Remove(packet.mail_idx);
        List<RewardBase> reward = ack.result_mail.rewards.Select(r => new RewardBase(r.reward_idn, r.reward_value)).ToList();
        Tooltip.Instance.ShowTooltip(eTooltipMode.Reward, reward, Localization.Get("PopupRewardTitle"), ack.result_mail.title, null, new OnPopupCloseDelegate(OnCheckMaxLevelReward));        
        
    }
}
