using PacketInfo;
using UnityEngine;

public class PopupExpPowderMove : PopupBase
{
    public UILabel m_title;
    public UILabel m_message;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
    }

    public void OnClickMoveStore()
    {
        parent.Close(true, true);
        GameMain.MoveStore("Mileage");
    }
    public void OnClickMoveTraining()
    {
        parent.Close(true, true);
        if (Popup.Instance.GetCurrentPopup() != null)
        {
            GameMain.Instance.StackPopup();
        }
        GameMain.Instance.ChangeMenu(GameMenu.Training);
    }
}
