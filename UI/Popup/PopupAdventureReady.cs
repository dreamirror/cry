using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using PacketInfo;
using PacketEnums;
using System;
using LinqTools;

public class PopupAdventureReady : PopupBase {

    public UIScrollView ScrollViewHeroes;
    //public UIScrollView ScrollViewRewards;

    [FormerlySerializedAs("GridStages")]
    public UIGrid GridHeroes;
    public UIGrid GridRewards;
    public UIGrid GridSelected;
    public PrefabManager EnchantHeroPrefab;
    public PrefabManager RewardItemPrefab;
    public PrefabManager EnchantMaterialPrefab;

    public UILabel LabelTimeLimit;
    public UILabel LabelTitle;
    public UILabel LabelCondition;

    public GameObject EmptyString;

    AdventureInfo m_Info;

    List<EnchantHero> m_Creatures = new List<EnchantHero>();
    List<EnchantHero> m_Selected = new List<EnchantHero>();
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length >= 1)
            m_Info = parms[0] as AdventureInfo;

        var team_data = TeamDataManager.Instance.GetTeam((pe_Team)m_Info.IDN);
        if(team_data != null && team_data.NoDuplicate)
        {
            return;
        }

        RewardItemPrefab.Clear();
        foreach (var loot_group in m_Info.DropInfo[0].groups)
        {
            if (string.IsNullOrEmpty(loot_group.show_id) == true)
                continue;
            var reward_item = RewardItemPrefab.GetNewObject<RewardItem>(GridRewards.transform, Vector3.zero);
            reward_item.InitReward(loot_group.show_id, loot_group.show_value);
        }
        GridRewards.Reposition();

        m_Selected.Clear();
        for (int i = 0; i < m_Info.NeedCreature; ++i)
        {
            var item = EnchantHeroPrefab.GetNewObject<EnchantHero>(GridSelected.transform, Vector3.zero);
            m_Selected.Add(item);
            item.InitDummy(null, 0, 0, 0);
        }
        GridSelected.Reposition();


        var heroes = CreatureManager.Instance.GetFilteredList(c => c.Grade >= m_Info.MinGrade && c.Info.ContainsTags(m_Info.AvailableTags)).OrderBy(s=>TeamDataManager.Instance.CheckTeam(s.Idx));        

        m_Creatures.Clear();
        foreach (var hero in heroes)
        {
            var item = EnchantHeroPrefab.GetNewObject<EnchantHero>(GridHeroes.transform, Vector3.zero);
            item.Init(hero, OnToggleCharacter);
            m_Creatures.Add(item);
            if (team_data != null && team_data.Contains(hero.Idx))
                item.OnBtnCreatureClick();
        }

        for (int i = 0; i < 5; ++i)
        {
            var item = EnchantHeroPrefab.GetNewObject<EnchantHero>(GridHeroes.transform, Vector3.zero);
            item.Init(null);
        }

        EmptyString.SetActive(heroes.Count() == 0);

        LabelTimeLimit.text = Localization.Format("HourMinute", m_Info.Period / 60, m_Info.Period % 60);
        LabelTitle.text = m_Info.ShowName;
        LabelCondition.text = m_Info.ShowCondition;

        GridHeroes.Reposition();
        ScrollViewHeroes.ResetPosition();
    }

    void ReorderSelected()
    {
        var materials = m_Selected.Where(m => m.Creature != null).Select(m => m.Creature).ToList();
        for (int i = 0; i < m_Info.NeedCreature; ++i)
        {
            if (i < materials.Count)
                m_Selected[i].Init(materials[i], OnClickSelected);
            else
                m_Selected[i].InitDummy(null, 0, 0, 0);
        }
    }


    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        GridSelected.Reposition();
        GridRewards.Reposition();
        ScrollViewHeroes.ResetPosition();
    }
    public override void OnFinishedHide()
    {
        base.OnFinishedHide();
        RewardItemPrefab.Clear();
        EnchantHeroPrefab.Clear();
    }

    public override void OnClose()
    {   
        base.OnClose();
    }

    public void OnClickBegin()
    {
        if(m_Selected.Exists(e=>e.Creature == null) == true)
        {
            Popup.Instance.ShowMessageKey("AdventureHeroesShortage");
            return;
        }
        foreach (var hero in m_Selected)
        {
            var team_type = TeamDataManager.Instance.CheckTeam(hero.Creature.Idx);
            if ((int)team_type != m_Info.IDN && team_type != pe_Team.Invalid)
            {
                var team_data = TeamDataManager.Instance.GetTeam((pe_Team)m_Info.IDN);
                if (team_data != null)
                    team_data.SetCreatures(m_Selected.Select(e=> new TeamCreature(e.Creature, 0)).ToList(), false);

                Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnConfirm), "AdventureConfirmInTeam");
                return;
            }
        }
        OnConfirm(true);
    }
    void OnConfirm(bool is_confirm)
    {
        if(is_confirm)
        {
            pd_TeamData team_data = new pd_TeamData();
            team_data.team_type = (pe_Team)m_Info.IDN;
            team_data.creature_infos = new List<pd_TeamCreature>();
            team_data.no_duplicate = true;
            m_Selected.ForEach(c => team_data.creature_infos.Add(new pd_TeamCreature(c.Creature.Idx)));

            C2G.AdventureBegin packet = new C2G.AdventureBegin();
            packet.map_id = m_Info.ID;
            packet.team_data = team_data;
            Network.GameServer.JsonAsync<C2G.AdventureBegin, C2G.AdventureBeginAck>(packet, OnAdventureBegin);
            return;
        }
    }
    void OnAdventureBegin(C2G.AdventureBegin packet, C2G.AdventureBeginAck ack)
    {
        AdventureInfoManager.Instance.SetInfoDetail(ack.adventure_info);
        var team_data = TeamDataManager.Instance.GetTeam((pe_Team)m_Info.IDN);
        if (team_data != null)
        {
            team_data.Set(new TeamData(packet.team_data));
        }
        else
            TeamDataManager.Instance.AddTeam(new TeamData(packet.team_data), true);

        TeamDataManager.Instance.UpdateAdventure();
        parent.Close(true, true);
    }
    bool OnToggleCharacter(EnchantHero hero, bool bSelected)
    {
        if(bSelected)
        {
            if (TeamDataManager.Instance.CheckAdventureTeam(hero.Creature.Idx))
            {
                Tooltip.Instance.ShowMessageKey("CreatureInAdventure");
                return false;
            }
            if (TeamDataManager.Instance.CheckTeam(hero.Creature.Idx, pe_Team.PVP_Defense))
            {
                Tooltip.Instance.ShowMessageKey("CreatureInPvpDefense");
                return false;
            }
            if (m_Selected.Exists(e=>e.Creature != null && e.Creature.Info.IDN == hero.Creature.Info.IDN) == true)
            {
                Tooltip.Instance.ShowMessageKey("CreatureNotUseSame");
                return false;
            }

            var item = m_Selected.Find(e => e.Creature == null);
            if (item == null)
            {
                Tooltip.Instance.ShowMessageKey("NoMoreSelect");
                return false;
            }
            var team_type = TeamDataManager.Instance.CheckTeam(hero.Creature.Idx);
            if(team_type == pe_Team.PVP_Defense)
            {
                Popup.Instance.ShowMessageKey("AdventureConfirmTeamPVPDefense");
                return false;
            }
            item.Init(hero.Creature, OnClickSelected);
        }
        else
        {
            RemoveSelectedCreature(hero.Creature);
        }

        return true;
    }

    bool OnClickSelected(EnchantHero hero, bool bSelected)
    {
        var item = m_Creatures.Find(c => c.Creature.Idx == hero.Creature.Idx);
        item.OnBtnCreatureClick();

        return true;
    }

    private void RemoveSelectedCreature(Creature creature)
    {
        var item = m_Selected.Find(e => e.Creature.Idx == creature.Idx);
        item.InitDummy(null, 0, 0, 0);
        ReorderSelected();
    }
}
