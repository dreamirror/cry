using PacketEnums;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupWorldBossPlayerInfo : PopupBase
{
    public PlayerProfile m_Profile;
    public UILabel m_LabelRank;
    public UILabel m_LabelMessage;
    public UILabel m_LabelTeamPower;

    public UILeaderSkill m_LeaderSkill;
    public UIGrid m_GridHeroes;
    public PrefabManager m_HeroManager;

    public UIToggleSprite m_BG;

    override public void SetParams(bool is_new, object[] parms)
    {
        Init(parms[0] as pd_WorldBossPlayerInfo, parms[1] as TeamData);
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

    public void Init(pd_WorldBossPlayerInfo player_info, TeamData team_data)
    {
        m_Profile.UpdateProfile(player_info.leader_creature, player_info.nickname, player_info.player_level);
        m_LeaderSkill.Init(team_data.LeaderCreature, team_data.UseLeaderSkillType);
        m_BG.SetSpriteActive(player_info.account_idx == SHSavedData.AccountIdx);

        m_LabelMessage.text = Localization.Format("WorldBossBestInBattle", player_info.score);
        m_LabelRank.text = Localization.Format("PVPRank", player_info.rank);
        foreach (var hero in team_data.Creatures.Select(c => c.creature))
        {
            DungeonHero hero_item = m_HeroManager.GetNewObject<DungeonHero>(m_GridHeroes.transform, Vector3.zero);
            hero_item.Init(hero, false, false);
            hero_item.m_icon.flip = UIBasicSprite.Flip.Horizontally;
        }
        m_GridHeroes.Reposition();

        m_LabelTeamPower.text = Localization.Format("PowerValue", team_data.Power);
    }

    public override void OnClose()
    {
        m_HeroManager.Clear();
        base.OnClose();
    }
}
