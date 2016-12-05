using UnityEngine;
using System.Collections;

public class PopupBattleOption: PopupBase
{
    override public bool bShowImmediately { get { return true; } }

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        switch (BattleBase.CurrentBattleMode)
        {
            case eBattleMode.RVR:
                break;

            default:
                TimeManager.Instance.SetPause(true);
                ConfigData.Instance.RefreshSleep();
                break;
        }
    }

    override public void OnClose()
    {
        parent.Close(true);
        if (Tutorial.Instance.gameObject.activeInHierarchy == false)
            TimeManager.Instance.SetPause(false);
        ConfigData.Instance.RefreshSleep();
    }

    public void OnExit()
    {
        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }

        BattleContinue.Instance.Clear();
        parent.Close(true);
        if (BattleBase.CurrentBattleMode != eBattleMode.RVR)
            Battle.Instance.SetBattleExit();
        else
            GameMain.SetBattleMode(eBattleMode.None);
    }

    public void OnClickMainMenu()
    {
        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }

        OnExit();
        GameMain.Instance.ChangeMenu(GameMenu.MainMenu);
    }

    public void OnClickSetup()
    {
//         if (Tutorial.Instance.Completed == false)
//         {
//             Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
//             return;
//         }

        Popup.Instance.Show(ePopupMode.Setting);
    }

    public void OnClickScreenLock()
    {
        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }

        if (Tooltip.Instance.IsShow(eTooltipMode.ScreenLock) == false)
            Tooltip.Instance.ShowScreenLock();
        OnClose();
    }
}
