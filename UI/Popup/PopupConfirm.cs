using UnityEngine;
using System.Collections;

public class PopupConfirm : PopupBase
{
    public UILabel mMessage, m_ConfirmLabel, m_CancelLabel;
    public delegate void Callback(bool confirm);
    Callback OnCallback;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms.Length < 2)
        {
            throw new System.Exception("have to a parameter");
        }
        OnCallback = (Callback)parms[0];
        SetText((string)parms[1]);
        if (parms.Length > 2)
            m_ConfirmLabel.text = (string)parms[2];
        else
            m_ConfirmLabel.text = Localization.Get("Confirm");

        if (parms.Length > 3)
            m_CancelLabel.text = (string)parms[3];
        else
            m_ConfirmLabel.text = Localization.Get("Cancel");
    }

    public void SetText(string text)
    {
        mMessage.text = text;
    }

    public void OnConfirm()
    {
        parent.Close(true, true);
        OnCallback(true);
    }

    public void OnCancel()
    {
        parent.Close(true, true);
        OnCallback(false);
    }
}
