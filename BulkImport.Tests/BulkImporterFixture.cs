// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using JameyMac.BulkImport;

namespace JameyMac.BulkImport.Tests
{
    [TestFixture]
    public class BulkImporterFixture
    {
        [Test]
        public void Execute_Fills_Table()
        {
            var rowCount = 100000;

            var schema = new
            {
                Id = typeof(long),
                FundName = typeof(string),
                Date = typeof(DateTime),
                Amount = typeof(decimal),
                SiteNumber = typeof(int)
            };

            const string tableName = "Execute_Fills_Table";
            const int siteNumber = 654321;
            var sql = string.Format(@"DELETE FROM {0} WHERE SiteNumber = {1}", tableName, siteNumber);
            var sw = new Stopwatch();
            sw.Start();
            int rowsDeleted = Helpers.ExecuteSql(sql, Helpers.ConnectionString);
            sw.Stop();
            Console.WriteLine(@"{0} Records deleted in: {1}ms", rowsDeleted, sw.ElapsedMilliseconds);

            var stream = Helpers.CreateCsv(rowCount, siteNumber);

            var si = new BulkImporter
                         {
                             ConnectionString = Helpers.ConnectionString,
                             TableName = tableName,
                             Schema = schema,
                             DataStream = stream
                         };
            
            sw.Reset();
            sw.Start();
            si.Execute();
            sw.Stop();
            Console.WriteLine(@"{0} Records imported in: {1}ms", rowCount, sw.ElapsedMilliseconds);
        }
    }
}
// ReSharper restore InconsistentNaming