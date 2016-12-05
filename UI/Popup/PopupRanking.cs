using PacketEnums;
using PacketInfo;
using System.Collections.Generic;
using UnityEngine;

public class PopupRanking : PopupBase
{
    public UIGrid m_GridReward;
    public PrefabManager RankItemPrefab;
    public UILabel m_Title;

    abstract public class RankingInfo
    {
        public string title;
        public int ranking;
        public List<RankingRewardData> data;

        abstract public void OnCreate(PrefabManager prefab_manager, Transform transform);
    }
    RankingInfo m_Info;

    List<pd_PvpPlayerInfo> m_Rankers;
    override public void SetParams(bool is_new, object[] parms)
    {
        m_Info = parms[0] as RankingInfo;
        Init();
    }
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
    }

    void OnEnable()
    {
    }
    void Update()
    {
    }

    public void Init()
    {
        m_Title.text = m_Info.title;

        m_Info.OnCreate(RankItemPrefab, m_GridReward.transform);

        m_GridReward.Reposition();
    }

    public void OnCancel()
    {
        RankItemPrefab.Clear();
        parent.Close();
    }
}
