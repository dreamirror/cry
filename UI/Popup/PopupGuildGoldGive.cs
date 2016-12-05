using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using PacketEnums;

public class PopupGuildGoldGive : PopupBase
{
    public UILabel m_LabelGiveGold;
    public UILabel m_LabelGiveGoldMaxDesc;
    public UIRepeatButton m_btn10000;
    public UIRepeatButton m_btn1000;

    int m_GiveGold = 0;
    long m_AvailableGold = 0;
    void Start()
    {
        m_btn1000._OnPressed = OnPress1000;
        m_btn1000._OnRepeat = OnRepeat1000;
        m_btn10000._OnPressed = OnPress10000;
        m_btn10000._OnRepeat = OnRepeat10000;

    }
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_GiveGold = 0;
        m_AvailableGold = Network.PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gold);
        m_LabelGiveGoldMaxDesc.text = Localization.Format("GuildGoldGiveMaxDesc", GuildInfoManager.Config.GiveGoldMax);
        OnClickReset();
    }
    void UpdateGold()
    {
        m_LabelGiveGold.text = Localization.Format("GoodsFormat", m_GiveGold);
    }
    void AddGold(int add_gold, UIRepeatButton btn = null)
    {
        if (m_GiveGold + add_gold > m_AvailableGold)
        {
            Tooltip.Instance.ShowMessageKey("NotEnoughtoken_gold");
            if (btn != null)
            {
                btn.enabled = false;
                btn.SetPressed(false);
            }
            return;
        }
        if (m_GiveGold + add_gold > GuildInfoManager.Config.GiveGoldMax)
        {
            //Tooltip.Instance.ShowMessageKey("NotEnoughtoken_gold");
            if (btn != null)
            {
                btn.enabled = false;
                btn.SetPressed(false);
            }
            return;
        }
        m_GiveGold += add_gold;
        UpdateGold();
    }
    void OnPress10000()
    {
        //AddGold(10000, m_btn10000);
    }
    void OnRepeat10000()
    {
        AddGold(10000, m_btn10000);
    }
    void OnPress1000()
    {
        //AddGold(1000, m_btn1000);
    }
    void OnRepeat1000()
    {
        AddGold(1000, m_btn1000);
    }

    public void OnClick1000()
    {
        AddGold(1000);
    }

    public void OnClick10000()
    {
        AddGold(10000);
    }

    public void OnClickReset()
    {
        m_GiveGold = 0;
        UpdateGold();
        m_btn1000.enabled = true;
        m_btn10000.enabled = true;
    }

    public void OnClickGive()
    {
        if(m_GiveGold == 0)
        {
            return;
        }
        C2G.GuildGoldGive packet = new C2G.GuildGoldGive();
        packet.guild_idx = GuildManager.Instance.GuildIdx;
        packet.give_gold = m_GiveGold;
        Network.GameServer.JsonAsync<C2G.GuildGoldGive, C2G.GuildAck>(packet, OnGuildGoldGive);
    }

    void OnGuildGoldGive(C2G.GuildGoldGive packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                Network.PlayerInfo.UseGoods(ack.use_goods);
                GameMain.Instance.UpdatePlayerInfo();
                GuildManager.Instance.SetGuildInfo(ack.guild_info);
                GameMain.Instance.UpdateMenu();
                base.OnClose();
                break;
        }
    }
}
