using UnityEngine;
using System;
using System.Collections.Generic;

public class KingsGift : MonoBehaviour {

    public UICharacterCutSceneContainer m_KingCharacter;
    public UILabel m_KingsScript;
    public UILabel m_KingsRemainTime;
    
    public UISprite m_KingsRewardSprite;
    public GameObject m_KingsReward;

    public TweenPosition m_KingsRewardTween;
    public TweenScale m_KingsScriptTween;

    public GameObject m_Button;
    
    DateTime KingsScriptStartTime;

    bool is_expire = false;

    public void KingsGiftUpdate()
    {
        if (KingsGiftInfoManager.Instance.IsKingsGiftActive == true)
            UpdateKingsGift();
    }

    public void Init()
    {
        m_KingsReward.SetActive(false);        
        m_KingsRewardTween.enabled = false;
        
        if (Network.PlayerInfo.kings_gift == null || Network.PlayerInfo.kings_gift.reward_data.goods_type == PacketInfo.pe_GoodsType.invalid)
            RequestKingsGift();
        else
            ChangeKingsPreset();

        m_KingsRewardSprite.spriteName = Network.PlayerInfo.kings_gift.reward_data.goods_type.ToString();

        m_KingsScript.gameObject.SetActive(false);
    }

    public void OnClickKingsGift()
    {

        m_KingsScript.gameObject.SetActive(true);
        
        KingsScriptStartTime = DateTime.Now;
        
        bool is_takeable = Network.PlayerInfo.kings_gift.takeable_at < Network.Instance.ServerTime;
        m_KingsScript.text = KingsGiftInfoManager.Instance.GetRandomScript(Network.PlayerInfo.kings_gift.kings_gift_idn, is_takeable);
        
        m_KingsScriptTween.enabled = true;
        m_KingsScriptTween.ResetToBeginning();
        m_KingsScriptTween.PlayForward();

        if (is_takeable == true)
        {//Complete
            m_Button.SetActive(false);

            m_KingsRewardTween.AddOnFinished(new EventDelegate(RequestKingsGift));

            m_KingsRewardTween.enabled = true;
            m_KingsRewardTween.ResetToBeginning();
            m_KingsRewardTween.PlayForward();
        }
        else
        {//Show Script            
            m_KingCharacter.Character.Stop();
            m_KingCharacter.Character.Play(false, "touch");
        }
    }

    void RequestKingsGift()
    {
        C2G.KingsGiftRefresh packet = new C2G.KingsGiftRefresh();
        packet.last_map_id = MapInfoManager.Instance.GetInfoByIdn(MapClearDataManager.Instance.GetLastMainStage().map_idn).ID;
        Network.GameServer.JsonAsync<C2G.KingsGiftRefresh, C2G.KingsGiftRefreshAck>(packet, OnKingsGift);
        
        m_KingsRewardTween.onFinished = new List<EventDelegate>();
    }

    void OnKingsGift(C2G.KingsGiftRefresh send, C2G.KingsGiftRefreshAck recv)
    {
        m_KingsReward.SetActive(false);
        m_KingsRewardTween.ResetToBeginning();
        if (recv.got_info.goods_type != PacketInfo.pe_GoodsType.invalid)
        {
            List<RewardBase> reward = new List<RewardBase>();
            reward.Add(new RewardBase(40000 + (int)recv.got_info.goods_type, (int)recv.got_info.goods_value));
            Popup.Instance.Show(ePopupMode.Reward, reward, Localization.Get("PopupRewardTitle"), Localization.Get("GetThisRewards"));
        }
        is_expire = false;

        Network.PlayerInfo.AddGoodsValue(Network.PlayerInfo.kings_gift.reward_data.goods_type, Network.PlayerInfo.kings_gift.reward_data.goods_value);
        GameMain.Instance.UpdatePlayerInfo();
        m_Button.SetActive(true);
        Network.PlayerInfo.kings_gift = recv.next_info;
        Init();
    }


    void UpdateKingsGift()
    {
        if (gameObject.activeSelf == false)
            return;
        
        UpdateKingsRemainTime();

        if ((DateTime.Now - KingsScriptStartTime).TotalSeconds > 2)
        {   
            m_KingsScript.gameObject.SetActive(false);
        }

        if (is_expire == false && Network.Instance.ServerTime > Network.PlayerInfo.kings_gift.takeable_at)
        {
            is_expire = true;
            ChangeKingsPreset();
        }
    }

    void UpdateKingsRemainTime()
    {
        TimeSpan ts = (Network.PlayerInfo.kings_gift.takeable_at - Network.Instance.ServerTime);
        if (ts.TotalSeconds > 0)
        {
            m_KingsRemainTime.gameObject.SetActive(true);            
            if (ts.Hours > 0)
                m_KingsRemainTime.text = Localization.Format("RemainsTime", Localization.Format("HourMinute", ts.Hours, ts.Minutes));
            else if (ts.Minutes > 0)
                m_KingsRemainTime.text = Localization.Format("RemainsTime", Localization.Format("MinuteSeconds", ts.Minutes, ts.Seconds));
            else
                m_KingsRemainTime.text = Localization.Format("RemainsTime", Localization.Format("Seconds", ts.Seconds));
        }
        else
        {               
            m_KingsRemainTime.gameObject.SetActive(false);
        }
    }

    void ChangeKingsPreset()
    {
        if (Network.PlayerInfo.kings_gift.takeable_at < Network.Instance.ServerTime == false)
        {
            KingsScriptSet script_set = KingsGiftInfoManager.Instance.GetScriptSetInfo(Network.PlayerInfo.kings_gift.kings_gift_idn);
            m_KingCharacter.Init(AssetManager.GetCharacterCutSceneAsset(string.Format("089_king_present_{0}", script_set.kings_preset)), "default");
        }
        else
        {
            is_expire = true;
            m_KingCharacter.Init(AssetManager.GetCharacterCutSceneAsset(string.Format("089_king_present_{0}", "default")), "default");
            m_KingsReward.SetActive(true);
        }
        gameObject.SetActive(true);
    }

}
