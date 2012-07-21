#region

using System;

#endregion

namespace JameyMac.BulkImport.Readers
{
    public static class SplitDelimitedExtensions
    {
        private static readonly string[] Seperator = new[] {"\",\""};

        public static string[] SplitDelimited(this string value, char separator, int count)
        {
            var fields = value.Split(Seperator, StringSplitOptions.None);
            if (fields.Length == count)
            {
                fields[0] = fields[0].TrimStart('"');
                fields[count - 1] = fields[count - 1].TrimEnd('"');
                return fields;
            }

            throw new Exception(string.Format("Error parsing row \"{0}\"", value));
        }
    }
}