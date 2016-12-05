using UnityEngine;
using System.Collections;
using SharedData;
using System.Collections.Generic;
using System.Linq;

public class PopupMissionItem : MonoBehaviour
{
    public UILabel m_Title, m_Description, m_ProgressText;
    public UISlicedFilledSprite m_Progress;
    public UISprite m_SpriteImage, m_BG, m_CompleteBG;
    public UIDisableButton m_btnMove, m_btnReward;

    public PrefabManager m_RewardItemPrefabManager, m_RewardItemExpPrefabManager;
    public GameObject m_RewardIndicator1, m_RewardIndicator2;

    System.Action OnRewardCallback;

    Quest m_Quest;
    public void OnMove()
    {
        Mission.MissionMove(m_Quest);
    }

    public void OnReward()
    {
        C2G.QuestReward packet = new C2G.QuestReward();
        packet.quest_id = m_Quest.Info.ID;
        Network.GameServer.JsonAsync<C2G.QuestReward, C2G.QuestRewardAck>(packet, OnQuestReward);
    }

    void OnQuestReward(C2G.QuestReward packet, C2G.QuestRewardAck ack)
    {
        m_Quest.Data.rewarded = true;
        m_Quest.Data.daily_index = Network.DailyIndex;
        m_Quest.Data.weekly_index = Network.WeeklyIndex;
        m_Quest.CheckComplete();
        QuestManager.Instance.SetUpdateNotify();

        var player_levelup = Network.PlayerInfo.UpdateExp(ack.player_add_exp_info);
        if (player_levelup.old_level < player_levelup.new_level)
        {
            TeamLevelUp.Instance.Show(player_levelup);
        }

        Network.Instance.ProcessReward3Ack(ack.reward_ack);

        Popup.Instance.Show(ePopupMode.Reward, m_Quest.Info.Rewards, m_Quest.Info.Title, Localization.Get("GetThisRewards"), (C2G.Reward3Ack)ack.reward_ack);
        if (OnRewardCallback != null)
            OnRewardCallback();
    }

    public void Init(Quest quest, System.Action reward_callback)
    {
        OnRewardCallback = reward_callback;

        this.name = quest.Info.ID;
        m_Quest = quest;

        m_SpriteImage.spriteName = m_Quest.Info.IconID;
        m_Title.text = m_Quest.Info.Title;
        m_Description.text = m_Quest.Info.Description;
        m_ProgressText.text = string.Format("{0} / {1}", quest.Progress, quest.Info.Condition.ProgressMax);
        m_Progress.fillAmount = (float)quest.Progress / quest.Info.Condition.ProgressMax;

//        m_Quest.CheckComplete();

        m_Progress.gameObject.SetActive(m_Quest.Info.Condition.ConditionType == eQuestCondition.progress);

        m_BG.gameObject.SetActive(!m_Quest.IsComplete);
        m_CompleteBG.gameObject.SetActive(m_Quest.IsComplete);

        if (m_Quest.IsComplete)
        {
            m_btnMove.gameObject.SetActive(false);
            m_btnReward.gameObject.SetActive(true);
        }
        else
        {
            m_btnReward.gameObject.SetActive(false);
            m_btnMove.gameObject.SetActive(m_Quest.Info.Move != null && m_Quest.Info.Move.MoveType != eQuestMove.Invalid);
        }

        GameObject reward_indicator = m_RewardIndicator1;
        if (m_Quest.Info.RewardExp > 0)
        {
            MissionRewardItemExp reward = m_RewardItemExpPrefabManager.GetNewObject<MissionRewardItemExp>(m_RewardIndicator1.transform, Vector3.zero);
            reward.Init(m_Quest.Info.RewardExp);

            reward_indicator = m_RewardIndicator2;
        }

        if (m_Quest.Info.Rewards.Count > 0)
        {
            MissionRewardItem reward = m_RewardItemPrefabManager.GetNewObject<MissionRewardItem>(reward_indicator.transform, Vector3.zero);
            reward.Init(m_Quest.Info.Rewards[0]);
        }
    }
}
