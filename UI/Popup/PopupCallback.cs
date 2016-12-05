using UnityEngine;
using System.Collections;
using System;

public class PopupCallback : PopupBase
{
    public class Callback
    {
        public Callback(Delegate callback, object[] param, string button_key = "Confirm")
        {
            this.callback = callback;
            this.param = param;
            this.button_key = button_key;
        }

        public void Invoke()
        {
            if (callback != null)
                callback.DynamicInvoke(param);
        }

        public Delegate callback;
        public object[] param;
        public string button_key;
    }

    public UILabel mMessage, mButton;

    Callback m_Callback = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        // param 1 : callback
        // param 2 : message
        if (parms.Length != 2)
        {
            Debug.LogError("invalid parameter");
            return;
        }
        m_Callback = (Callback)parms[0];
        mButton.text = Localization.Get(m_Callback.button_key);
        SetText((string)parms[1]);
    }
    public void SetText(string text)
    {
        mMessage.text = text;
    }

    override public void OnClose()
    {
        parent.Close(true, true);
        m_Callback.Invoke();
    }
}
