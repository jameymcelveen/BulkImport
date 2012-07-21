// ReSharper disable InconsistentNaming

using NUnit.Framework;
using JameyMac.BulkImport.Readers;

namespace JameyMac.BulkImport.Tests.Readers
{
    [TestFixture]
    public class SplitDelimitedExtensionsFixture
    {
        [Test]
        public void SplitDelimited_Parses_Quoted_Text_Correctly()
        {
            const string testValue = @"""1234"",""General Fund"",""12/12/2012"",""12.34""";
            var expectedResults = new[] { "1234", "General Fund", "12/12/2012", "12.34" };
            var results = testValue.SplitDelimited(',', expectedResults.Length);

            Assert.AreEqual(expectedResults.Length, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(expectedResults[i], results[i]);
            }                        
        }

        // TODO Update CSV Parser to handle mixed quotes
        // [Test]
        public void SplitDelimited_Parses_UnQuoted_Text_Correctly()
        {
            const string testValue = @"1234,""General Fund"",12/12/2012,12.34";
            var expectedResults = new [] { "1234", "General Fund", "12/12/2012", "12.34" };
            var results = testValue.SplitDelimited(',', expectedResults.Length);
            
            Assert.AreEqual(expectedResults.Length, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(expectedResults[i].Trim(), results[i].Trim());
            }
        }
    }
}
// ReSharper restore InconsistentNaming