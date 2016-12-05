using PacketInfo;
using UnityEngine;

public class PopupStuffConfirm : PopupBase
{
    public UILabel m_title;
    public UILabel m_message;

    Item m_item;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        m_item = parms[0] as Item;
        m_message.text = Localization.Get(parms[1] as string);
        Init();
    }

    void Enable()
    {
    }

    void Init()
    {
    }

    public void OnClickOK()
    {
        parent.Close(true, true);
        Popup.Instance.Show(ePopupMode.Stuff, m_item.Info);
    }
}
