using UnityEngine;
using System.Collections;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;

public class ConfigData : MNS.Singleton<ConfigData>
{

    ////////////////////////Setting::Game Tab////////////////////////
    public string GraphicsOption
    {
        get { return PlayerPrefs.GetString("GraphicsOption", "High"); }
        set { PlayerPrefs.SetString("GraphicsOption", value); GraphicQualityApply(); }
    }

    public int TargetFrame
    {
        get { return PlayerPrefs.GetInt("TargetFrame", 60); }
        set { PlayerPrefs.SetInt("TargetFrame", value); GraphicQualityApply(); }
    }

    public bool UseMusic
    {
        get
        {
            return GetBool("Music");
        }
        set
        {
            SetBool("Music", value);
            if (value)
            {
                SoundManager.Instance.PlayBGM();
            }
            else
            {
                SoundManager.Instance.StopBGM();
            }
        }
    }
    public bool UseSound { get { return GetBool("Sound"); } set { SetBool("Sound", value); SoundManager.Instance.mute = !value; } }
    public bool UseSleepBlock { get { return GetBool("SleepBlock"); } set { SetBool("SleepBlock", value); RefreshSleep(); } }

    public bool IsMute { get { return ContinueBattleMute == true && BattleContinue.Instance.IsPlaying == true; } }

    ////////////////////////Setting::Alarm Tab////////////////////////
    public bool UseGoodsRefreshAlarm { get { return GetBool("GoodsRefresh"); } set { SetBool("GoodsRefresh", value); } }
    public bool UseFreeEnergyAlarm { get { return GetBool("FreeEnergy"); } set { SetBool("FreeEnergy", value); } }
    public bool UseEnergyFullChargeAlarm { get { return GetBool("EnergyFull"); } set { SetBool("EnergyFull", value); } }
    public bool UseNoticeAlarm { get { return GetBool("NoticeAlarm"); } set { SetBool("NoticeAlarm", value); } }

    ////////////////////////Setting::Battle Tab////////////////////////
    public bool UseBattleEffect { get { return GetBool("BattleEffect"); } set { SetBool("BattleEffect", value); } }
    public bool UseVibrate { get { return GetBool("Vibration"); } set { SetBool("Vibration", value); } }
    public bool ContinueBattleFinishWhenFail { get { return GetBool("ContinueBattleFinishWhenFail", false); } set { SetBool("ContinueBattleFinishWhenFail", value); } }
    public bool ContinueBattleLowBattery { get { return GetBool("ContinueBattleLowBattery", false); } set { SetBool("ContinueBattleLowBattery", value); ChangeBatteryLow(); } }
    public bool ContinueBattleScreenLock { get { return GetBool("ContinueBattleScreenLock", false); } set { SetBool("ContinueBattleScreenLock", value); } }
    public bool ContinueBattleMute { get { return GetBool("ContinueBattleMute", false); } set { SetBool("ContinueBattleMute", value); RefreshSound(); } }

    ////////////////////////Setting::Language Tab////////////////////////
    public string Language { get { return PlayerPrefs.GetString("Language", "Korean"); } set { PlayerPrefs.SetString("Language", value); } }

    //push key 
    public string GoodsRefreshPushKey { get { return PlayerPrefs.GetString("GoodsRefreshPush", string.Empty); } set { PlayerPrefs.SetString("GoodsRefreshPush", value); } }
    public int EnergyFullChargePushKey { get { return PlayerPrefs.GetInt("EnergyFullPush", -1); } set { PlayerPrefs.SetInt("EnergyFullPush", value); } }
    public string FreeEnergyPushKey { get { return PlayerPrefs.GetString("FreeEnergyPush", string.Empty); } set { PlayerPrefs.SetString("FreeEnergyPush", value); } }

    //PUSH INFO
    public string PushToken { get { return ObscuredPrefs.GetString("PushToken", ""); } set { ObscuredPrefs.SetString("PushToken", value); } }

    public void Init()
    {
//         GraphicsOption = GraphicsOption;
//         TargetFrame = TargetFrame;
        GraphicQualityApply();
        UseMusic = UseMusic;
        UseSound = UseSound;
        UseSleepBlock = UseSleepBlock;

        UseBattleEffect = UseBattleEffect;
        UseVibrate = UseVibrate;
        ContinueBattleFinishWhenFail = ContinueBattleFinishWhenFail;
        ContinueBattleLowBattery = ContinueBattleLowBattery;

        UseGoodsRefreshAlarm = UseGoodsRefreshAlarm;
        UseFreeEnergyAlarm = UseFreeEnergyAlarm;
        UseEnergyFullChargeAlarm = UseEnergyFullChargeAlarm;
        UseNoticeAlarm = UseNoticeAlarm;
        Localization.language = Language;

        PushToken = PushToken;
    }
    
    public void GraphicQualityApply()
    {
        QualitySettings.SetQualityLevel(QualitySettings.names.ToList().IndexOf(GraphicsOption), true);
        Application.targetFrameRate = TargetFrame;
    }

    bool GetBool(string key, bool value = true)
    {
        return PlayerPrefs.GetInt(key, value?1:0) == 1;
    }
    void SetBool(string key, bool bTrue)
    {
        PlayerPrefs.SetInt(key, bTrue ? 1 : 0);
    }

    void ChangeBatteryLow()
    {
        if (BattleBase.CurrentBattleMode == eBattleMode.None || BattleContinue.Instance.IsPlaying == false)
            return;

        if (ContinueBattleLowBattery == true)
        {
            ApplyBatteryLow();
        }
        else
        {
            GraphicQualityApply();
        }
    }

    public void ApplyBatteryLow()
    {
        if (ContinueBattleLowBattery == false || BattleBase.CurrentBattleMode == eBattleMode.None || BattleContinue.Instance.IsPlaying == false)
            return;

        QualitySettings.SetQualityLevel(QualitySettings.names.ToList().IndexOf("Normal"), true);
        Application.targetFrameRate = 15;
    }

    public void StopBatteryLow()
    {
        if (ContinueBattleLowBattery == false)
            return;

        GraphicQualityApply();
    }

    public void ApplyScreenLock()
    {
        if (ContinueBattleScreenLock == false || BattleBase.CurrentBattleMode == eBattleMode.None || BattleContinue.Instance.IsPlaying == false)
            return;

        Tooltip.Instance.ShowScreenLock();
    }

    public void StopScreenLock()
    {
        Tooltip.Instance.CloseTooltip(eTooltipMode.ScreenLock);
    }

    bool IsInBattle()
    {
        return BattleBase.CurrentBattleMode != eBattleMode.None && Time.timeScale > 0f && (BattleBase.Instance == null || BattleBase.Instance.IsBattleEnd == false);
    }

    public void RefreshSleep()
    {
        if (UseSleepBlock == true)
        {
            if (Screen.sleepTimeout != SleepTimeout.NeverSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                Debug.LogWarning("[Sleep] NeverSleep");
            }
        }
        else
        {
            if (IsInBattle() == true)
            {
                if (Screen.sleepTimeout != SleepTimeout.NeverSleep)
                {
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Debug.LogWarning("[Sleep] NeverSleep");
                }
            }
            else
            {
                if (Screen.sleepTimeout != SleepTimeout.SystemSetting)
                {
                    Screen.sleepTimeout = SleepTimeout.SystemSetting;
                    Debug.LogWarning("[Sleep] SystemSetting");
                }
            }
        }
    }

    public void RefreshSound()
    {
        if (IsMute == true)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.StopSound();
        }
        else
        {
            if (UseMusic)
                SoundManager.Instance.PlayBGM();
        }
    }
}
