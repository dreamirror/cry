//#define USE_LOG
using BestHTTP;
using System;
using System.Reflection;
using UnityEngine;

public delegate void OnPostCallback();

public class Networking : MonoBehaviour
{
    static Networking m_Instance = null;
    static public Networking Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = ((GameObject)GameObject.Instantiate(Resources.Load("Prefab/Networking"))).GetComponent<Networking>();
                GameObject.DontDestroyOnLoad(m_Instance.gameObject);
                m_Instance.gameObject.SetActive(false);
            }
            return m_Instance;
        }
    }

    public float showNetworkingDelay = 0f;

    public GameObject contents;

    public UIPlayTween m_TWBlock;
    public UIPlayTween m_TWImmediately;

    public UILabel m_LabelTooltip;

    float m_showNetworkTime = 0f;

    void OnEnable()
    {
#if USE_LOG
        Debug.Log("Networking.Active");
#endif
        m_LabelTooltip.text = Localization.Get("OnNetworking");
    }

	void Update () 
    {
        if(m_showNetworkTime != 0f && m_showNetworkTime < Time.time)
        {
#if USE_LOG
            Debug.Log("Networking.Show");
#endif
            m_LabelTooltip.gameObject.SetActive(true);
        }
	}

    //public void JsonAsyncForGameServer<SendT, RecvT>(SendT packet, Action<SendT, RecvT> callback)
    //    where SendT : class
    //    where RecvT : class
    //{
    //    JsonAsync(Network.GameServer, packet, callback);
    //}

    //public void JsonAsync<SendT, RecvT>(CS_Client server, SendT packet, Action<SendT, RecvT> callback)
    //    where SendT : class
    //    where RecvT : class
    //{
    //    if (server == null) return;

    //    gameObject.SetActive(true);
    //    m_showNetworkTime = Time.time + showNetworkingDelay;
    //    server.JsonAsync(packet, callback, OnCallbackPost);
    //}

    //public void ResendPacket(CS_Client server, HTTPRequest request)
    //{
    //    gameObject.SetActive(true);
    //    m_showNetworkTime = Time.time + showNetworkingDelay;
    //    server.ResendPacket(request);
    //}

    public void OnPostCallback()
    {//unblock input
        //if (GameMain.destory)
        //    return;

        m_LabelTooltip.gameObject.SetActive(false);
        gameObject.SetActive(false);
        m_showNetworkTime = 0f;

#if USE_LOG
        Debug.Log("Networking.Deactive");
#endif
    }

    public void Block()
    {//block input
#if USE_LOG
        Debug.Log("Networking.Block");
#endif
        gameObject.SetActive(true);
        m_showNetworkTime = Time.time + showNetworkingDelay;
    }

    public void BlockDirect()
    {//block input
#if USE_LOG
        Debug.Log("Networking.BlockDirect");
#endif
        gameObject.SetActive(true);
        m_showNetworkTime = Time.time;
    }
}
