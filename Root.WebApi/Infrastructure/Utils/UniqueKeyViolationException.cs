using System;
using System.Runtime.Serialization;

namespace Utils
{
    [Serializable]
    public class UniqueKeyViolationException : Exception
    {
        public UniqueKeyViolationException()
        {
        }

        public UniqueKeyViolationException(string message) : base(message)
        {
        }

        public UniqueKeyViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UniqueKeyViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
