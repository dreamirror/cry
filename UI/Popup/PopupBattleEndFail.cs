using UnityEngine;
using PacketEnums;

public class PopupBattleEndFail: PopupBase
{
    public UILabel m_LabelTeamLevel;
    public UISprite m_SpriteFail;
    public UISprite m_SpriteTimeover;
    public UILabel m_LabelContinue;

    public UIButton m_BtnRetry;

    float m_ContinueTime = 0f;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms != null && parms.Length == 1)
        {
            EventParamBattleEnd _param = (EventParamBattleEnd)parms[0];

            m_LabelTeamLevel.text = Localization.Format("TeamLevelFormat", _param.player_levelup.old_level);
            m_SpriteFail.gameObject.SetActive(_param.end_type == pe_EndBattle.Lose);
            m_SpriteTimeover.gameObject.SetActive(_param.end_type == pe_EndBattle.Timeout);
        }
        else
            throw new System.Exception("invalid parms");

        m_ContinueTime = Time.time + 2f;

        m_LabelContinue.gameObject.SetActive(false);

        if (BattleContinue.Instance.IsPlaying)
        {
            m_LabelContinue.gameObject.SetActive(true);
            m_LabelContinue.text = Localization.Format("BattleContinueDesc", BattleContinue.Instance.BattleCount, BattleContinue.Instance.RequestCount);
        }

        m_BtnRetry.gameObject.SetActive(BattleStage.Instance != null);
    }

    void Update()
    {
        float time = Time.time;

        if (BattleContinue.Instance.IsPlaying && m_ContinueTime < time)
        {
            if (ConfigData.Instance.ContinueBattleFinishWhenFail)
            {
                BattleContinue.Instance.SetFinish(eBattleContinueFinish.Fail);
            }
            TeamLevelUp.Instance.Close();
            Tooltip.Instance.CloseAllTooltip();
            OnExit();
        }
    }

    public void OnExit()
    {
        parent.Close(true, true);
        GameMain.SetBattleMode(eBattleMode.None);
    }

    public void OnClickBattleInfo()
    {
//        parent.Close(true);
    }

    public void OnRetry()
    {
        parent.Close(true, true);
        if (BattleContinue.Instance.IsPlaying == false || ConfigData.Instance.ContinueBattleFinishWhenFail)
        {
            BattleContinue.Instance.Clear();
            BattleContinue.Instance.SetRetry();
        }
        GameMain.SetBattleMode(eBattleMode.None);
    }
}
