using UnityEngine;
using System.Collections;

public class PopupChatLine : MonoBehaviour {
    
    public UILabel m_MainLabel;

    ChatLine line;

    public ChatLine Line { get { return line; } }
    PopupChat.OnNicknameClickDelegate nickname_delegate = null;
    PopupChat.OnItemClickDelegate item_delegate = null;

    public void Init(ChatLine line, PopupChat.OnNicknameClickDelegate nick_delegate, PopupChat.OnItemClickDelegate item_delegate) 
    {
        nickname_delegate = nick_delegate;
        this.item_delegate = item_delegate;
        
        this.line = line;
        m_MainLabel.color = line.GetColor();
        m_MainLabel.text = line.Msg;
    }

    public void OnClick()
    {
        string msg = m_MainLabel.GetUrlAtPosition(UICamera.lastWorldPosition);

        if (string.IsNullOrEmpty(msg) == true)
            return;

        switch (msg)
        {
            case "Character":
                item_delegate(this);
                break;
            case "Profile":
                nickname_delegate(this);
                break;
            default:
                break;
        }
    }
}
