using System;
using UnityEngine;

public enum eBattleContinueFinish
{
    None,
    Count,
    NotEnoughEnergy,
    NotEnoughCreatureSlot,
    NotEnoughRuneSlot,
    Fail,
}

public class BattleContinue : MNS.Singleton<BattleContinue>
{
    eBattleContinueFinish FinishType = eBattleContinueFinish.None;

    public BattleContinue()
    {

    }

    public void SetContinue(int count)
    {
        BattleCount = 1;
        RequestCount = count;

        ConfigData.Instance.ApplyBatteryLow();
        ConfigData.Instance.ApplyScreenLock();
        ConfigData.Instance.RefreshSound();
    }

    public void IncreaseBattle()
    {
        ++BattleCount;
    }

    public void SetRetry()
    {
        IsRetry = true;
    }

    public void Clear()
    {
        FinishType = eBattleContinueFinish.None;
        IsRetry = false;
        BattleCount = 0;
        RequestCount = 0;

        ConfigData.Instance.StopBatteryLow();
        ConfigData.Instance.StopScreenLock();
        ConfigData.Instance.RefreshSound();
    }

    public void SetFinish(eBattleContinueFinish type)
    {
        FinishType = type;
    }

    public bool CheckFinish()
    {
        if (FinishType != eBattleContinueFinish.None)
        {
            Finish(FinishType);
            return true;
        }
        return false;
    }

    public bool Finish(eBattleContinueFinish type)
    {
        if (IsPlaying == false && type != eBattleContinueFinish.Count)
            return false;

        FinishType = type;

        Popup.Instance.ShowMessageKey("BattleContinueEnd", Math.Min(BattleCount, RequestCount));
        Clear();
        if (ConfigData.Instance.UseVibrate)
        {
            Handheld.Vibrate();
        }
        SoundManager.PlaySound(AssetManager.GetSound("battle_continue"));

        return true;
    }

    public bool IsPlaying
    {
        get
        {
            return RequestCount > 0 && BattleCount <= RequestCount;
        }
    }

    public bool IsRetry { get; set; }
    public int BattleCount { get; private set; }
    public int RequestCount { get; private set; }
}
