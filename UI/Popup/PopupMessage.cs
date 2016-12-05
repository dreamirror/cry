using UnityEngine;
using System.Collections;

public class PopupMessage : PopupBase
{
    public UILabel mMessage;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms.Length != 1)
        {
            throw new System.Exception("have to a parameter");
        }
        SetText((string)parms[0]);
    }

    public void SetText(string text)
    {
        mMessage.text = text;
    }

}
