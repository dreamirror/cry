using UnityEngine;
using System.Collections.Generic;
using PacketInfo;

public class PopupReward : PopupBase {

    public PrefabManager RewardManager;
    public UIGrid RewardGrid;
    public UILabel RewardLabel;
    public UILabel TitleLabel;
    public GameObject Effect;

    List<RewardBase> m_Rewards;

    OnPopupCloseDelegate _OnPopupCloseDelegate = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        Init();    
    }

    // Use this for initialization
    void Start ()
    {
	
	}

    C2G.Reward3Ack m_reward_ack;

    void Init()
    {
        if (m_parms == null)
            return;
        
        m_Rewards = m_parms[0] as List<RewardBase>;
        TitleLabel.text = (string)m_parms[1];
        RewardLabel.text = (string)m_parms[2];
        if (m_parms.Length >= 4)
            m_reward_ack = (C2G.Reward3Ack)m_parms[3];
        else
            m_reward_ack = null;

        if(m_parms.Length >= 5)
            _OnPopupCloseDelegate = m_parms[4] as OnPopupCloseDelegate;

        Effect.SetActive(m_reward_ack != null);

        foreach (var reward in m_Rewards)
        {
            var reward_item = RewardManager.GetNewObject<RewardItem>(RewardGrid.transform, Vector3.zero);
            reward_item.InitReward(reward);
        }
        RewardGrid.Reposition();
    }

    public void OnClickConfirm()
    {
        OnClose();
    }

    public override void OnClose()
    {
        RewardManager.Destroy();

        if (m_reward_ack != null && m_reward_ack.loots != null && m_reward_ack.loots.Count > 0)
        {
            Popup.Instance.Close(true, true);
            m_reward_ack.loots.Reverse();
            foreach (var loot in m_reward_ack.loots)
            {
                switch (loot.type)
                {
                    case pe_RewardLootType.Hero:
                        Popup.Instance.StackPopup(ePopupMode.LootCharacter, m_reward_ack.loot_creatures[loot.index].creature_idx, false, true);
                        break;

                    case pe_RewardLootType.Token:
                        Popup.Instance.Show(ePopupMode.LootItem, new LootItemInfo(TokenInfoManager.Instance.GetInfoByType(m_reward_ack.add_goods[loot.index].goods_type).IDN, (int)m_reward_ack.add_goods[loot.index].goods_value));
                        break;

                    case pe_RewardLootType.Item:
                        Popup.Instance.Show(ePopupMode.LootItem, new LootItemInfo(m_reward_ack.loot_items[loot.index].item_idn, m_reward_ack.loot_items[loot.index].add_piece_count));
                        break;

                    case pe_RewardLootType.Rune:
                        Popup.Instance.Show(ePopupMode.LootItem, new LootItemInfo(m_reward_ack.loot_runes[loot.index].rune_idn, 0));
                        break;
                }
            }
            Popup.Instance.Show(true);
        }
        else
        {
            base.OnClose();
            if(_OnPopupCloseDelegate != null)
                _OnPopupCloseDelegate();
        }
    }
}
