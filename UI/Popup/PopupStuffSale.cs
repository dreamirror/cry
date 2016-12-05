using UnityEngine;
using System.Collections;

public class PopupStuffSale : PopupBase {

    public UILabel TitleLabel;
    public UILabel CurrentLabel;

    public UILabel PriceLabel;

    short m_ChosenCount;

    Item m_ItemInfo;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms == null || parms.Length != 1)
            throw new System.Exception("invalid parms");

        m_ItemInfo = (Item)parms[0];
        m_ChosenCount = 1;
        
        Init();
    }

    void Init()
    {
        TitleLabel.text = m_ItemInfo.Info.Name;
        
        UpdateLabels(m_ChosenCount);
    }

    void UpdateLabels(int change_count)
    {
        PriceLabel.text = string.Format("{0}", change_count * m_ItemInfo.Info.SalePrice);
        CurrentLabel.text = string.Format("{0} / {1}", change_count, m_ItemInfo.Count);
    }

    public void OnMaxBtn()
    {
        m_ChosenCount = m_ItemInfo.Count;
        UpdateLabels(m_ChosenCount);
    }

    public void OnAddBtn()
    {
        if (m_ChosenCount == m_ItemInfo.Count)
            return;
        UpdateLabels(++m_ChosenCount);
    }

    public void OnRemoveBtn()
    {
        if (m_ChosenCount <= 1)
            return;
        UpdateLabels(--m_ChosenCount);
    }

    public void OnClickConfirm()
    {
        C2G.ItemSale packet = new C2G.ItemSale();
        packet.item_idn = m_ItemInfo.Info.IDN;
        packet.item_count = m_ChosenCount;

        Network.GameServer.JsonAsync<C2G.ItemSale, C2G.ItemSaleAck>(packet, OnItemSaleHandler);
        
    }
    public void OnItemSaleHandler(C2G.ItemSale send, C2G.ItemSaleAck recv)
    {
        Tooltip.Instance.ShowMessageKey("SuccessSale");
        m_ItemInfo.UseItem(send.item_count);
        Network.PlayerInfo.AddGoods(recv.add_gold);
        GameMain.Instance.UpdatePlayerInfo();

        OnClose();
    }

    public void OnClickCancel()
    {
        OnClose();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
