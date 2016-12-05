#if UNITY_EDITOR
#define USE_SECRET
#endif

#define USE_SECRET


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using MNS;
using System.Security.Cryptography;
using Newtonsoft.Json;
using CodeStage.AntiCheat.ObscuredTypes;
using PacketEnums;
using System;

public class SHSavedData : Singleton<SHSavedData> //这是一个单例类
{
    static public uint crc32(string input) //应该是在加密
    {
        var table = new uint[]{ //0x表示是16进制
            0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F,
                0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
                0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
                0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
                0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9,
                0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
                0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C,
                0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
                0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
                0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
                0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x01DB7106,
                0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
                0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D,
                0x91646C97, 0xE6635C01, 0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
                0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
                0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
                0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7,
                0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
                0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA,
                0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
                0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
                0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
                0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84,
                0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
                0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB,
                0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
                0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
                0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
                0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55,
                0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
                0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28,
                0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
                0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
                0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
                0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242,
                0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
                0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69,
                0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
                0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
                0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
                0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693,
                0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
                0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D //这里是256个 即2 的8次方
        };

        unchecked //看上去 是在解密
        {
            // unit 表示32位的无符号型的整型
            uint crc = (uint)(((uint)0) ^ (-1));
            var len = input.Length; //取出输入的字符的长度
            for (var i = 0; i < len; i++)
            {
                crc = (crc >> 8) ^ table[
                    (crc ^ (byte)input[i]) & 0xFF //这里表示取
            ];//byte是无符号8位整数
            }
            crc = (uint)(crc ^ (-1));

            if (crc < 0)
            {
                crc += (uint)4294967296; //这个数就是2 的32次方
            }

            return crc;
        }
    }

    public int InfoVersion
    {
        get { return ObscuredPrefs.GetInt("InfoVersion", 1); }
        set { ObscuredPrefs.SetInt("InfoVersion", value); }
    }

    //static account info
    /////////////////////////////////////////////////////////////////////////
    const string AccountIdxKey = "account_idx";
    const string LoginTokenKey = "login_token";
    const string AccessTokenKey = "access_token";
    const string LoginPlatformKey = "login_platform";

    static public DateTime PauseTime
    {
        get { return DateTime.Parse(ObscuredPrefs.GetString("pause_time", DateTime.Now.ToString())); }
        set { ObscuredPrefs.SetString("pause_time", value.ToString()); }
    }
    static public long AccountIdx
    {
        get { return ObscuredPrefs.GetLong(AccountIdxKey, -1); }
        set { ObscuredPrefs.SetLong(AccountIdxKey, value); }
    }
    static public int LoginToken
    {
        get { return ObscuredPrefs.GetInt(LoginTokenKey, -1); }
        set { ObscuredPrefs.SetInt(LoginTokenKey, value); }
    }
    static public int AccessToken
    {
        get { return ObscuredPrefs.GetInt(AccessTokenKey, -1); }
        set { ObscuredPrefs.SetInt(AccessTokenKey, value); }
    }
    static public LoginPlatform LoginPlatform
    {
        get { return (LoginPlatform)System.Enum.Parse(typeof(LoginPlatform), ObscuredPrefs.GetString(LoginPlatformKey, "Invalid")); }
        set { ObscuredPrefs.SetString(LoginPlatformKey, value.ToString()); }
    }
    static public bool IsNewAccount { get { return AccountIdx == -1; } }

    static public void RemoveAccountInfo()
    {
        ObscuredPrefs.DeleteKey(AccountIdxKey);
        ObscuredPrefs.DeleteKey(LoginTokenKey);
        ObscuredPrefs.DeleteKey(LoginPlatformKey);
    }

    static public int FriendDeleteCount
    {
        get { return Network.DailyIndex == Network.PlayerInfo.friends_delete_daily_index ? ObscuredPrefs.GetInt("FriendDeleteCount", 0) : 0; }
        set { ObscuredPrefs.SetInt("FriendDeleteCount", value); }
    }
    /////////////////////////////////////////////////////////////////////////

    public static string CrashReportKey = "CrashReport";
    public string LastCrashReport
    {
        get { return ObscuredPrefs.GetString(CrashReportKey, ""); }
        set { ObscuredPrefs.SetString(CrashReportKey, value); }
    }

    public static string BundleVersionKey = "BundleVersion";
    public int BundleVersion
    {
        get { return ObscuredPrefs.GetInt(BundleVersionKey, 1); }
        set { ObscuredPrefs.SetInt(BundleVersionKey, value); }
    }

    public int LastAttendCheckDailyIndex
    {
        get { return PlayerPrefs.GetInt("LastAttendCheckDailyIndex", 0); }
        set { PlayerPrefs.SetInt("LastAttendCheckDailyIndex", value); }
    }

    public int IsOpenedContents(string id)
    {
        return PlayerPrefs.GetInt(id, 0);
    }
    public void SetOpenedContents(string id)
    {
        PlayerPrefs.SetInt(id, 1);
    }

    /////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////
    /// <summary>
    /// save downloaded Game info from server
    /// </summary>
    /// <param name="infoFiles"></param>
    /// <param name="version"></param>
    public void SaveGameInfo(List<C2G.p_InfoFile> infoFiles, int version)
    {
        string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(infoFiles);
        InfoVersion = version;

#if USE_SECRET
        SaveStringEncrypt(string.Format("{0}.dat", version), string.Format("ms_sh_{0:00}", version), jsonStr);
#else
        SaveString(string.Format("{0}.dat", version), jsonStr);
#endif

    }

    static public void SaveStringEncrypt(string filename, string SECRET_KEY, string str) //将字符串加密并且存储
    {
        SECRET_KEY = crc32(SECRET_KEY).ToString("x8"); //将字符串加密 并且转化为16进制  X表示16进制 8 表示8位 不知用0补齐

        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        cryptoProvider.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(SECRET_KEY);
        cryptoProvider.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(SECRET_KEY);


        string path = GetDocumentsFilePath(filename); //得到文件的路径


        using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            using (CryptoStream cs = new CryptoStream(fs, cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs, Encoding.UTF8))
                {
                    try
                    {
                        sw.Write(str);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex.ToString());
                    }
                    finally
                    {
                        sw.Close();
                        cs.Close();
                        fs.Close();
                    }
                }
            }
        }
    }

    static public void SaveString(string filename, string str) //将字符串 以UTF-8的格式写进文件里面
    {
        string path = GetDocumentsFilePath(filename);
        try
        {
#if !UNITY_WEBPLAYER
            System.IO.File.WriteAllText(path, str, Encoding.UTF8);
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.ToString());        	
        }
    }

    static public void SaveSaveData(string filename, string str) //往文件中存数据
    {
#if USE_SECRET
        SaveStringEncrypt(filename, LoginToken.ToString(), str);
#else
        SaveString(filename, str);
#endif
    }

    static public string LoadSaveData(string filename) //加载文件中的信息
    {
#if USE_SECRET
        return LoadFromEncryptFile(filename, LoginToken.ToString());
#else
        return SHSavedData.LoadFromFile(filename);
#endif
    }


    public void LoadData() //通过版本加载一些信息
    {
        string filename = string.Format("{0}.dat", InfoVersion);
#if USE_SECRET

        string secretKey = string.Format("ms_sh_{0:00}", InfoVersion);  // must be 64bit, 8bytes
#else
        filename += ".json";
#endif

        string jsonStr;
#if USE_SECRET
        jsonStr = LoadFromEncryptFile(filename, secretKey);
        //#if UNITY_EDITOR
        //        SaveString(filename + ".json", jsonStr);
        //#endif

#else
        jsonStr = LoadFromFile(filename);
#endif
        //Debug.LogFormat("{0} :\n {1}", path, jsonStr);

        List<C2G.p_InfoFile> info_files = JsonConvert.DeserializeObject<List<C2G.p_InfoFile>>(jsonStr);

        foreach (var info in info_files) //给不同为类初始化数据
        {
            Debug.LogFormat("LoadInfo : {0}", info.filename);
            switch (info.filename)
            {
                case "CreatureInfo.xml": CreatureInfoManager.Instance.LoadData(info.data); break;
                case "SoulStoneInfo.xml": SoulStoneInfoManager.Instance.LoadData(info.data); break;
                case "RuneInfo.xml": RuneInfoManager.Instance.LoadData(info.data); break;
                case "RuneTable.csv": RuneTableManager.Instance.LoadData(info.data); break;

                case "Localization.csv": Localization.LoadCSV(UTF8Encoding.UTF8.GetBytes(info.data)); break;

                case "TokenInfo.xml": TokenInfoManager.Instance.LoadData(info.data); break;
                case "SkillInfo.xml": SkillInfoManager.Instance.LoadData(info.data); break;
                case "ItemInfo.xml": ItemInfoManager.Instance.LoadData(info.data); break;
                case "StuffInfo.xml": StuffInfoManager.Instance.LoadData(info.data); break;
                case "EquipInfo.xml": EquipInfoManager.Instance.LoadData(info.data); break;
                case "MapInfo.xml": MapInfoManager.Instance.LoadData(info.data); break;
                case "MapInfo_Event.xml": MapInfoManager.Instance.LoadData(info.data, true); break;
                case "MapInfo_Weekly.xml": MapInfoManager.Instance.LoadData(info.data, true); break;
                case "MapInfo_Boss.xml": MapInfoManager.Instance.LoadData(info.data, true); break;
                case "MapInfo_Worldboss.xml": MapInfoManager.Instance.LoadData(info.data, true); break;
                case "LevelTable.csv": LevelInfoManager.Instance.LoadData(info.data); break;
                case "StoreInfo.xml": StoreInfoManager.Instance.LoadData(info.data); break;
                case "CheatInfo.xml": CheatInfoManager.Instance.LoadData(info.data); break;
                case "TutorialInfo.xml": TutorialInfoManager.Instance.LoadData(info.data); break;
                case "QuestInfo.xml": QuestInfoManager.Instance.LoadData(info.data); break;
                case "MapClearRewardInfo.xml": MapClearRewardInfoManager.Instance.LoadData(info.data); break;
                case "CreatureBook.xml": CreatureBookInfoManager.Instance.LoadData(info.data); break;
                case "PvpReward.csv": PvpRewardDataManager.Instance.LoadData(info.data); break;
                case "WorldBossReward.csv": WorldBossRewardDataManager.Instance.LoadData(info.data); break;
                case "AttendInfo.xml": AttendInfoManager.Instance.LoadData(info.data); break;
                case "AdventureInfo.xml": AdventureInfoManager.Instance.LoadData(info.data); break;
                case "GuildInfo.xml": GuildInfoManager.Instance.LoadData(info.data); break;
                case "KingsGiftInfo.xml": KingsGiftInfoManager.Instance.LoadData(info.data); break;
                case "EventInfo.xml": HottimeEventInfoManager.Instance.LoadData(info.data); break;
            }
        }
    }

    public static string GetDocumentsFilePath(string fileName)
    {
        if (Application.isEditor)
        {
            string datapath = "Assets";
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - datapath.Length) + "/SaveData/";
            return Path.Combine(path, fileName);
        }
        else
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                return Path.Combine(Application.persistentDataPath, fileName);
            }
            else
            {
                return Path.Combine(Application.dataPath, fileName);
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////
    static public string LoadFromFile(string filename)
    {
        string res = "";
        string _filePath = GetDocumentsFilePath(filename);
        try
        {
#if !UNITY_WEBPLAYER
            res = System.IO.File.ReadAllText(_filePath, Encoding.UTF8);
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat(ex.ToString());
        }
        return res;
    }

    static public bool FileExists(string filename)
    {
        string _filePath = GetDocumentsFilePath(filename);
        return File.Exists(_filePath);
    }

    //-----------------------------------------------------------------------------
    static public string LoadFromEncryptFile(string filename, string SECRET_KEY)
    {
        SECRET_KEY = crc32(SECRET_KEY).ToString("x8");

        string _filePath = GetDocumentsFilePath(filename);

        if (File.Exists(_filePath) == false)
        {
            Debug.LogWarningFormat("{0} is not exists.", _filePath);
            return "";
        }

        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        cryptoProvider.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(SECRET_KEY);
        cryptoProvider.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(SECRET_KEY);


        using (FileStream fs = File.Open(_filePath, FileMode.Open))
        {
            using (CryptoStream cs = new CryptoStream(fs, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                {
                    try
                    {
                        return sr.ReadToEnd();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex.ToString());
                        //_deserializeErrorString = ex.ToString();
                    }
                    finally
                    {
                        sr.Close();
                        cs.Close();
                        fs.Close();
                    }
                }
            }
        }

        return "";
    }
}
