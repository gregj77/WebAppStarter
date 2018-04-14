using System;
using System.Runtime.Serialization;

namespace Utils
{
    [Serializable]
    public class OperationException : Exception
    {
        public object Content { get; }

        public OperationException(object content) : base(string.Empty)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public OperationException(string message, object content) : base(message)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public OperationException(string message, object content, Exception innerException) : base(message, innerException)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        protected OperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var contentType = Type.GetType(info.GetString("contentType"), true);
            Content = info.GetValue(nameof(Content), contentType);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("contentType", Content.GetType().AssemblyQualifiedName);
            info.AddValue(nameof(Content), Content);
            base.GetObjectData(info, context);
        }
    }
}