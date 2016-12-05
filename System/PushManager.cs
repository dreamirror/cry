using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class PushManager : MNS.Singleton<PushManager>
{
    bool OneSignalInit;

    string push_id;
    string push_token;
    public string PushID { get { return string.IsNullOrEmpty(push_id) ? string.Empty : push_id; } private set { push_id = value; }   }
    public string PushToken { get { return string.IsNullOrEmpty(push_token) ? string.Empty : push_token; } private set { push_token = value; } }

    public void Init()
    {
        if (!OneSignalInit)
        {
            switch (Application.bundleIdentifier)
            {   
                case "com.monsmile.projecth.ios.dev":
                case "com.monsmile.herocry.ios.dev":
                    OneSignal.Init("b4c915a0-4494-4d6e-8ade-efce0a40ad01", string.Empty, HandleNotification);
                    break;
                case "com.monsmile.projecth.ios":
                    OneSignal.Init("517412e4-b97d-4b24-9681-9f4dfc26fa00", string.Empty, HandleNotification);
                    break;
                case "com.monsmile.projecth.android.dev":
                    OneSignal.Init("b4c915a0-4494-4d6e-8ade-efce0a40ad01", "357018248098", HandleNotification,true);
                    OneSignal.EnableNotificationsWhenActive(false);
                    break;
                case "com.monsmile.projecth.android.test":
                    OneSignal.Init("517412e4-b97d-4b24-9681-9f4dfc26fa00", "725610003103", HandleNotification);
                    OneSignal.EnableNotificationsWhenActive(false);
                    break;
            }            
            OneSignalInit = true;
        }
        ResetBadgeNumber();
        
        OneSignal.SetSubscription(true);
        OneSignal.GetIdsAvailable(SetPushInfo);
    }
    public void SetPushInfo(string id, string token)
    {
        PushID = id;
        PushToken = token;
        //Debug.LogWarningFormat("ID : {0} / TK : {1}", PushID, PushToken);
    }

    private void HandleNotification(string message, Dictionary<string, object> additionalData, bool isActive)
    {
        if (isActive && !string.IsNullOrEmpty(message) )
        {   
            //Tooltip.Instance.ShowMessage(message);
        }
    }

    public void RefreshEnergy()
    {
        RefreshFullEnergyAlarm();
    }

    public void ReloadDefaulLocalNotifiation()
    {   
        ResetBadgeNumber();
        UM_NotificationController.Instance.CancelAllLocalNotifications();
        ConfigData.Instance.GoodsRefreshPushKey = string.Empty;
        ConfigData.Instance.FreeEnergyPushKey = string.Empty;
        ConfigData.Instance.EnergyFullChargePushKey = 0;
        RefreshGoodsAlarm();        //상점 상퓸 교체시간 알림
        RefreshFreeEnergyAlarm();   //미션 행동력 보너스 알림
        RefreshFullEnergyAlarm();   //행동력 풀 차지 알림
    }

    public int AddLocalNotificationBySeconds(string title, string msg, int seconds)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            int next_id = AndroidNotificationManager.Instance.GetNextId;
            AndroidNotificationBuilder builder = new AndroidNotificationBuilder(next_id, title, msg, seconds);
            builder.SetLargeIcon("pushicon_72x72");
            builder.SetIconName("pushicon_36x36");
            return AndroidNotificationManager.Instance.ScheduleLocalNotification(builder);
        }
        else
        {
            return UM_NotificationController.Instance.ScheduleLocalNotification(title, msg, seconds);
        }
    }

    public int AddLocalNotificationByOnTime(string title, string msg, DateTime release_at)
    {
        int seconds = (int)(release_at - DateTime.Now).TotalSeconds;
        if (seconds < 0)
            return -1;

        return AddLocalNotificationBySeconds(title, msg, seconds);
    }

    public void CancelLocalNotification(int NotificationID)
    {
        UM_NotificationController.Instance.CancelLocalNotification(NotificationID);
    }

    public void ResetBadgeNumber()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidNotificationManager.Instance.HideAllNotifications();
            OneSignal.ClearOneSignalNotifications();
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            IOSNotificationController.Instance.ApplicationIconBadgeNumber(0);
    }

    void RefreshGoodsAlarm()
    {
        if (!ConfigData.Instance.UseGoodsRefreshAlarm)
            return;

        //if (string.IsNullOrEmpty(ConfigData.Instance.GoodsRefreshPushKey) == false)
        //{
        //    foreach (var pushkey in ConfigData.Instance.GoodsRefreshPushKey.Split('|'))
        //        UM_NotificationController.Instance.CancelLocalNotification(int.Parse(pushkey));
        //    ConfigData.Instance.GoodsRefreshPushKey = string.Empty;
        //}

        List<short> used_hour = new List<short>();

        foreach (var store_item in StoreInfoManager.Instance.Values.Where(i => i.m_RefreshTimes != null) )
        {
            foreach (var hour in store_item.m_RefreshTimes)
            {
                if (used_hour.Contains(hour) == true)
                    continue;
                used_hour.Add(hour);
                int add_seconds = DateTime.Now.Hour >= hour ? (int)(DateTime.Today.AddDays(1).AddHours(hour) - DateTime.Now).TotalSeconds : (int)(DateTime.Today.AddHours(hour) - DateTime.Now).TotalSeconds;
                add_seconds += 1;

                var push = AddLocalNotificationBySeconds(string.Format(Application.productName), string.Format(Localization.Get("PushGoodsRefresh"), hour), add_seconds);
                //ConfigData.Instance.GoodsRefreshPushKey = ConfigData.Instance.GoodsRefreshPushKey + "|" + push.ToString();
            }
        }
    }

    void RefreshFreeEnergyAlarm()
    {
        if (!ConfigData.Instance.UseFreeEnergyAlarm)
            return;
        
        //if (string.IsNullOrEmpty(ConfigData.Instance.FreeEnergyPushKey) == false)
        //{
        //    foreach (var pushkey in ConfigData.Instance.FreeEnergyPushKey.Split('|'))
        //        UM_NotificationController.Instance.CancelLocalNotification(int.Parse(pushkey));
        //    ConfigData.Instance.FreeEnergyPushKey = string.Empty;
        //}

        foreach (var info in QuestManager.Instance.Data.Where(v => v.Info.ID.Contains("BonusEnergy")))
        {   
            int hour = (info.Info.Condition as QuestConditionTime).time_begin.Hours;
            
            int add_seconds = DateTime.Now.Hour >= hour ? (int)(DateTime.Today.AddDays(1).AddHours(hour) - DateTime.Now).TotalSeconds : (int)(DateTime.Today.AddHours(hour) - DateTime.Now).TotalSeconds;
            add_seconds += 1;

             ConfigData.Instance.FreeEnergyPushKey = ConfigData.Instance.FreeEnergyPushKey + "|" + AddLocalNotificationBySeconds(string.Format(Application.productName), string.Format(Localization.Get("PushFreeEnergy"), hour), add_seconds).ToString();
        }
    }
    
    void RefreshFullEnergyAlarm()
    {
        if (!ConfigData.Instance.UseEnergyFullChargeAlarm)
            return;

        int not_enough_full = Network.PlayerInfo.energy_max - Network.PlayerInfo.GetEnergy();

        if (not_enough_full <= 0)
            return;
        
        UM_NotificationController.Instance.CancelLocalNotification(ConfigData.Instance.EnergyFullChargePushKey);

        int second = (GameConfig.Get<int>("energy_regen_time") * not_enough_full) - (int)(Network.Instance.ServerTime - Network.PlayerInfo.energy_time).TotalSeconds % GameConfig.Get<int>("energy_regen_time");

        ConfigData.Instance.EnergyFullChargePushKey = AddLocalNotificationBySeconds(Application.productName, Localization.Get("PushFullEnergy"),second);
    }
}
