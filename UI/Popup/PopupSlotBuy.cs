using PacketInfo;
using UnityEngine;
using System.Collections.Generic;
using PacketEnums;

public class PopupSlotBuy: PopupBase
{
    public delegate void OnOkDeleage();
    OnOkDeleage _OnOK;

    public UIToggle m_toggle;
    public UILabel m_price, m_SlotCount, m_SlotCountNew, m_title, m_desc, m_desc_max;

    pe_SlotBuy m_BuyType;
    SlotInfo m_slot;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_BuyType = (pe_SlotBuy)parms[0];
        switch(m_BuyType)
        {
            case pe_SlotBuy.Creature:
                m_title.text = Localization.Get("HeroSlotBuy");
                m_desc.text = Localization.Get("HeroSlotBuyDesc");
                m_desc_max.text = Localization.Get("HeroSlotLimited");
                m_slot = CreatureInfoManager.Instance.Slot;
                m_SlotCount.text = Network.PlayerInfo.creature_count_max.ToString();
                m_SlotCountNew.text = (Network.PlayerInfo.creature_count_max + m_slot.AddCount).ToString();
                m_toggle.value = Network.PlayerInfo.creature_count_max >= m_slot.CountMax;
                break;

            case pe_SlotBuy.Rune:
                m_title.text = Localization.Get("RuneSlotBuy");
                m_desc.text = Localization.Get("RuneSlotBuyDesc");
                m_desc_max.text = Localization.Get("RuneSlotLimited");
                m_slot = RuneInfoManager.Instance.Slot;
                m_SlotCount.text = Network.PlayerInfo.rune_count_max.ToString();
                m_SlotCountNew.text = (Network.PlayerInfo.rune_count_max + m_slot.AddCount).ToString();
                m_toggle.value = Network.PlayerInfo.rune_count_max >= m_slot.CountMax;
                break;
        }

        m_price.text = Localization.Format("GoodsFormat", Network.PlayerInfo.GetSlotBuyCash(m_slot));
        if (parms.Length > 1)
            _OnOK = (OnOkDeleage)parms[1];
        else
            _OnOK = null;
    }

    public void OnCancel()
    {
        parent.Close();
    }

    public void OnClickOK()
    {
        if (Network.Instance.CheckGoods(pe_GoodsType.token_gem, Network.PlayerInfo.GetSlotBuyCash(m_slot)) == false)
            return;

        C2G.SlotBuy packet = new C2G.SlotBuy();
        switch(m_BuyType)
        {
            case pe_SlotBuy.Creature:
                packet.buy_type = pe_SlotBuy.Creature;
                packet.count_buy_count = Network.PlayerInfo.creature_count_buy_count;
                break;

            case pe_SlotBuy.Rune:
                packet.buy_type = pe_SlotBuy.Rune;
                packet.count_buy_count = Network.PlayerInfo.rune_count_buy_count;
                break;
        }
        Network.GameServer.JsonAsync<C2G.SlotBuy, C2G.SlotBuyAck>(packet, OnSlotBuy);
    }

    void OnSlotBuy(C2G.SlotBuy packet, C2G.SlotBuyAck ack)
    {
        Network.PlayerInfo.UseGoodsValue(pe_GoodsType.token_gem, ack.use_gem);
        switch (packet.buy_type)
        {
            case pe_SlotBuy.Creature:
                Tooltip.Instance.ShowMessageKey("HeroSlotAdded");
                Network.PlayerInfo.creature_count_max = ack.count_max;
                Network.PlayerInfo.creature_count_buy_count = ack.count_buy_count;
                break;

            case pe_SlotBuy.Rune:
                Tooltip.Instance.ShowMessageKey("RuneSlotAdded");
                Network.PlayerInfo.rune_count_max = ack.count_max;
                Network.PlayerInfo.rune_count_buy_count = ack.count_buy_count;
                break;
        }
        GameMain.Instance.UpdatePlayerInfo();

        parent.Close(true, true);

        if (_OnOK != null)
        {
            _OnOK();
        }
    }
}
