using System.IO;
using System.Text;

namespace StrArcTool.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void WriteEncodedString(this BinaryWriter writer, string s, Encoding encoding)
        {
            var bytes = encoding.GetBytes(s);

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)~bytes[i];
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }
}
