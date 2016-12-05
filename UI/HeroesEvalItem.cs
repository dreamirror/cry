using UnityEngine;
using System.Collections;
using PacketInfo;

public delegate void OnItemRemovedCallback(long board_idx);
public delegate void OnEvalStateChangeCallback(long board_idx, pe_EvalState eval_state);

public class HeroesEvalItem : MonoBehaviour {

    public UIToggleSprite m_GoodBtn;
    public UIToggleSprite m_BadBtn;

    public UILabel m_GoodCountLabel;
    public UILabel m_BadCountLabel;

    public GameObject m_DeleteBtn;

    public GameObject m_BestEval;
    public UISprite m_ProfileSprite;
    public UILabel m_Level;
    public UILabel m_Nickname;

    public UILabel m_Message;

    public UISprite m_BGSprite;
    public BoxCollider2D m_BoxCollider;
    
    [HideInInspector]
    public pd_CreatureEvalBoard info = null;
    CreatureInfo creature_info;

    OnItemRemovedCallback remove_callback = null;
    OnEvalStateChangeCallback change_callback = null;
    
    public void Init(pd_CreatureEvalBoard init_info, string creature_id, OnItemRemovedCallback remove_callback, OnEvalStateChangeCallback change_callback)
    {
        info = init_info;
        creature_info = CreatureInfoManager.Instance.GetInfoByID(creature_id);
        this.remove_callback = remove_callback;
        this.change_callback = change_callback;

        string sprite_name = info.thumb_info.leader_creature.GetProfileName();
        m_ProfileSprite.spriteName = sprite_name;
        m_Nickname.text = info.thumb_info.nickname;
        m_Level.text = info.thumb_info.player_level.ToString();

        m_BestEval.SetActive(info.is_best);        
        m_DeleteBtn.SetActive(info.thumb_info.account_idx == SHSavedData.AccountIdx);
        m_Message.text = info.message;

        RefreshGoodBadCount();
    }

    C2G.CreatureEvalStateUpdate packet;
    public void OnClickStateUpdateBtn(GameObject obj)
    {
        packet = new C2G.CreatureEvalStateUpdate();
        packet.creature_id = creature_info.ID;
        packet.board_idx = info.board_idx;
        if (obj.name.Contains("good"))
        {
            if (m_GoodBtn.ActiveSprite == true)
                packet.eval_state = pe_EvalState.None;
            else
                packet.eval_state = pe_EvalState.Good;
        }
        else
        {
            if (m_BadBtn.ActiveSprite == true)
                packet.eval_state = pe_EvalState.None;
            else
                packet.eval_state = pe_EvalState.Bad;
        }

        if (packet.eval_state == info.my_eval_state)
            return;

        string confirm_msg;
        if (info.my_eval_state != pe_EvalState.None)
        {
            if (packet.eval_state != pe_EvalState.None)
                confirm_msg = Localization.Format("EvalStateChange", Localization.Get(string.Format("{0}{1}", "Eval", info.my_eval_state)), Localization.Get(string.Format("{0}{1}", "Eval", packet.eval_state)));
            else
                confirm_msg = Localization.Format("EvalStateCancel", Localization.Get(string.Format("{0}{1}", "Eval", info.my_eval_state)));

            Popup.Instance.ShowConfirm(new PopupConfirm.Callback(ConfirmStateUpdate), confirm_msg);
        }   
        else
            Network.GameServer.JsonAsync<C2G.CreatureEvalStateUpdate, C2G.CreatureEvalStateUpdateAck>(packet, EvalStateUpdateHandler);
    }

    void ConfirmStateUpdate(bool confirmed)
    {
        if(confirmed == true)
            Network.GameServer.JsonAsync<C2G.CreatureEvalStateUpdate, C2G.CreatureEvalStateUpdateAck>(packet, EvalStateUpdateHandler);
    }

    public void OnClickDeleteBtn()
    {
        Popup.Instance.ShowConfirm(new PopupConfirm.Callback(ConfirmBoardDelete), Localization.Get("EvalBoardDelete"));
    }

    public void ConfirmBoardDelete(bool confirmed)
    {
        if (confirmed == false)
            return;
        C2G.CreatureEvalBoardDelete packet = new C2G.CreatureEvalBoardDelete();
        packet.creature_id = creature_info.ID;
        packet.board_idx = info.board_idx;

        Network.GameServer.JsonAsync<C2G.CreatureEvalBoardDelete, C2G.CreatureEvalBoardDeleteAck>(packet, DeleteHandler);
    }

    public void RefreshGoodBadCount()
    {
        switch (info.my_eval_state)
        {
            case pe_EvalState.None:
                m_GoodBtn.SetSpriteActive(false);
                m_BadBtn.SetSpriteActive(false);
                break;
            case pe_EvalState.Good:
                m_GoodBtn.SetSpriteActive(true);
                m_BadBtn.SetSpriteActive(false);
                break;
            case pe_EvalState.Bad:
                m_GoodBtn.SetSpriteActive(false);
                m_BadBtn.SetSpriteActive(true);
                break;
        }

        m_GoodCountLabel.text = info.good.ToString();
        m_BadCountLabel.text = info.bad.ToString();
    }

    void DeleteHandler(C2G.CreatureEvalBoardDelete send, C2G.CreatureEvalBoardDeleteAck recv)
    {
        remove_callback(info.board_idx);
    }

    void EvalStateUpdateHandler(C2G.CreatureEvalStateUpdate send, C2G.CreatureEvalStateUpdateAck recv)
    {
        change_callback(info.board_idx, send.eval_state);
    }

    public void ChangeEvalState(pe_EvalState now)
    {
        switch (info.my_eval_state)
        {
            case pe_EvalState.None:
                if (now == pe_EvalState.Good)
                    info.good += 1;
                else
                    info.bad += 1;
                break;
            case pe_EvalState.Good:
                info.good -= 1;
                if (now == pe_EvalState.Bad)
                    info.bad += 1;
                break;

            case pe_EvalState.Bad:
                info.bad -= 1;
                if (now == pe_EvalState.Good)
                    info.good += 1;
                break;
        }
        info.my_eval_state = now;
        RefreshGoodBadCount();
    }

}
