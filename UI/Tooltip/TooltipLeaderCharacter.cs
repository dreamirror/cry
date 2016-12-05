using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class TooltipLeaderCharacter : TooltipBase
{
    [FormerlySerializedAs("m_DungeonHeroPrefab")]
    public GameObject m_HeroPrefab;
    public UIGrid m_GridCharacter;

    EventDelegate m_deleage = null;
    public override void Init(params object[] parms)
    {
        gameObject.SetActive(true);

        m_GridCharacter.GetChildList().ForEach(e => DestroyImmediate(e));
        var leader_creature_info = Network.PlayerInfo.leader_creature;
        foreach (var group in CreatureManager.Instance.Creatures.GroupBy(c => c.Info.IDN))
        {
            foreach (var value in group.Distinct())
            {
                CreatureInfo creature_info = value.Info;
                DungeonHeroRecommend hero = NGUITools.AddChild(m_GridCharacter.gameObject, m_HeroPrefab).GetComponent<DungeonHeroRecommend>();
                hero.Init(creature_info, OnToggleCharacter);
            }
        }

        for(int i=0; i<5; ++i)
        {
            DungeonHeroRecommend hero = NGUITools.AddChild(m_GridCharacter.gameObject, m_HeroPrefab).GetComponent<DungeonHeroRecommend>();
            hero.Init(null);
        }

        transform.localPosition = new Vector3(190f, 0f, 0f);

        if (parms.Length > 0 && parms[0] != null)
        {
            m_deleage = parms[0] as EventDelegate;
        }
    }

    bool OnToggleCharacter(CreatureInfo hero)
    {
        var leader_creature = new PacketInfo.pd_LeaderCreatureInfo { leader_creature_idn = hero.IDN, leader_creature_skin_index = 0 };
        Network.Instance.SetLeader(leader_creature);
        Network.PlayerInfo.leader_creature = leader_creature;
        GameMain.Instance.UpdatePlayerInfo();
        if (m_deleage != null)
            m_deleage.Execute();
        Close();
        return true;
    }

    public void Close()
    {
        OnFinished();
    }
}
