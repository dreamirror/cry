using UnityEngine;
using PacketInfo;
using PacketEnums;

public class PopupFriendsRequest : PopupBase {

    public UILabel m_LabelMessage;
    public UILabel m_InputMessage;
    public UIInput m_Input;

    System.Action<string> OnSuccessCallback = null;
    int NicknameMin = 2;
    int NicknameMax = 8;
    public override void SetParams(bool is_new, object[] parms)
    {
        NicknameMin = GameConfig.Get<int>("nickname_min");
        NicknameMax = GameConfig.Get<int>("nickname_max");
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
            OnSuccessCallback = parms[0] as System.Action<string>;

        m_Input.characterLimit = NicknameMax;
        m_Input.value = "";
        m_LabelMessage.text = Localization.Format("InputNicknameFormat", NicknameMin, NicknameMax);
    }

    public void OnClickRequest()
    {
        int len = m_InputMessage.text.Length;
        if(len < NicknameMin || len > NicknameMax)
        {
            Tooltip.Instance.ShowMessageKey("NicknameNotAvailable");
            return;
        }

        C2G.FriendsRequestWithNickname packet = new C2G.FriendsRequestWithNickname();
        packet.nickname = m_InputMessage.text;
        Network.GameServer.JsonAsync<C2G.FriendsRequestWithNickname, C2G.FriendsAckBase>(packet, OnFriendsRequestWithNicknameHandler);
    }

    void OnFriendsRequestWithNicknameHandler(C2G.FriendsRequestWithNickname packet, C2G.FriendsAckBase ack)
    {
        switch (ack.result)
        {
            case pe_FriendsResult.Success:
                base.OnClose();
                if (OnSuccessCallback != null)
                    OnSuccessCallback(packet.nickname);
                break;
            default:
                Friends.ShowFriendsErrorTooltip(ack.result);
                break;
        }
    }
}
