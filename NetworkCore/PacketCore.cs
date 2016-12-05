using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Reflection;

namespace NetworkCore
{
#if UNITY_5
    public static class TypeExtensions
    {
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
#endif

    public class PacketUtil
    {
        static JsonSerializerSettings m_json_ops = null;

        public static JsonSerializerSettings json_ops
        {
            get
            {
                if (m_json_ops == null)
                {
                    m_json_ops = new JsonSerializerSettings();
                    m_json_ops.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    m_json_ops.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                    m_json_ops.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
                    m_json_ops.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                }
                return m_json_ops;
            }
        }
    }

    public class Packet
    {
        public string Name;
        public string Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public PacketAttribute Attr;
    }

    public class PacketCore : Packet
    {
        public List<PacketPre> PrePackets;
        public List<PacketPost> PostPackets;
    }

    public class PacketPre : Packet
    {
    }

    public class PacketPost : Packet
    {
    }

    public class AckDefault
    {
    }

    public enum PacketLogType
    {
        None,
        Detail,
        Always,
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class PacketAttribute : System.Attribute
    {
        static public PacketAttribute GetAttr(System.Type packetType)
        {
            foreach (object attribute in packetType.GetTypeInfo().GetCustomAttributes(typeof(PacketAttribute), false))
            {
                return (PacketAttribute)attribute;
            }
            throw new System.Exception("packet type is wrong");
        }

        public PacketLogType LogType { get; set; }
        public bool Caching { get; set; }

        public PacketAttribute(PacketLogType log_type = PacketLogType.Detail, bool caching = true)
        {
            LogType = log_type;
            Caching = caching;
        }

        static public bool IsCaching(System.Type packetType)
        {
            foreach (object attribute in packetType.GetTypeInfo().GetCustomAttributes(typeof(PacketAttribute), false))
            {
                return ((PacketAttribute)attribute).Caching;
            }
            return true;
        }

        public bool IsLog(bool log)
        {
            return log == true && LogType != PacketLogType.None || LogType == PacketLogType.Always;
        }
    }

    public class PACKET<T>
    {
        static PacketAttribute m_Attr = null;
        static public PacketAttribute Attr
        {
            get
            {
                if (m_Attr == null)
                {
                    foreach (object attribute in typeof(T).GetTypeInfo().GetCustomAttributes(typeof(PacketAttribute), false))
                    {
                        m_Attr = (PacketAttribute)attribute;
                        break;
                    }
                    if (m_Attr == null)
                        m_Attr = new PacketAttribute();
                }
                return m_Attr;
            }
        }
    };

    public enum PacketErrorType
    {
        Ignore,             // 무시해도 됨
        Retry,              // 재시도 필요
        Title,              // 타이틀화면으로
        SessionExpired,     // 메세지 출력후 타이틀 화면으로
        NeedToUpdate,       // url포함
        Confirm,            // 예/아니오 물어봐야 함. 추후 구현.
        Maintenance,        // 점검중
        UserLimit,          // 인원제한
        Quit,
        ServerForward,      // 서버 이동
        Message,            // 기본 ignore임
        MessageKey,         // 기본 ignore임
        Timeout,            // timeout처리
        Reconnect,          // reconnnect됨 자동 resend처리
    }

    public class PacketError
    {
        public string message;
        public PacketErrorType type;
    }

    public class PacketHandleException : System.Exception
    {
        public PacketErrorType type;

        public PacketHandleException(PacketErrorType type, string message, params object[] args)
            : base(string.Format(message, args))
        {
            this.type = type;
        }

        public PacketCore Packet
        {
            get
            {
                return HandleError(type, Message);
            }
        }

        public static PacketCore HandleError(PacketErrorType error_type, string message, params object[] args)
        {
            PacketCore _ack = new PacketCore();

            PacketError _PacketError = new PacketError();
            _PacketError.type = error_type;
            _PacketError.message = string.Format(message, args);

            _ack.Name = typeof(PacketError).FullName;
            _ack.Data = JsonConvert.SerializeObject(_PacketError, Formatting.None, NetworkCore.PacketUtil.json_ops);

            return _ack;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ProcedureParamListAttribute : System.Attribute
    {
        public string ValueName { get; private set; }
        public int StartIndex { get; private set; }
        public int Count { get; private set; }
        public object InvalidKey { get; private set; }
        public string KeyName { get; private set; }

        public ProcedureParamListAttribute(int startIndex, int count, string keyName, object invalidKey)
        {
            KeyName = keyName;
            StartIndex = startIndex;
            Count = count;
            InvalidKey = invalidKey;

            if (!invalidKey.GetType().GetTypeInfo().IsClass)
                ValueName = keyName;
        }
    }

    public class AttributeGetter<T> where T : System.Attribute
    {
        static public T GetAttribute(System.Reflection.FieldInfo info)
        {
            foreach (object attr in info.GetCustomAttributes(typeof(T), false))
            {
                return (T)attr;
            }
            return null;
        }

        static public T GetAttribute(Type type)
        {
            foreach (object attr in type.GetTypeInfo().GetCustomAttributes(typeof(T), false))
            {
                return (T)attr;
            }
            return null;
        }
    }
}
