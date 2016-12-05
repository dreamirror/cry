using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopupRuneEnchant : PopupBase {

    public delegate void OnOkDeleagate(Rune rune, bool is_success);

    public List<GameObject> m_RuneLevelObjs;
    public List<GameObject> m_RuneInfoObjs;

    public RuneItem m_Rune;

    public UILabel m_GoldSuccessPercentLabel, m_GoldCostLabel;
    public UILabel m_CashSuccessPercentLabel, m_CashCostLabel;
    public UILabel m_EnchantRuneLabel;

    public TweenFillAmount m_SuccessTween, m_FailTween;
    public UIPlayTween m_PTScale;

    public GameObject m_EnchantingBlock;

    public UIParticleContainer m_RuneEnchantFail, m_RuneEnchantSuccess;

    public GameObject m_EventNormal;
    public GameObject m_EventPremium;

    public Rune Rune { get; private set; }

    OnOkDeleagate EnchantDelegate = null;    

    //Color32 normal_color = new Color32(66, 33, 0, 255);
    Color32 new_color = new Color32(162, 238, 30, 255);
    Color32 new_color_bonus = new Color32(255, 30, 150, 255);

    EventDelegate SuccessTweenDelegate;
    EventDelegate FailTweenDelegate;
    

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        
        Rune = parms[0] as Rune;
        EnchantDelegate = parms[1] as OnOkDeleagate;
        Init();

        SuccessTweenDelegate = new EventDelegate(OnFinishSuccessTween);
        FailTweenDelegate = new EventDelegate(OnFinishFailTween);

        //SuccessIdleDelegate = new EventDelegate(OnFinishSuccess);
        //FailIdleDelegate = new EventDelegate(OnFinishFail);
        m_EnchantingBlock.SetActive(false);
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    void Init()
    {
        m_SuccessTween.gameObject.SetActive(false);
        m_FailTween.gameObject.SetActive(false);        

        m_Rune.RefreshRuneInfo(Rune);
        m_Rune.GetComponent<BoxCollider2D>().enabled = false;

        m_EnchantRuneLabel.text = Rune.GetName();

        m_RuneLevelObjs[0].GetComponent<UILabel>().text = Localization.Get("RuneLevel");
        m_RuneLevelObjs[1].GetComponent<UILabel>().text = Rune.Level.ToString();

        m_RuneInfoObjs[0].GetComponent<UILabel>().text = Localization.Get(string.Format("StatType_{0}", SkillInfoManager.Instance.GetInfoByID(Rune.Info.Skill.ID).Actions[0].statType));
        m_RuneInfoObjs[1].GetComponent<UILabel>().text = Rune.GetValue().ToString();

        if (Rune.Level == Rune.Info.GradeInfo.MaxLevel)
        {
            m_RuneLevelObjs[1].GetComponent<UILabel>().text = Localization.Get("MAX");
            //m_RuneInfoObjs[1].GetComponent<UILabel>().text = Rune.GetValue().ToString();

            m_RuneLevelObjs[2].SetActive(false);
            m_RuneLevelObjs[3].SetActive(false);
            m_RuneLevelObjs[4].SetActive(false);
            m_RuneInfoObjs[2].SetActive(false);
            m_RuneInfoObjs[3].SetActive(false);

            m_GoldCostLabel.text = Localization.Get("MAX");
            m_GoldSuccessPercentLabel.text = string.Format("{0:F2} %", 0);
            m_CashCostLabel.text = Localization.Get("MAX");
            m_CashSuccessPercentLabel.text = string.Format("{0:F2} %", 0);

            m_EventNormal.SetActive(false);
            m_EventPremium.SetActive(false);
        }
        else
        {
            var enchanted_rune = Rune.Clone();
            enchanted_rune.OnLevelUp();

            m_RuneLevelObjs[2].SetActive(true);
            m_RuneLevelObjs[3].SetActive(true);
            //m_RuneLevelObjs[4].SetActive(true);

            m_RuneLevelObjs[3].GetComponent<UILabel>().text = string.Format("{0}", enchanted_rune.Level);
            m_RuneLevelObjs[3].GetComponent<UILabel>().color = enchanted_rune.Level % 5 == 0 ? new_color_bonus : new_color;
            m_RuneLevelObjs[3].GetComponent<UILabel>().effectStyle = UILabel.Effect.Outline;

            //m_RuneLevelObjs[4].GetComponent<UILabel>().text = string.Format("/ {0}", enchanted_rune.Info.GradeInfo.MaxLevel);

            m_RuneInfoObjs[2].SetActive(true);
            m_RuneInfoObjs[3].SetActive(true);
            m_RuneInfoObjs[3].GetComponent<UILabel>().text = enchanted_rune.GetValue().ToString();
            m_RuneInfoObjs[3].GetComponent<UILabel>().color = enchanted_rune.Level % 5 == 0 ? new_color_bonus : new_color;
            m_RuneInfoObjs[3].GetComponent<UILabel>().effectStyle = UILabel.Effect.Outline;

            int cost = Rune.GetEnchantCostValue(false);
            int premium_cost = Rune.GetEnchantCostValue(true);
            var premium_event_info = EventHottimeManager.Instance.GetInfoByID("rune_enchant_premium_discount");
            var event_info = EventHottimeManager.Instance.GetInfoByID("rune_enchant_discount");

            if (event_info != null)
                cost = (int)(cost * event_info.Percent);
            if (premium_event_info != null)
                premium_cost = (int)(premium_cost * event_info.Percent);

            m_GoldCostLabel.text = cost.ToString();
            m_GoldSuccessPercentLabel.text = string.Format("{0:F2} %", Rune.GetEnchantPercent(false));

            m_CashCostLabel.text = premium_cost.ToString();
            m_CashSuccessPercentLabel.text = string.Format("{0:F2} %", Rune.GetEnchantPercent(true));
            
            m_EventNormal.SetActive(EventHottimeManager.Instance.IsRuneEnchantNormalEvent);
            m_EventPremium.SetActive(EventHottimeManager.Instance.IsRuneEnchantPremiumEvent);
        }
    }

    //public void OnValueChanged(UIToggle toggle)
    //{
    //    if (toggle.instantTween == true)
    //        return;
    //    int cost = Rune.GetEnchantCostValue(toggle.name.Equals("EnchantNormal"));
    //    EventInfo event_info;
    //    if (toggle.name.Equals("EnchantNormal") == true)
    //    {
    //        event_info = EventInfoManager.Instance.GetInfoByID("rune_enchant_discount");
    //        m_EnchantSuccessLabel.text = string.Format("{0} %", Rune.GetEnchantPercent(true));
    //    }
    //    else
    //    {
    //        event_info = EventInfoManager.Instance.GetInfoByID("rune_enchant_premium_discount");
    //        m_EnchantSuccessLabel.text = string.Format("{0} %", Rune.GetEnchantPercent(false));
    //    }
    //    if (event_info.IsEventTime())
    //        cost = (int)(cost * event_info.Percent);
    //    m_EnchantCostLabel.text = cost.ToString();
    //}

    public void OnClickConfirm(GameObject btn_obj)
    {
        bool is_gold = btn_obj.name.Contains("Gold");
        if (Rune.Level == Rune.Info.GradeInfo.MaxLevel)
        {
            Tooltip.Instance.ShowMessageKey("MaxRuneLevel");
            return;
        }

        int cost;
        PacketInfo.pd_EventHottime event_info;
        if (is_gold)
        {
            cost = Rune.GetEnchantCostValue(false);
            event_info = EventHottimeManager.Instance.GetInfoByID("rune_enchant_discount");
        }
        else
        {
            cost = Rune.GetEnchantCostValue(true);
            event_info = EventHottimeManager.Instance.GetInfoByID("rune_enchant_premium_discount");
        }
        if (event_info != null)
            cost = (int)(cost * event_info.Percent);

        if (Network.PlayerInfo.GetGoodsValue(is_gold ? PacketInfo.pe_GoodsType.token_gold : PacketInfo.pe_GoodsType.token_gem) < cost)
        {
            Popup.Instance.Show(ePopupMode.MoveStore, is_gold ? PacketInfo.pe_GoodsType.token_gold : PacketInfo.pe_GoodsType.token_gem);
            return;
        }

        m_EnchantingBlock.SetActive(true);

        C2G.RuneEnchant packet = new C2G.RuneEnchant();
        packet.is_premium = !is_gold;
        packet.rune_idx = Rune.RuneIdx;
        packet.rune_level = Rune.Level;
        packet.rune_grade = Rune.Info.Grade;
        Network.GameServer.JsonAsync<C2G.RuneEnchant, C2G.RuneEnchantAck>(packet, OnEnchantResultHandler);
    }

    public void OnEnchantResultHandler(C2G.RuneEnchant send , C2G.RuneEnchantAck recv)
    {
        //m_RuneEnchantIdle.Play();
        Network.PlayerInfo.UseGoods(recv.use_goods);
        GameMain.Instance.UpdatePlayerInfo();
        if (recv.is_success == true)
        {
            RuneManager.Instance.EnchantRune(Rune);

            m_SuccessTween.gameObject.SetActive(true);
            m_SuccessTween.AddOnFinished(SuccessTweenDelegate);
            m_SuccessTween.ResetToBeginning();
            m_SuccessTween.PlayForward();

            m_RuneEnchantSuccess.Play();
        }
        else
        {
            m_FailTween.gameObject.SetActive(true);
            m_FailTween.AddOnFinished(FailTweenDelegate);
            m_FailTween.ResetToBeginning();
            m_FailTween.PlayForward();

            m_RuneEnchantFail.Play();
        }
    }

    public void OnFinishSuccessTween()
    {           
        m_SuccessTween.RemoveOnFinished(SuccessTweenDelegate);

        m_EnchantingBlock.SetActive(false);
        m_PTScale.Play(true);

        Init();

        if (Rune.CreatureIdx != 0)
        {
            CreatureManager.Instance.GetInfoByIdx(Rune.CreatureIdx).EnchantRune(Rune);
        }
        m_SuccessTween.gameObject.SetActive(false);
        //Tooltip.Instance.ShowMessageKey("RuneEnchantSuccess");

        if (EnchantDelegate != null)
            EnchantDelegate(Rune, true);
    }

    public void OnFinishFailTween()
    {
        m_FailTween.RemoveOnFinished(FailTweenDelegate);

        m_EnchantingBlock.SetActive(false);
        m_FailTween.gameObject.SetActive(false);
        //Tooltip.Instance.ShowMessageKey("RuneEnchantFail");
    }    

    IEnumerator SuccessAction()
    {
        yield return new WaitForSeconds(0.5f);
    }
}
