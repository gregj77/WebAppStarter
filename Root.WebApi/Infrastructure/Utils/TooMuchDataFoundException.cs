using System;
using System.Runtime.Serialization;

namespace Utils
{
    [Serializable]
    public class TooMuchDataFoundException : Exception
    {
        public TooMuchDataFoundException()
        {
        }

        public TooMuchDataFoundException(string message) : base(message)
        {
        }

        public TooMuchDataFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TooMuchDataFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}