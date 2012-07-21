using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace JameyMac.BulkImport.Tests
{
    public class TestEntity
    {
        public long Id { get; set; }
        public string FundName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public static class Helpers
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["SQLSERVER_CONNECTION_STRING"].ConnectionString;
                return cs;
            }
        }

        public static int ExecuteSql(string sql, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                return cmd.ExecuteNonQuery();
            }
        }

        public static void DropTable(string tableName, string connectionString)
        {
            var sql = string.Format("DROP TABLE {0};", tableName);
            ExecuteSql(sql, connectionString);
        }

        public static void TruncateTable(string tableName, string connectionString)
        {
            var sql = string.Format("TRUNCATE TABLE {0};", tableName);
            ExecuteSql(sql, connectionString);
        }

        public static Stream CreateCsv(int size, int siteNumber = 123456, List<TestEntity> testRecords = null)
        {
            var f = @"""{0}"",""{1}"",""{2}"",""{3}"",""" + siteNumber.ToString() + @"""";
            if (testRecords == null)
            {
                testRecords = new List<TestEntity>();
            }
            return InternalCreateCsv(size, testRecords, f);
        }

        public static Stream CreateMixedCsv(int size, List<TestEntity> testRecords = null)
        {
            const string f = @"{0},""{1}"",{2},{3},123456";
            if (testRecords == null)
            {
                testRecords = new List<TestEntity>();
            }
            return InternalCreateCsv(size, testRecords, f);
        }

        private static Stream InternalCreateCsv(int size, List<TestEntity> testRecords, string formatString)
        {
            var dateFraction = -365 * 2 / size;
            var s = new MemoryStream();
            var w = new StreamWriter(s);
            w.WriteLine(@"""id"",""FundName"",""Date"",""Amount"",""SiteNumber""");
            for (int i = 0; i < size; i++)
            {
                var dateOffset = dateFraction * i;
                var id = (long)i + 1;
                var fundName = i % 7 == 0 ? "Missions" : "General Fund";
                var date = DateTime.Now.AddDays(dateOffset).Date;
                var amount = (decimal)((i % 999.99) + 1.0);
                testRecords.Add(new TestEntity { Id = id, FundName = fundName, Date = date, Amount = amount });
                w.WriteLine(formatString, id, fundName, date.ToShortDateString(), amount);
            }
            w.Flush();
            s.Position = 0;
            return s;
        }
    }
}
