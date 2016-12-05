#if UNITY_EDITOR
//#define TEST_LOGIN_POPUP
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using PacketEnums;
using Facebook.Unity;


enum eTitleState //状态枚举
{
	idle,
	init,
	connecting,
	connected,
	touchToStart,
}   

public class Title : MonoBehaviour 
{
	public UIAtlas m_AtlasTitle;
	public UIAtlas m_Atlas;
	public MeshRenderer m_BG; //背景图片的renderer
	public UILabel m_Version; //版本号
	public GameObject m_TouchToStart;//点击组件
	public UILabel m_LabelTouchToStart; //点击开始的label
//    public SRDebugger.SRDebuggerInit m_SRDebuggerInit;

	eTitleState state = eTitleState.idle; //默认的状态是 idle
	// Use this for initialization
	
	void OnEnable() //脚本被激活的时候
	{
		Network.HideIndicator();//停止活跃指示器？
	}

	void Start () {
		//         if (m_SRDebuggerInit != null && Debug.isDebugBuild)
		//             m_SRDebuggerInit.gameObject.SetActive(true);
#if UNITY_ANDROID //如果是安卓平台
		AndroidKeyboardManager.Install();
		//AndroidKeyboard.AdditionalOptions.fullScreen = false;
		//AndroidKeyboard.AdditionalOptions.noSuggestions = true;
		//AndroidKeyboard.TouchScreenKeyboard.hideInput = false;
#endif

		ConfigData.Instance.Init(); //初始化配置数据

		SoundManager.Instance.PlayBGM("Title"); //播放背景音效
		Localization.language = ConfigData.Instance.Language; //初始化游戏语言


#if UNITY_EDITOR //如果是在unity编辑器中运行
		if (m_Atlas.spriteMaterial == null || m_Atlas.spriteMaterial.name != "SHUIAtlas_mat")
			m_Atlas.replacement = AssetManager.LoadUIAtlas();
		if (m_AtlasTitle.spriteMaterial == null)
			m_AtlasTitle.replacement = AssetManager.LoadTitleAtlas();
#else
		m_Atlas.replacement = AssetManager.LoadUIAtlas();
		m_AtlasTitle.replacement = AssetManager.LoadTitleAtlas();
#endif

		m_BG.material.mainTexture = AssetManager.LoadBG("000_title"); //背景图片
		

#if UNITY_EDITOR
		m_Version.text = string.Format("{0} (editor, asset:{1})", Application.version, SHSavedData.Instance.BundleVersion); //版本信息的文字
#elif SH_DEV
		m_Version.text = string.Format("{0} (dev, asset:{1})", Application.version, SHSavedData.Instance.BundleVersion);
#else
		m_Version.text = string.Format("{0} (test, asset:{1})", Application.version, SHSavedData.Instance.BundleVersion);
#endif

		m_TouchToStart.SetActive(true); //开启点击的组件

		MetapsAnalyticsScript.Init(Application.bundleIdentifier);

		if (!FB.IsInitialized)
		{
			// Initialize the Facebook SDK
			FB.Init(InitFacebookCallback, OnHideUnity);
		}
		else
		{
			FB.ActivateApp();
		}

#if TEST_LOGIN_POPUP
		Popup.Instance.Show(ePopupMode.Login, new Action(TouchToStart));
#endif
	}

	// Update is called once per frame
	void Update () {
		switch(state)
		{
			case eTitleState.idle: //空闲的状态
			case eTitleState.init: //初始化的状态
				m_LabelTouchToStart.text = Localization.Get("TouchToStart"); //初始化点击提示的文本
				if (Network.ConnectState == eConnectState.connecting || Network.ConnectState == eConnectState.connected)
					state = eTitleState.connecting; //状态改为正在连接

				if (Input.GetKeyDown(KeyCode.Escape))
				{
					Popup.PopupInfo popup = Popup.Instance.GetCurrentPopup();
					if (popup == null)
						Popup.Instance.ShowConfirmKey(new PopupConfirm.Callback(OnExit), "ConfirmQuit");
				}
				break;

			case eTitleState.connecting:
				m_LabelTouchToStart.text = Localization.Get("ConnectingToGameServer");

				if (Network.ConnectState == eConnectState.connected)
				{
					state = eTitleState.connected;
				}
				else if (Network.ConnectState == eConnectState.idle)
				{
					state = eTitleState.init;
					m_LabelTouchToStart.text = Localization.Get("TouchToStart");
					return;
				}

				break;

			case eTitleState.connected:
				Network.ShowIndicator();
				UnityEngine.SceneManagement.SceneManager.LoadScene("Main", UnityEngine.SceneManagement.LoadSceneMode.Single); //跳转到main场景
				//state = eTitleState.touchToStart;
				break;
		}
	}

	void OnExit(bool is_confirm)
	{
		if (is_confirm)
			Application.Quit();
	}

	public void TouchToStart()
	{
		switch (SHSavedData.LoginPlatform)
		{
			case LoginPlatform.Facebook:
				if (FB.IsLoggedIn == true)
				{
					FacebookLogined();
				}
				else
				{
					var perms = new List<string>() { "public_profile", "email", "user_friends" };
					FB.LogInWithReadPermissions(perms, AuthCallback);
				}
				return;

			case LoginPlatform.GameCenter:
			case LoginPlatform.GooglePlay:
				if (UM_GameServiceManager.Instance.IsConnected == false)
				{
					UM_GameServiceManager.OnConnectionStateChnaged += OnConnectedGameCenter;
					UM_GameServiceManager.Instance.Connect();
					return;
				}
				break;

			default:
				Popup.Instance.Show(ePopupMode.Login, new Action(TouchToStart));
				return;
	
			case LoginPlatform.Guest:
			case LoginPlatform.Betakey:
				break;
		}

		ConnectToGameserver();
	}

	public void OnApplicationPause(bool pause)
	{
		Debug.LogFormat("OnApplicationPause : {0}", pause);
		if (pause == false)
			PushManager.Instance.ResetBadgeNumber();
	}

	private void ConnectToGameserver()
	{
		if (state == eTitleState.idle)
		{
			Network.Instance.Init();
			state = eTitleState.init;
		}

		if (state == eTitleState.init)
		{
			Network.Instance.StartConnect();
			state = eTitleState.connecting;
		}
	}

	void OnConnectedGameCenter(UM_ConnectionState um_connection_state)
	{
		if (um_connection_state != UM_ConnectionState.CONNECTED)
		{
			Debug.LogWarningFormat("UM_ConnectionState : {0}", um_connection_state);
			return;
		}
		UM_GameServiceManager.OnConnectionStateChnaged -= OnConnectedGameCenter;

		Debug.LogFormat("user id : {0}", UM_GameServiceManager.Instance.Player.PlayerId);

		ConnectToGameserver();
	}


	//////////////////////////////////////////////////////////////////////////
	// Facebook 
	//////////////////////////////////////////////////////////////////////////
	private void InitFacebookCallback()
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
			Debug.Log("User cancelled login");
		}
	}

	void FacebookLogined()
	{
		Debug.LogFormat("user id : {0}\naccess token : {1}",AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
		SHSavedData.LoginPlatform = LoginPlatform.Facebook;
		ConnectToGameserver();
	}
}
