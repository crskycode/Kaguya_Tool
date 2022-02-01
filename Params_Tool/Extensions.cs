using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_Tool
{
    static class Extensions
    {
        public static byte[] ReadByteLengthBlock(this BinaryReader reader)
        {
            var length = reader.ReadByte();
            return reader.ReadBytes(length);
        }

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

        public static string ReadString(this BinaryReader reader, int bytesLength, Encoding encoding)
        {
            var bytes = reader.ReadBytes(bytesLength);
            return encoding.GetString(bytes);
        }

        public static string ReadStringField(this BinaryReader reader)
        {
            var type = reader.ReadInt32();

            if (type != 0)
            {
                throw new Exception("文字列データではない！");
            }

            return reader.ReadWordLengthUnicodeString();
        }

        public static int ReadInt32Field(this BinaryReader reader)
        {
            var type = reader.ReadInt32();

            if (type != 1)
            {
                throw new Exception("数値データではない！");
            }

            return reader.ReadInt32();
        }

        public static Tuple<int, int> ReadCoord2dField(this BinaryReader reader)
        {
            var type = reader.ReadInt32();

            if (type != 2)
            {
                throw new Exception("座標値データではない！");
            }

            var x = reader.ReadInt32();
            var y = reader.ReadInt32();

            return Tuple.Create(x, y);
        }
    }
}
