using System;
using System.Data;
using SAP.Middleware.Connector;

namespace SapDataCopyApp
{
    /// <summary>
    /// SAPのテーブルをIDataReaderとして扱うためのクラス
    /// </summary>
    public class SapTableReader : IDataReader
    {
        // カラム名と順番の一覧
        private readonly string[] ColumnNames = new[]
        {
            "CURRENCY",
            "CURRENCY_ISO",
            "ALT_CURR",
            "VALID_TO",
            "LONG_TEXT"
        };

        private readonly IRfcTable _table;
        private int _index = -1;

        public SapTableReader(IRfcTable table)
        {
            _table = table;
        }

        /// <summary>
        /// カラム数。今回は固定で5を返す。
        /// </summary>
        public int FieldCount => 5;

        /// <summary>
        /// カラム番号に応じた値をRfcTableから取得する。
        /// </summary>
        public object GetValue(int i)
        {
            // カラム番号に応じたカラム名の取得
            string columnName = ColumnNames[i];

            // 対象のデータカラムの取得
            IRfcStructure row = _table[_index];
            IRfcField col = row[columnName];

            // カラムの値を取得
            switch (col.Metadata.DataType)
            {
                case RfcDataType.DATE:
                    return col.GetValue();

                default:
                    return col.GetString();
            }
        }

        /// <summary>
        /// 次の行を読み込むために内部のインデックスを増加させる。
        /// </summary>
        public bool Read()
        {
            if (_index >= _table.Count - 1)
            {
                return false;
            }

            _index++;
            return true;
        }

#region Unused

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }

#endregion
    }
}