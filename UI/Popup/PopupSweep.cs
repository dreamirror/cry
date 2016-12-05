using PacketEnums;
using System.Collections.Generic;
using UnityEngine;

public class PopupSweep : PopupBase
{
    public PrefabManager m_RewardPrefabManager;
    public UIGrid m_GridReward;
    public UILabel m_Title;
    public UILabel m_LabelTeamLevel;
    public UILabel m_LabelTeamExp;

    List<EventParamItemMade> m_MadeList = null;
    float showMadeItemTooltip;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        Init((EventParamBattleSweep)parms[0]);

        showMadeItemTooltip = Time.time + 0.8f;
        m_MadeList = new List<EventParamItemMade>(ItemManager.Instance.ItemMadeList);
        ItemManager.Instance.ItemMadeList.Clear();
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
        if (showMadeItemTooltip < Time.time)
        {
            if (m_MadeList.Count > 0)
            {
                Tooltip.Instance.ShowItemMade(m_MadeList[0].item.Info);
                m_MadeList.RemoveAt(0);
                showMadeItemTooltip = Time.time + 0.7f;
            }
            else
                showMadeItemTooltip = float.MaxValue;

        }
    }

    public void Init(EventParamBattleSweep _param)
    {
        m_Title.text = Localization.Format("SweepTitle", _param.sweep_count);

        m_LabelTeamLevel.text = Localization.Format("TeamLevelFormat", _param.player_levelup.new_level);
        if (_param.player_levelup.old_level < _param.player_levelup.new_level)
        {
            TeamLevelUp.Instance.Show(_param.player_levelup);
        }
        m_LabelTeamExp.text = Localization.Format("AddTeamExp", _param.player_levelup.add_exp);

        m_GridReward.GetChildList().ForEach(ch => Destroy(ch.gameObject));
        if (_param.loot_creature != null)
        {
            int loot_count = 0;
            int creature_idn = 0;
            creature_idn = _param.loot_creature.creature.creature_idn;
//            loot_count = _param.loot_creature.creature.creature_piece_count;
            RewardItem reward = m_RewardPrefabManager.GetNewObject<RewardItem>(m_GridReward.transform, Vector3.zero);
            reward.InitReward(creature_idn, loot_count);
        }
        for (int i = 0; i < _param.add_goods.Count; ++i)
        {
            RewardItem reward = m_RewardPrefabManager.GetNewObject<RewardItem>(m_GridReward.transform, Vector3.zero);
            reward.InitReward(40000 + (int)_param.add_goods[i].goods_type, (int)_param.add_goods[i].goods_value);
        }
        for (int i = 0; i < _param.loot_items.Count; ++i)
        {
            RewardItem reward = m_RewardPrefabManager.GetNewObject<RewardItem>(m_GridReward.transform, Vector3.zero);
            reward.InitReward(_param.loot_items[i].item_idn, _param.loot_items[i].add_piece_count);
        }
        m_GridReward.Reposition();
    }

    public void OnCancel()
    {
        parent.Close();
    }
}
