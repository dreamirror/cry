using PacketEnums;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupPVPBattleReady : PopupBase
{
    public PlayerProfile m_Profile;
    public UILabel m_LabelRank;
    public UILabel m_LabelMessage;
    public UILabel m_LabelTeamPower;

    public UILeaderSkill m_LeaderSkill;
    public UIGrid m_GridHeroes;
    public PrefabManager m_HeroManager;

    public UIGrid m_GridButtons;
    public GameObject m_ReadyButtonObj;
    public UILabel m_LabelCancel;

    public UIToggleSprite m_BG;

    TeamData m_EnemyDefenseTeam;
    bool m_bReadyToBattle = false;
    override public void SetParams(bool is_new, object[] parms)
    {
        if (parms != null && parms.Length == 1)
            m_bReadyToBattle = (bool)parms[0];
        else
            m_bReadyToBattle = true;
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
        m_Profile.UpdateProfile(Network.PVPBattleInfo.enemy_info.leader_creature, Network.PVPBattleInfo.enemy_info.nickname, Network.PVPBattleInfo.enemy_info.player_level);
        m_EnemyDefenseTeam = Network.PVPBattleInfo.enemy_team_data;
        m_LeaderSkill.Init(m_EnemyDefenseTeam.LeaderCreature, m_EnemyDefenseTeam.UseLeaderSkillType);
        m_BG.SetSpriteActive(Network.PVPBattleInfo.enemy_info.account_idx == SHSavedData.AccountIdx);

        m_LabelMessage.text = Network.PVPBattleInfo.enemy_info.message;
        m_LabelRank.text = Localization.Format("PVPRank", Network.PVPBattleInfo.enemy_info.rank);
        foreach (var hero in m_EnemyDefenseTeam.Creatures.Select(c => c.creature))
        {
            DungeonHero hero_item = m_HeroManager.GetNewObject<DungeonHero>(m_GridHeroes.transform, Vector3.zero);
            hero_item.Init(hero, false, false);
            hero_item.m_icon.flip = UIBasicSprite.Flip.Horizontally;
        }
        m_GridHeroes.Reposition();

        m_LabelTeamPower.text = Localization.Format("PowerValue", m_EnemyDefenseTeam.Power);

        m_ReadyButtonObj.SetActive(m_bReadyToBattle);
        m_GridButtons.Reposition();
        if (m_bReadyToBattle)
            m_LabelCancel.text = Localization.Get("Cancel");
        else
            m_LabelCancel.text = Localization.Get("OK");
    }

    public override void OnClose()
    {
        Network.PVPBattleInfo = null;
        m_HeroManager.Clear();
        base.OnClose();
    }

    public void OnClickReady()
    {
        base.OnClose();
        m_HeroManager.Clear();
        GameMain.Instance.ChangeMenu(GameMenu.PVPDeckInfo);
    }
}
