using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class PopupMailbox : PopupBase
{
    public UILabel EmptyLabel;
    public UILabel TitleLabel;
    public UIScrollView ItemScrollView;
    public UIGrid ItemGrid;

    public PrefabManager MailBoxItem;

    public GameObject BottomTooltip;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        
        Init();
    }

    public override void OnFinishedShow()
    {        
        base.OnFinishedShow();

        EmptyLabel.gameObject.SetActive(true);
        if (MailManager.Instance.IsInit == false)
            return;

        if (MailManager.Instance.Mails.Count == 0)
        {
            BottomTooltip.SetActive(false);
            return;
        }

        foreach (Mail info in MailManager.Instance.Mails)
        {
            if (EmptyLabel.gameObject.activeSelf)
                EmptyLabel.gameObject.SetActive(false);
            var item = MailBoxItem.GetNewObject<PopupMailboxItem>(ItemGrid.transform, Vector3.zero);

            item.Init(info.MailIdx);
        }
        BottomTooltip.SetActive(true);

        ItemGrid.Reposition();
        ItemScrollView.ResetPosition();
    }

    public void OnClickClose()
    {
        parent.Close();
    }

    void Init()
    {
        EmptyLabel.gameObject.SetActive(false);
    }
}
