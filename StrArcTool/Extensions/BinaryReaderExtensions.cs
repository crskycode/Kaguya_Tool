using System.IO;
using System.Text;

namespace StrArcTool.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static string ReadEncodedString(this BinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)~bytes[i];
            }

            if (length == 0)
            {
                return string.Empty;
            }

            return encoding.GetString(bytes);
        }
    }
}
