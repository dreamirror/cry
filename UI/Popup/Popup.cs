using UnityEngine;
using System.Collections.Generic;

public enum ePopupMode
{
	Message,
	Character,
	BattleOption,
	BattleEnd,
	BattleEndFail,
	BookCharacter,
	PieceCharacter,
	LootCharacter,
	LootCharacter10,
	LeaderSkillSelect,
	Item,
	Stuff,
	Callback,
	Enchant,
	EquipUpgrade,
	Colosseum,
	Community,
	Devil,
	Training,
	TrainingDifficulty,
	StoreConfirm,
	LootItem,
	LootItem10,
	MoveStore,
	Cheat,
	Input,
	Mission,
	Sweep,
	CharacterLevelup,
	Setting,
	Profile,
	Nickname,
	FriendsRequest,
	MailBox,
	MailDetail,
	Chat,
	PVPBattleReady,
	PVPBattleEnd,
	Reward,
	PVPDelayReset,
	Ranking,
	RankingReward,
	EquipEnchant,
	SkillEnchant,
	Confirm,
	HeroDetail,
	SlotBuy,
	BossInfo,
	BossDetail,
	Attend,
	StuffSale,
	RuneEnchant,
	EnchantNew,
	Login,
	EvalScore,
	StuffConfirm,
	ExpPowderMove,
	WeeklyDungeon,
	Policy,
	Adventure,
	AdventureReady,
	GuildEmblem,
	GuildGoldGive,
	GuildSetting,
	GuildMemberInfo,
	GuildRequestedInfo,
	GuildMemberState,
	GuildSearch,
	WorldBossPlayerInfo,
	WorldBossBattleEnd,
    HottimeEvent,
}

public interface IPopup //定义了一个接口 接口 的方法都要实现
{
	Popup parent { get; set; }
	bool bCloseExternalClick { get; }
	void SetParams(bool is_new, object[] parms);
	bool bShowImmediately { get; }
}

public abstract class PopupBase : MonoBehaviour, IPopup //这是一个抽象的类
{
	public bool ExteralClose;

	protected Popup mParent;
	protected object[] m_parms;
	protected bool is_new;
	protected bool bShowed = false;
	virtual public Popup parent { get { return mParent; } set { mParent = value; } }
	virtual public bool bCloseExternalClick { get { return ExteralClose; } }
	virtual public void SetParams(bool is_new, object[] parms)
	{
		this.is_new = is_new;
		m_parms = parms;
	}
	virtual public bool bShowImmediately { get { return false; } }

	virtual public void OnFinishedTweenImmediately()
	{
		bShowed = true;
	}
	virtual public void OnFinishedShow()
	{
		bShowed = true;
	}
	virtual public void OnFinishedHide()
	{
		bShowed = false;
	}
	virtual public void OnClose()
	{
		parent.Close(true);
	}
}

public class PopupContainer 
{
	public PopupContainer(List<Popup.PopupInfo> stacks) //构造函数
	{
		Stacks = new List<Popup.PopupInfo>(stacks);
	}
	public List<Popup.PopupInfo> Stacks;
}

public class Popup : MonoBehaviour
{
	static Popup m_Instance = null;
	static public Popup Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = ((GameObject)GameObject.Instantiate(Resources.Load("Prefab/Menu/Popup"))).GetComponent<Popup>(); //在Popup预设体上面把Popup脚本取出来
				GameObject.DontDestroyOnLoad(m_Instance.gameObject);
			}
			return m_Instance;
		}
	}

	public GameObject contents;

	public UIPlayTween m_TWBlock;
	public UIPlayTween m_TWShow;
	public UIPlayTween m_TWHide;
	public UIPlayTween m_TWImmediately;

	bool bInit = false;
	bool bDelayedShow = false;
	bool bShow = false;
	// Use this for initialization
	void Awake()
	{
	}

	void Start () {
		bInit = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (bInit == true && bDelayedShow == true)
		{
			bDelayedShow = false;
			m_TWShow.Play(true);
		}
	}
	
	virtual public void OnFinishedTweenScale()
	{
		Debug.Log("OnFinishedTweenScale");
	}
	public void OnFinishedTweenImmediately()
	{
		bShow = true;
		PopupInfo info = GetCurrentPopup();
		if (info != null && info.Obj != null)
			info.Obj.OnFinishedTweenImmediately();
	}
	public void OnFinishedShow()
	{
		bShow = true;
		PopupInfo info = GetCurrentPopup();
		if (info != null && info.Obj != null)
			info.Obj.OnFinishedShow();
	}
	public void OnFinishedHide()
	{
		PopPopup().SetActive(false);
		if (mPopupStack.Count == 0)
		{
			m_TWBlock.gameObject.SetActive(false);
			bShow = false;
		}
		else
		{
			Show(false, GetCurrentPopup().bShowImmediately);
		}
	}

	void InitCurrentPopup(bool is_new)
	{
		PopupInfo info = GetCurrentPopup();
		IPopup popup = info.Obj.GetComponent<IPopup>();
		try
		{
			if (popup == null)
				throw new System.Exception(string.Format("not found {0}", info.mode));
			info.bExternalClose = popup.bCloseExternalClick;
			popup.parent = this;
			popup.SetParams(is_new, info.parms);
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
		}        
	}

	public void Show(bool is_new = false, bool bImmediately = false)
	{
		InitCurrentPopup(is_new);

		gameObject.SetActive(true);
		m_TWBlock.gameObject.SetActive(true);

		contents.transform.localScale = Vector3.one;
		GetCurrentPopup().SetActive(true);

		if (bInit == true)
		{
			if (bImmediately)
				m_TWImmediately.Play(true);
			else
				m_TWShow.Play(true);
		}
		else
			bDelayedShow = true;
	}

	public void Close(bool bForce = false, bool bImmediately = false)
	{
		if (GetCurrentPopup() == null) return;
		if (bShow == false) return;

		if (bForce == true || GetCurrentPopup().bExternalClose == true)
		{
			if (bImmediately == true)
			{
				PopPopup().SetActive(false);
				if (GetCurrentPopup() == null)
					gameObject.SetActive(false);
				else
					Show();
			}
			else
				m_TWHide.Play(true);
		}
	}

	public class PopupInfo
	{
		public ePopupMode mode;
		PopupBase obj;
		public bool bExternalClose = false;
		public object[] parms;

		public PopupBase Obj
		{
			get
			{
				if (obj == null)
				{
					obj = Popup.Instance.Load(mode);
				}
				return obj;
			}
		}

		public PopupInfo(ePopupMode _mode, PopupBase _obj, params object[] _parms)
		{
			mode = _mode;
			obj = _obj;
			parms = _parms;
		}
		public void SetActive(bool active)
		{
			if(active == false)
				Obj.OnFinishedHide();
			Obj.gameObject.SetActive(active);
		}
		public bool bShowImmediately { get { return obj == null ? false : obj.bShowImmediately; } }
	}

	public List<PopupInfo> mPopupStack = new List<PopupInfo>();
	public Dictionary<ePopupMode, PopupBase> mPopupCache = new Dictionary<ePopupMode, PopupBase>();
	public PopupInfo GetCurrentPopup()
	{
		PopupInfo result = null;
		if (mPopupStack.Count > 0)
		{
			result = mPopupStack[0];
		}
		return result;
	}
	PopupInfo PopPopup()
	{
		PopupInfo result = null;
		if (mPopupStack.Count > 0)
		{
			result = mPopupStack[0];
			mPopupStack.RemoveAt(0);
		}
		return result;
	}
	void StackPopup(PopupInfo menu)
	{
		PopupInfo current = GetCurrentPopup();
		if(current != null)
		{
			current.SetActive(false);
		}
		mPopupStack.Insert(0, menu);
	}

	public void StackPopup(ePopupMode mode, params object[] objs)
	{
		PopupInfo menu = new PopupInfo(mode, null, objs);
		mPopupStack.Insert(0, menu);
	}

	PopupBase Load(ePopupMode mode)
	{
		PopupBase result = null;

		//Debug.LogFormat("Load {0}", mode);
		if (mPopupCache.TryGetValue(mode, out result) == true)
		{
			return result;
		}

		string classname = string.Format("Popup{0}", mode.ToString());
		string path = string.Format("Prefab/Popup/{0}", classname);
		//Debug.LogFormat("{0}", path);
		Object loadPrefab = Resources.Load(path, typeof(GameObject));
		if (loadPrefab != null)
		{
			GameObject obj = (Instantiate(loadPrefab) as GameObject);
			obj.transform.parent = contents.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.SetActive(false);

			var popup_base = obj.GetComponent<PopupBase>();
			if (mPopupCache.ContainsKey(mode) == false)
			{
				mPopupCache.Add(mode, popup_base);
			}

			return popup_base;
		}
		else
			Debug.LogError("Resources.Load() failed. " + path);

		return result;
	}

	public void Show(ePopupMode mode, params object[] objs)
	{
		if (Application.isPlaying == false)
			return;

		PopupBase obj = Load(mode);
		if(obj != null)
		{
			if (gameObject.activeInHierarchy == true && m_TWHide.IsPlaying())
			{
				m_TWHide.Stop();
			}

			StackPopup(new PopupInfo(mode, obj, objs));
			Show(true, obj.bShowImmediately);
		}
	}

	public void ShowImmediately(ePopupMode mode, params object[] objs)
	{
		PopupBase obj = Load(mode);
		if (obj != null)
		{
			StackPopup(new PopupInfo(mode, obj, objs));
			Show(true, true);
		}
	}


	public void ShowMessage(string message, params object[] objs)
	{
		Show(ePopupMode.Message, string.Format(message, objs));
	}

	public void ShowMessageKey(string message_key, params object[] objs)
	{
		ShowMessage(Localization.Get(message_key), objs);
	}

	public void ShowConfirm(PopupConfirm.Callback callback, string message, string confirm = "", string cancel = "", params object[] objs)
	{
		if (string.IsNullOrEmpty(confirm) == true) confirm = Localization.Get("Confirm");
		if (string.IsNullOrEmpty(cancel) == true) cancel = Localization.Get("Cancel");
		Show(ePopupMode.Confirm, callback, string.Format(message, objs), confirm, cancel);
	}

	public void ShowConfirmKey(PopupConfirm.Callback callback, string message_key, string confirm_key = "", string cancel_key = "", params object[] objs)
	{
		if (string.IsNullOrEmpty(confirm_key) == true) confirm_key = "Confirm";
		if (string.IsNullOrEmpty(cancel_key) == true) cancel_key = "Cancel";
		ShowConfirm(callback, Localization.Get(message_key), Localization.Get(confirm_key), Localization.Get(cancel_key), objs);
	}

	public void ShowCallback(PopupCallback.Callback callback, string message, params object[] objs)
	{
		Show(ePopupMode.Callback, callback, string.Format(message, objs));
	}

	public void ShowCallbackKey(PopupCallback.Callback callback, string message_key, params object[] objs)
	{
		ShowCallback(callback, Localization.Get(message_key), objs);
	}

	public PopupContainer PopStacks()
	{
		var container = new PopupContainer(mPopupStack);

		GetCurrentPopup().SetActive(false);
		gameObject.SetActive(false);

		mPopupStack.Clear();

		return container;
	}
	
	public void PushStacks(PopupContainer container)
	{
		mPopupStack.AddRange(container.Stacks);

		Show(false, GetCurrentPopup().bShowImmediately);
	}
}
