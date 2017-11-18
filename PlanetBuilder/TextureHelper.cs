using System;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public static class TextureHelper
    {
        public static Texture<byte> LoadRaw8(string inputFilename, int width, int height)
        {
            var texture = new Texture<byte>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                for (int y = 0; y < height; y++)
                    stream.Read(texture.Data[y], 0, width);
            }
            return texture;
        }
        public static Texture<short> LoadRaw16(string inputFilename, int width, int height)
        {
            var texture = new Texture<short>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[2 * width];
                for (int y = 0; y < height; y++)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    Buffer.BlockCopy(buffer, 0, texture.Data[y], 0, buffer.Length);
                }
            }
            return texture;
        }

        public static Texture<float> LoadRaw32f(string inputFilename, int width, int height)
        {
            var texture = new Texture<float>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[4 * width];
                for (int y = 0; y < height; y++)
                {
                    stream.Read(buffer, 0, buffer.Length);

                    Buffer.BlockCopy(buffer, 0, texture.Data[y], 0, buffer.Length);
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
                image.Endian = Endian.LSB;
                buffer = image.ToByteArray();
            }

            var texture = new Texture<short>(width, height);

            int idx = 0;
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(buffer, idx, texture.Data[y], 0, width * 2);
                idx += width * 2;
            }

            return texture;
        }


        public static Texture<byte> LoadAny8(string inputFilename)
        {
            int width;
            int height;
            byte[] buffer;
            using (var image = new MagickImage(inputFilename))
            {
                width = image.Width;
                height = image.Height;

                image.Format = MagickFormat.Gray;
                image.Endian = Endian.LSB;
                buffer = image.ToByteArray();
            }

            var texture = new Texture<byte>(width, height);

            int idx = 0;
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(buffer, idx, texture.Data[y], 0, width);
                idx += width;
            }

            return texture;
        }

        public static void SaveRaw8(string outputFilename, Texture<byte> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                SaveRaw8(outputStream, texture);
            }
        }

        public static void SaveRaw8(Stream outputStream, Texture<byte> texture)
        {
            for (int y = 0; y < texture.Height; y++)
                outputStream.Write(texture.Data[y], 0, texture.Width);
        }

        public static void SaveRaw16(string outputFilename, Texture<short> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                SaveRaw16(outputStream, texture);
            }
        }

        public static void SaveRaw16(Stream outputStream, Texture<short> texture)
        {
            var buffer = new byte[2 * texture.Width];

            for (int y = 0; y < texture.Height; y++)
            {
                Buffer.BlockCopy(texture.Data[y], 0, buffer, 0, buffer.Length);
                outputStream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void SavePng8(string outputFilename, Texture<byte> texture)
        {
            var buffer = new byte[texture.Width * texture.Height];

            int idx = 0;
            for (int y = 0; y < texture.Height; y++)
            {
                Buffer.BlockCopy(texture.Data[y], 0, buffer, idx, texture.Width);
                idx += texture.Width;
            }

            using (var image = new MagickImage(buffer, new MagickReadSettings { Format = MagickFormat.Gray, Width = texture.Width, Height = texture.Height}))
            {
                image.Format = MagickFormat.Png;
                image.Write(outputFilename);
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

        public static Texture<B> Convert<A,B>(Texture<A> inputTexture, Func<A, B> func)
        {
            int width = inputTexture.Width;
            int height = inputTexture.Height;
            var outputTexture = new Texture<B>(width, height);
            for(int y =0;y<height;y++)
            {
                var inputLine = inputTexture.Data[y];
                var outputLine = outputTexture.Data[y];
                for(int x =0;x<width;x++)
                    outputLine[x] = func(inputLine[x]);
            }
            return outputTexture;
        }

        public static void Process<A>(Texture<A> inputTexture, Func<A, A> func)
        {
            int width = inputTexture.Width;
            int height = inputTexture.Height;
            for(int y =0;y<height;y++)
            {
                var line = inputTexture.Data[y];
                for(int x =0;x<width;x++)
                    line[x] = func(line[x]);
            }
        }

    }
}