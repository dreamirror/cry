using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using System.Collections.Generic;
using PacketInfo;
using LinqTools;

public class RankingItem : MonoBehaviour
{
    public GameObject m_ProfilePrefab;
    public GameObject m_ProfileIndicator;
    public UILabel m_labelRank;
    public UISprite m_SpriteRank;
    public UILabel m_labelMessage;
    public UIDisableButton m_BtnDetail;
    public UIToggleSprite m_BG;

    PlayerProfile m_Profile;
    pd_PvpPlayerInfo m_PVPInfo;
    pd_WorldBossPlayerInfo m_WorldBossInfo;

    void Start()
    {
    }

    //---------------------------------------------------------------------------
    public void Init(pd_PvpPlayerInfo info, bool selected)
    {
        m_PVPInfo = info;
        if (info.rank <= 3)
        {
            m_SpriteRank.spriteName = string.Format("arena_ranking_{0}", info.rank);
            m_labelRank.gameObject.SetActive(false);
            m_SpriteRank.gameObject.SetActive(true);
        }
        else
        {
            m_labelRank.text = info.rank.ToString();
            m_labelRank.gameObject.SetActive(true);
            m_SpriteRank.gameObject.SetActive(false);
        }

        m_labelMessage.text = info.message;

        if (m_Profile == null)
            m_Profile = NGUITools.AddChild(m_ProfileIndicator, m_ProfilePrefab).GetComponent<PlayerProfile>();

        m_Profile.UpdateProfile(info.leader_creature, info.nickname, info.player_level);

// #if !UNITY_EDITOR
//         if (selected == true)
//         {
//             m_BtnDetail.GetComponent<BoxCollider2D>().enabled = false;
//             m_BtnDetail.SetState(UIButtonColor.State.Disabled, true);
//             m_BtnDetail.enabled = false;
//         }
//         else
// #endif
//         {
//             m_BtnDetail.GetComponent<BoxCollider2D>().enabled = true;
//             m_BtnDetail.SetState(UIButtonColor.State.Normal, true);
//             m_BtnDetail.enabled = true;
//         }
        m_BG.SetSpriteActive(selected);
    }

    public void Init(pd_WorldBossPlayerInfo info, bool selected)
    {
        m_WorldBossInfo = info;

        m_labelRank.text = info.rank.ToString();
        m_labelRank.gameObject.SetActive(true);
        m_SpriteRank.gameObject.SetActive(false);

        if (info.rank <= 3)
        {
            m_SpriteRank.spriteName = string.Format("arena_ranking_{0}", info.rank);
            m_labelRank.gameObject.SetActive(false);
            m_SpriteRank.gameObject.SetActive(true);
        }
        else
        {
            m_labelRank.text = info.rank.ToString();
            m_labelRank.gameObject.SetActive(true);
            m_SpriteRank.gameObject.SetActive(false);
        }

        m_labelMessage.text = Localization.Format("WorldBossBestInBattle", info.score);

        if (m_Profile == null)
            m_Profile = NGUITools.AddChild(m_ProfileIndicator, m_ProfilePrefab).GetComponent<PlayerProfile>();

        m_Profile.UpdateProfile(info.leader_creature, info.nickname, info.player_level);

// #if !UNITY_EDITOR
//         if (selected == true)
//         {
//             m_BtnDetail.GetComponent<BoxCollider2D>().enabled = false;
//             m_BtnDetail.SetState(UIButtonColor.State.Disabled, true);
//             m_BtnDetail.enabled = false;
//         }
//         else
// #endif
//         {
//             m_BtnDetail.GetComponent<BoxCollider2D>().enabled = true;
//             m_BtnDetail.SetState(UIButtonColor.State.Normal, true);
//             m_BtnDetail.enabled = true;
//         }
        m_BG.SetSpriteActive(selected);
    }

    //---------------------------------------------------------------------------

    public void OnClickDetail()
    {
        if (m_PVPInfo != null)
        {
            C2G.PvpGetBattleInfo packet = new C2G.PvpGetBattleInfo();
            packet.enemy_account_idx = m_PVPInfo.account_idx;
            Network.GameServer.JsonAsync<C2G.PvpGetBattleInfo, C2G.PvpGetBattleInfoAck>(packet, OnPvpGetBattleInfoHandler);
        }
        else if (m_WorldBossInfo != null)
        {
            C2G.WorldBossGetBattleInfo packet = new C2G.WorldBossGetBattleInfo();
            packet.ranker_account_idx = m_WorldBossInfo.account_idx;
            Network.GameServer.JsonAsync<C2G.WorldBossGetBattleInfo, C2G.WorldBossGetBattleInfoAck>(packet, OnWorldBossGetBattleInfoHandler);
        }
    }

    void OnPvpGetBattleInfoHandler(C2G.PvpGetBattleInfo packet, C2G.PvpGetBattleInfoAck ack)
    {
        Network.PVPBattleInfo = new PVPBattleInfo(m_PVPInfo, ack);

        Popup.Instance.Show(ePopupMode.PVPBattleReady, false);
    }

    void OnWorldBossGetBattleInfoHandler(C2G.WorldBossGetBattleInfo packet, C2G.WorldBossGetBattleInfoAck ack)
    {
        TeamData team_data = new TeamData(ack.team_data.team_type, null);
        List<Creature> Creatures = new List<Creature>();

        for (int i = 0; i < ack.creatures.Count; ++i)
        {
            List<pd_EquipData> equips = ack.equips.FindAll(e => e.creature_idx == ack.creatures[i].creature_idx);
            pd_EquipData weapon = equips.Find(e => EquipInfoManager.Instance.GetInfoByIdn(e.equip_idn).CategoryInfo.EquipType == SharedData.eEquipType.weapon);
            pd_EquipData armor = equips.Find(e => EquipInfoManager.Instance.GetInfoByIdn(e.equip_idn).CategoryInfo.EquipType == SharedData.eEquipType.armor);
            List<Rune> runes = ack.runes.FindAll(r => r.creature_idx == ack.creatures[i].creature_idx).Select(e => new Rune(e)).ToList();
            Creatures.Add(new Creature(ack.creatures[i], weapon, armor, runes));
        }

        team_data.SetCreatures(ack.team_data.creature_infos.Select(c => new TeamCreature(Creatures.Find(lc => lc.Idx == c.team_creature_idx), c.auto_skill_index)).ToList(), false);
        if (ack.team_data.leader_creature_idx > 0)
        {
            Creature leader_creature = Creatures.Find(c => c.Idx == ack.team_data.leader_creature_idx);
            if (leader_creature != null)
                team_data.SetLeaderCreature(leader_creature, ack.team_data.use_leader_skill_type);
        }

        Popup.Instance.Show(ePopupMode.WorldBossPlayerInfo, m_WorldBossInfo, team_data);
    }
}
