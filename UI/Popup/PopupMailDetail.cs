using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;

public class PopupMailDetail : PopupBase {

    public UILabel MailTitle;
    public GameObject RewardSet;
    public UILabel RewardLabel;

    public GameObject NoRewardSet;
    public UILabel NoRewardLabel;

    public GameObject RewardObject;

    public GameObject ExistRewardBtnSet;
    public GameObject NoRewardBtnSet;
    public PrefabManager RewardPrefabManager;

    public UIGrid RewardGrid;

    pd_MailDetailInfo mail_info;
    OnPopupCloseDelegate CloseCallback = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (m_parms == null)
            return;

        if (is_new == true)
            mail_info = m_parms[0] as pd_MailDetailInfo;

        if (m_parms.Length > 1)
            CloseCallback = m_parms[1] as OnPopupCloseDelegate;

        Init();
    }

    void Init()
    {
        MailTitle.text = mail_info.title;

        if (mail_info.rewards.Count > 0)
        {
            RewardObject.SetActive(true);
            RewardSet.SetActive(true);
            NoRewardSet.SetActive(false);

            foreach (var reward in mail_info.rewards)
            {
                var reward_item = RewardPrefabManager.GetNewObject<RewardItem>(RewardGrid.transform, Vector3.zero);
                reward_item.InitReward(new RewardBase(reward.reward_idn, reward.reward_value));
            }

            RewardObject.SetActive(true);

            RewardGrid.Reposition();
            RewardLabel.text = mail_info.body_message;
        }
        else
        {
            RewardObject.SetActive(false);
            RewardSet.SetActive(false);
            NoRewardSet.SetActive(true);
            NoRewardLabel.text = mail_info.body_message;
        }

        if (mail_info.rewards.Count > 0 && mail_info.used_reward == false)
        {
            ExistRewardBtnSet.SetActive(true);
            NoRewardBtnSet.SetActive(false);
        }
        else
        {
            ExistRewardBtnSet.SetActive(false);
            NoRewardBtnSet.SetActive(true);
        }

    }

    public void OnRecvMailReward()
    {
        if (mail_info.rewards.Count > 0 == true && mail_info.used_reward == false)
        {
            C2G.MailReward packet = new C2G.MailReward();
            packet.mail_idx = mail_info.mail_idx;
            packet.rewards = mail_info.rewards;
            Network.GameServer.JsonAsync<C2G.MailReward, C2G.MailRewardAck>(packet, MailRewardHandler);
        }
        else
            OnClose();
    }

    void MailRewardHandler(C2G.MailReward send, C2G.MailRewardAck recv)
    {
        Network.Instance.ProcessReward3Ack(recv.reward_ack);
        
        MailManager.Instance.SetRewarded(send.mail_idx);

        GameMain.Instance.UpdateNotify(false);

        OnClose();

        List<RewardBase> rewards = send.rewards.Select(r => new RewardBase(r.reward_idn, r.reward_value)).ToList();
        Popup.Instance.Show(ePopupMode.Reward, rewards, mail_info.title, Localization.Get("GetThisRewards"), recv.reward_ack, m_parms[1]);

        Network.Instance.SetUnreadMail(MailManager.Instance.GetUnreadState());
    }

    public void OnBtnClose()
    {
        if (CloseCallback != null)
            CloseCallback();
        OnClose();
    }

    public override void OnClose()
    {
        RewardPrefabManager.Destroy();
        base.OnClose();
        parent.Close();
    }
}
