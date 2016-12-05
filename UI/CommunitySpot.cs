using UnityEngine;
using System.Collections;

public class CommunitySpot : SubMenuSpot
{
    public UIToggle m_Toggle;
    public GameObject m_Nofity;

    override public void Init()
    {
        switch(gameObject.name)
        {
            case "Friends":
                m_Toggle.value = false;
                m_Nofity.SetActive(Network.Instance.NotifyMenu.is_friends_requested);
                break;
            case "Guild":
                m_Toggle.value = false;
                m_Nofity.SetActive(false);
                break;
            default:
                m_Toggle.value = true;
                m_Nofity.SetActive(false);
                break;
        }
//        m_Nofity.SetActive(m_Toggle.value == false && clear_rate == 0);
    }

    public void OnBtnClicked()
    {
        switch(gameObject.name)
        {
            case "Friends":
                GameMain.Instance.ChangeMenu(GameMenu.Friends);
                break;

            case "Guild":
                GameMain.Instance.ChangeMenu(GameMenu.Guild);
                break;
            default:
                Tooltip.Instance.ShowMessageKey("NotImplement");
                break;
        }
    }
}
