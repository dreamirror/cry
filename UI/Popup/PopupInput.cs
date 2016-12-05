using UnityEngine;
using System.Collections;

public class PopupInput : PopupBase
{
    public UILabel mMessage, mInputMessage;
    public UIInput mInput;

    System.Action<string> callback;

    void OnEnable()
    {
        mInput.isSelected = true;
    }

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        if (parms.Length != 2)
        {
            throw new System.Exception("have to a parameter");
        }
        SetText((string)parms[0]);
        callback = (System.Action<string>)parms[1];
    }

    public void SetText(string text)
    {
        mMessage.text = text;
        mInputMessage.text = "";
        mInput.value = "";
    }

    override public void OnClose()
    {
        parent.Close(true);
    }

    public void Submit()
    {
        parent.Close(true);
        callback(mInputMessage.text);
    }
}
