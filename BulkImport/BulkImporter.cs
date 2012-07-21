#region

using System.Data.SqlClient;
using System.IO;
using System.Linq;
using JameyMac.BulkImport.Readers;

#endregion

namespace JameyMac.BulkImport
{
    public class BulkImporter
    {
        public BulkImporter()
        {
            ImportTimeout = 60*5;
            SkipHeaderRow = true;
        }

        public bool SkipHeaderRow { get; set; }
        public int ImportTimeout { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public Stream DataStream { get; set; }
        public object Schema { get; set; }

        public void Execute()
        {
            using (var sbc = new SqlBulkCopy(ConnectionString))
            {
                var reader = DelimitedDataReader.Create(DataStream, Schema, SkipHeaderRow);
                sbc.DestinationTableName = TableName;
                reader.ColumnNames.ToList().ForEach(i => sbc.ColumnMappings.Add(i, i));
                sbc.BulkCopyTimeout = ImportTimeout;
                sbc.WriteToServer(reader);
            }
        }
    }
}