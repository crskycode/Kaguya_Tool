using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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

        public GameScriptGameSystem GameSystem { get; set; } = new();
        public GameScriptPattern Pattern { get; set; } = new();
        public GameScriptSceneLabel SceneLabel { get; set; } = new();

        public double Version { get; set; } = 1.0;

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

            Version = 1.0;

            foreach (var version in versions)
            {
                var signature = Encoding.ASCII.GetBytes($"[SCR-PARAMS]v0{version}");

                reader.BaseStream.Position = 0;

                if (reader.ReadBytes(signature.Length).SequenceEqual(signature))
                {
                    Version = version;
                    break;
                }
            }

            if (Version == 1.0)
            {
                throw new Exception("The file is not a valid game script parameters file.");
            }

            GameSystem = new GameScriptGameSystem();
            GameSystem.Deserialize(reader, Version);

            Pattern = new GameScriptPattern();
            Pattern.Deserialize(reader, Version);

            SceneLabel = new GameScriptSceneLabel();
            SceneLabel.Deserialize(reader, Version);

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
            if (Pattern.Cg is null || Pattern.FileMap is null || Pattern.Files is null)
            {
                throw new Exception("not enough data");
            }

            var cgSet = new CgSet();

            // lookup cg

            for (var i = 0; i < Pattern.Cg.Items.Count; i++)
            {
                var cg = Pattern.Cg.Items[i];

                var def = new CgDef();
                def.Name = cg.Name;

                // lookup differences

                for (var j = 0; j < cg.Items.Count; j++)
                {
                    var it = cg.Items[j];

                    var diff = new CgDiff();
                    diff.Index = j;

                    // lookup files of diff

                    for (var k = 0; k < Pattern.FileMap.Items[it].Items.Count; k++)
                    {
                        var file = Pattern.Files.Items[Pattern.FileMap.Items[it].Items[k]];

                        diff.Files.Add(file.Name);
                    }

                    def.Diff.Add(diff);
                }

                cgSet.Defs.Add(def);
            }

            //var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            //var json = JsonSerializer.Serialize(cgSet, options);
            //File.WriteAllText(filePath, json);
        }

        #endregion

        public void SaveToJson(string filePath)
        {
            var options = new JsonSerializerOptions {
                //IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
    }

    class GameScriptGameSystem
    {
        #region Classes

        public class CInstall
        {
            public class Item
            {
                public string Name { get; set; } = string.Empty;
                public string Source { get; set; } = string.Empty;
            }

            public List<Item> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    if (version >= 5.0)
                    {
                        var item = new Item
                        {
                            Name = reader.ReadWordLengthUnicodeString(),
                            Source = reader.ReadWordLengthUnicodeString()
                        };

                        Collection.Add(item);
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
            public class Item
            {
                public string Key { get; set; } = string.Empty;
                public string Value { get; set; } = string.Empty;
            }

            public string Name { get; set; } = string.Empty;
            public List<Item> Values { get; set; } = new();
            public List<SettingTag> Children { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                Name = reader.ReadWordLengthUnicodeString();

                var count = reader.ReadInt32();

                Values.Clear();
                Values.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Item
                    {
                        Key = reader.ReadWordLengthUnicodeString(),
                        Value = reader.ReadWordLengthUnicodeString()
                    };

                    Values.Add(item);
                }

                count = reader.ReadInt32();

                Children.Clear();
                Children.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var child = new SettingTag();
                    child.Deserialize(reader, version);
                    Children.Add(child);
                }
            }
        }

        public class StructA0
        {
            public int Field_0 { get; set; }
            public int Field_4 { get; set; }
            public int Field_8 { get; set; }

            public void Deserialize(BinaryReader reader, double version)
            {
                Field_0 = reader.ReadInt32();
                Field_4 = reader.ReadInt32();
                Field_8 = reader.ReadInt32();
            }
        }

        public class StructA0Collection
        {
            public List<StructA0> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new StructA0();
                    item.Deserialize(reader, version);
                    Collection.Add(item);
                }
            }
        }

        public class Demo
        {
            public interface ICmd
            {
                void Deserialize(BinaryReader reader, double version);
            }

            #region Commands

            public class CmdEnd : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            public class CmdNext : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            public class CmdWait : ICmd
            {
                public int Field_4 { get; set; }
                public int Field_8 { get; set; }

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByte();
                    Field_8 = reader.ReadInt32();
                }
            }

            public class CmdSound : ICmd
            {
                public int Field_4 { get; set; }
                public int Field_5 { get; set; }
                public string Field_8 { get; set; } = string.Empty;

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByte();
                    Field_5 = reader.ReadByte();
                    Field_8 = reader.ReadByteLengthUnicodeString();
                }
            }

            public class CmdLoad : ICmd
            {
                public int Field_4 { get; set; }
                public string Field_8 { get; set; } = string.Empty;

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByte();
                    Field_8 = reader.ReadByteLengthUnicodeString();
                }
            }

            public class CmdTransit : ICmd
            {
                public string Field_4 { get; set; } = string.Empty;
                public int Field_8 { get; set; }
                public string Field_C { get; set; } = string.Empty;

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByteLengthUnicodeString();
                    Field_8 = reader.ReadInt32();
                    Field_C = reader.ReadByteLengthUnicodeString();
                }
            }

            public class CmdDisp : ICmd
            {
                public int Field_4 { get; set; }
                public int Field_8 { get; set; }

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByte();
                    Field_8 = reader.ReadByte();
                }
            }

            public class CmdUpdate : ICmd
            {
                public void Deserialize(BinaryReader reader, double version)
                {
                }
            }

            public class CmdMove : ICmd
            {
                public int Field_4 { get; set; }
                public int Field_8 { get; set; }
                public int Field_C { get; set; }
                public int Field_10 { get; set; }
                public int Field_14 { get; set; }
                public int Field_18 { get; set; }

                public void Deserialize(BinaryReader reader, double version)
                {
                    Field_4 = reader.ReadByte();
                    Field_8 = reader.ReadByte();
                    Field_C = reader.ReadInt32();
                    Field_10 = reader.ReadInt32();
                    Field_14 = reader.ReadInt32();
                    Field_18 = reader.ReadInt32();
                }
            }

            #endregion

            public string Name { get; set; } = string.Empty;
            public List<ICmd> Cmds { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                Name = reader.ReadWordLengthUnicodeString();

                var signature = reader.ReadString(9, Encoding.ASCII);

                if (signature != "[Demo3.0]")
                {
                    throw new Exception("未対応のデモデータのフォーマットです！");
                }

                var count = reader.ReadInt16();

                Cmds.Clear();
                Cmds.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var addr = reader.BaseStream.Position;

                    var code = reader.ReadByte();
                    var size = reader.ReadByte();

                    ICmd? cmd = code switch
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

                    Cmds.Add(cmd);
                }
            }
        }

        public class DemoCollection
        {
            public List<Demo> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Demo();
                    item.Deserialize(reader, version);
                    Collection.Add(item);
                }
            }
        }

        public class StringList
        {
            public List<string> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = reader.ReadWordLengthUnicodeString();
                    Collection.Add(item);
                }
            }
        }

        public class StringListCollection
        {
            public List<StringList> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new StringList();
                    item.Deserialize(reader, version);
                    Collection.Add(item);
                }
            }
        }

        public class Place
        {
            public string Field_0 { get; set; } = string.Empty;
            public int Field_4 { get; set; }

            public void Deserialize(BinaryReader reader, double version)
            {
                Field_0 = reader.ReadWordLengthUnicodeString();
                Field_4 = reader.ReadInt32();
            }
        }

        public class PlaceCollection
        {
            public List<Place> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Place();
                    item.Deserialize(reader, version);
                    Collection.Add(item);
                }
            }
        }

        public class Thumbnail
        {
            public string Name { get; set; } = string.Empty;
            public List<string> List { get; set; } = new();
            public int Field_20 { get; set; }
            public int Field_24 { get; set; }
            public int Field_28 { get; set; }

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                Name = reader.ReadStringField();
                i++;

                List.Clear();
                List.EnsureCapacity(7);

                for (var j = 0; j < 7; j++)
                {
                    var s = reader.ReadStringField();
                    i++;
                    List.Add(s);
                }

                Field_20 = reader.ReadInt32Field();
                i++;
                Field_24 = reader.ReadInt32Field();
                i++;
                Field_28 = reader.ReadInt32Field();
                i++;
            }
        }

        public class ThumbnailCollection
        {
            public List<Thumbnail> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count;)
                {
                    var item = new Thumbnail();
                    item.Deserialize(reader, version, ref i);
                    Collection.Add(item);
                }
            }
        }

        public class RegistCg
        {
            public class Item
            {
                public string Field_0 { get; set; } = string.Empty;
                public int Field_4 { get; set; }
                public int Field_8 { get; set; }
                public int Field_C { get; set; }
            }

            public string Name { get; set; } = string.Empty;
            public List<Item> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                Name = reader.ReadStringField();
                i++;

                var count = reader.ReadInt32Field();
                i++;

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var j = 0; j < count; j++)
                {
                    var item = new Item();

                    item.Field_0 = reader.ReadStringField();
                    i++;

                    var coord = reader.ReadCoord2dField();
                    i++;

                    item.Field_4 = coord.Item1;
                    item.Field_8 = coord.Item2;

                    item.Field_C = reader.ReadInt32Field();
                    i++;

                    Collection.Add(item);
                }
            }
        }

        public class RegistCgCollection
        {
            public List<RegistCg> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count;)
                {
                    var item = new RegistCg();
                    item.Deserialize(reader, version, ref i);
                    Collection.Add(item);
                }
            }
        }

        public class RegistScene
        {
            public class Item
            {
                public string Name { get; set; } = string.Empty;
                public string File { get; set; } = string.Empty;
            }

            public string Name { get; set; } = string.Empty;
            public List<Item> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version, ref int i)
            {
                Name = reader.ReadStringField();
                i++;

                var count = reader.ReadInt32Field();
                i++;

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var j = 0; j < count; j++)
                {
                    var item = new Item();

                    item.Name = reader.ReadStringField();
                    i++;

                    item.File = reader.ReadStringField();
                    i++;

                    Collection.Add(item);
                }
            }
        }

        public class RegistSceneCollection
        {
            public List<RegistScene> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count;)
                {
                    var item = new RegistScene();
                    item.Deserialize(reader, version, ref i);
                    Collection.Add(item);
                }
            }
        }

        #endregion

        #region Fields

        public int Field_4 { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public byte[] Field_10 { get; set; } = Array.Empty<byte>();
        public string MainTitle { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string CompanyInfo { get; set; } = string.Empty;
        public int Field_2C { get; set; }
        public string PlayerFirstName { get; set; } = string.Empty;
        public string PlayerLastName { get; set; } = string.Empty;
        public CInstall Install { get; set; } = new();
        public int Field_44 { get; set; }
        public int Field_48 { get; set; }
        public int Field_4C { get; set; }
        public int Field_50 { get; set; }
        public int Field_54 { get; set; }
        public SettingTag ColorSettings { get; set; } = new();
        public SettingTag SoundSettings { get; set; } = new();
        public SettingTag WindowSettings { get; set; } = new();
        public StructA0Collection Field_A0 { get; set; } = new();
        public byte[] BmpKey { get; set; } = Array.Empty<byte>();
        public DemoCollection Demos { get; set; } = new();
        public StringList Field_C8 { get; set; } = new();
        public PlaceCollection Places { get; set; } = new();
        public string Field_E0 { get; set; } = string.Empty;
        public StringListCollection Field_E4 { get; set; } = new();
        public ThumbnailCollection Thumbnails { get; set; } = new();
        public List<string> Scenes { get; set; } = new();
        public RegistCgCollection RegistCgs { get; set; } = new();
        public RegistSceneCollection RegistScenes { get; set; } = new();

        #endregion

        public void Deserialize(BinaryReader reader, double version)
        {
            if (version >= 3.0)
            {
                Field_4 = reader.ReadInt16();
            }

            ScreenWidth = reader.ReadInt32();
            ScreenHeight = reader.ReadInt32();

            Field_10 = reader.ReadByteLengthBlock();

            if (version >= 5.0)
            {
                MainTitle = reader.ReadWordLengthUnicodeString();
                SubTitle = reader.ReadWordLengthUnicodeString();
                CompanyInfo = reader.ReadWordLengthUnicodeString();
            }
            else
            {
                throw new NotImplementedException();
            }

            Field_2C = reader.ReadByte();

            if (version >= 5.0)
            {
                PlayerFirstName = reader.ReadWordLengthUnicodeString();
                PlayerLastName = reader.ReadWordLengthUnicodeString();
            }
            else
            {
                throw new NotImplementedException();
            }

            Install.Deserialize(reader, version);

            Field_44 = reader.ReadInt32();
            Field_48 = reader.ReadInt32();
            Field_4C = reader.ReadInt32();

            if (version >= 5.3)
            {
                Field_50 = reader.ReadInt32();
            }

            if (version >= 5.5)
            {
                Field_54 = reader.ReadByte();
            }

            if (version >= 5.2)
            {
                if (reader.ReadByte() != 0)
                {
                    SoundSettings.Deserialize(reader, version);
                }

                if (reader.ReadByte() != 0)
                {
                    ColorSettings.Deserialize(reader, version);
                }

                if (reader.ReadByte() != 0)
                {
                    WindowSettings.Deserialize(reader, version);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            if (version >= 5.3)
            {
                Field_A0.Deserialize(reader, version);
            }

            var length = reader.ReadInt32();
            BmpKey = reader.ReadBytes(length);

            if (version >= 5.2)
            {
                Demos.Deserialize(reader, version);
            }

            if (version >= 5.1)
            {
                Field_C8.Deserialize(reader, version);
                Places.Deserialize(reader, version);
            }

            if (version >= 5.4)
            {
                Field_E0 = reader.ReadWordLengthUnicodeString();
                Field_E4.Deserialize(reader, version);
            }

            if (version >= 5.3)
            {
                Thumbnails.Deserialize(reader, version);
            }

            var sceneCount = reader.ReadInt32();

            Scenes.Clear();
            Scenes.EnsureCapacity(sceneCount);

            for (var i = 0; i < sceneCount; i++)
            {
                var s = reader.ReadStringField();
                Scenes.Add(s);
            }

            RegistCgs.Deserialize(reader, version);
            RegistScenes.Deserialize(reader, version);
        }
    }

    class GameScriptPattern
    {
        public class FileGroup
        {
            public List<string> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var s = reader.ReadWordLengthUnicodeString();
                    Items.Add(s);
                }
            }
        }

        public class ExcPosition
        {
            public string Name { get; set; } = string.Empty;
            public int Field_4 { get; set; }
            public int Field_8 { get; set; }

            public void Deserialize(BinaryReader reader, double version)
            {
                Name = reader.ReadWordLengthUnicodeString();
                Field_4 = reader.ReadInt32();
                Field_8 = reader.ReadInt32();
            }
        }

        public class FileNameList
        {
            public string Name { get; set; } = string.Empty;
            public int Type { get; set; }
            public FileGroup? FileGroup { get; set; }
            public ExcPosition? ExcPosition { get; set; }

            public void Deserialize(BinaryReader reader, double version)
            {
                Name = reader.ReadWordLengthUnicodeString();

                Type = reader.ReadByte();

                if (Type == 1)
                {
                    FileGroup = new FileGroup();
                    FileGroup.Deserialize(reader, version);
                }
                else if (Type == 2)
                {
                    ExcPosition = new ExcPosition();
                    ExcPosition.Deserialize(reader, version);
                }
                else if (Type != 0)
                {
                    throw new Exception("ファイル名リストのタイプが未対応！！");
                }
            }
        }

        public class FileNameListCollection
        {
            public List<FileNameList> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new FileNameList();
                    item.Deserialize(reader, version);
                    Items.Add(item);
                }
            }
        }

        public class ByteLengthIntCollection
        {
            public List<int> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadByte();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var val = reader.ReadInt32();
                    Items.Add(val);
                }
            }
        }

        public class FileMapCollection
        {
            public List<ByteLengthIntCollection> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new ByteLengthIntCollection();
                    item.Deserialize(reader, version);
                    Items.Add(item);
                }
            }
        }

        public class Group
        {
            public string Name { get; set; } = string.Empty;
            public List<int> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                if (version >= 5.0)
                    Name = reader.ReadWordLengthUnicodeString();
                else
                    throw new NotImplementedException();

                int count;

                if (version >= 5.6)
                    count = reader.ReadInt16();
                else
                    count = reader.ReadByte();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var val = reader.ReadInt32();
                    Items.Add(val);
                }
            }
        }

        public class GroupCollection
        {
            public List<Group> Items { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Items.Clear();
                Items.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Group();
                    item.Deserialize(reader, version);
                    Items.Add(item);
                }
            }
        }

        public int XorKey { get; set; }
        public FileNameListCollection Files { get; set; } = new();
        public FileMapCollection FileMap { get; set; } = new();
        public GroupCollection Cg { get; set; } = new();
        public GroupCollection Bg { get; set; } = new();

        public void Deserialize(BinaryReader reader, double version)
        {
            if (version < 5.0)
            {
                XorKey = reader.ReadByte();
            }

            if (version < 4.0)
            {
                throw new NotImplementedException();
            }

            if (version >= 5.7)
            {
                Files.Deserialize(reader, version);
            }

            FileMap.Deserialize(reader, version);
            Cg.Deserialize(reader, version);
            Bg.Deserialize(reader, version);
        }
    }

    class GameScriptSceneLabel
    {
        public class Scene
        {
            public string Name { get; set; } = string.Empty;
            public int Field_4 { get; set; }
            public int Field_8 { get; set; }

            public void Deserialize(BinaryReader reader, double version)
            {
                Name = reader.ReadWordLengthUnicodeString();
                Field_4 = reader.ReadInt32();
                Field_8 = reader.ReadInt32();
            }
        }

        public class SceneCollection
        {
            public List<Scene> Collection { get; set; } = new();

            public void Deserialize(BinaryReader reader, double version)
            {
                var count = reader.ReadInt32();

                Collection.Clear();
                Collection.EnsureCapacity(count);

                for (var i = 0; i < count; i++)
                {
                    var item = new Scene();
                    item.Deserialize(reader, version);
                    Collection.Add(item);
                }
            }
        }

        public SceneCollection Scenes { get; set; } = new();

        public void Deserialize(BinaryReader reader, double version)
        {
            Scenes.Deserialize(reader, version);
        }
    }
}
