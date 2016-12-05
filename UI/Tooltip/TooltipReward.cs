using UnityEngine;
using System.Collections;
using SharedData;
using System.Collections.Generic;

public class TooltipReward : TooltipBase
{
    public PrefabManager m_RewardManager;
    public UIGrid m_RewardGrid;
    public UILabel m_RewardLabel;
    public UILabel m_TitleLabel;
    public GameObject m_Effect;

    public TweenScale m_TweenScale;

    List<RewardBase> m_Rewards;

    OnPopupCloseDelegate _OnPopupCloseDelegate = null;

    C2G.Reward3Ack m_reward_ack;

    public override void Init(params object[] parms)
    {
        if (parms == null)
            return;

        m_TweenScale.ResetToBeginning();
        m_TweenScale.Play(true);
        m_Rewards = parms[0] as List<RewardBase>;
        m_TitleLabel.text = (string)parms[1];
        m_RewardLabel.text = (string)parms[2];
        if (parms.Length >= 4)
            m_reward_ack = (C2G.Reward3Ack)parms[3];
        else
            m_reward_ack = null;

        if (parms.Length >= 5)
            _OnPopupCloseDelegate = parms[4] as OnPopupCloseDelegate;

        m_Effect.SetActive(m_reward_ack != null);

        foreach (var reward in m_Rewards)
        {
            var reward_item = m_RewardManager.GetNewObject<RewardItem>(m_RewardGrid.transform, Vector3.zero);
            reward_item.InitReward(reward);
        }
        m_RewardGrid.Reposition();
    }

    public void OnClickConfirm()
    {
        foreach (var reward in m_Rewards)
        {   
            Network.PlayerInfo.AddGoods(new PacketInfo.pd_GoodsData(PacketInfo.pe_GoodsType.token_gem, reward.Value));
            GameMain.Instance.UpdatePlayerInfo();
        }
        OnFinished();
        if (_OnPopupCloseDelegate != null)
            _OnPopupCloseDelegate();
    }
}
