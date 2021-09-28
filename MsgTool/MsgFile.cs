using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable IDE0017
#pragma warning disable IDE0063

namespace ScriptTool
{
    class MsgFile
    {
        static readonly byte[] SIGNATURE = Encoding.ASCII.GetBytes("[SCR-MESSAGE]");

        //static readonly Encoding ENCODING = Encoding.GetEncoding("shift_jis");

        Encoding _readEncoding;
        Encoding _writeEncoding;

        bool _encrypted;
        byte _xorKey;

        List<string> _nameStringList;
        List<string> _choiceStringList;
        List<Msg> _msgList;
        List<Cmd> _cmdList;

        public void Load(string filePath, string encoding)
        {
            _readEncoding = Encoding.GetEncoding(encoding);

            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
        }

        public void Save(string filePath, string encoding)
        {
            _writeEncoding = Encoding.GetEncoding(encoding);

            using (var stream = File.Create(filePath))
            using (var writer = new BinaryWriter(stream))
            {
                Write(writer);
            }
        }

        void Read(BinaryReader reader)
        {
            if (!reader.ReadBytes(SIGNATURE.Length).SequenceEqual(SIGNATURE))
            {
                throw new Exception("The file is not a valid scene message file.");
            }

            var version = reader.ReadBytes(6);

            if (version[3] != '4')
            {
                throw new Exception("The version of the file is not supported.");
            }

            _encrypted = reader.ReadByte() != 0;
            _xorKey = reader.ReadByte();

            // Name

            var nameCount = reader.ReadInt32();

            _nameStringList = new List<string>(nameCount);

            for (var i = 0; i < nameCount; i++)
            {
                _nameStringList.Add(ReadStringItem(reader));
            }

            // Choice

            var choiceCount = reader.ReadInt32();

            _choiceStringList = new List<string>(choiceCount);

            for (var i = 0; i < choiceCount; i++)
            {
                _choiceStringList.Add(ReadStringItem(reader));
            }

            // Message

            var msgCount = reader.ReadInt32();

            _msgList = new List<Msg>(msgCount);

            for (var i = 0; i < msgCount; i++)
            {
                _msgList.Add(ReadMsgItem(reader));
            }

            // Command

            var count = reader.ReadInt32();

            _cmdList = new List<Cmd>(count);

            for (var i = 0; i < count; i++)
            {
                _cmdList.Add(ReadCmdItem(reader));
            }
        }

        void Write(BinaryWriter writer)
        {
            if (_nameStringList == null)
                return;

            writer.Write(SIGNATURE);

            writer.Write(new byte[] { (byte)'v', (byte)'e', (byte)'r', (byte)'4', (byte)'.', (byte)'0' });

            writer.Write(true);

            writer.Write(_xorKey);

            // Name

            writer.Write(_nameStringList.Count);

            foreach (var s in _nameStringList)
            {
                WriteStringItem(writer, s);
            }

            // Choice

            writer.Write(_choiceStringList.Count);

            foreach (var s in _choiceStringList)
            {
                WriteStringItem(writer, s);
            }

            // Message

            writer.Write(_msgList.Count);

            foreach (var msg in _msgList)
            {
                WriterMsgItem(writer, msg);
            }

            // Command

            writer.Write(_cmdList.Count);

            foreach (var cmd in _cmdList)
            {
                WriteCmdItem(writer, cmd);
            }

            writer.Flush();
        }

        string ReadStringItem(BinaryReader reader)
        {
            var length = reader.ReadInt16();
            var bytes = reader.ReadBytes(length);

            if (_encrypted)
                Decrypt(bytes, _xorKey);

            var s = _readEncoding.GetString(bytes);

            return s;
        }

        void WriteStringItem(BinaryWriter writer, string s)
        {
            var bytes = _writeEncoding.GetBytes(s);

            if (_encrypted)
                Decrypt(bytes, _xorKey);

            writer.Write(Convert.ToInt16(bytes.Length));
            writer.Write(bytes);
        }

        Msg ReadMsgItem(BinaryReader reader)
        {
            var msg = new Msg();

            var length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);

            if (_encrypted)
                Decrypt(bytes, _xorKey);

            var offset = 0;

            var textLen = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            msg.Text = _readEncoding.GetString(bytes, offset, textLen);
            offset += textLen;

            var count = bytes[offset];
            offset += sizeof(byte);

            msg.Voices = new string[count];

            for (var i = 0; i < count; i++)
            {
                var ofs = offset;

                while (BitConverter.ToInt16(bytes, ofs) != 0)
                    ofs += sizeof(short);

                var len = ofs - offset;

                var voice = Encoding.Unicode.GetString(bytes, offset, len);
                offset = ofs + sizeof(short);

                msg.Voices[i] = voice;
            }

            Debug.Assert(offset == bytes.Length);

            return msg;
        }

        void WriterMsgItem(BinaryWriter writer, Msg msg)
        {
            using var itemStream = new MemoryStream();
            using var itemWriter = new BinaryWriter(itemStream);

            var msgBytes = _writeEncoding.GetBytes(msg.Text);

            itemWriter.Write(msgBytes.Length); // Int32
            itemWriter.Write(msgBytes);

            itemWriter.Write(Convert.ToByte(msg.Voices.Length));

            foreach (var e in msg.Voices)
            {
                var voBytes = Encoding.Unicode.GetBytes(e);

                itemWriter.Write(voBytes);
                itemWriter.Write((short)0);
            }

            var itemBytes = itemStream.ToArray();

            if (_encrypted)
                Decrypt(itemBytes, _xorKey);

            writer.Write(itemBytes.Length);
            writer.Write(itemBytes);
        }

        static Cmd ReadCmdItem(BinaryReader reader)
        {
            var cmd = new Cmd();

            cmd.Id = reader.ReadInt32();

            // Params

            var count = reader.ReadByte();
            cmd.Params = new int[count];

            for (var i = 0; i < count; i++)
            {
                cmd.Params[i] = reader.ReadInt32();
            }

            return cmd;
        }

        static void WriteCmdItem(BinaryWriter writer, Cmd cmd)
        {
            writer.Write(cmd.Id);

            writer.Write(Convert.ToByte(cmd.Params.Length));

            foreach (var e in cmd.Params)
            {
                writer.Write(e);
            }
        }

        static void Decrypt(byte[] bytes, byte key)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key;
            }
        }

        public void ExportText(string filePath)
        {
            using var writer = File.CreateText(filePath);

            // Name

            for (var i = 0; i < _nameStringList.Count; i++)
            {
                var s = Escape(_nameStringList[i]);

                writer.WriteLine($"◇A{i:X8}◇{s}");
                writer.WriteLine($"◆A{i:X8}◆{s}");
                writer.WriteLine();
            }

            // Choice

            for (var i = 0; i < _choiceStringList.Count; i++)
            {
                var s = Escape(_choiceStringList[i]);

                writer.WriteLine($"◇B{i:X8}◇{s}");
                writer.WriteLine($"◆B{i:X8}◆{s}");
                writer.WriteLine();
            }

            // Message

            for (var i = 0; i < _msgList.Count; i++)
            {
                var s = Escape(_msgList[i].Text);

                writer.WriteLine($"◇C{i:X8}◇{s}");
                writer.WriteLine($"◆C{i:X8}◆{s}");
                writer.WriteLine();
            }

            // Finished

            writer.Flush();
        }

        public void ImportText(string filePath)
        {
            using var reader = File.OpenText(filePath);

            var _lineNo = 0;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var lineNo = _lineNo++;

                if (line.Length == 0 || line[0] != '◆')
                    continue;

                var m = Regex.Match(line, @"◆(\w+)◆(.+$)");

                if (!m.Success || m.Groups.Count != 3)
                {
                    throw new Exception($"Bad format at line: {lineNo}");
                }

                var strIndex = m.Groups[1].Value;
                var strVal = m.Groups[2].Value;

                switch (strIndex[0])
                {
                    case 'A':
                    {
                        var index = int.Parse(strIndex[1..], NumberStyles.HexNumber);

                        if (index < 0 || index >= _nameStringList.Count)
                        {
                            throw new Exception($"Bad text index at line: {lineNo}");
                        }

                        _nameStringList[index] = Unescape(strVal);

                        break;
                    }
                    case 'B':
                    {
                        var index = int.Parse(strIndex[1..], NumberStyles.HexNumber);

                        if (index < 0 || index >= _choiceStringList.Count)
                        {
                            throw new Exception($"Bad text index at line: {lineNo}");
                        }

                        _choiceStringList[index] = Unescape(strVal);

                        break;
                    }
                    case 'C':
                    {
                        var index = int.Parse(strIndex[1..], NumberStyles.HexNumber);

                        if (index < 0 || index >= _msgList.Count)
                        {
                            throw new Exception($"Bad text index at line: {lineNo}");
                        }

                        _msgList[index].Text = Unescape(strVal);

                        break;
                    }
                    default:
                    {
                        throw new Exception($"Bad text type at line: {lineNo}");
                    }
                }
            }
        }

        static string Escape(string s)
        {
            return s.Replace("\n", "\\n");
        }

        static string Unescape(string s)
        {
            return s.Replace("\\n", "\n");
        }

        class Msg
        {
            public string Text;
            public string[] Voices;
        }

        class Cmd
        {
            public int Id;
            public int[] Params;
        }
    }
}
