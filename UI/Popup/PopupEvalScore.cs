using UnityEngine;
using System.Collections.Generic;

public delegate void OnStarUpdate(double avg_score, int my_score);

public class PopupEvalScore : PopupBase {
    
    public UIToggleSprite[] m_Stars;
    public UILabel m_TitleLabel;
    public UILabel m_ConfirmBtnLabel;

    int my_score;
    string creature_id;
    OnStarUpdate OnStarUpdateCallback;

    public override void SetParams(bool is_new, object[] parms)
    {
        my_score = (int)parms[0];
        creature_id = parms[1].ToString();
        OnStarUpdateCallback = parms[2] as OnStarUpdate;
        Init();
    }

    void Init()
    {
        SetStar(my_score);

        if (my_score == 0)
        {
            m_TitleLabel.text = Localization.Get("Evaluating");
            m_ConfirmBtnLabel.text = Localization.Get("Confirm");
        }
        else
        {
            m_TitleLabel.text = Localization.Get("EvalUpdating");
            m_ConfirmBtnLabel.text = Localization.Get("EvalUpdate");
        }
    }

    public void OnClickStar(GameObject obj)
    {
        int count;
        if (int.TryParse(obj.name, out count) == true)
        {
            SetStar(count);
            my_score = count;
        }
    }

    public void OnClickConfirm()
    {
        if (my_score == 0)
        {
            Tooltip.Instance.ShowMessageKey("EvalScoreSelect");
            return;
        }

        C2G.CreatureEvalScoreUpdate packet = new C2G.CreatureEvalScoreUpdate();
        packet.creature_id = creature_id;
        packet.score = my_score;

        Network.GameServer.JsonAsync<C2G.CreatureEvalScoreUpdate, C2G.CreatureEvalScoreUpdateAck>(packet,OnCreatureEvalScoreUpdateAckHandler);
    }

    void OnCreatureEvalScoreUpdateAckHandler(C2G.CreatureEvalScoreUpdate send, C2G.CreatureEvalScoreUpdateAck recv)
    {
        OnStarUpdateCallback(recv.avg_score, send.score);
        OnClose();

        Tooltip.Instance.ShowMessageKey("EvalScoreUpdate");
    }

    void SetStar(int star)
    {
        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].SetSpriteActive(star > i);
        }
    }

}
