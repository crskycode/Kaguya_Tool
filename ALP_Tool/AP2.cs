using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

#pragma warning disable IDE0017

namespace ALP_Tool
{
    class AP2
    {
        class Metadata
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
        }

        public static void Extract(string filePath, string pngPath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            if (reader.ReadInt32() != 0x322D5041)
            {
                throw new Exception("Not a valid AP-2 file.");
            }

            var metadata = new Metadata
            {
                OffsetX = reader.ReadInt32(),
                OffsetY = reader.ReadInt32(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32()
            };

            if (metadata.Width > 0x8000 || metadata.Height > 0x8000)
            {
                throw new Exception("Not a valid AP-2 file.");
            }

            stream.Position = 0x18;

            var dataSize = metadata.Width * metadata.Height * 4;

            var pixelData = reader.ReadBytes(dataSize);

            if (pixelData.Length != dataSize)
            {
                throw new Exception("Failed to read pixel data.");
            }

            var image = Image.LoadPixelData<Bgra32>(pixelData, metadata.Width, metadata.Height);

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            image.SaveAsPng(pngPath);

            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.WriteIndented = true;
            var json = JsonSerializer.Serialize(metadata, serializerOptions);

            var jsonPath = Path.ChangeExtension(pngPath, ".metadata.json");
            File.WriteAllText(jsonPath, json);
        }

        public static void Create(string filePath, string sourcePath)
        {
            var source = Image.Load(sourcePath);

            Metadata metadata = null;

            try
            {
                var jsonPath = Path.ChangeExtension(sourcePath, ".metadata.json");
                var json = File.ReadAllText(jsonPath);
                metadata = JsonSerializer.Deserialize<Metadata>(json);
            }
            catch (Exception)
            {
                Console.WriteLine("WARNING: Metadata failed to load.");

                metadata = new Metadata();
                metadata.Width = source.Width;
                metadata.Height = source.Height;
            }

            if (source.Width != metadata.Width || source.Height != metadata.Height)
            {
                Console.WriteLine("WARNING: Size of the image file does not match the metadata.");

                metadata.Width = source.Width;
                metadata.Height = source.Height;
            }

            if (source.PixelType.BitsPerPixel != 32)
            {
                throw new Exception("Only 32-bit image files are supported.");
            }

            using var stream = File.Create(filePath);
            using var writer = new BinaryWriter(stream);

            writer.Write(0x322D5041); // "AP-2"
            writer.Write(metadata.OffsetX);
            writer.Write(metadata.OffsetY);
            writer.Write(metadata.Width);
            writer.Write(metadata.Height);
            writer.Write(24);

            var image = source.CloneAs<Bgra32>();

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            for (var y = 0; y < image.Height; y++)
            {
                var span = image.GetPixelRowSpan(y);

                for (var x = 0; x < image.Width; x++)
                {
                    writer.Write(span[x].Bgra);
                }
            }

            writer.Flush();
        }
    }
}
