#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

#endregion

namespace JameyMac.BulkImport.Readers
{
    public enum FileDataReaderHeader
    {
        None = 0,
        DataTypeOnly = 1,
        ColumnNameAndDataType = 2
    }

    public class DelimitedDataReader : IDataReader
    {
        #region FileField Type

        #region Nested type: FileField

        private class FileField
        {
            public string ColumnName;
            public string Raw;
            private Type _dataType;
            private ReaderDataType _rdt;

            public Type DataType
            {
                get { return _dataType; }
                set
                {
                    _dataType = value;
                    _rdt = ReaderDataType.RNone;
                    if (DataType == typeof (string))
                        _rdt = ReaderDataType.RString;
                    if (DataType == typeof (int))
                        _rdt = ReaderDataType.RInt;
                    if (DataType == typeof (long))
                        _rdt = ReaderDataType.RLong;
                    if (DataType == typeof (decimal))
                        _rdt = ReaderDataType.RDecimal;
                    if (DataType == typeof (Guid))
                        _rdt = ReaderDataType.RGuid;
                    if (DataType == typeof (DateTime))
                        _rdt = ReaderDataType.RDateTime;
                }
            }

            public object Data
            {
                get
                {
                    try
                    {
                        if (String.IsNullOrEmpty(Raw))
                            return null;

                        switch (_rdt)
                        {
                            case ReaderDataType.RString:
                                return Raw;
                            case ReaderDataType.RInt:
                                return int.Parse(Raw);
                            case ReaderDataType.RLong:
                                return long.Parse(Raw);
                            case ReaderDataType.RDecimal:
                                return decimal.Parse(Raw);
                            case ReaderDataType.RDateTime:
                                return DateTime.Parse(Raw);
                            case ReaderDataType.RGuid:
                                return Guid.Parse(Raw);
                            default:
                                return Convert.ChangeType(Raw, DataType);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new DelimitedDataReaderException(
                            string.Format(@"DelimitedDataReader cannot convert the text ""{0}"" into the Type {1}",
                                          _dataType, Raw), ex);
                    }
                }
            }
        }

        #endregion

        #region Nested type: ReaderDataType

        private enum ReaderDataType
        {
            RNone,
            RString,
            RInt,
            RLong,
            RDecimal,
            RDateTime,
            RGuid
        }

        #endregion

        #endregion

        #region Private Fields

        private readonly char _columnDelimiter;
        private readonly List<FileField> _data;
        private readonly StreamReader _streamReader;
        private bool _isClosed;
        private int _lineNumber;

        #endregion

        #region Support

        public IEnumerable<string> ColumnNames
        {
            get { return _data.Select(i => i.ColumnName); }
        }

        public static DelimitedDataReader Create(Stream stream, object schema, bool skipHeaderRow)
        {
            return new DelimitedDataReader(new StreamReader(stream), ',', schema, skipHeaderRow);
        }

        #endregion

        #region Contructors

        public DelimitedDataReader(StreamReader streamReader, char columnDelimiter, object schema,
                                   bool skipHeaderRow = false)
        {
            // skip the first row if we have a header
            if (skipHeaderRow)
            {
                streamReader.ReadLine();
            }
            _columnDelimiter = columnDelimiter;
            _streamReader = streamReader;
            _isClosed = false;
            _data = BuildSchema(schema);
        }

        #endregion

        #region Private Methods

        private bool Eof
        {
            get { return _streamReader.EndOfStream; }
        }

        private static FileField GetFileField(string columnName, Type dataType)
        {
            return new FileField {ColumnName = columnName, DataType = dataType};
        }

        private static List<FileField> BuildSchema(object schema)
        {
            var d = new Dictionary<string, Type>();

            if (schema != null)
            {
                var props = TypeDescriptor.GetProperties(schema);
                foreach (PropertyDescriptor prop in props)
                {
                    var val = prop.GetValue(schema) as Type;
                    d.Add(prop.Name, val);
                }
            }

            return BuildSchemaFromDictionary(d);
        }

        private static List<FileField> BuildSchemaFromDictionary(Dictionary<string, Type> schema)
        {
            return schema.Select(pair => GetFileField(pair.Key, pair.Value)).ToList();
        }

        private bool ReadRecord()
        {
            if (Eof)
                return false;

            var fields = SplitString(_streamReader.ReadLine());
            _lineNumber++;
            if (fields.Length != _data.Count)
                throw new Exception(String.Format("Header and Record Column count do not match (Line {0}).", _lineNumber));

            for (var i = 0; i < _data.Count; i++)
            {
                _data[i].Raw = fields[i];
            }

            return true;
        }

        private string[] SplitString(string line)
        {
            return line.SplitDelimited(_columnDelimiter, FieldCount);
        }

        private void ClearData()
        {
            foreach (var t in _data)
            {
                t.Raw = null;
            }
        }

        #endregion

        #region IDataReader Members

        public void Dispose()
        {
            _streamReader.Close();
            _streamReader.Dispose();
        }

        public void Close()
        {
            _isClosed = true;
            _lineNumber = 0;
            _streamReader.Close();
            ClearData();
        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsClosed
        {
            get { return _isClosed; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return ReadRecord();
        }

        public int RecordsAffected
        {
            get { return -1; }
        }

        public int FieldCount
        {
            get { return _data.Count; }
        }

        public bool GetBoolean(int index)
        {
            return Convert.ToBoolean(_data[index].Data);
        }

        public byte GetByte(int index)
        {
            return Convert.ToByte(_data[index].Data);
        }

        public long GetBytes(int index, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public char GetChar(int index)
        {
            return Convert.ToChar(_data[index].Data);
        }

        public long GetChars(int index, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDataReader GetData(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetDataTypeName(int index)
        {
            return GetFieldType(index).FullName;
        }

        public DateTime GetDateTime(int index)
        {
            return Convert.ToDateTime(_data[index].Data);
        }

        public decimal GetDecimal(int index)
        {
            return Convert.ToDecimal(_data[index].Data);
        }

        public double GetDouble(int index)
        {
            return Convert.ToDouble(_data[index].Data);
        }

        public Type GetFieldType(int index)
        {
            return _data[index].DataType;
        }

        public float GetFloat(int index)
        {
            return Convert.ToSingle(_data[index].Data);
        }

        public Guid GetGuid(int index)
        {
            return (Guid) _data[index].Data;
        }

        public short GetInt16(int index)
        {
            return Convert.ToInt16(_data[index].Data);
        }

        public int GetInt32(int index)
        {
            return Convert.ToInt32(_data[index].Data);
        }

        public long GetInt64(int index)
        {
            return Convert.ToInt64(_data[index].Data);
        }

        public string GetName(int index)
        {
            return _data[index].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < _data.Count; i++)
            {
                if (name == _data[i].ColumnName)
                    return i;
            }

            return -1;
        }

        public string GetString(int index)
        {
            return Convert.ToString(_data[index].Data);
        }

        public object GetValue(int index)
        {
            return _data[index].Data;
        }

        public int GetValues(object[] values)
        {
            var n = Math.Min(values.Length, _data.Count);

            for (var index = 0; index < n; index++)
                values[index] = GetValue(index);

            return n;
        }

        public bool IsDBNull(int index)
        {
            return _data[index].Data == null;
        }

        public object this[string name]
        {
            get
            {
                var i = GetOrdinal(name);
                return GetValue(i);
            }
        }

        public object this[int index]
        {
            get { return GetValue(index); }
        }

        #endregion
    }
}