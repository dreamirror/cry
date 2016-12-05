using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetworkCore;
using Newtonsoft.Json;
using System;
using BestHTTP;
using System.Net;

public class CS_Client
{
    System.Uri using_server_uri = null;
    string server_uri;
    string packet_namespace;
    public OnPostCallback PostCallback { get; set; }
    public int SendIndex { get; private set; }

    public bool IsInit { get { return using_server_uri != null; } }
    public bool IsConnected { get { return AccessToken != 0; } }

    public long AccountIdx { get; private set; }
    public int AccessToken { get; private set; }
    public int ReconnectIndex { get; private set; }

    Dictionary<string, string> m_Headers = new Dictionary<string, string>();

    public interface IHandler
    {
        void handle(HTTPRequest request, HTTPResponse response);
        string Name { get; }
    }

    public class Handler<SendT, RecvT> : IHandler
        where SendT : class
        where RecvT : class
    {
        System.Action<SendT, RecvT> callback;
        CS_Client client;
        SendT send_packet;

        public Handler(CS_Client client, System.Action<SendT, RecvT> callback, SendT send_packet)
        {
            this.client = client;
            this.callback = callback;
            this.send_packet = send_packet;
        }

        public string Name { get { return typeof(RecvT).FullName; } }

        public void handle(HTTPRequest request, HTTPResponse response)
        {
            if (request.State != HTTPRequestStates.Finished)
            {
                //                 if (request.Exception is System.Net.Sockets.SocketException)
                //                 {
                //                     System.Net.Sockets.SocketException socket_exception = request.Exception as System.Net.Sockets.SocketException;
                //                     client.handle_error(Name, socket_exception.SocketErrorCode.ToString(), "Network Error");
                //                 }
                //                 else
                {
                    Debug.LogWarningFormat("Error in {0} : [ServerError:{1}] {2}", Name, request.State, request.Exception != null ? request.Exception.Message : "Network Error");

                    client.handle_error(Name, request, null);
                }
                return;
            }

            if (response.IsSuccess == false)
            {
//                 switch(response.StatusCode)
//                 {
//                     case 500:
//                         return;
//                 }
                Debug.LogWarningFormat("Error in {0} : [ServerError:{1}] {2}", Name, response.StatusCode, response.Message);
                client.handle_error(Name, request, response);
                return;
            }

            PacketCore packet_core = JsonConvert.DeserializeObject<PacketCore>(response.DataAsText, NetworkCore.PacketUtil.json_ops);
            if (packet_core.Name != typeof(RecvT).FullName)
            {
                if (client.HandleCommon(request, send_packet, packet_core) == true)
                {
                    return;
                }
                else
                {
                    if (client.PostCallback != null) client.PostCallback();
                    throw new System.Exception(string.Format("packet name({0}) != {1}", packet_core.Name, typeof(RecvT).FullName));
                }
            }

            if (packet_core.PrePackets != null && packet_core.PrePackets.Count > 0)
            {
                foreach (var pre_packet in packet_core.PrePackets)
                {
                    Debug.LogFormat("[PacketPre:{0}] {1}", pre_packet.Name, pre_packet.Data);
                    if (client.HandlePre(request, send_packet, pre_packet) == false)
                    {
                        Debug.LogErrorFormat("can't find pre packet handler : {0}", pre_packet.Name);
                    }
                }
            }

            if (packet_core.Data.Length > 1000)
                Debug.Log(string.Format("[Packet:{0}] {1}", typeof(RecvT).FullName, packet_core.Data.Substring(0, 1000)+"...."));
            else
                Debug.Log(string.Format("[Packet:{0}] {1}", typeof(RecvT).FullName, packet_core.Data));
            if (callback != null)
            {
                if (client.PostCallback != null) client.PostCallback();
                callback(send_packet, JsonConvert.DeserializeObject<RecvT>(packet_core.Data, NetworkCore.PacketUtil.json_ops));
            }

            if (packet_core.PostPackets != null && packet_core.PostPackets.Count > 0)
            {
                foreach (var post_packet in packet_core.PostPackets)
                {
                    if (client.HandlePost(request, send_packet, post_packet) == false)
                    {
                        Debug.LogErrorFormat("can't find post packet handler : {0}", post_packet.Name);
                    }
                    else
                    {
                        Debug.LogFormat("[PacketPost:{0}] {1}", post_packet.Name, post_packet.Data);
                    }
                }
            }

        }
    }

    interface ICommonHandler
    {
        void handle(CS_Client server, HTTPRequest request, object send_packet, Packet packet_core);
        string Name { get; }
    }

    public class CommonHandler<RecvT> : ICommonHandler
        where RecvT : class
    {
        System.Action<CS_Client, HTTPRequest, object, RecvT> callback;
        public CS_Client client { get; private set; }

        public CommonHandler(CS_Client client, System.Action<CS_Client, HTTPRequest, object, RecvT> callback)
        {
            this.client = client;
            this.callback = callback;
        }

        public string Name { get { return typeof(RecvT).FullName; } }

        public void handle(CS_Client server, HTTPRequest request, object send_packet, Packet packet_core)
        {
            callback(server, request, send_packet, JsonConvert.DeserializeObject<RecvT>(packet_core.Data, NetworkCore.PacketUtil.json_ops));
        }
    }

    System.Action<CS_Client, string, HTTPRequest, HTTPResponse> error_callback = null;
    void handle_error(string name, HTTPRequest request, HTTPResponse response)
    {
        if (error_callback != null)
            error_callback(this, name, request, response);
        if (PostCallback != null)
            PostCallback();
    }

    Dictionary<string, ICommonHandler> m_CommonHandlers = new Dictionary<string, ICommonHandler>();
    public void AddCommonHandler<RecvT>(Action<CS_Client, HTTPRequest, object, RecvT> callback) where RecvT : class
    {
        m_CommonHandlers.Add(typeof(RecvT).FullName, new CommonHandler<RecvT>(this, callback));
    }

    Dictionary<string, ICommonHandler> m_PreHandlers = new Dictionary<string, ICommonHandler>();
    public void AddPreHandler<RecvT>(Action<CS_Client, HTTPRequest, object, RecvT> callback) where RecvT : class
    {
        m_PreHandlers.Add(typeof(RecvT).FullName, new CommonHandler<RecvT>(this, callback));
    }

    Dictionary<string, ICommonHandler> m_PostHandlers = new Dictionary<string, ICommonHandler>();
    public void AddPostHandler<RecvT>(Action<CS_Client, HTTPRequest, object, RecvT> callback) where RecvT : class
    {
        m_PostHandlers.Add(typeof(RecvT).FullName, new CommonHandler<RecvT>(this, callback));
    }

    public void InitPacketNamespace(string packet_namespace)
    {
        this.packet_namespace = packet_namespace;
        SetUri();
    }

    public void InitUri(string server_uri)
    {
        this.server_uri = server_uri;
        SetUri();
    }

    System.Uri MakeUri(string server_uri, string packet_namespace)
    {
        return new System.Uri(string.Format("{0}/api/{1}_/", server_uri, packet_namespace));
    }

    void SetUri()
    {
        using_server_uri = MakeUri(server_uri, packet_namespace);
        Debug.LogWarning(string.Format("[CS_Client] SetUri : {0}", using_server_uri));
    }

    public void InitServer(long account_idx, string server_uri, string packet_namespace, System.Action<CS_Client, string, HTTPRequest, HTTPResponse> error_callback)
    {
        this.error_callback = error_callback;
        this.packet_namespace = packet_namespace;
        InitUri(server_uri);

        ClearSession();

        m_Headers.Remove("account_idx");
        m_Headers.Add("account_idx", account_idx.ToString());
        AccountIdx = account_idx;

        HTTPManager.IsCachingDisabled = true;
        HTTPManager.IsCookiesEnabled = false;
        HTTPManager.ConnectTimeout = TimeSpan.FromSeconds(5f);
        HTTPManager.RequestTimeout = TimeSpan.FromSeconds(5f);
    }

    public void InitAccountIdx(long account_idx)
    {
        Debug.LogWarning(string.Format("[CS_Client] InitAccountIdx : {0}", account_idx));

        m_Headers.Remove("account_idx");
        m_Headers.Add("account_idx", account_idx.ToString());
        AccountIdx = account_idx;
    }

    public void InitReconnectIndex(int reconnect_index)
    {
        Debug.LogWarning(string.Format("[CS_Client] InitReconnectIndex : {0}", reconnect_index));

        m_Headers.Remove("reconnect_index");
        m_Headers.Add("reconnect_index", reconnect_index.ToString());
        ReconnectIndex = reconnect_index;
    }

    public void InitSession(long account_idx, int access_token, int reconnect_index)
    {
        Debug.LogWarning(string.Format("[CS_Client] InitSession : {0}, {1}", account_idx, access_token));

        SendIndex = 1;

        m_Headers.Remove("account_idx");
        m_Headers.Add("account_idx", account_idx.ToString());
        AccountIdx = account_idx;

        m_Headers.Remove("access_token");
        m_Headers.Add("access_token", access_token.ToString());
        AccessToken = access_token;

        m_Headers.Remove("reconnect_index");
        m_Headers.Add("reconnect_index", reconnect_index.ToString());
        ReconnectIndex = reconnect_index;
    }

    public void ClearSession()
    {
        m_Headers.Clear();
        m_Headers.Add("Content-Type", "application/json");
        m_Headers.Add("User-Agent", "SH_Client");
        AccountIdx = 0;
        AccessToken = 0;
    }

    public CS_Client()
    {
    }

    public void JsonAsyncNamespace<SendT, RecvT>(string packet_namespace, SendT packet, Action<SendT, RecvT> callback)
        where SendT : class
        where RecvT : class
    {
        JsonAsyncInternal<SendT, RecvT>(MakeUri(server_uri, packet_namespace), packet_namespace, packet, callback, false);
    }

    public void JsonAsync<SendT, RecvT>(SendT packet, Action<SendT, RecvT> callback)
        where SendT : class
        where RecvT : class
    {
        JsonAsyncInternal<SendT, RecvT>(using_server_uri, packet_namespace, packet, callback, true);
    }

    void JsonAsyncInternal<SendT, RecvT>(System.Uri uri, string packet_namespace, SendT packet, Action<SendT, RecvT> callback, bool use_crc)
        where SendT : class
        where RecvT : class
    {
        if (uri == null)
            throw new System.Exception("Please call CS_Client.InitServer");

        PacketCore _PacketCore = new PacketCore();
        _PacketCore.Name = typeof(SendT).FullName;

        if (_PacketCore.Name.StartsWith(packet_namespace) == false)
            throw new System.Exception(string.Format("Packet namesapce fault({0}) : {1}", packet_namespace, _PacketCore.Name));

        _PacketCore.Data = JsonConvert.SerializeObject(packet, Formatting.None, NetworkCore.PacketUtil.json_ops);

        HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Post, new Handler<SendT, RecvT>(this, callback, packet).handle);

        foreach (var header in m_Headers)
            request.AddHeader(header.Key, header.Value);
        request.RawData = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_PacketCore, Formatting.None, NetworkCore.PacketUtil.json_ops));

        Debug.Log(string.Format("[Packet:{0},{2}] {1}", _PacketCore.Name, _PacketCore.Data, SendIndex));
        if (PostCallback != null && callback != null)
            Networking.Instance.Block();

        if (use_crc == true && SendIndex > 0)
        {
            uint checksum = Octodiff.Core.Adler32RollingChecksum.CalculateChecksum(System.Text.Encoding.UTF8.GetBytes(_PacketCore.Name));
            checksum = Octodiff.Core.Adler32RollingChecksum.Add(checksum, System.Text.Encoding.UTF8.GetBytes(_PacketCore.Data));
            checksum = Octodiff.Core.Adler32RollingChecksum.Add(checksum, BitConverter.GetBytes(AccountIdx));
            checksum = Octodiff.Core.Adler32RollingChecksum.Add(checksum, BitConverter.GetBytes(AccessToken));
            checksum = Octodiff.Core.Adler32RollingChecksum.Add(checksum, BitConverter.GetBytes(SendIndex));
            checksum = Octodiff.Core.Adler32RollingChecksum.Add(checksum, BitConverter.GetBytes(checksum));
            request.AddHeader("Checksum", checksum.ToString());
            ++SendIndex;
        }

        request.Send();
    }

    public void ResendPacket(HTTPRequest request)
    {
        if(PostCallback != null)
            Networking.Instance.BlockDirect();
        request.RemoveHeader("resend_index");
        request.AddHeader("resend_index", (SendIndex-1).ToString());
        Debug.Log(string.Format("[Resend] {0}", System.Text.Encoding.UTF8.GetString(request.RawData)));
        request.Send();
    }

    public bool HandleCommon(HTTPRequest request, object send_packet, Packet packet)
    {
        ICommonHandler handler;
        if (m_CommonHandlers.TryGetValue(packet.Name, out handler) == true)
        {
            if (PostCallback != null) PostCallback();
            handler.handle(this, request, send_packet, packet);
            return true;
        }
        return false;
    }

    public bool HandlePre(HTTPRequest request, object send_packet, Packet packet)
    {
        ICommonHandler handler;
        if (m_PreHandlers.TryGetValue(packet.Name, out handler) == true)
        {
            if (PostCallback != null) PostCallback();
            handler.handle(this, request, send_packet, packet);
            return true;
        }
        return false;
    }

    public bool HandlePost(HTTPRequest request, object send_packet, Packet packet)
    {
        ICommonHandler handler;
        if (m_PostHandlers.TryGetValue(packet.Name, out handler) == true)
        {
            if (PostCallback != null) PostCallback();
            handler.handle(this, request, send_packet, packet);
            return true;
        }
        return false;
    }

}
