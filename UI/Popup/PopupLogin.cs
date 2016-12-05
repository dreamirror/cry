using UnityEngine;
using Facebook.Unity;
using PacketEnums;
using System.Collections.Generic;
using System;

public class PopupLogin : PopupBase
{
    public UIGrid m_Grid;
    public GameObject m_Google;
    public GameObject m_Gamecenter;

    Action m_LoginCallback = null;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if (parms != null && parms.Length == 1)
            m_LoginCallback = parms[0] as Action;
        
        Init();
    }
    public override void OnFinishedShow()
    {
        base.OnFinishedShow();
        m_Grid.Reposition();
    }
    void Init()
    {
        switch(Application.platform)
        {
            case RuntimePlatform.Android:
            default:
                m_Google.SetActive(true);
                m_Gamecenter.SetActive(false);
                break;
            case RuntimePlatform.IPhonePlayer:
                m_Google.SetActive(false);
                m_Gamecenter.SetActive(true);
                break;
        }
        m_Grid.repositionNow = true;
    }

    void LoginCallback()
    {
        base.OnClose();
        if (m_LoginCallback != null)
            m_LoginCallback();
    }
    public void OnClickFacebook()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }

        if (FB.IsLoggedIn == true)
        {
            FacebookLogined();
        }
        else
        {
            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }
    }

    public void OnClickGoogle()
    {
        UM_GameServiceManager.OnConnectionStateChnaged += OnConnectedGameCenter;
        UM_GameServiceManager.Instance.Connect();
    }
    public void OnClickGamecenter()
    {
        UM_GameServiceManager.OnConnectionStateChnaged += OnConnectedGameCenter;
        UM_GameServiceManager.Instance.Connect();
    }
    public void OnClickGuest()
    {
        SHSavedData.LoginPlatform = LoginPlatform.Guest;
#if SH_TEST || SH_DEV
        SHSavedData.LoginPlatform = LoginPlatform.Betakey;
#endif
        LoginCallback();
    }
    public override void OnClose()
    {
        //base.OnClose();
    }

    //////////////////////////////////////////////////////////////////////////
    // Facebook 
    //////////////////////////////////////////////////////////////////////////
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }

            FacebookLogined();
        }
        else
        {
#if SH_TEST || SH_DEV
            Tooltip.Instance.ShowMessageKey("NotImplement");
#endif
            Debug.Log("User cancelled login");
        }
    }

    void FacebookLogined()
    {
        Debug.LogFormat("user id : {0}\naccess token : {1}", AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
        SHSavedData.LoginPlatform = LoginPlatform.Facebook;
        LoginCallback();
    }

    /////////////////////////////////////////////////////////////////////////
    // gamecenter or google
    void OnConnectedGameCenter(UM_ConnectionState um_connection_state)
    {
        if(um_connection_state != UM_ConnectionState.DISCONNECTED)
        {
#if SH_TEST || SH_DEV
            Tooltip.Instance.ShowMessageKey("NotImplement");
#endif
            return;
        }
        if (um_connection_state != UM_ConnectionState.CONNECTED)
        {
            return;
        }
        UM_GameServiceManager.OnConnectionStateChnaged -= OnConnectedGameCenter;
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
                SHSavedData.LoginPlatform = LoginPlatform.GameCenter;
                break;
            case RuntimePlatform.Android:
                SHSavedData.LoginPlatform = LoginPlatform.GooglePlay;
                break;
        }

        Debug.LogFormat("user id : {0}", UM_GameServiceManager.Instance.Player.PlayerId);

        LoginCallback();
    }
}
