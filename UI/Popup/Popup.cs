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

public abstract class PopupBase : MonoBehaviour, IPopup //这是一个抽象的类 是一个窗口的基础类
{
	public bool ExteralClose;

	protected Popup mParent; //父节点
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
			m_TWShow.Play(true); //播放一个窗口的效果
		}
	}
	
	virtual public void OnFinishedTweenScale() //下面的一些列都是NGUI的函数
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

	void InitCurrentPopup(bool is_new) //初始化当前的弹出框
	{
		PopupInfo info = GetCurrentPopup(); //得到当前的弹出框
		IPopup popup = info.Obj.GetComponent<IPopup>(); //得到接口
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

	public void Show(bool is_new = false, bool bImmediately = false) //展现弹出框
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

	public void Close(bool bForce = false, bool bImmediately = false) //关闭弹出框
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

	public class PopupInfo //弹出窗口的信息 相当于把一个窗口的几个信息 包括有mode obj 和 parms 都整合到一起
	{
		public ePopupMode mode; //窗口的类型
		PopupBase obj; //床窗口的基础类
		public bool bExternalClose = false;
		public object[] parms;

		public PopupBase Obj //窗口的基础类
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

		public PopupInfo(ePopupMode _mode, PopupBase _obj, params object[] _parms) //构造函数
		{
			mode = _mode;
			obj = _obj;
			parms = _parms;
		}
		public void SetActive(bool active) //将弹出框激活
		{
			if(active == false)
				Obj.OnFinishedHide(); //将弹出框显示出来
			Obj.gameObject.SetActive(active); //
		}
		public bool bShowImmediately { get { return obj == null ? false : obj.bShowImmediately; } }
	}

	public List<PopupInfo> mPopupStack = new List<PopupInfo>(); //管理弹出框的栈
	public Dictionary<ePopupMode, PopupBase> mPopupCache = new Dictionary<ePopupMode, PopupBase>(); //弹出框的字典
	public PopupInfo GetCurrentPopup() //从栈中取出当前的弹出框
	{
		PopupInfo result = null;
		if (mPopupStack.Count > 0)
		{
			result = mPopupStack[0];
		}
		return result;
	}
	PopupInfo PopPopup() //移除栈顶的窗口
	{
		PopupInfo result = null;
		if (mPopupStack.Count > 0)
		{
			result = mPopupStack[0];
			mPopupStack.RemoveAt(0);
		}
		return result;
	}
	void StackPopup(PopupInfo menu) //入栈
	{
		PopupInfo current = GetCurrentPopup();
		if(current != null)
		{
			current.SetActive(false);
		}
		mPopupStack.Insert(0, menu);
	}

	public void StackPopup(ePopupMode mode, params object[] objs) //入栈
	{
		PopupInfo menu = new PopupInfo(mode, null, objs);
		mPopupStack.Insert(0, menu);
	}

	PopupBase Load(ePopupMode mode) //加载弹出框
	{
		PopupBase result = null;

		//Debug.LogFormat("Load {0}", mode);
		if (mPopupCache.TryGetValue(mode, out result) == true) //如果字典中已经有了就直接返回
		{
			return result;
		}

		string classname = string.Format("Popup{0}", mode.ToString()); //根据不同的mode获取不停的预设体来创建弹出框
		string path = string.Format("Prefab/Popup/{0}", classname); //生成路径
		//Debug.LogFormat("{0}", path);
		Object loadPrefab = Resources.Load(path, typeof(GameObject)); //加载预设体
		if (loadPrefab != null)
		{
            //设置预设体的信息
			GameObject obj = (Instantiate(loadPrefab) as GameObject); //实例化一个弹出框 并把它转换为GameObj类型
			obj.transform.parent = contents.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.SetActive(false);

			var popup_base = obj.GetComponent<PopupBase>(); //为啥能得到这个控件
			if (mPopupCache.ContainsKey(mode) == false) //如果没有这个类型的弹出框
			{
				mPopupCache.Add(mode, popup_base); //讲预设体和对应的mode名称存入字典当中
			}

			return popup_base;
		}
		else
			Debug.LogError("Resources.Load() failed. " + path);

		return result;
	}

	public void Show(ePopupMode mode, params object[] objs) //重载了上面的show
	{
		if (Application.isPlaying == false)
			return;

        //在这里只做了 停止当前的弹出框动画以及 讲现在的弹出框入栈的操作上面的show才是真正的show操作
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

	public PopupContainer PopStacks() //讲栈中元素全部弹出
	{
		var container = new PopupContainer(mPopupStack);

		GetCurrentPopup().SetActive(false);
		gameObject.SetActive(false);

		mPopupStack.Clear();

		return container;
	}
	
	public void PushStacks(PopupContainer container)
	{
		mPopupStack.AddRange(container.Stacks); //讲contaimer这个列表放到mPopupStack 的末尾

		Show(false, GetCurrentPopup().bShowImmediately);
	}
}
