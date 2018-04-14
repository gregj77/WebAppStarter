using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utils
{
    public sealed class ByteSerializer
    {
        public MemoryStream SerializeToStream(object instance)
        {
            var stream = new MemoryStream();
            try
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(stream, instance);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public byte[] SerializeToBytes(object instance)
        {
            using (var stream = SerializeToStream(instance))
            {
                return stream.ToArray();
            }
        }
    }
}
