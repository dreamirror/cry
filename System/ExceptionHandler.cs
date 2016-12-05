using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public static class ExceptionHandlerAutoInitialize
{
    [RuntimeInitializeOnLoadMethod]
    public static void OnLoad()
    {
        if (ExceptionHandler.Instance == null)
        {
            new GameObject("ExceptionHandler", typeof(ExceptionHandler));
        }
    }
}

[DisallowMultipleComponent]
public class ExceptionHandler : MonoBehaviour {

    static public bool IsInit { get; private set; }
    static public bool IsException { get; private set; }

    const int LogCount = 10;
    List<string> log_list = new List<string>();

    static ExceptionHandler m_Instance;
    static public ExceptionHandler Instance
    {
        get
        {
            return m_Instance;
        }
    }

    void Awake()
    {
        if (IsInit == true)
            return;

        m_Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
//        System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
        Application.logMessageReceived += CaptureDebugLog;
        IsInit = true;
    }

//     void UnhandledExceptionEventHandler(object sender, System.UnhandledExceptionEventArgs e)
//     {
//         if (e.ExceptionObject is System.Exception == false)
//             return;
// 
//         var ex = e.ExceptionObject as System.Exception;
// 
//         CaptureDebugLog(ex.Message, ex.StackTrace, LogType.Exception);
//     }

    void CaptureDebugLog(string logString, string stackTrace, LogType type)
    {
        if (IsException == true)
            return;

        log_list.Add(string.Format("{0} : {1}", type, logString));
        if (log_list.Count > LogCount)
            log_list.RemoveAt(0);

        if (type != LogType.Exception && type != LogType.Assert)
            return;

        if (string.IsNullOrEmpty(stackTrace) == true)
            stackTrace = StackTraceUtility.ExtractStackTrace();

        UIButton.current = null;

        Network.HideIndicator();
        TimeManager.Instance.SetPause(true);

        C2D.CrashReport packet = new C2D.CrashReport();
        packet.file_account_idx = SHSavedData.AccountIdx;
        packet.report = logString;
        packet.stack_trace = stackTrace;
        packet.log = log_list.Reverse<string>().ToList();
        packet.app_info = Network.Instance.GetAppInfo();

        SHSavedData.Instance.LastCrashReport = JsonConvert.SerializeObject(packet, Formatting.None, NetworkCore.PacketUtil.json_ops);
        Debug.Log("CrashReport Saved");

        if (Network.GameServer != null)
        {
            Network.GameServer.JsonAsyncNamespace<C2D.CrashReport, NetworkCore.AckDefault>("C2D", packet, OnCrashReport);
        }

        IsException = true;
        int linefeed = logString.IndexOf('\n');
        if (linefeed >= 0)
            logString = logString.Substring(0, linefeed);
        Popup.Instance.ShowCallback(new PopupCallback.Callback(new System.Action(Callback), null), logString);
    }

    public void OnCrashReport(C2D.CrashReport packet, NetworkCore.AckDefault ack)
    {
        SHSavedData.Instance.LastCrashReport = "";
    }

    public void SendLastReport()
    {
        string last_report = SHSavedData.Instance.LastCrashReport;
        if (string.IsNullOrEmpty(last_report) == true)
            return;

        try
        {
            C2D.CrashReport packet = JsonConvert.DeserializeObject<C2D.CrashReport>(last_report, NetworkCore.PacketUtil.json_ops);
            Network.GameServer.JsonAsync<C2D.CrashReport, NetworkCore.AckDefault>(packet, OnCrashReport);
        }
        catch(System.Exception)
        {

        }
    }

    void Callback()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void Reset()
    {
        IsException = false;
    }
}
