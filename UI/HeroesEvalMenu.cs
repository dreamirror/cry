using UnityEngine;
using System.Collections.Generic;
using PacketInfo;
using LinqTools;
using System;

public class HeroesEvalMenu : MenuBase {

    public List<UIToggleSprite> m_Stars;

    public UIInput m_InputMessageLabel;
    public UISprite m_InputLabelSprite;

    public UILabel m_CharacterNameLabel;
    public UILabel m_FirstObtainerLabel;
    public UILabel m_EvalScoreLabel;

    public UILabel m_EvalNoItemLabel;

    public UIToggle m_ScoreToggle;

    public UIScrollView m_EvalBoardScroll;
    
    public PrefabManager m_EvalBoardPrefab, m_EvalReadMorePrefab;

    public UICharacterContainer m_CharacterContainer;

    const int GRID_ITEM_HEIGHT_SIZE = 110;

    C2G.CreatureEvalInitInfoAck info;
    string creature_id;
    int last_loaded_count = 0;
    List<pd_CreatureEvalBoard> item_list;    
    List<pd_CreatureEvalBoard> OrderedItemList()
    {
        List<pd_CreatureEvalBoard> list = new List<pd_CreatureEvalBoard>();
        list.AddRange(item_list.Where(l => l.is_best).OrderByDescending(l => l.good));
        list.AddRange(item_list.Where(l => l.is_best == false).OrderByDescending(l => l.board_idx));
        return list;
    }
    public override bool Init(MenuParams parms)
    {
        creature_id = parms.GetObject<string>("CreatureID");
        info = parms.GetObject<C2G.CreatureEvalInitInfoAck>("InitInfo");
        Init(parms.bBack, info );

        return true;
    }

    public void OnEnable()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                AndroidKeyboard.AdditionalOptions.fullScreen = true;
        #endif
    }

    public void OnDisable()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                AndroidKeyboard.AdditionalOptions.fullScreen = false;
        #endif
    }

    public override void UpdateMenu()
    {
        //Init(false);
    }

    public void Init(bool bBack, C2G.CreatureEvalInitInfoAck init_info = null)
    {
        //if (bBack == true)
        //    return;
        
        m_InputMessageLabel.defaultText = Localization.Get("EvalInputStartingMsg");
        CreatureInfo creature = CreatureInfoManager.Instance.GetInfoByID(creature_id);

        m_CharacterContainer.Init(AssetManager.GetCharacterAsset(creature_id, "default"), UICharacterContainer.Mode.UI_Normal);
        m_CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        m_CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);

        item_list = new List<pd_CreatureEvalBoard>();
        item_list = init_info.board_info;

        last_loaded_count = item_list.Count(i => i.is_best == false);

        DrawItems();

        m_ScoreToggle.value = (CreatureBookManager.Instance.IsExistBook(creature.IDN));
        m_EvalScoreLabel.text = string.Format("({0})", info.avg_score.ToString("F1"));
        m_FirstObtainerLabel.text = string.IsNullOrEmpty(info.first_obtainer_nickname) ? Localization.Get("EvalNoneFirstObtainer") : info.first_obtainer_nickname;
        m_CharacterNameLabel.text = creature.Name;

        for (int i = 0; i < m_Stars.Count; ++i)
            m_Stars[i].SetSpriteActive( Math.Truncate(init_info.avg_score) > i);

        m_EvalBoardScroll.ResetPosition();
    }

    public void OnClickWriteConfirmBtn()
    {
        if (m_InputMessageLabel.value.Equals(Localization.Get("EvalInputStartingMsg")) == true)
            return;

        if (m_InputMessageLabel.value.Length < 6 || m_InputMessageLabel.value.Length > 150)
        {
            Tooltip.Instance.ShowMessageKey("EvalInputErrorMsg");
            return;
        }

        C2G.CreatureEvalBoardWrite packet = new C2G.CreatureEvalBoardWrite();
        packet.creature_id = creature_id;
        packet.message = m_InputMessageLabel.value;

        Network.GameServer.JsonAsync<C2G.CreatureEvalBoardWrite, C2G.CreatureEvalBoardWriteAck>(packet, WriteAckHandler);
    }

    public void OnChangeInputLabel()
    {
        if (m_InputMessageLabel.value.Contains("\n") == true)
            m_InputMessageLabel.label.fontSize = 22 - (int)((m_InputMessageLabel.value.Split('\n').Length * 1.6) - 1);
        else
            m_InputMessageLabel.label.fontSize = 22;
    }

    public void OnClickCheckScore()
    {
        Popup.Instance.Show(ePopupMode.EvalScore, info.my_score, creature_id, new OnStarUpdate(EvalScoreChangeHandler));
    }

    bool IsDraggingCharacter = false;
    Vector2 m_TouchPosition = Vector2.zero, m_FirstTouchPosition = Vector2.zero;

    public void OnCharacterPress()
    {
        m_FirstTouchPosition = m_TouchPosition = UICamera.lastTouchPosition;
        IsDraggingCharacter = true;
    }

    public void OnCharacterRelease()
    {
        if (m_FirstTouchPosition == UICamera.lastTouchPosition)
        {
            m_CharacterContainer.PlayRandomAction();
        }
        m_TouchPosition = Vector2.zero;
        IsDraggingCharacter = false;
    }

    public void OnClickReadMore()
    {
        C2G.CreatureEvalMoreBoard packet = new C2G.CreatureEvalMoreBoard();
        packet.smallest_board_idx = OrderedItemList().Last().board_idx;
        packet.creature_id = creature_id;

        Network.GameServer.JsonAsync<C2G.CreatureEvalMoreBoard, C2G.CreatureEvalMoreBoardAck>(packet, OnReadMorePacketHandler);
    }

    public void OnClickInputbar()
    {
    }

    void OnReadMorePacketHandler(C2G.CreatureEvalMoreBoard send, C2G.CreatureEvalMoreBoardAck recv)
    {
        last_loaded_count = recv.board_info.Count;
        recv.board_info.ForEach(b => item_list.Add(b));
        DrawItems();
    }

    void EvalScoreChangeHandler(double avg_score, int my_score)
    {
        for (int i = 0; i < m_Stars.Count; ++i)
        {
            m_Stars[i].SetSpriteActive(avg_score > i);
        }
        m_EvalScoreLabel.text = string.Format("({0})", avg_score.ToString("F1"));

        info.avg_score = avg_score;
        info.my_score = my_score;

        //last_score_at = DateTime.Now;
    }

    void Update()
    {
        if (IsDraggingCharacter)
            UpdateDragCharacter();
    }

    void UpdateDragCharacter()
    {
        Vector2 pos = UICamera.lastTouchPosition;
        float delta = m_TouchPosition.x - pos.x;
        float speed = 0.5f;
        m_TouchPosition = pos;

        m_CharacterContainer.transform.localRotation *= Quaternion.Euler(0f, delta * speed, 0f);
    }

    void DrawItems()
    {
        m_EvalBoardPrefab.Destroy();
        m_EvalReadMorePrefab.Destroy();

        if (item_list.Count == 0)
        {
            m_EvalNoItemLabel.gameObject.SetActive(true);
            return;
        }

        m_EvalNoItemLabel.gameObject.SetActive(false);

        float last_position_y = 150f;
        
        foreach (var board in OrderedItemList())
        {
            var item = m_EvalBoardPrefab.GetNewObject<HeroesEvalItem>(m_EvalBoardScroll.transform, Vector3.zero);
            item.Init(board, creature_id, OnEvalBoardRemoveHandler, OnEvalChangeHandler);
            
            //collider size calc
            item.m_BoxCollider.size = new Vector2(item.m_BoxCollider.size.x, item.m_BoxCollider.size.y + item.m_Message.height - item.m_Message.fontSize);
            item.m_BoxCollider.offset = new Vector2(0, item.m_BoxCollider.offset.y - ((item.m_Message.height - item.m_Message.fontSize) / 2) );
            
            //item position calc
            item.transform.localPosition = new Vector3(0, last_position_y);
            last_position_y -= GRID_ITEM_HEIGHT_SIZE + (item.m_Message.height - item.m_Message.fontSize) ;
        }

        if (last_loaded_count >= 10)
        {
            var readmore_btn = m_EvalReadMorePrefab.GetNewObject<HeroesEvalReadMore>(m_EvalBoardScroll.transform, Vector3.zero);
            readmore_btn.Init(OnClickReadMore);
            readmore_btn.transform.localPosition = new Vector3(0, last_position_y);
        }
    }

    void WriteAckHandler(C2G.CreatureEvalBoardWrite send, C2G.CreatureEvalBoardWriteAck recv)
    {
        if (recv.is_success == true)
        {
            pd_CreatureEvalBoard data = new pd_CreatureEvalBoard();
            data.thumb_info = new pd_ThumbInfo();
            data.thumb_info.leader_creature = Network.PlayerInfo.leader_creature;
            data.thumb_info.nickname = Network.PlayerInfo.nickname;
            data.thumb_info.player_level = Network.PlayerInfo.player_level;
            data.thumb_info.account_idx = SHSavedData.AccountIdx;
            data.bad = 0;
            data.good = 0;
            data.board_idx = recv.board_idx;
            data.is_best = false;
            data.message = recv.message;
            data.my_eval_state = pe_EvalState.None;

            item_list.Add(data);

            DrawItems();
            m_EvalBoardScroll.ResetPosition();

            m_InputMessageLabel.value = string.Empty;

            Tooltip.Instance.ShowMessageKey("EvalWriteComplete");
        }
        else
            Popup.Instance.Show(ePopupMode.Message, Localization.Format("EvalWroteError", 1));
    }

    void OnEvalBoardRemoveHandler(long board_idx)
    {
        item_list.RemoveAll(i => i.board_idx == board_idx);
        DrawItems();
    }

    void OnEvalChangeHandler(long board_idx, pe_EvalState state)
    {
        m_EvalBoardScroll.GetComponentsInChildren<HeroesEvalItem>().Where(b => b.info.board_idx == board_idx).ToList().ForEach(b => b.ChangeEvalState(state));
    }
}
