using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PacketInfo;
using System.Linq;
using Newtonsoft.Json;
using System;

public class MailManager : MNS.Singleton<MailManager>
{
    public bool IsInit { get; private set; }
    public MailManager()
    {
        Mails = new List<Mail>();
    }

    public void Init(List<pd_MailInfo> mail_list)
    {
        IsInit = true;

        if (Mails.Count > 0)
        {
            List<Mail> noexist = new List<Mail>();
            foreach (var mail in Mails)
                if (mail_list.Exists(m => m.mail_idx == mail.MailIdx) == false)
                    noexist.Add(mail);
            if(noexist.Count > 0)
                noexist.ForEach(i => Mails.Remove(i));
        }

        foreach (var info in mail_list)
        {
            int index = Mails.FindIndex(mail => mail.MailIdx == info.mail_idx);
            if (index >= 0)
                Mails[index].Data = info;
            else
                Mails.Add(new Mail(info));
        }

        OrderMailData();
    }

    ////////////////////////////////////////////////////////////////
    public List<Mail> Mails { get; private set; }
    
    void OrderMailData()
    {
        Mails = Mails.OrderByDescending(mail => mail.Data.is_read == false || mail.Data.is_rewarded == false).ThenByDescending(mail => mail.Data.created_at).ToList();        
    }

    public void SetDetail(long mail_idx, pd_MailDetailInfo detail)
    {
        int mail_node = Mails.FindIndex(i => i.Data.mail_idx == mail_idx);
        if (mail_node >= 0)
            Mails[mail_node].SetDetail(detail);
    }

    public void SetRewarded(long mail_idx)
    {
        Mails.Find(mail => mail.MailIdx == mail_idx).SetRewarded();
        OrderMailData();
    }

    public Mail GetMail(long mail_idx)
    {
        return Mails.Find(m => m.Data.mail_idx == mail_idx);
    }

    public void SetNotifyMail(C2G.NotifyMailGetAck info)
    {
        info.mail_info = info.mail_info.OrderByDescending(mail => mail.mail_idx).ToList();
        foreach (var detail_info in info.mail_info)
        {
            int index = Mails.FindIndex(mail => mail.MailIdx == detail_info.mail_idx);
            if (index >= 0)
            {
                Mails[index].Detail = detail_info;
                Mails[index].Data.is_read = false;
            }
            else
                Mails.Add(new Mail(detail_info));
        }
    }

    public List<pd_MailDetailInfo> GetNotifyMail()
    {
        List<pd_MailDetailInfo> list = new List<pd_MailDetailInfo>();
        Mails.FindAll(mail => mail.MainNotifyMail && mail.IsRewarded == false && mail.IsRead == false).ForEach(mail => list.Add(mail.Detail));
        return list;
    }

    public void ReadNotifyMail(pd_MailDetailInfo notify_mail)
    {
        Mails.Find(mail => mail.MailIdx == notify_mail.mail_idx).Data.is_read = true;
    }
    
    public PacketEnums.pe_UnreadMailState GetUnreadState()
    {
        if (Mails.Any(mail => mail.MainNotifyMail && mail.IsRewarded == false))
            return PacketEnums.pe_UnreadMailState.MainMenuOpen;
        else if (Mails.Any(mail => mail.IsRead == false || (mail.ExistReward == true && mail.IsRewarded == false)))
            return PacketEnums.pe_UnreadMailState.UnreadMail;
        else
            return PacketEnums.pe_UnreadMailState.None;
    }
}

public class Mail
{
    public pd_MailDetailInfo Detail { get; set; }
    public pd_MailInfo Data { get; set; }

    public Mail(pd_MailInfo data)
    {
        Data = data;
        Detail = new pd_MailDetailInfo();
    }
    /// <summary>
    /// NOTIFY MAIL ONLY
    /// </summary>
    /// <param name="detail_data"></param>
    public Mail(pd_MailDetailInfo detail_data)
    {
        Data = new pd_MailInfo();        
        Data.mail_idx = detail_data.mail_idx;        
        Data.exists_reward = detail_data.rewards.Count > 0;        
        Data.open_direct = true;
        Detail = detail_data;
    }

    public void SetRewarded()
    {
        Data.is_rewarded = true;
        Detail.used_reward = true;
    }
    public void SetRead()
    {
        Data.is_read = true;
    }

    public void SetDetail(pd_MailDetailInfo detail)
    {
        Data.is_read = true;
        Detail = detail;

        IsExistDetail = true;
    }
    public bool IsExistDetail { get; private set; }
    public bool IsActiveMail { get { return (ExistReward == false && IsRead == false) || (ExistReward == true && IsRewarded == false); } }
    
    public long MailIdx { get { return Data.mail_idx; } }

    public bool IsRewarded { get { return Data.is_rewarded; } }
    public bool IsRead { get { return Data.is_read; } }
    public bool ExistReward { get { return Data.exists_reward; } }
    public bool MainNotifyMail { get { return Data.open_direct; } }

    public string Title { get { return Data.title; } }
    public string Message { get { return Detail.body_message; } }
    public string Nickname { get { return Data.sender_nickname; } }
    public string CreatedTimeString { get { return Data.created_at.ToString(Localization.Get("MailRecvAtFormat")); } }
    public DateTime CreatedDateTime { get { return Data.created_at; } }
}
