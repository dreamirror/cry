using UnityEngine;
using System.Collections.Generic;
using PacketInfo;

public class PopupMailboxItem : MonoBehaviour {
    
    public UILabel title_label;
    public UILabel sender_name;
    public UILabel recv_at;
    
    public UISprite icon_boarder;
    public UISprite icon_normal;
    public UISprite panel_new;
    public UISprite panel_old;

    Mail mail_info;
    
    public void OnClickReadBtn()
    {
        if (mail_info.IsExistDetail == true)
            Popup.Instance.Show(ePopupMode.MailDetail, MailManager.Instance.GetMail(mail_info.MailIdx).Detail, null);
        else
        {
            C2G.MailRead packet = new C2G.MailRead();
            packet.mail_idx = mail_info.MailIdx;
            packet.is_read = mail_info.IsRead;
            packet.is_exist_reward = mail_info.ExistReward;
            Network.GameServer.JsonAsync<C2G.MailRead, C2G.MailReadAck>(packet, OnMailReadHandler);
        }
    }

    public void OnMailReadHandler(C2G.MailRead send, C2G.MailReadAck recv)
    {
        MailManager.Instance.SetDetail(send.mail_idx, recv.detail_info);
        Network.Instance.SetUnreadMail(MailManager.Instance.GetUnreadState());
        Popup.Instance.Show(ePopupMode.MailDetail, recv.detail_info,null);
    }

    public void Init(long mail_idx)
    {
        mail_info = MailManager.Instance.GetMail(mail_idx);

        title_label.text = mail_info.Title;
        sender_name.text = string.Format("{0}: {1}", Localization.Get("MailSender"), mail_info.Nickname);
        recv_at.text = string.Format("{0}: {1}", Localization.Get("MailRecv"), mail_info.CreatedTimeString);

        if (mail_info.IsActiveMail)
        {
            icon_normal.gameObject.SetActive(true);
            icon_normal.spriteName = "mailbox_mail";
            panel_old.gameObject.SetActive(true);
            panel_new.gameObject.SetActive(false);
        }
        else
        {   
            icon_normal.gameObject.SetActive(true);
            icon_normal.spriteName = "mailbox_mail_open";
            panel_old.gameObject.SetActive(false);
            panel_new.gameObject.SetActive(true);
        }
    }
}
