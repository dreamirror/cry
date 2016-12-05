using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;

public class PopupSetting : PopupBase
{
    public UIToggle TabGame, TabBattle;

    //game tab
    public UIToggle GraphicsHigh;
    public UIToggle GraphicsMid;
    public UIToggle GraphicsLow;

    public UIToggle FrameHigh;
    public UIToggle FrameMid;
    public UIToggle FrameLow;

    public UIToggle MusicOn;
    public UIToggle MusicOff;

    public UIToggle FXSoundOn;
    public UIToggle FXSoundOff;

    public UIToggle SleepBlockOn;
    public UIToggle SleepBlockOff;

    //alarm tab
    public UIToggle GoodsRefreshOn;
    public UIToggle GoodsRefreshOff;

    public UIToggle FreeEnergyOn;
    public UIToggle FreeEnergyOff;

    public UIToggle EnergyFullChargeOn;
    public UIToggle EnergyFullChargeOff;

    public UIToggle NoticeOn;
    public UIToggle NoticeOff;

    // battle tab
    public UIToggle BattleEFXOn;
    public UIToggle BattleEFXOff;

    public UIToggle VibrateOn;
    public UIToggle VibrateOff;

    public UIToggle ContinueBattleFinishWhenFailOn;
    public UIToggle ContinueBattleFinishWhenFailOff;

    public UIToggle ContinueBattleLowBatteryOn;
    public UIToggle ContinueBattleLowBatteryOff;

    public UIToggle ContinueBattleScreenLockOn;
    public UIToggle ContinueBattleScreenLockOff;

    public UIToggle ContinueBattleMuteOn;
    public UIToggle ContinueBattleMuteOff;

    //info tab
    public UILabel PlayerIDLabel;    
    public UILabel GameVersion;

    public UILabel FacebookLabel;
    public UILabel GooglePlusLabel;

    //language tab
    public UIGrid Grid;

    public GameObject NationPrefab;
    PopupNations m_Nations = null;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        Init();
    }

    void Start()
    {
        // game tab
        string graphics_option = ConfigData.Instance.GraphicsOption;
        if (graphics_option.Equals("High"))
            GraphicsHigh.value = true;
        else if (graphics_option.Equals("Good"))
            GraphicsMid.value = true;
        else
            GraphicsLow.value = true;

        int frame = ConfigData.Instance.TargetFrame;
        if (frame >= 60)
            FrameHigh.value = true;
        else if (frame >= 30)
            FrameMid.value = true;
        else
            FrameLow.value = true;

        if (ConfigData.Instance.UseMusic) MusicOn.value = true; else MusicOff.value = true;
        if (ConfigData.Instance.UseSound) FXSoundOn.value = true; else FXSoundOff.value = true;
        if (ConfigData.Instance.UseSleepBlock) SleepBlockOn.value = true; else SleepBlockOff.value = true;

        // alarm tab
        if (ConfigData.Instance.UseGoodsRefreshAlarm) GoodsRefreshOn.value = true; else GoodsRefreshOff.value = true;
        if (ConfigData.Instance.UseFreeEnergyAlarm) FreeEnergyOn.value = true; else FreeEnergyOff.value = true;
        if (ConfigData.Instance.UseEnergyFullChargeAlarm) EnergyFullChargeOn.value = true; else EnergyFullChargeOff.value = true;
        if (ConfigData.Instance.UseNoticeAlarm) NoticeOn.value = true; else NoticeOff.value = true;

        // battle tab
        if (ConfigData.Instance.UseBattleEffect) BattleEFXOn.value = true; else BattleEFXOff.value = true;
        if (ConfigData.Instance.UseVibrate) VibrateOn.value = true; else VibrateOff.value = true;
        if (ConfigData.Instance.ContinueBattleFinishWhenFail) ContinueBattleFinishWhenFailOn.value = true; else ContinueBattleFinishWhenFailOff.value = true;
        if (ConfigData.Instance.ContinueBattleLowBattery) ContinueBattleLowBatteryOn.value = true; else ContinueBattleLowBatteryOff.value = true;
        if (ConfigData.Instance.ContinueBattleScreenLock) ContinueBattleScreenLockOn.value = true; else ContinueBattleScreenLockOff.value = true;
        if (ConfigData.Instance.ContinueBattleMute) ContinueBattleMuteOn.value = true; else ContinueBattleMuteOff.value = true;

        // info tab
        GameVersion.text = Application.version;
        PlayerIDLabel.text = ObscuredPrefs.DeviceId;

        // language tab
        foreach (string nation in Localization.dictionary["LanguageSet"].ToList())
        {
            string[] splited = nation.Split('|');
            m_Nations = NGUITools.AddChild(Grid.gameObject, NationPrefab).GetComponent<PopupNations>();
            m_Nations.Init(splited[0], splited[1], ConfigData.Instance.Language.Equals(splited[1]));
        }
        Grid.Reposition();
    }

    void Init()
    {        
        UIToggle select_tab = null;
        if (GameMain.Instance.CurrentGameMenu == GameMenu.Battle)
            select_tab = TabBattle;
        else
            select_tab = TabGame;

        select_tab.value = true;
        foreach (var tab in TabBattle.transform.parent.GetComponentsInChildren<UIToggle>())
        {
            if (select_tab != tab)
                tab.Set(false);
        }
    }

    public void OnClickPlayerIDCopy()
    {
        PBN_System.Instance.SetClipboard(PlayerIDLabel.text);
        Tooltip.Instance.ShowMessage(Localization.Get("CopyComplete"));
    }

    public void OnClickFaceBook()
    {
#if SH_TEST || SH_DEV
        Tooltip.Instance.ShowMessageKey("NotImplement");
#else
        GameMain.Instance.Logout(true);
#endif
    }

    public void OnClickGooglePlus()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void OnClickNotice()
    {
        Application.OpenURL(GameConfig.Get<string>("notice_url") );
    }

    public void OnClickCoupon()
    {
        Tooltip.Instance.ShowMessageKey("NotImplement");
    }

    public void OnClickQA()
    {
        Application.OpenURL(GameConfig.Get<string>("qa_url"));
    }

    public void OnClickCommunity()
    {
        Application.OpenURL(GameConfig.Get<string>("community_url"));
    }

    public void OnGraphicChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.GraphicsOption = toggle.name;
    }

    public void OnFrameChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.TargetFrame = int.Parse(toggle.name);
    }

    public void OnMusicChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.UseMusic = toggle.name.Equals("On");
    }
    
    public void OnEffectSoundChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.UseSound = toggle.name.Equals("On");
    }

    public void OnSleepBlockChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.UseSleepBlock = toggle.name.Equals("On");
    }

    public void OnBattleEffectChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.UseBattleEffect = toggle.name.Equals("On");
    }
    
    public void OnVibrateChange(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.UseVibrate = toggle.name.Equals("On");
    }

    public void OnContinueBattleFinishWhenFail(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value)
            return;
        ConfigData.Instance.ContinueBattleFinishWhenFail = toggle.name.Equals("On");
    }

    public void OnContinueBattleLowBattery(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.ContinueBattleLowBattery = toggle.name.Equals("On");
    }

    public void OnContinueBattleScreenLock(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.ContinueBattleScreenLock = toggle.name.Equals("On");
    }

    public void OnContinueBattleMute(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.ContinueBattleMute = toggle.name.Equals("On");
    }

    public void OnStoreGoodsRefresh(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.UseGoodsRefreshAlarm = toggle.name.Equals("On");
        PushManager.Instance.ReloadDefaulLocalNotifiation();
    }

    public void OnFreeEnergyAlarm(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.UseFreeEnergyAlarm = toggle.name.Equals("On");
        PushManager.Instance.ReloadDefaulLocalNotifiation();
    }

    public void OnEnergyFullCharge(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.UseEnergyFullChargeAlarm = toggle.name.Equals("On");
        PushManager.Instance.ReloadDefaulLocalNotifiation();
    }

    public void OnNoticeAlarm(UIToggle toggle)
    {
        if (toggle.instantTween || !toggle.value) return;

        ConfigData.Instance.UseNoticeAlarm = toggle.name.Equals("On");
        PushManager.Instance.ReloadDefaulLocalNotifiation();
    }
}