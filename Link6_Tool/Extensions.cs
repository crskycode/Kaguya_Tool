using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link6_Tool
{
    static class Extensions
    {
        public static string ReadByteLengthAnsiString(this BinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            return encoding.GetString(bytes);
        }

        public static void WriteByteLengthAnsiString(this BinaryWriter writer, string s, Encoding encoding)
        {
            var bytes = encoding.GetBytes(s);
            writer.Write(Convert.ToByte(bytes.Length));
            writer.Write(bytes);
        }

        public static string ReadWordLengthAnsiString(this BinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadInt16();
            var bytes = reader.ReadBytes(length);
            return encoding.GetString(bytes);
        }

        public static string ReadByteLengthUnicodeString(this BinaryReader reader)
        {
            var length = reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            return Encoding.Unicode.GetString(bytes);
        }

        public static string ReadWordLengthUnicodeString(this BinaryReader reader)
        {
            var length = reader.ReadInt16();
            var bytes = reader.ReadBytes(length);
            return Encoding.Unicode.GetString(bytes);
        }

        public static void WriteWordLengthUnicodeString(this BinaryWriter writer, string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            writer.Write(Convert.ToUInt16(bytes.Length));
            writer.Write(bytes);
        }
    }
}
