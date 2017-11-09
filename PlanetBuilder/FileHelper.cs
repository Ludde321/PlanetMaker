using System;
using System.IO;

namespace PlanetBuilder
{
    public static class FileHelper
    {
        public static Texture16 LoadRaw16(string inputFilename, int width, int height)
        {
            var map = new short[width * height];

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[1024 * 1024];

                int p = 0;
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; )
                    {
                        byte b0 = buffer[i++];
                        byte b1 = buffer[i++];
                        map[p++] = (short)((b0 << 8) + b1);
                    }
                }
            }
            return new Texture16(map, width, height);
        }

        public static void SaveFile16(string outputFilename, Texture16 texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                var buffer = new byte[2 * texture.Data.Length];

                int p = 0;
                for (int i = 0; i < texture.Data.Length; i++)
                {
                    short h = texture.Data[i];

                    buffer[p++] = (byte)(h >> 8);
                    buffer[p++] = (byte)h;
                }

                outputStream.Write(buffer, 0, buffer.Length);
            }
        }


    }
}