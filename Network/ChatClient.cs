//using UnityEngine;
//using System.Collections;
//using BestHTTP;
//using BestHTTP.WebSocket;
//using System;

//public class ChatClient
//{
//    WebSocket m_Socket;

//    public void Init()
//    {
//        m_Socket = new WebSocket(new Uri("ws://192.168.0.51:3200/chat"));
//        m_Socket.OnMessage += OnMessage;
//        m_Socket.OnOpen += OnOpen;
//        m_Socket.OnClosed += OnClosed;
//        m_Socket.OnError += OnError;

//        m_Socket.Open();
//    }

//    void OnMessage(WebSocket socket, string message)
//    {
//        Debug.Log(message);
//    }

//    void OnOpen(WebSocket socket)
//    {
//        Debug.Log("Open");
//        m_Socket.Send("AAA");
////        m_Socket.Close();
//    }

//    void OnClosed(WebSocket webSocket, UInt16 code, string message)
//    {
//        Debug.Log("Closed : "+message);
//    }

//    void OnError(WebSocket webSocket, Exception ex)
//    {
//        Debug.LogError(ex.ToString());
//    }
//}
