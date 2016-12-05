using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MNS
{
    public abstract class InfoBaseString
    {
        public int IDN { get; protected set; }
        public string ID { get; protected set; }
        override public string ToString() { return ID; }
        public bool IsEnabled { get; protected set; }

        virtual public void Load(XmlNode node)
        {
            IDN = int.Parse(node.Attributes["idn"].Value);
            ID = node.Attributes["id"].Value;

            XmlAttribute enabled_attr = node.Attributes["enabled"];
            if (enabled_attr != null)
                IsEnabled = bool.Parse(enabled_attr.Value);
            else
                IsEnabled = true;
        }
    }

    abstract public class InfoManager<ValueType, BaseType, ManagerType> : MNS.Singleton<ManagerType>
        where BaseType : InfoBaseString
        where ValueType : BaseType, new()
        where ManagerType : class, new()
    {
        protected List<BaseType> m_Infos;
        protected Dictionary<long, BaseType> m_InfosByIdn;
        protected Dictionary<string, BaseType> m_InfosByKey;
        public IEnumerable<BaseType> Values { get { return m_Infos; } }

        public int Count { get { return m_Infos.Count; } }
        public int CountByKey { get { return m_InfosByKey.Count; } }

        public InfoManager()
        {
        }

        virtual protected void PreLoadData(XmlNode node) { }
        virtual protected void PostLoadData(XmlNode node) { }

        public void LoadData(string xml, bool bMerge = false)
        {
            XmlReaderSettings xSetting = new XmlReaderSettings();
            xSetting.IgnoreComments = true;
            xSetting.IgnoreWhitespace = true;

            XmlReader xr = XmlReader.Create(new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)), xSetting);

            XmlDocument doc = new XmlDocument();
            doc.Load(xr);

            if (bMerge == false)
            {
                m_Infos = new List<BaseType>();
                m_InfosByKey = new Dictionary<string, BaseType>();
                m_InfosByIdn = new Dictionary<long, BaseType>();
            }
            XmlNode infoListNode = doc.SelectSingleNode("InfoList");

            PreLoadData(infoListNode);

            foreach (XmlNode infoNode in infoListNode.ChildNodes)
            {
                if (infoNode.NodeType == XmlNodeType.Comment || infoNode.Attributes["ignore"] != null && bool.Parse(infoNode.Attributes["ignore"].Value) == true)
                    continue;

                BaseType value = new ValueType();
                try
                {
                    value.Load(infoNode);
                }
                catch(System.Exception ex)
                {
                    //throw ex;
                    UnityEngine.Debug.LogException(new System.Exception(string.Format("{1} in {0}", typeof(ManagerType).Name, value.ID)));
                    throw ex;
                }

                XmlAttribute overrideAttr = infoNode.Attributes["info_override"];
                if (overrideAttr != null && bool.Parse(overrideAttr.Value) == true)
                {
                    BaseType find_value = m_Infos.Find(i => i.IDN == value.IDN && i.ID == value.ID);
                    if (find_value != null)
                    {
                        m_Infos.Remove(find_value);
                        m_InfosByIdn.Remove(value.IDN);
                        m_InfosByKey.Remove(value.ID.ToLower());
                    }
                    else
                        throw new System.Exception(string.Format("can't find override base value idn : {0}, id: {1})", value.IDN, value.ID));
                }

                //if (value.IDN > lastIdn)
                //    throw new System.Exception(string.Format("idn of {0} is over lastIdn({1})", value.IDN, lastIdn));

                m_Infos.Add(value);

                BaseType tempValue;
                if (m_InfosByIdn.TryGetValue(value.IDN, out tempValue) == true)
                    throw new System.Exception(string.Format("using same idn {0} with {1}", value.ID, tempValue.ID));
                m_InfosByIdn.Add(value.IDN, value);

                if (m_InfosByKey.TryGetValue(value.ID.ToLower(), out tempValue) == true)
                    throw new System.Exception(string.Format("using same id {0}", value.ID));

                m_InfosByKey.Add(value.ID.ToLower(), value);
            }

            PostLoadData(infoListNode);
        }

        virtual public BaseType GetInfoByID(string key)
        {
            BaseType value;
            if (m_InfosByKey.TryGetValue(key.ToLower(), out value) == false)
                throw new System.Exception(string.Format("not exist data {0}", key));

            return value;
        }

        virtual public BaseType GetInfoByIdn(long key)
        {
            BaseType value;
            if (m_InfosByIdn.TryGetValue(key, out value) == false)
                throw new System.Exception(string.Format("not exist data {0}", key.ToString()));

            return value;
        }

        virtual public BaseType GetAt(int index)
        {
            if(Contains(index) == false)
                throw new System.Exception("overflow");

            return m_Infos[index];
        }

        //         public BaseType GetInfo(int index)
        //         {
        //             return m_Infos[index];
        //         }

        public bool Contains(int index) { return index < m_Infos.Count; }
        public bool ContainsIdn(long idn) { return m_InfosByIdn.ContainsKey(idn); }
        virtual public bool ContainsKey(string key) { return m_InfosByKey.ContainsKey(key.ToLower()); }
    }
}