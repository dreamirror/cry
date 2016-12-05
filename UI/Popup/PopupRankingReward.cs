using PacketEnums;
using PacketInfo;
using System.Collections.Generic;
using UnityEngine;

public class PopupRankingReward : PopupBase
{
    public class RankingRewardInfo
    {
        public string title;
        public string description;
        public pe_GoodsType token_type;
        public int ranking;
        public List<RankingRewardData> data;
    }

    public PrefabManager RewardItemPRefabManager;
    RankingRewardInfo m_RewardInfo;

    public UILabel title, description;
    public UIGrid grid;

    override public void SetParams(bool is_new, object[] parms)
    {
        m_RewardInfo = (RankingRewardInfo)parms[0];
        Init();
    }
    //////////////////////////////////////////////////////////////////////////////////////

    public void Init()
    {
        title.text = m_RewardInfo.title;
        description.text = m_RewardInfo.description;

        RewardItemPRefabManager.Clear();

        int last_rank = 0;
        foreach (var reward_info in m_RewardInfo.data)
        {
            RankingRewardItem new_item = RewardItemPRefabManager.GetNewObject<RankingRewardItem>(grid.transform, Vector3.zero);
            new_item.Init(last_rank+1, reward_info.Rank, reward_info.Gem, reward_info.Token, m_RewardInfo.token_type, last_rank < m_RewardInfo.ranking && (m_RewardInfo.ranking <= reward_info.Rank || reward_info.Rank == -1));
            last_rank = reward_info.Rank;
        }
        grid.Reposition();
    }

    public void OnCancel()
    {
        RewardItemPRefabManager.Clear();

        base.OnClose();
    }
}
