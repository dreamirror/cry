using PacketInfo;
using UnityEngine;
using System.Collections.Generic;
using PacketEnums;

public class PopupPVPDelayReset : PopupBase
{
    public UILabel m_title;
    public UILabel m_message;

    public UISprite m_icon;
    public UILabel m_price;

    public UIButton m_btnOK;

    int price;
    bool bDelayReset;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        bDelayReset = (bool)parms[0];
        if (bDelayReset)
        {
            m_title.text = Localization.Get("PVPDelayResetTitle");
            m_message.text = Localization.Get("PVPDelayResetConfirm");
            price = GameConfig.Get<int>("pvp_delay_reset_cost");
            m_price.text = Localization.Format("GoodsFormat", price);
        }
        else
        {
            m_title.text = Localization.Get("PVPCountResetTitle");
            m_message.text = Localization.Format("PVPCountResetConfirm", GameConfig.Get<int>("pvp_daily_battle_count_max"));

            price = GameConfig.Get<int>("pvp_battle_count_cost");
            m_price.text = Localization.Format("GoodsFormat", price);
        }
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
    }

    public void OnCancel()
    {
        base.OnClose();
    }

    public void OnClickOK()
    {
        if(price > Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gem))
        {
            Popup.Instance.Show(ePopupMode.MoveStore, pe_GoodsType.token_gem);
            return;
        }

        if (bDelayReset)
        {
            C2G.PvpBuyBattleTime packet = new C2G.PvpBuyBattleTime();
            Network.GameServer.JsonAsync<C2G.PvpBuyBattleTime, C2G.PvpBuyBattleTimeAck>(packet, OnPvpBuyBattleTime);
        }
        else
        {
            C2G.PvpBuyBattleCount packet = new C2G.PvpBuyBattleCount();
            Network.GameServer.JsonAsync<C2G.PvpBuyBattleCount, C2G.PvpBuyBattleCountAck>(packet, OnPvpBuyBattleCount);
        }
    }

    void OnPvpBuyBattleTime(C2G.PvpBuyBattleTime packet, C2G.PvpBuyBattleTimeAck ack)
    {
        if (ack.use_goods != null)
        {
            Network.PlayerInfo.UseGoods(ack.use_goods);
            GameMain.Instance.UpdatePlayerInfo();
            PVP pvp_menu = GameMain.Instance.GetCurrentMenu().obj.GetComponent<PVP>();
            if (pvp_menu != null)
                pvp_menu.ResetAvailableBattleTime();
            base.OnClose();
        }
        else
        {
            //Tooltip.Instance.ShowMessageKey("");
        }
    }

    void OnPvpBuyBattleCount(C2G.PvpBuyBattleCount packet, C2G.PvpBuyBattleCountAck ack)
    {
        if (ack.use_goods != null)
        {
            Network.PlayerInfo.UseGoods(ack.use_goods);
            GameMain.Instance.UpdatePlayerInfo();
            PVP pvp_menu = GameMain.Instance.GetCurrentMenu().obj.GetComponent<PVP>();
            if (pvp_menu != null)
                pvp_menu.ResetBattleCount();
            base.OnClose();
        }
    }
}
