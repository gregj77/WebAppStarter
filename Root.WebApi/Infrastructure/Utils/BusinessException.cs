using System;
using System.Runtime.Serialization;

namespace Utils
{
    [Serializable]
    public class BusinessException : Exception
    {
        public object Content { get; }

        public BusinessException(object content) : base(string.Empty)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public BusinessException(string message, object content) : base(message)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public BusinessException(string message, Exception innerException, object content) : base(message, innerException)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var contentType = Type.GetType(info.GetString("contentType"), true);
            Content = info.GetValue(nameof(Content), contentType ?? throw new InvalidOperationException());
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("contentType", Content.GetType().AssemblyQualifiedName);
            info.AddValue(nameof(Content), Content);
            base.GetObjectData(info, context);
        }
    }
}
