using System.IO;
using System.Collections.Generic;

namespace MNS
{
    public class CSVReader
    {
        Dictionary<string, int> m_ColumnHeaders = null;
        public Dictionary<string, int> ColumnHeaders { get { return m_ColumnHeaders; } }

        public class RowData
        {
            public RowData(List<string> datas)
            {
                this.datas = datas;
            }

            List<string> datas;
            public int Count { get { return datas.Count; } }

            public string GetData(int column_index)
            {
                return datas[column_index];
            }
        }
        public List<RowData> Rows { get; private set; }
        public int RowCount { get { return Rows.Count; } }

        public int GetFieldIndex(string field_name)
        {
            int index = -1;
            if (m_ColumnHeaders.TryGetValue(field_name, out index) == false)
                throw new System.Exception(string.Format("not exists column : {0}", field_name));
            return index;
        }

        public RowData GetRow(int row_index)
        {
            return Rows[row_index];
        }

        public string GetData(int row_index, int column_index)
        {
            return GetRow(row_index).GetData(column_index);
        }

        public CSVReader()
        {
        }

        readonly char[] delimiter = ",\"".ToCharArray();

        public void Load(Stream stream)
        {
            m_ColumnHeaders = null;
            Rows = null;

            StreamReader reader = new StreamReader(stream);

            string strLine;
            int findIndex;

            while (reader.EndOfStream == false)
            {
                strLine = reader.ReadLine();
                if (string.IsNullOrEmpty(strLine.Trim()) == true)
                    break;

                string data;
                List<string> datas = new List<string>();

                for (int lastFindIndex = 0; lastFindIndex < strLine.Length; )
                {
                    findIndex = strLine.IndexOfAny(delimiter, lastFindIndex);
                    if (findIndex == -1)
                    {
                        data = strLine.Substring(lastFindIndex);
                        datas.Add(data);
                        break;
                    }
                    else
                    {
                        if (strLine[findIndex] == '\"')
                        {
                            do
                            {
                                findIndex = strLine.IndexOf('\"', findIndex + 1);
                            } while (strLine[findIndex - 1] != '\\');
                            findIndex = strLine.IndexOf('m');
                        }
                        data = strLine.Substring(lastFindIndex, findIndex - lastFindIndex);
                        lastFindIndex = findIndex + 1;
                    }

                    datas.Add(data);
                }
                RowData _RowData = new RowData(datas);

                if (m_ColumnHeaders == null)
                {
                    m_ColumnHeaders = new Dictionary<string, int>();
                    Rows = new List<RowData>();

                    for (int i = 0; i < _RowData.Count; ++i)
                    {
                        m_ColumnHeaders.Add(_RowData.GetData(i), i);
                    }
                }
                else
                    Rows.Add(_RowData);
            }
        }
    }
}