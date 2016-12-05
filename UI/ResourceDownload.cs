#define USE_THREAD

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using BestHTTP;

#if USE_THREAD
using System.Threading;
#endif

enum ePatchState
{
    Ready,
    Download,
    DownloadFull,
    Processing,
    ApplyDelta,
    PatchEnd,
    LoadScene,
}
public class ResourceDownload : MonoBehaviour
{
    public string DownloadUrl { get; private set; }
    public UIProgressBar m_ProgressBar;
    public UILabel m_DownloadLabel;

    readonly string delta_filename = "patch.diff";
    const int MEGABYTE = 1024 * 1024;
    ePatchState m_State = ePatchState.Ready;
    float m_ProgressValue;

    // Use this for initialization
    void Start()
    {
        pre_download_size_check = false;
        Network.Instance.Init();
        Network.Instance.RequestAssetBundleVersion(OnRequestAssetBundleVersion);
    }

    void OnRequestAssetBundleVersion(C2L.RequestAssetBundleVersion packet, C2L.RequestAssetBundleVersionAck ack) //网络初始化之后的回调
    {
        DownloadUrl = ack.download_url; //更新的地址
        m_SkipHashCheck = ack.skip_hash_check; //跳过hash校验
        ProcessBundle(ack.asset_bundle_version, ack.asset_bundle_delta_version); //可能是当前的版本和下一个版本
    }

    public void OnApplicationPause(bool pause)
    {
        Debug.LogFormat("OnApplicationPause : {0}", pause);
        if (pause == false)
            PushManager.Instance.ResetBadgeNumber();
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_State)
        {
            case ePatchState.Ready:
                m_ProgressBar.value = 0f;
                m_DownloadLabel.text = Localization.Get("ReadytoDownload");
                break;

            case ePatchState.Download:
                m_ProgressBar.value = m_ProgressValue;
                m_DownloadLabel.text = string.Format("{0} : {1:n1}%", m_State, m_ProgressBar.value * 100);
                break;

            case ePatchState.DownloadFull:
                m_DeltaVersion = 0;
                InitDownload();
                break;

            case ePatchState.Processing:
                //if (lzma.trueTotalFiles == 0) return;
                m_ProgressBar.value = (float)uncompressed_bytes[0] / uncompressed_size;

                m_DownloadLabel.text = string.Format("{0} : {1:n1}%", m_State, m_ProgressBar.value * 100);

                if (uncompressed_bytes[0] == uncompressed_size)
                {
                    //Debug.LogFormat("[m_DeltaVersion{0}]", m_DeltaVersion);
                    File.Delete(unzip_source_path);

                    if (m_DeltaVersion > 0)
                    {
                        ApplyDelta();
                        m_State = ePatchState.ApplyDelta;
                    }
                    else
                        m_State = ePatchState.PatchEnd;
                }
                break;

            case ePatchState.ApplyDelta:
                m_ProgressBar.value = m_DeltaApplyProgress.Percent;
                if (m_DeltaApplyProgress.IsFail == true)
                {
                    m_State = ePatchState.DownloadFull;
                }
                else if (m_DeltaApplyProgress.IsSuccess == true)
                {
                    m_State = ePatchState.PatchEnd;
                }
                else
                {
                    if (m_SkipHashCheck == true && m_DeltaApplyProgress.IsEnd)
                        m_DownloadLabel.text = string.Format("{0} : {1:n1}%", "CheckFile", m_ProgressBar.value * 100);
                    else
                        m_DownloadLabel.text = string.Format("{0} : {1:n1}%", m_State, m_ProgressBar.value * 100);
                }
                break;

            case ePatchState.PatchEnd:
                m_State = ePatchState.LoadScene;

                SHSavedData.Instance.BundleVersion = m_BundleVersion;
                UnityEngine.SceneManagement.SceneManager.LoadScene("title", UnityEngine.SceneManagement.LoadSceneMode.Single); //进入title场景

                Debug.LogFormat("PatchComplete! [m_BundleVersion:{0}] [m_DeltaVersion:{1}]", m_BundleVersion, m_DeltaVersion);
                break;
        }
    }
    void InitDownload()
    {

        //Application.internetReachability检查网络是不是畅通的
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) //
            Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnStartDownload), "ConfirmDownload"); //确认下载 的耳机确认框
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) //如果是本地的局域网就直接下载
            OnDownload(m_BundleVersion);
    }
    void OnStartDownload(bool isConfirm)
    {
        if (isConfirm)
            OnDownload(m_BundleVersion);
        else
            Application.Quit();
    }
    ////////////////////////////////////////////////////////////////////////////////

    int m_BundleVersion = 0, m_DeltaVersion = 0;
    bool m_SkipHashCheck = false;

    void ProcessBundle(int server_version, int delta_version)
    {
        m_BundleVersion = server_version; //服务器版本
        bool bundle_exists = File.Exists(AssetManager.AssetPath + AssetManager.AssetBundleFilename);

        Debug.LogWarningFormat("AssetBundleVersion : {0}", server_version);

        if (bundle_exists == false ||
            SHSavedData.Instance.BundleVersion != server_version
#if UNITY_EDITOR //如果定义了UNITYEDITOR 就是直接执行下面的
                || true
#endif
                )
        {
            //Debug.LogFormat("[bundle_exists:{0}] [delta_version:{1}] [SHSavedData.Instance.BundleVersion:{2}] [server_version:{3}] [delta_version:{4}]", bundle_exists, delta_version, SHSavedData.Instance.BundleVersion, server_version, delta_version);

            if (bundle_exists == true && delta_version > 0 && SHSavedData.Instance.BundleVersion < server_version && SHSavedData.Instance.BundleVersion >= delta_version)
            {
                m_DeltaVersion = SHSavedData.Instance.BundleVersion;
            }
            InitDownload();
        }
        else
        {
            AssetManager.bundleVersion = SHSavedData.Instance.BundleVersion;
            UnityEngine.SceneManagement.SceneManager.LoadScene("title", UnityEngine.SceneManagement.LoadSceneMode.Single); //进入title场景
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    string GetDownloadURL(string filename)
    {
        string url = string.Format("{0}bundle/{1}/{2}", DownloadUrl, m_BundleVersion, filename);

        return string.Format("{0}?refresh={1}", url, DateTime.Now.Ticks);
    }

    void UnZipThread()
    {
        //FileStream stream = new FileStream(source_path, FileMode.Open);
        //int lzres = lzma.doDecompress7zip(source_path, dest_path, ref progress, true, true);

        //int lzres = fLZ.decompressFile(source_path, dest_path, true, progress);

        //int[] bytes = new int[1];
#if !UNITY_WEBPLAYER        
        int lzres = LZ4.decompress(unzip_source_path, unzip_dest_path, uncompressed_bytes);

        Debug.LogFormat("lzres = {0}", lzres);
#endif
    }

    //ulong []progress = new ulong[1];//for flz
    int[] uncompressed_bytes = new int[1];//for lz4
    int uncompressed_size = 100;
    string unzip_source_path;
    string unzip_dest_path;
    void Unzip(string source, string dest)
    {
        m_State = ePatchState.Processing;

        unzip_source_path = source;
        if (m_DeltaVersion > 0)
            unzip_dest_path = dest + delta_filename;
        else
            unzip_dest_path = dest + AssetManager.AssetBundleFilename;

        Debug.Log(unzip_source_path);
        Debug.Log(unzip_dest_path);
        if (Directory.Exists(dest) == false)
            Directory.CreateDirectory(dest);

#if !UNITY_WEBPLAYER
        uncompressed_bytes[0] = 0;
        uncompressed_size = LZ4.uncompressedSize(unzip_source_path);

        //Debug.LogFormat("[Uncompressed Byte:{0}MB] [Available Size:{1}MB]", uncompressed_size / MAGABYTE, GetStorageFreeSpace());
        if (uncompressed_size / MEGABYTE > GetStorageFreeSpace())
        {
            Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(Start), null), string.Format("Not Enough Free Space! \n [Need:{0}] [Now Free Space:{1}]", uncompressed_size / MEGABYTE, GetStorageFreeSpace()));
            return;
        }


#if USE_THREAD
        Thread th = new Thread(UnZipThread);
        th.Start();
#else
        UnZipThread();
#endif
#endif  
    }

    void Unzip()
    {
        Unzip(m_DownloadDir + m_DownloadFilename, m_DownloadDir);
    }

    int GetStorageFreeSpace()
    {
        return SimpleDiskUtils.DiskUtils.freeStorageSize();
    }

    ////////////////////////////////////////////////////////////////////////////////

    string m_DownloadDir = "TODO!";
    string m_DownloadFilename = "TODO!";
    HTTPRequest m_Request = null;
    bool pre_download_size_check = false;

    void OnDownload(int bundleVersion)
    {
#if UNITY_EDITOR //是否在 当前的unity编辑器中运行
        Debug.Log("OnDownload");
#endif
        HTTPManager.IsCachingDisabled = true; //是否正在缓存
        HTTPManager.IsCookiesEnabled = false;//
        HTTPManager.ConnectTimeout = TimeSpan.FromSeconds(5f);
        HTTPManager.RequestTimeout = TimeSpan.FromSeconds(120f);

        string status;

        AssetManager.bundleVersion = bundleVersion;
        m_DownloadDir = AssetManager.AssetPath;
        if (Directory.Exists(m_DownloadDir) == false)
            Directory.CreateDirectory(m_DownloadDir);

        if (m_DeltaVersion > 0)
        {
#if UNITY_ANDROID
        m_DownloadFilename = string.Format("{0}_Android.zip", m_DeltaVersion);
#elif UNITY_IOS
        m_DownloadFilename = string.Format("{0}_iOS.zip", m_DeltaVersion);
#else
            m_DownloadFilename = string.Format("{0}_StandaloneWindows.zip1", m_DeltaVersion);
#endif
        }
        else
        {
#if UNITY_ANDROID
        m_DownloadFilename = "Android.zip";
#elif UNITY_IOS
        m_DownloadFilename = "iOS.zip";
#else
            m_DownloadFilename = "StandaloneWindows.zip";
#endif
        }

        string URL = GetDownloadURL(m_DownloadFilename); //生成下载链接

        if (File.Exists(m_DownloadDir + m_DownloadFilename) == true)
            File.Delete(m_DownloadDir + m_DownloadFilename); //删除以前的包

        m_Request = new HTTPRequest(new Uri(URL), false, true, (req, resp) =>  //这是一个lambda 表达式 参数 是req 和resp 的匿名函数
        {
            //check available space
            if (pre_download_size_check == false)
            {
                pre_download_size_check = true;

                //Debug.LogFormat("[Download File Size:{0}MB] [AvailableSize:{1}MB]", req.DownloadLength / MAGABYTE, GetStorageFreeSpace());
                if (((req.DownloadLength / MEGABYTE) < GetStorageFreeSpace()) == false)
                {
                    Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(Start), null), string.Format("Not Enough Free Space! \n [Need:{0}] [Now Free Space:{1}]", req.DownloadLength / MEGABYTE, GetStorageFreeSpace()));
                    req.Abort();
                    m_Request = null;
                    return;
                }
            }

            switch (req.State)
            {
                // The request is currently processed. With UseStreaming == true, we can get the streamed fragments here

                case HTTPRequestStates.Processing:
                    // Get the fragments, and save them
                    ProcessFragments(resp.GetStreamedFragments());

                    m_ProgressValue = req.Downloaded / (float)req.DownloadLength;

                    break;

                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        // Save any remaining fragments
                        ProcessFragments(resp.GetStreamedFragments());

                        // Completly finished
                        if (resp.IsStreamingFinished)
                        {
                            Unzip();

                            m_Request = null;
                        }
                    }
                    else
                    {
                        if (resp.StatusCode == 404 && m_DeltaVersion > 0)
                        {
                            m_State = ePatchState.DownloadFull;
                        }
                        else
                        {
                            Popup.Instance.ShowMessage("{0} {1}", resp.StatusCode, resp.Message);
                            status = string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                            resp.StatusCode,
                                                            resp.Message,
                                                            resp.DataAsText);
                            Debug.LogWarning(URL);
                            Debug.LogWarning(status);
                        }
                        m_Request = null;
                    }
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error: //http访问出错
                    status = "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    Debug.LogError(status);
                    Popup.Instance.ShowMessage(req.Exception != null ? req.Exception.Message : "HTTPRequestStates.Error");

                    m_Request = null;
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    status = "Request Aborted!";
                    Debug.LogWarning(status);
                    Popup.Instance.ShowMessage(status);

                    m_Request = null;
                    break;

                // Ceonnecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut: //htp连接超时
                    status = "Connection Timed Out!";
                    Debug.LogError(status);

                    Popup.Instance.ShowMessage(status);

                    m_Request = null;
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut: //http访问超时
                    status = "Processing the request Timed Out!";
                    Debug.LogError(status);

                    Popup.Instance.ShowMessage(status);

                    m_Request = null;
                    break;
            }
        });

        //// Are there any progress, that we can continue?
        //if (PlayerPrefs.HasKey("DownloadProgress"))
        //    // Set the range header
        //    request.SetRangeHeader(PlayerPrefs.GetInt("DownloadProgress"));
        //else
        //    // This is a new request
        //    PlayerPrefs.SetInt("DownloadProgress", 0);

        // If we are writing our own file set it true(disable), so don't duplicate it on the filesystem
        m_Request.DisableCache = true;

        // We want to access the downloaded bytes while we are still downloading
        m_Request.UseStreaming = true;

        // Set a reasonable high fragment size. Here it is 5 megabytes.
        m_Request.StreamFragmentSize = HTTPResponse.MinBufferSize;

        // Start Processing the request
        m_Request.Send();

        m_State = ePatchState.Download;
    }

    /// <summary>
    /// In this function we can do whatever we want with the downloaded bytes. In this sample we will do nothing, just set the metadata to display progress.
    /// </summary>
    void ProcessFragments(List < byte[] > fragments)
    {
            if (fragments != null && fragments.Count > 0)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(m_DownloadDir, m_DownloadFilename), System.IO.FileMode.Append))
                {
                    for (int i = 0; i < fragments.Count; ++i)
                    {
                        fs.Write(fragments[i], 0, fragments[i].Length);
                    }
                }
            }
        }

    void ApplyDelta()
    {
        m_DeltaApplyProgress = new DeltaProgress();

        int need_size = (int)(new FileStream(m_DownloadDir + AssetManager.AssetBundleFilename, FileMode.Open, FileAccess.Read, FileShare.Read).Length + new FileStream(unzip_dest_path, FileMode.Open, FileAccess.Read, FileShare.Read).Length) / MEGABYTE;

        //Debug.LogFormat("[Need Apply Byte:{0}MB] [AvailableSize:{1}MB]", need_size , GetStorageFreeSpace());
        if (need_size > GetStorageFreeSpace())
        {
            Popup.Instance.ShowCallbackKey(new PopupCallback.Callback(new Action(Start), null), string.Format("Not Enough Free Space! \n [Need:{0}] [Now Free Space:{1}]", need_size, GetStorageFreeSpace()));
            return;
        }
#if USE_THREAD
        Thread th = new Thread(ApplyDeltaThread);
        th.Start();
#else
        ApplyDeltaThread();
#endif
    }

    void ApplyDeltaThread()
    {
            var delta = new Octodiff.Core.DeltaApplier
            {
                SkipHashCheck = m_SkipHashCheck
            };

            using (var basisStream = new FileStream(m_DownloadDir + AssetManager.AssetBundleFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(unzip_dest_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFileStream = new FileStream(m_DownloadDir + AssetManager.AssetBundleFilename + "_new", FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                try
                {
                    delta.Apply(basisStream, new Octodiff.Core.BinaryDeltaReader(deltaStream, m_DeltaApplyProgress), newFileStream, m_DeltaApplyProgress);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                    m_DeltaApplyProgress.IsFail = true;

                    deltaStream.Close();
                    newFileStream.Close();
                    File.Delete(unzip_dest_path);
                    File.Delete(m_DownloadDir + AssetManager.AssetBundleFilename + "_new");
                    return;
                }
            }
            try
            {
#if !UNITY_WEBPLAYER
                File.Delete(unzip_dest_path);
                File.Delete(m_DownloadDir + AssetManager.AssetBundleFilename);
                File.Move(m_DownloadDir + AssetManager.AssetBundleFilename + "_new", m_DownloadDir + AssetManager.AssetBundleFilename);
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                m_DeltaApplyProgress.IsFail = true;
            }

            m_DeltaApplyProgress.IsSuccess = true;

            //        File.Delete(bundle_filename);
        }

        DeltaProgress m_DeltaApplyProgress = null;
    public class DeltaProgress : Octodiff.Diagnostics.IProgressReporter
    {
        public float Percent { get; private set; }
        public bool IsEnd { get; private set; }
        public bool IsFail { get; set; }
        public bool IsSuccess { get; set; }

        public DeltaProgress()
        {
        }

        public void ReportProgress(string operation, long currentPosition, long total)
        {
            if (IsEnd == false)
                IsEnd = currentPosition == total;
            Percent = (float)currentPosition / total;
        }
    }

}
