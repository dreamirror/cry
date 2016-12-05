using UnityEngine;
using System.Collections.Generic;
using PacketEnums;

public class PopupWorldBossBattleEnd : PopupBase
{
    //public GameObject m_Win;
    //public GameObject m_Defeat;
    public UIToggledObjects m_ToggleWin;
    public UIToggledObjects m_ToggleRankWin;
    public UIToggledObjects m_ToggleScoreWin;

    public UIPlayTween m_PlayTween;

    public UILabel m_LabelRank;
    public UILabel m_LabelRankUP;

    public UILabel m_LabelScore;
    public UILabel m_LabelScoreUP;

    public UIButton m_BtnConfirm;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
        {
            EventParamWorldBossBattleEnd _param = (EventParamWorldBossBattleEnd)parms[0];

            if (_param.is_first == true)
            {
                m_ToggleWin.Set(true);
                m_ToggleScoreWin.Set(false);
            }
            else if (_param.score_up > 0)
            {
                m_ToggleWin.Set(true);
                m_ToggleScoreWin.Set(true);

                m_LabelScoreUP.text = Localization.Format("WorldBossScore", _param.score_up);
            }
            else
            {
                m_ToggleWin.Set(false);
                m_ToggleScoreWin.Set(false);
            }

            if (_param.rank_up > 0)
            {
                m_ToggleRankWin.Set(true);

                m_LabelRankUP.text = _param.rank_up.ToString();
            }
            else
            {
                m_ToggleRankWin.Set(false);
            }

            m_LabelScore.text = Localization.Format("WorldBossScore", _param.score);
            m_LabelRank.text = _param.rank.ToString();
        }
        else
            throw new System.Exception(string.Format("invalid parms", this.name));
    }
    void Update()
    {
    }

    void OnEnable()
    {
        m_PlayTween.Play(true);
    }

    public void OnFinishedTweenStar()
    {

    }

    public void OnExit()
    {
        parent.Close(true, true);
        GameMain.SetBattleMode(eBattleMode.None);
    }

    public void OnClickBattleInfo()
    {
    }
}
