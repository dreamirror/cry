using UnityEngine;
using System.Collections.Generic;
using LinqTools;

public class PopupSkillEnchant : PopupBase
{
    public PrefabManager m_LevelupContainer;
    public PrefabManager m_SkillEnchantItemPrefabManager;
    public UIGrid m_SkillEnchantGrid;

    public UILabel m_SkillEnchantCost;

    Creature m_Creature;

    System.Action OnSkillEnchantCallback = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_Creature = parms[0] as Creature;
        OnSkillEnchantCallback = parms[1] as System.Action;
        Init();
    }

    List<SkillEnchantItem> m_Items = new List<SkillEnchantItem>();
    void Init()
    {
        m_SkillEnchantItemPrefabManager.Clear();
        m_Items.Clear();
        for (int i = 1; i < m_Creature.Skills.Count; ++i)
        {
            SkillEnchantItem obj = m_SkillEnchantItemPrefabManager.GetNewObject<SkillEnchantItem>(m_SkillEnchantGrid.transform, Vector3.zero);
            obj.Init(m_Creature.Skills[i], m_LevelupContainer);
            obj._OnSkillEnchant = new System.Action(SkillEnchantCallback);
            m_Items.Add(obj);
        }
        m_SkillEnchantGrid.Reposition();

        UpdateEnchantCost();
    }

    private void UpdateEnchantCost()
    {
        int enchant_cost = m_Creature.AllSkillEnchantCost();
        m_SkillEnchantCost.text = Localization.Format("GoodsFormat", enchant_cost);
        if (enchant_cost > Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gold))
            m_SkillEnchantCost.color = Color.red;
        else
            m_SkillEnchantCost.color = Color.white;
    }

    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_SkillEnchant_Title"), Localization.Get("Help_SkillEnchant"));
    }

    void SkillEnchantCallback()
    {
        OnSkillEnchantCallback();
        UpdateEnchantCost();
        m_Items.ForEach(s => s.UpdateEnchantCost());
    }
    public void OnClickAllMax()
    {
        if(m_Creature.AllSkillEnchantCost() == 0)
        {
            return;
        }
        if (Network.Instance.CheckGoods(PacketInfo.pe_GoodsType.token_gold, m_Creature.AllSkillEnchantCost()) == false)
            return;

        C2G.SkillEnchantAllMax packet = new C2G.SkillEnchantAllMax();
        packet.creature_idx = m_Creature.Idx;
        packet.creature_id = m_Creature.Info.ID;
        packet.creature_level = m_Creature.Level;
        packet.skill_level = m_Creature.Skills.GetRange(1, m_Creature.Skills.Count - 1).Select(s => s.Level).ToList();

        if(Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
            tutorial_packet.skill_enchant_all_max = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialSkillEnchant);
        }
        else
            Network.GameServer.JsonAsync<C2G.SkillEnchantAllMax, C2G.SkillEnchantAck>(packet, OnSkillEnchantAllMax);
    }
    void OnSkillEnchantAllMax(C2G.SkillEnchantAllMax packet, C2G.SkillEnchantAck ack)
    {
        Network.PlayerInfo.UseGoods(ack.use_gold);
        m_Items.ForEach(i => i.PlayEffectSkillEnchantAll());
        m_Creature.SkillEnchantAllMax();
        Init();
        GameMain.Instance.UpdatePlayerInfo();
        OnSkillEnchantCallback();
    }

    void OnTutorialSkillEnchant(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnSkillEnchantAllMax(packet.skill_enchant_all_max, ack.skill_enchant);
        Tutorial.Instance.AfterNetworking();
    }
}
