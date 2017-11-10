using System;
using System.IO;

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

        public static void SaveFile16(string outputFilename, Texture<short> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
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
        }
    }
}