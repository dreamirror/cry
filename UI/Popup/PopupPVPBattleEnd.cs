using UnityEngine;
using System.Collections.Generic;
using PacketEnums;

public class PopupPVPBattleEnd : PopupBase
{
    //public GameObject m_Win;
    //public GameObject m_Defeat;
    public UIToggle m_ToggleWin;

    public UIToggleSprite [] m_Star;

    public UIPlayTween m_PlayTween;
    public GameObject m_Star1;
    public GameObject m_Star2;
    public GameObject m_Star3;

    public UILabel m_LabelRank;
    public UILabel m_LabelRankUP;

    public UIButton m_BtnConfirm;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
        {
            EventParamPVPBattleEnd _param = (EventParamPVPBattleEnd)parms[0];

            if (_param.end_type == pe_EndBattle.Win)
            {
                m_ToggleWin.value = true;
                m_Star1.SetActive(true);
                m_Star2.SetActive(true);
                m_Star3.SetActive(true);

                m_LabelRankUP.text = _param.rank_up.ToString();
            }
            else
            {
                m_ToggleWin.value = false;
            }

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
