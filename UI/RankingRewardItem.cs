
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using System.Collections.Generic;
using PacketInfo;
using LinqTools;

public class RankingRewardItem : MonoBehaviour
{
    public UILabel m_labelRank;
    public UISprite m_SpriteRank;
    public UISprite m_SpriteToken;
    public UIToggleSprite m_BG;

    public GameObject m_SpriteGem;
    public GameObject m_SpritePvp;
    public UILabel m_labelTokenGem;
    public UILabel m_labelTokenPvp;

    //---------------------------------------------------------------------------
    public void Init(int rank_s, int rank_e, int gem, int pvp, pe_GoodsType token_type, bool selected)
    {
        if (rank_e <= 3 && rank_e > 0)
        {
            m_SpriteRank.spriteName = string.Format("arena_ranking_{0}", rank_e);
            m_labelRank.gameObject.SetActive(false);
            m_SpriteRank.gameObject.SetActive(true);
        }
        else
        {
            if (rank_e < 0)
                m_labelRank.text = Localization.Format("PVPRewardRank", rank_s, "");
            else
                m_labelRank.text = Localization.Format("PVPRewardRank", rank_s, rank_e);

            m_labelRank.gameObject.SetActive(true);
            m_SpriteRank.gameObject.SetActive(false);
        }
        //Debug.LogFormat("{0}, {1}",gem, pvp);
        m_labelTokenGem.text = "x" + Localization.Format("GoodsFormat", gem);
        m_labelTokenPvp.text = "x" + Localization.Format("GoodsFormat", pvp);

        m_SpriteGem.SetActive(gem > 0);
        m_SpritePvp.SetActive(pvp > 0);
        m_SpriteToken.spriteName = token_type.ToString();
        m_BG.SetSpriteActive(selected);
    }
}
