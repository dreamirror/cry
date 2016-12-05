using MNS;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MNS
{
    public interface CSVData<T>
    {
        T Key { get; }
        void Load(CSVReader reader, CSVReader.RowData row);
    }

    abstract public class InfoManagerCSV<T, TKey, TData> : MNS.Singleton<T>
        where T : class, new()
        where TData : CSVData<TKey>, new()
    {
        protected Dictionary<TKey, TData> m_Datas;
        protected List<TData> m_List;

        public void LoadData(string csv)
        {
            Debug.LogFormat("Load CSV : {0}", typeof(T).FullName);

            CSVReader reader = new CSVReader();
            reader.Load(new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(csv)));

            m_Datas = new Dictionary<TKey, TData>();
            m_List = new List<TData>();
            foreach (var row in reader.Rows)
            {
                TData data = new TData();
                data.Load(reader, row);
                m_Datas.Add(data.Key, data);
                m_List.Add(data);
            }
        }

    }
}