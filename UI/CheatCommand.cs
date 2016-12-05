using UnityEngine;
using System.Collections;
using System;

public class CheatCommand : MonoBehaviour {
    public UILabel label;
    Action<string> callback = null;

    public void Init(string text, Action<string> callback)
    {
        this.name = text;
        label.text = text;
        this.callback = callback;
    }

    void OnClick()
    {
        if (callback != null)
            this.callback(name);
    }
}
