using UnityEngine;
using System.Collections;

public static class TimeManagerAutoInitialize
{
	[RuntimeInitializeOnLoadMethod] //表示在运行的时候会自动执行这个方法
	public static void OnLoad()
	{
		if (TimeManager.Instance == null)
		{
			new GameObject("TimeManager", typeof(TimeManager)); //创建一个gameobj并将TimeManager 绑定上去
		}
	}
}

[DisallowMultipleComponent] 
public class TimeManager : MonoBehaviour
{
	float m_TimeScale = 1f;
	public bool IsPause { get; private set; }
	public bool IsBoost { get; private set; }

	static TimeManager m_Instance;
	static public TimeManager Instance //获取实例
	{
		get
		{
			return m_Instance;
		}
	}

	void Awake()
	{
		m_Instance = this;
		GameObject.DontDestroyOnLoad(gameObject);
	}

	// Update is called once per frame
	void Update ()
	{
#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.LeftShift))
		{
			if (IsBoost == false)
			{
				IsBoost = true;
				RefreshTimeScale();
			}
		}
		else if (IsBoost == true)
		{
			IsBoost = false;
			RefreshTimeScale();
		}
#endif
	}

	public void SetPause(bool pause)
	{
		IsPause = pause;
		RefreshTimeScale();
	}

	public void SetTimeScale(float time_scale)
	{
		m_TimeScale = time_scale;
		RefreshTimeScale();
	}

	public void ResetTimeScale()
	{
		SetTimeScale(1f);
	}

	void RefreshTimeScale()
	{
		if (IsPause == true)
		{
			Time.timeScale = 0f;
			AudioListener.pause = true;
		}
		else
		{
			if (IsBoost == true)
				Time.timeScale = 5f;
			else
				Time.timeScale = m_TimeScale;
			AudioListener.pause = false;
		}
	}
}
