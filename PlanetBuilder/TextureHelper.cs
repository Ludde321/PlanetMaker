using System;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public static class TextureHelper
    {
        public static Texture<short> LoadRaw16(string inputFilename, int width, int height)
        {
            var texture = new Texture<short>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[2 * width];
                for (int y = 0; y < height; y++)
                {
                    stream.Read(buffer, 0, buffer.Length);

                    var line = texture.Data[y];
                    for (int x = 0; x < width; x++)
                    {
                        byte b0 = buffer[x + x];
                        byte b1 = buffer[x + x + 1];
                        line[x] = (short)((b0 << 8) + b1);
                    }
                }
            }
            return texture;
        }

        public static Texture<short> LoadTiff16(string inputFilename)
        {
            int width;
            int height;
            byte[] buffer;
            using (var image = new MagickImage(inputFilename))
            {
                width = image.Width;
                height = image.Height;

                image.Format = MagickFormat.Gray;
                image.Endian = Endian.MSB;
                buffer = image.ToByteArray();
            }

            var texture = new Texture<short>(width, height);

            int idx = 0;
            for (int y = 0; y < height; y++)
            {
                var line = texture.Data[y];
                for (int x = 0; x < width; x++)
                {
                    byte b0 = buffer[idx++];
                    byte b1 = buffer[idx++];
                    line[x] = (short)((b0 << 8) + b1);
                }
            }

            return texture;
        }

        public static void SaveFile16(string outputFilename, Texture<short> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                SaveFile16(outputStream, texture);
            }
        }

        public static void SaveFile16(Stream outputStream, Texture<short> texture)
        {
            var buffer = new byte[2 * texture.Width];

            for (int y = 0; y < texture.Height; y++)
            {
                var line = texture.Data[y];
                for (int x = 0; x < texture.Width; x++)
                {
                    short h = line[x];

                    buffer[x + x] = (byte)(h >> 8);
                    buffer[x + x + 1] = (byte)h;
                }
                outputStream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void SavePng8(string outputFilename, Texture<short> texture)
        {
            var buffer = new byte[texture.Width * texture.Height];

            int idx = 0;
            for (int y = 0; y < texture.Height; y++)
            {
                var line = texture.Data[y];
                for (int x = 0; x < texture.Width; x++)
                {
                    short h = line[x];

                    buffer[idx++] = (byte)(h >> 8);
                }
            }

            using (var image = new MagickImage(buffer, new MagickReadSettings { Format = MagickFormat.Gray, Width = texture.Width, Height = texture.Height}))
            {
                image.Format = MagickFormat.Png;
                image.Write(outputFilename);
            }
        }

    }
}