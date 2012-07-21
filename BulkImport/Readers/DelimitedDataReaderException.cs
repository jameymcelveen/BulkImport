#region

using System;
using System.Runtime.Serialization;

#endregion

namespace JameyMac.BulkImport.Readers
{
    [Serializable]
    public class DelimitedDataReaderException : Exception
    {
        public DelimitedDataReaderException()
        {
        }

        public DelimitedDataReaderException(string message) : base(message)
        {
        }

        public DelimitedDataReaderException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DelimitedDataReaderException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}