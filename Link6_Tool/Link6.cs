using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0063

namespace Link6_Tool
{
    class Link6
    {
        static readonly byte[] SIGNATURE = Encoding.ASCII.GetBytes("LINK6");

        public static void Create(string filePath, string rootPath, string archiveName)
        {
            using var writer = new BinaryWriter(File.Create(filePath));

            writer.Write(SIGNATURE);

            writer.Write((short)0); // Zero

            writer.WriteByteLengthAnsiString(archiveName, Encoding.ASCII);

            foreach (var entryLocalPath in Directory.EnumerateFiles(rootPath, "*.*"))
            {
                var name = Path.GetFileName(entryLocalPath);

                Console.WriteLine($"Add \"{name}\"");

                var data = File.ReadAllBytes(entryLocalPath);

                var chunkAddr = writer.BaseStream.Position;

                writer.Write(0); // chunk size

                writer.Write((short)0); // flags

                writer.Write((short)0);

                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);

                writer.WriteWordLengthUnicodeString(name);

                writer.Write(data);

                var chunkSize = Convert.ToUInt32(writer.BaseStream.Position - chunkAddr);

                var nextChunkAddr = writer.BaseStream.Position;

                writer.BaseStream.Position = chunkAddr;
                writer.Write(chunkSize);
                writer.BaseStream.Position = nextChunkAddr;
            }

            writer.Write(0); // End Of File

            writer.Flush();
        }
    }
}
