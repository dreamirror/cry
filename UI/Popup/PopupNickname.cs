using UnityEngine;
using PacketInfo;
using PacketEnums;
using System.Linq;

public class PopupNickname : PopupBase
{
    public UILabel m_Title;
    public UILabel m_Message, m_Add;
    public UILabel m_InputMessage;
    public UIInput m_Input;
    public GameObject m_PriceBG;
    public UILabel m_Price;
    public GameObject m_BtnCancel;
    public UIGrid m_GridBtns;
    public UILabel m_LabelBtnOk;

    int price;
    bool is_change;
    EventDelegate.Callback m_callback = null;

    int NicknameMin = 2;
    int NicknameMax = 8;

    void OnEnable()
    {
        m_Input.isSelected = true;
    }

    public override void SetParams(bool is_new, object[] parms)
    {
        NicknameMin = GameConfig.Get<int>("nickname_min");
        NicknameMax = GameConfig.Get<int>("nickname_max");

        base.SetParams(is_new, parms);

        is_change = true;
        if (parms != null)
        {
            if(parms.Length > 0)
                is_change = (bool)parms[0];
            if (parms.Length > 1)
                m_callback = (EventDelegate.Callback)parms[1];
        }


        m_Message.text = Localization.Format("NicknameMessageFormat", NicknameMin, NicknameMax);
        m_Input.characterLimit = NicknameMax;

        if (is_change)
        {
            m_Title.text = Localization.Get("NicknameChangeTitle");
            m_InputMessage.text = Network.PlayerInfo.nickname;
            m_Input.value = Network.PlayerInfo.nickname;
            m_PriceBG.SetActive(true);
            m_Add.gameObject.SetActive(false);
            price = GameConfig.Get<int>("change_nickname_price");
            m_Price.text = Localization.Format("GoodsFormat", price);
            m_LabelBtnOk.text = Localization.Get("NicknameChange");
            m_BtnCancel.SetActive(true);
        }
        else
        {
            m_Title.text = Localization.Get("NicknameSetTitle");
            m_InputMessage.text = "";
            m_Input.value = "";
            m_BtnCancel.SetActive(false);
            m_PriceBG.SetActive(false);
            m_Add.gameObject.SetActive(true);
            m_LabelBtnOk.text = Localization.Get("NicknameSet");
        }

        m_GridBtns.Reposition();
    }

    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        m_GridBtns.Reposition();
    }
    public void OnClickChange()
    {
        int len = m_InputMessage.text.Length;
        if (m_InputMessage.text.All(t => char.IsLetterOrDigit(t)) == false || len < NicknameMin || len > NicknameMax)
        {
            Tooltip.Instance.ShowMessageKey("NicknameNotAvailable");
            return;
        }

        if (is_change && Network.PlayerInfo.GetGoodsValue(pe_GoodsType.token_gem) < price)
        {
            Tooltip.Instance.ShowMessageKey(string.Format("NotEnough{0}", pe_GoodsType.token_gem));
            return;
        }

        C2G.NicknameSet packet = new C2G.NicknameSet();
        packet.nickname = m_InputMessage.text;
        Network.GameServer.JsonAsync<C2G.NicknameSet, C2G.NicknameSetAck>(packet, OnNicknameSetAckHandler);
    }

    void OnNicknameSetAckHandler(C2G.NicknameSet packet, C2G.NicknameSetAck ack)
    {
        switch(ack.result)
        {
            case pe_NicknameResult.Success:
                Network.PlayerInfo.nickname = packet.nickname;

                if (is_change)
                {
                    Tooltip.Instance.ShowMessageKey("NicknameChanged");
                    GameMain.Instance.UpdatePlayerInfo();
                }
                Network.PlayerInfo.UseGoodsValue(pe_GoodsType.token_gem, price);

                if (Tutorial.Instance.Completed == false)
                    parent.Close(true, true);
                else
                    base.OnClose();

                if(is_change == false && m_callback != null)
                {
                    m_callback();
                }

                if (Network.ChatServer.IsConnected == true)
                    Network.ChatServer.ChangeNickname(packet.nickname);
                else
                    ChattingMain.Instance.Init();

                break;
            default:
                Tooltip.Instance.ShowMessageKey("NicknameNotAvailable");
                break;
        }
    }
}
