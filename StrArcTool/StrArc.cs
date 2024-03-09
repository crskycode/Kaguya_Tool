using StrArcTool.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StrArcTool
{
    public partial class StrArc
    {
        public List<StrEntry> _entries;

        public StrArc()
        {
            _entries = [];
        }

        public void Load(string path, string enc)
        {
            var stream = File.OpenRead(path);
            var reader = new BinaryReader(stream);

            var encoding = Encoding.GetEncoding(enc);

            var magic = reader.ReadInt32();

            if (magic != 0x31304655)
            {
                throw new InvalidDataException();
            }

            var pos = reader.ReadInt32();

            var count = (int)(stream.Length - pos) / 4;
            var indices = new List<int>(count);

            stream.Position = pos;

            while (stream.Position < stream.Length)
            {
                indices.Add(reader.ReadInt32());
            }

            _entries = new List<StrEntry>(indices.Count);

            for (var i = 0; i < indices.Count; i++)
            {
                stream.Position = indices[i];

                var v1 = reader.ReadInt32();
                var v2 = reader.ReadEncodedString(encoding);

                var entry = new StrEntry
                {
                    Type = v1,
                    Text = v2
                };

                _entries.Add(entry);
            }

            stream.Dispose();
        }

        public void Save(string path, string enc)
        {
            Console.WriteLine("Rebuilding...");

            var stream = File.Create(path);
            var writer = new BinaryWriter(stream);

            var encoding = Encoding.GetEncoding(enc);

            writer.Write(0x31304655);
            writer.Write(0);

            var indices = new List<int>(_entries.Count);

            foreach (var entry in _entries)
            {
                indices.Add(Convert.ToInt32(stream.Position));

                writer.Write(entry.Type);
                writer.WriteEncodedString(entry.Text, encoding);
            }

            var pos = Convert.ToInt32(stream.Position);

            foreach (var e in indices)
            {
                writer.Write(e);
            }

            stream.Position = 4;
            writer.Write(pos);

            writer.Flush();
            writer.Dispose();

            Console.WriteLine("Finished.");
        }

        public void Export(string path)
        {
            Console.WriteLine("Generating text...");

            var writer = File.CreateText(path);

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                var v1 = entry.Type;
                var v2 = entry.Text.Escape();

                writer.WriteLine("◇{0:D6}|{1:D2}◇{2}", i, v1, v2);
                writer.WriteLine("◆{0:D6}|{1:D2}◆{2}", i, v1, v2);
                writer.WriteLine();
            }

            writer.Flush();
            writer.Dispose();

            Console.WriteLine("Finished.");
        }

        [GeneratedRegex("◆(\\d+)\\|(\\d+)◆(.+$)")]
        private static partial Regex LineRegex();

        public void Import(string path)
        {
            Console.WriteLine("Loading translation...");

            var reader = File.OpenText(path);
            var num = 0;

            while (!reader.EndOfStream)
            {
                var n = num++;
                var line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line[0] != '◆')
                {
                    continue;
                }

                var match = LineRegex().Match(line);

                if (match.Groups.Count != 4)
                {
                    throw new Exception($"Unexpected format at line {n}");
                }

                var v1 = int.Parse(match.Groups[1].Value);
                var v2 = int.Parse(match.Groups[2].Value);
                var v3 = match.Groups[3].Value;

                if (v1 < 0 || v1 >= _entries.Count)
                {
                    throw new Exception($"Unexpected ID at line {n}");
                }

                if (v2 != _entries[v1].Type)
                {
                    throw new Exception($"Unexpected type at line {n}");
                }

                _entries[v1].Text = v3.Unescape();
            }

            reader.Dispose();
        }
    }
}
