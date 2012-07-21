using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using JameyMac.BulkImport.Readers;

namespace JameyMac.BulkImport.Tests.Readers
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class DelimitedDataReaderFixture
    {
        [Test]
        public void CsvDataReader_Parses_CSV_File()
        {
            var testRecords = new List<TestRecord>();
            var stream = CreateCsv(1000, testRecords);
            if (stream == null) return;
            using (var r = DelimitedDataReader.Create(stream,
                new
                {
                    Id = typeof(long),
                    FundName = typeof(string),
                    Date = typeof(DateTime),
                    Amount = typeof(decimal)
                }, true))
            {
                // Make sure we have correct # of records from parser
                var actualRowCount = 0;
                const int expectedRowCount = 1000;
                while (r.Read())
                {
                    actualRowCount++;
                    Assert.AreEqual(typeof(long), r["Id"].GetType());
                    Assert.AreEqual(typeof(string), r["FundName"].GetType());
                    Assert.AreEqual(typeof(DateTime), r["Date"].GetType());
                    Assert.AreEqual(typeof(decimal), r["Amount"].GetType());
                }
                Assert.AreEqual(expectedRowCount, actualRowCount);
            }
        }

        [Test]
        public void StreamDataReader_Can_Read_Big_Csv_File()
        {
            const int size = 10000; // 10k
            var testRecords = new List<TestRecord>();
            var stream = CreateCsv(size, testRecords);
            var sw = new Stopwatch();
            sw.Start();
            TestStreamDataReaderWithSize(stream, size, testRecords);
            sw.Stop();
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            var message = string.Format("10k took: {0}ms", elapsedMilliseconds);
            Console.WriteLine(message);
            if (elapsedMilliseconds > 0.2 * 1000)
            {
                Assert.Fail(message);
            }
        }

        [Test]
        public void StreamDataReader_Can_Read_Large_Csv_File()
        {
            const int size = 100000; // 100k
            var testRecords = new List<TestRecord>();
            var stream = CreateCsv(size, testRecords);
            var sw = new Stopwatch();
            sw.Start();
            TestStreamDataReaderWithSize(stream, size, testRecords);
            sw.Stop();
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            var message = string.Format("100k took: {0}ms", elapsedMilliseconds);
            Console.WriteLine(message);
            if(elapsedMilliseconds > 2 * 1000)
            {
                Assert.Fail(message);
            }

        }

        private void TestStreamDataReaderWithSize(Stream stream, int size, List<TestRecord> testRecords)
        {
            using (var r = DelimitedDataReader.Create(stream,
                new
                {
                    Id = typeof(long),
                    FundName = typeof(string),
                    Date = typeof(DateTime),
                    Amount = typeof(decimal)
                }, true))
            {
                // Make sure we have correct # of records from parser
                int index = 0;
                var actualRowCount = 0;
                int expectedRowCount = size;
                while (r.Read())
                {
                    actualRowCount++;
                    //if (index < 1000)
                    {
                        Assert.AreEqual(testRecords[index].Id, r["Id"]);
                        Assert.AreEqual(testRecords[index].FundName, r["FundName"]);
                        Assert.AreEqual(testRecords[index].Date, r["Date"]);
                        Assert.AreEqual(testRecords[index].Amount, r["Amount"]);
                        index++;
                    }
                }

                Assert.AreEqual(expectedRowCount, actualRowCount);
            }
        }
   
        private class TestRecord
        {
            public long Id { get; set; }
            public string FundName { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
        }

        private Stream CreateCsv(int size, List<TestRecord> testRecords)
        {
            var s = new MemoryStream();
            var w = new StreamWriter(s);
            w.WriteLine(@"""id"",""FundName"",""Date"",""Amount""");
            for (int i = 0; i < size; i++)
            {
                var offset = -(i % 365);
                long Id = i + 1;
                string FundName = i % 7 == 0 ? "Missions" : "General, Fund";
                DateTime Date = DateTime.Now.Date;
                decimal Amount = (decimal)((i % 999.99) + 1.0);
                testRecords.Add(new TestRecord { Id = Id, FundName = FundName, Date = Date, Amount = Amount });
                w.WriteLine(@"""{0}"",""{1}"",""{2}"",""{3}""", Id, FundName, Date.ToShortDateString(), Amount);
                //w.WriteLine(@"{0}, ""{1}"", {2}, {3}", Id, FundName, Date.ToShortDateString(), Amount);
            }
            w.Flush();
            s.Position = 0;
            return s;
        }
    }
    // ReSharper restore InconsistentNaming
}