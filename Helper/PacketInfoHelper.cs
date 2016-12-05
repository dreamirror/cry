using UnityEngine;
using System.Collections;
using PacketInfo;

static public class PacketInfoHelper
{
    static public string GetProfileName(this pd_LeaderCreatureInfo info)
    {
        CreatureInfo creature_info = CreatureInfoManager.Instance.GetInfoByIdn(info.leader_creature_idn);
        if (info.leader_creature_skin_index == 0)
            return string.Format("profile_{0}", creature_info.ID);
        return string.Format("profile_{0}_{1}", creature_info.ID, creature_info.GetSkinName(info.leader_creature_skin_index));
    }

    static public CreatureInfo GetCreatureInfo(this pd_LeaderCreatureInfo info)
    {
        return CreatureInfoManager.Instance.GetInfoByIdn(info.leader_creature_idn);
    }
}
