using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

#pragma warning disable IDE0017
#pragma warning disable IDE0052
#pragma warning disable IDE0060
#pragma warning disable IDE0063

namespace Params_Tool
{
    class GameScriptParams
    {
        #region Fields

        GameScriptGameSystem _gameSystem;
        GameScriptPattern _pattern;
        GameScriptSceneLabel _sceneLabel;

        double _version = 1.0;

        #endregion

        #region Properties

        public GameScriptGameSystem GameSystem { get => _gameSystem; }
        public GameScriptPattern Pattern { get => _pattern; }
        public GameScriptSceneLabel SceneLabel { get => _sceneLabel; }

        public double Version { get => _version; }

        #endregion

        public void Load(string filePath)
        {
            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                Read(reader);
            }
        }

        void Read(BinaryReader reader)
        {
            var versions = new double[] { 5.7, 5.6, 5.5, 5.4, 5.3, 5.2, 5.1, 5.0, 4.0, 3.0, 2.0 };

            _version = 1.0;

            foreach (var version in versions)
            {
                var signature = Encoding.ASCII.GetBytes($"[SCR-PARAMS]v0{version}");

                reader.BaseStream.Position = 0;

                if (reader.ReadBytes(signature.Length).SequenceEqual(signature))
                {
                    _version = version;
                    break;
                }
            }

            if (_version == 1.0)
            {
                throw new Exception("The file is not a valid game script parameters file.");
            }

            _gameSystem = new GameScriptGameSystem();
            _gameSystem.Deserialize(reader, _version);

            _pattern = new GameScriptPattern();
            _pattern.Deserialize(reader, _version);

            _sceneLabel = new GameScriptSceneLabel();
            _sceneLabel.Deserialize(reader, _version);

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        }

        #region CG information

        class CgDiff
        {
            public int Index { get; set; }
            public List<string> Files { get; set; } = new();
        }

        class CgDef
        {
            public string Name { get; set; } = string.Empty;
            public List<CgDiff> Diff { get; set; } = new();
        }

        class CgSet
        {
            public List<CgDef> Defs { get; set; } = new();
        }

        public void DumpCgSet(string filePath)
        {
            if (_pattern.Cg is null || _pattern.FileMap is null || _pattern.Files is null)
            {
                throw new Exception("not enough data");
            }

            var cgSet = new CgSet();

            // lookup cg

            for (var i = 0; i < _pattern.Cg.Count; i++)
            {
                var cg = _pattern.Cg[i];

                var def = new CgDef();
                def.Name = cg.Name;

                // lookup differences

                for (var j = 0; j < cg.Items.Count; j++)
                {
                    var it = cg.Items[j];

                    var diff = new CgDiff();
                    diff.Index = j;

                    // lookup files of diff

                    for (var k = 0; k < _pattern.FileMap[it].Count; k++)
                    {
                        var file = _pattern.Files[_pattern.FileMap[it][k]];

                        diff.Files.Add(file.Name);
                    }

                    def.Diff.Add(diff);
                }

                cgSet.Defs.Add(def);
            }

            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(cgSet, options);
            File.WriteAllText(filePath, json);
        }

        #endregion
    }

    class GameScriptGameSystem
    {
        #region Classes

        public class Install
        {
            class Item
            {
                public string FileName;
                public string Source;
            }

            List<Item> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                _collection = new List<Item>(count);

                for (var i = 0; i < count; i++)
                {
                    if (version >= 5.0)
                    {
                        var item = new Item
                        {
                            FileName = reader.ReadWordLengthUnicodeString(),
                            Source = reader.ReadWordLengthUnicodeString()
                        };

                        _collection.Add(item);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public class SettingTag
        {
            class Item
            {
                public string Key;
                public string Value;
            }

            string _name;
            List<Item> _values;
            List<SettingTag> _children;

            public void Deserialize(BinaryReader reader, double version)
            {
                _name = reader.ReadWordLengthUnicodeString();

                var count = reader.ReadInt32();

                _values = new List<Item>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Item
                    {
                        Key = reader.ReadWordLengthUnicodeString(),
                        Value = reader.ReadWordLengthUnicodeString()
                    };

                    _values.Add(item);
                }

                count = reader.ReadInt32();

                _children = new List<SettingTag>(count);

                for (var i = 0; i < count; i++)
                {
                    var child = new SettingTag();
                    child.Deserialize(reader, version);
                    _children.Add(child);
                }
            }
        }

        public class StructA0
        {
            public int field_0;
            public int field_4;
            public int field_8;

            public void Deserialize(BinaryReader reader, double version)
            {
                field_0 = reader.ReadInt32();
                field_4 = reader.ReadInt32();
                field_8 = reader.ReadInt32();
            }
        }

        public class StructA0Collection
        {
            List<StructA0> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<StructA0>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new StructA0();
                    item.Deserialize(reader, version);
                    _collection.Add(item);
                }
            }
        }

        public class Demo
        {
            interface ICmd
            {
                void Deserialize(BinaryReader reader, double version);
            }

            #region Commands

            class CmdEnd : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            class CmdNext : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            class CmdWait : ICmd
            {
                int field_4;
                int field_8;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByte();
                    field_8 = reader.ReadInt32();
                }
            }

            class CmdSound : ICmd
            {
                int field_4;
                int field_5;
                string field_8;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByte();
                    field_5 = reader.ReadByte();
                    field_8 = reader.ReadByteLengthUnicodeString();
                }
            }

            class CmdLoad : ICmd
            {
                int field_4;
                string field_8;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByte();
                    field_8 = reader.ReadByteLengthUnicodeString();
                }
            }

            class CmdTransit : ICmd
            {
                string field_4;
                int field_8;
                string field_C;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByteLengthUnicodeString();
                    field_8 = reader.ReadInt32();
                    field_C = reader.ReadByteLengthUnicodeString();
                }
            }

            class CmdDisp : ICmd
            {
                int field_4;
                int field_8;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByte();
                    field_8 = reader.ReadByte();
                }
            }

            class CmdUpdate : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            class CmdMove : ICmd
            {
                int field_4;
                int field_8;
                int field_C;
                int field_10;
                int field_14;
                int field_18;

                public void Deserialize(BinaryReader reader, double version)
                {
                    field_4 = reader.ReadByte();
                    field_8 = reader.ReadByte();
                    field_C = reader.ReadInt32();
                    field_10 = reader.ReadInt32();
                    field_14 = reader.ReadInt32();
                    field_18 = reader.ReadInt32();
                }
            }

            #endregion

            string _name;
            List<ICmd> _cmds;

            public void Deserialize(BinaryReader reader, double version)
            {
                _name = reader.ReadWordLengthUnicodeString();

                var signature = reader.ReadString(9, Encoding.ASCII);

                if (signature != "[Demo3.0]")
                {
                    throw new Exception("未対応のデモデータのフォーマットです！");
                }

                var count = reader.ReadInt16();

                _cmds = new List<ICmd>(count);

                for (var i = 0; i < count; i++)
                {
                    var addr = reader.BaseStream.Position;

                    var code = reader.ReadByte();
                    var size = reader.ReadByte();

                    ICmd cmd = code switch
                    {
                        0 => new CmdEnd(),
                        1 => new CmdNext(),
                        2 => new CmdWait(),
                        3 => new CmdSound(),
                        4 => new CmdLoad(),
                        5 => new CmdTransit(),
                        6 => new CmdDisp(),
                        7 => new CmdUpdate(),
                        8 => new CmdMove(),
                        _ => null
                    };

                    if (cmd == null)
                    {
                        throw new Exception("未対応のデモデータのコマンドです！");
                    }

                    cmd.Deserialize(reader, version);

                    if (reader.BaseStream.Position != addr + size)
                    {
                        throw new Exception("Demo command data parsing failed.");
                    }

                    _cmds.Add(cmd);
                }
            }
        }

        public class DemoCollection
        {
            List<Demo> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                _collection = new List<Demo>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Demo();
                    item.Deserialize(reader, version);
                    _collection.Add(item);
                }
            }
        }

        public class StringList
        {
            List<string> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<string>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = reader.ReadWordLengthUnicodeString();
                    _collection.Add(item);
                }
            }
        }

        public class StringListCollection
        {
            List<StringList> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<StringList>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new StringList();
                    item.Deserialize(reader, version);
                    _collection.Add(item);
                }
            }
        }

        public class Place
        {
            public string field_0;
            public int field_4;

            public void Deserialize(BinaryReader reader, double version)
            {
                field_0 = reader.ReadWordLengthUnicodeString();
                field_4 = reader.ReadInt32();
            }
        }

        public class PlaceCollection
        {
            List<Place> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<Place>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Place();
                    item.Deserialize(reader, version);
                    _collection.Add(item);
                }
            }
        }

        public class Thumbnail
        {
            string _name;
            List<string> _list;
            int field_20;
            int field_24;
            int field_28;

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                _name = reader.ReadStringField();
                i++;

                _list = new List<string>(7);

                for (var j = 0; j < 7; j++)
                {
                    var s = reader.ReadStringField();
                    i++;
                    _list.Add(s);
                }

                field_20 = reader.ReadInt32Field();
                i++;
                field_24 = reader.ReadInt32Field();
                i++;
                field_28 = reader.ReadInt32Field();
                i++;
            }
        }

        public class ThumbnailCollection
        {
            List<Thumbnail> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<Thumbnail>();

                for (var i = 0; i < count;)
                {
                    var item = new Thumbnail();
                    item.Deserialize(reader, version, ref i);
                    _collection.Add(item);
                }
            }
        }

        public class RegistCg
        {
            class Item
            {
                public string field_0;
                public int field_4;
                public int field_8;
                public int field_C;
            }

            string _name;
            List<Item> _collection;

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                _name = reader.ReadStringField();
                i++;

                var count = reader.ReadInt32Field();
                i++;

                _collection = new List<Item>(count);

                for (var j = 0; j < count; j++)
                {
                    var item = new Item();

                    item.field_0 = reader.ReadStringField();
                    i++;

                    var coord = reader.ReadCoord2dField();
                    i++;

                    item.field_4 = coord.Item1;
                    item.field_8 = coord.Item2;

                    item.field_C = reader.ReadInt32Field();
                    i++;

                    _collection.Add(item);
                }
            }
        }

        public class RegistCgCollection
        {
            List<RegistCg> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<RegistCg>();

                for (var i = 0; i < count;)
                {
                    var item = new RegistCg();
                    item.Deserialize(reader, version, ref i);
                    _collection.Add(item);
                }
            }
        }

        public class RegistScene
        {
            class Item
            {
                public string field_0;
                public string field_4;
            }

            string _name;
            List<Item> _collection;

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                _name = reader.ReadStringField();
                i++;

                var count = reader.ReadInt32Field();
                i++;

                _collection = new List<Item>(count);

                for (var j = 0; j < count; j++)
                {
                    var item = new Item();

                    item.field_0 = reader.ReadStringField();
                    i++;

                    item.field_4 = reader.ReadStringField();
                    i++;

                    _collection.Add(item);
                }
            }
        }

        public class RegistSceneCollection
        {
            List<RegistScene> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<RegistScene>();

                for (var i = 0; i < count;)
                {
                    var item = new RegistScene();
                    item.Deserialize(reader, version, ref i);
                    _collection.Add(item);
                }
            }
        }

        #endregion

        #region Fields

        int _field_4;
        int _field_8;
        int _field_C;
        byte[] _field_10;
        string _mainTitle;
        string _subTitle;
        string _companyInfo;
        int _field_2C;
        string _developerFirstName;
        string _developerLastName;
        Install _install;
        int _field_44;
        int _field_48;
        int _field_4C;
        int _field_50;
        int _field_54;
        SettingTag _colorSettings;
        SettingTag _soundSettings;
        SettingTag _windowSettings;
        StructA0Collection _field_A0;
        byte[] _bmpKey;
        DemoCollection _demos;
        StringList _field_C8;
        PlaceCollection _places;
        string _field_E0;
        StringListCollection _field_E4;
        ThumbnailCollection _thumbnails;
        List<string> _scenes;
        RegistCgCollection _registCgs;
        RegistSceneCollection _registScenes;

        #endregion

        #region Properties

        public byte[] BmpKey { get => _bmpKey; }

        #endregion

        public void Deserialize(BinaryReader reader, double version)
        {
            if (version >= 3.0)
            {
                _field_4 = reader.ReadInt16();
            }

            _field_8 = reader.ReadInt32();
            _field_C = reader.ReadInt32();

            _field_10 = reader.ReadByteLengthBlock();

            if (version >= 5.0)
            {
                _mainTitle = reader.ReadWordLengthUnicodeString();
                _subTitle = reader.ReadWordLengthUnicodeString();
                _companyInfo = reader.ReadWordLengthUnicodeString();
            }
            else
            {
                throw new NotImplementedException();
            }

            _field_2C = reader.ReadByte();

            if (version >= 5.0)
            {
                _developerFirstName = reader.ReadWordLengthUnicodeString();
                _developerLastName = reader.ReadWordLengthUnicodeString();
            }
            else
            {
                throw new NotImplementedException();
            }

            _install = new Install();
            _install.Deserialize(reader, version);

            _field_44 = reader.ReadInt32();
            _field_48 = reader.ReadInt32();
            _field_4C = reader.ReadInt32();

            if (version >= 5.3)
            {
                _field_50 = reader.ReadInt32();
            }

            if (version >= 5.5)
            {
                _field_54 = reader.ReadByte();
            }

            if (version >= 5.2)
            {
                if (reader.ReadByte() != 0)
                {
                    _soundSettings = new SettingTag();
                    _soundSettings.Deserialize(reader, version);
                }

                if (reader.ReadByte() != 0)
                {
                    _colorSettings = new SettingTag();
                    _colorSettings.Deserialize(reader, version);
                }

                if (reader.ReadByte() != 0)
                {
                    _windowSettings = new SettingTag();
                    _windowSettings.Deserialize(reader, version);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            if (version >= 5.3)
            {
                _field_A0 = new StructA0Collection();
                _field_A0.Deserialize(reader, version);
            }

            var length = reader.ReadInt32();
            _bmpKey = reader.ReadBytes(length);

            if (version >= 5.2)
            {
                _demos = new DemoCollection();
                _demos.Deserialize(reader, version);
            }

            if (version >= 5.1)
            {
                _field_C8 = new StringList();
                _field_C8.Deserialize(reader, version);

                _places = new PlaceCollection();
                _places.Deserialize(reader, version);
            }

            if (version >= 5.4)
            {
                _field_E0 = reader.ReadWordLengthUnicodeString();

                _field_E4 = new StringListCollection();
                _field_E4.Deserialize(reader, version);
            }

            if (version >= 5.3)
            {
                _thumbnails = new ThumbnailCollection();
                _thumbnails.Deserialize(reader, version);
            }

            var sceneCount = reader.ReadInt32();

            _scenes = new List<string>(sceneCount);

            for (var i = 0; i < sceneCount; i++)
            {
                var s = reader.ReadStringField();
                _scenes.Add(s);
            }

            _registCgs = new RegistCgCollection();
            _registCgs.Deserialize(reader, version);

            _registScenes = new RegistSceneCollection();
            _registScenes.Deserialize(reader, version);
        }
    }

    class GameScriptPattern
    {
        #region Classes

        public class FileGroup : IReadOnlyList<string>
        {
            readonly List<string> _items = new();

            #region IReadOnlyList

            public string this[int index] => _items[index];

            public int Count => _items.Count;

            public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

            #endregion

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var s = reader.ReadWordLengthUnicodeString();
                    _items.Add(s);
                }
            }
        }

        public class ExcPosition
        {
            public string _name;
            public int field_4;
            public int field_8;

            public void Deserialize(BinaryReader reader, double version)
            {
                _name = reader.ReadWordLengthUnicodeString();
                field_4 = reader.ReadInt32();
                field_8 = reader.ReadInt32();
            }
        }

        public class FileNameList
        {
            public string Name { get => _name; }
            public int Type { get => _type; }
            public FileGroup? Group { get => _fileGroup; }
            public ExcPosition? Exc { get => _excPosition; }

            string _name = string.Empty;
            int _type;
            FileGroup? _fileGroup;
            ExcPosition? _excPosition;

            public void Deserialize(BinaryReader reader, double version)
            {
                _name = reader.ReadWordLengthUnicodeString();

                _type = reader.ReadByte();

                if (_type == 1)
                {
                    _fileGroup = new FileGroup();
                    _fileGroup.Deserialize(reader, version);
                }
                else if (_type == 2)
                {
                    _excPosition = new ExcPosition();
                    _excPosition.Deserialize(reader, version);
                }
                else if (_type != 0)
                {
                    throw new Exception("ファイル名リストのタイプが未対応！！");
                }
            }
        }

        public class FileNameListCollection : IReadOnlyList<FileNameList>
        {
            readonly List<FileNameList> _items = new();

            #region IReadOnlyList

            public FileNameList this[int index] => _items[index];

            public int Count => _items.Count;

            public IEnumerator<FileNameList> GetEnumerator() => ((IEnumerable<FileNameList>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

            #endregion

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new FileNameList();
                    item.Deserialize(reader, version);
                    _items.Add(item);
                }
            }
        }

        public class ByteLengthIntCollection : IReadOnlyList<int>
        {
            readonly List<int> _items = new();

            #region IReadOnlyList

            public int this[int index] => _items[index];

            public int Count => _items.Count;

            public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

            #endregion

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var val = reader.ReadInt32();
                    _items.Add(val);
                }
            }
        }

        public class FileMapCollection : IReadOnlyList<ByteLengthIntCollection>
        {
            readonly List<ByteLengthIntCollection> _items = new();

            #region IReadOnlyList

            public ByteLengthIntCollection this[int index] => _items[index];

            public int Count => _items.Count;

            public IEnumerator<ByteLengthIntCollection> GetEnumerator() => ((IEnumerable<ByteLengthIntCollection>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

            #endregion

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new ByteLengthIntCollection();
                    item.Deserialize(reader, version);
                    _items.Add(item);
                }
            }
        }

        public class Group
        {
            public string Name { get => _name; }
            public IReadOnlyList<int> Items { get => _items; }

            string _name = string.Empty;
            readonly List<int> _items = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                if (version >= 5.0)
                    _name = reader.ReadWordLengthUnicodeString();
                else
                    throw new NotImplementedException();

                int count;

                if (version >= 5.6)
                    count = reader.ReadInt16();
                else
                    count = reader.ReadByte();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var val = reader.ReadInt32();
                    _items.Add(val);
                }
            }
        }

        public class GroupCollection : IReadOnlyList<Group>
        {
            readonly List<Group> _items = new();

            #region IReadOnlyList

            public Group this[int index] => _items[index];

            public int Count => _items.Count;

            public IEnumerator<Group> GetEnumerator() => ((IEnumerable<Group>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

            #endregion

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _items.Clear();
                _items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Group();
                    item.Deserialize(reader, version);
                    _items.Add(item);
                }
            }
        }

        #endregion

        #region Properties

        public FileNameListCollection? Files { get => _files; }
        public FileMapCollection? FileMap { get => _fileMap; }
        public GroupCollection? Cg { get => _cg; }
        public GroupCollection? Bg { get => _bg; }

        #endregion

        #region Fields

        int _xorKey;
        FileNameListCollection _files;
        FileMapCollection _fileMap;
        GroupCollection _cg;
        GroupCollection _bg;

        #endregion

        public void Deserialize(BinaryReader reader, double version)
        {
            if (version < 5.0)
            {
                _xorKey = reader.ReadByte();
            }

            if (version < 4.0)
            {
                throw new NotImplementedException();
            }

            if (version >= 5.7)
            {
                _files = new FileNameListCollection();
                _files.Deserialize(reader, version);
            }

            _fileMap = new FileMapCollection();
            _fileMap.Deserialize(reader, version);

            _cg = new GroupCollection();
            _cg.Deserialize(reader, version);

            _bg = new GroupCollection();
            _bg.Deserialize(reader, version);
        }
    }

    class GameScriptSceneLabel
    {
        #region Classes

        class Scene
        {
            string _name;
            int field_4;
            int field_8;

            public void Deserialize(BinaryReader reader, double version)
            {
                _name = reader.ReadWordLengthUnicodeString();
                field_4 = reader.ReadInt32();
                field_8 = reader.ReadInt32();
            }
        }

        class SceneCollection
        {
            List<Scene> _collection;

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                _collection = new List<Scene>(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Scene();
                    item.Deserialize(reader, version);
                    _collection.Add(item);
                }
            }
        }

        #endregion

        #region Fields

        SceneCollection _scenes;

        #endregion

        public void Deserialize(BinaryReader reader, double version)
        {
            _scenes = new SceneCollection();
            _scenes.Deserialize(reader, version);
        }
    }
}
