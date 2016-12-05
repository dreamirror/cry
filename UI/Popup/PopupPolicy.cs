using UnityEngine;
using System.Collections;

public class PopupPolicy : PopupBase
{
    public UIToggle m_TogglePrivate;
    public UIToggle m_ToggleGame;
    public UIToggleSprite m_Confirm;

    System.Action m_Callback = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
            m_Callback = parms[0] as System.Action;
        else
            Debug.LogError("invalid PopupPolicy param");
    }

    public void OnClickPolicyPrivate()
    {
        Application.OpenURL("http://www.monsmile.com/support3.html");
    }
    public void OnClickPolicyGame()
    {
        Application.OpenURL("http://www.monsmile.com/support2.html");
    }
    public void OnClickConfirm()
    {
        if (m_TogglePrivate.value && m_ToggleGame.value)
        {            
            Network.IsAgree = true;
            base.OnClose();
            if (m_Callback != null) m_Callback();
            return;
        }

    }

    public void OnValueChangedToggle()
    {
        if (m_TogglePrivate.value && m_ToggleGame.value)
            m_Confirm.SetSpriteActive(true);
        else
            m_Confirm.SetSpriteActive(false);

    }
    public override void OnClose()
    {
    }
}
