using PacketEnums;

namespace C2L
{
    public enum ErrorType
    {
        LoginDuplicated,
    }

    public class ErrorMessageAck
    {
        public ErrorType error_type;
        public string error_message;
    }

    public class AppInfo
    {
        public string platform;
        public string bundle_identifier;
        public string bundle_version;
        public string device_info;
    }

    public class RequestAssetBundleVersion
    {
        public AppInfo app_info;
    }

    public class RequestAssetBundleVersionAck
    {
        public int asset_bundle_version;
        public int asset_bundle_delta_version;
        public string download_url;
        public bool skip_hash_check;
    }

    public enum eLoginResult
    {
        Successed,
        Blocked,
        WaitWithDrawal,
        Maintenance,
        NeedUpdate,
        UserLimit,
        SessionExpired,
        NotExistAccount,
        NotExistBetakey,
        BetakeyExpired,
        BetakeyNoCount,
        NeedAgree,
    }

    public class PlatformCheck
    {
        public LoginPlatform login_platform;
        public string login_id;
    }

    public class PlatformCheckAck
    {
        public bool is_exist;
        public string nickname;
    }

    public class PlatformLogin
    {
        public LoginPlatform login_platform;
        public string login_id;
        public string bundle_identifier;

        public bool is_new;
    }

    public class PlatformLoginAck
    {
        public eLoginResult result;
        public long account_idx;
        public int login_token;
    }

    public class GuestLogin
    {
        public string bundle_identifier;
    }

    public class GuestLoginAck
    {
        public eLoginResult result;
        public long account_idx;
        public int login_token;
    }

    public class BetakeyLogin
    {
        public string bundle_identifier;
        public string betakey;
    }

    public class BetakeyLoginAck
    {
        public eLoginResult result;
        public long account_idx;
        public int login_token;
    }

    public class LoginAuto
    {
        public long account_idx;
        public int login_token;
        public int access_token;

        public int info_version;

        public string push_id;
        public string push_token;

        public LoginPlatform login_platform;
        public AppInfo app_info;

        public bool agree;
    }

    public class LoginAutoAck
    {
        public eLoginResult result;

        public int access_token;
        public int reconnect_index;

        public bool request_info;
        public bool request_data;
    }
}
