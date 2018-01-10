using System;
using System.IO;
using TiffExpress;

namespace PlanetBuilder
{
    public static class BitmapHelper
    {
        public static Bitmap<byte> LoadRaw8(string inputFilename, int width, int height)
        {
            var texture = new Bitmap<byte>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                for (int y = 0; y < height; y++)
                    stream.Read(texture.Rows[y], 0, width);
            }
            return texture;
        }
        public static Bitmap<short> LoadRaw16(string inputFilename, int width, int height)
        {
            var texture = new Bitmap<short>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[2 * width];
                for (int y = 0; y < height; y++)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    Buffer.BlockCopy(buffer, 0, texture.Rows[y], 0, buffer.Length);
                }
            }
            return texture;
        }

        public static Bitmap<float> LoadRaw32f(string inputFilename, int width, int height)
        {
            var texture = new Bitmap<float>(width, height);

            using (var stream = File.OpenRead(String.Format(inputFilename, width, height)))
            {
                var buffer = new byte[4 * width];
                for (int y = 0; y < height; y++)
                {
                    stream.Read(buffer, 0, buffer.Length);

                    Buffer.BlockCopy(buffer, 0, texture.Rows[y], 0, buffer.Length);
                }
            }
            return texture;
        }

        public static Bitmap<short> LoadTiff16(string inputFilename)
        {
            using (var tiffReader = new TiffReader(File.OpenRead(inputFilename)))
            {
                return tiffReader.ReadImageFile<short>().ToBitmap();
            }
        }

        public static void SaveRaw8(string outputFilename, Bitmap<byte> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                SaveRaw8(outputStream, texture);
            }
        }

        public static void SaveRaw8(Stream outputStream, Bitmap<byte> texture)
        {
            for (int y = 0; y < texture.Height; y++)
                outputStream.Write(texture.Rows[y], 0, texture.Width);
        }

        public static void SaveRaw16(string outputFilename, Bitmap<short> texture)
        {
            using (var outputStream = new FileStream(String.Format(outputFilename, texture.Width, texture.Height), FileMode.Create))
            {
                SaveRaw16(outputStream, texture);
            }
        }

        public static void SaveRaw16(Stream outputStream, Bitmap<short> texture)
        {
            var buffer = new byte[2 * texture.Width];

            for (int y = 0; y < texture.Height; y++)
            {
                Buffer.BlockCopy(texture.Rows[y], 0, buffer, 0, buffer.Length);
                outputStream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void SaveTiff8(string outputFilename, IBitmap<byte> bitmap)
        {
            using (var tiffWriter = new TiffWriter(File.Create(outputFilename)))
            {
                tiffWriter.WriteImageFile(bitmap);
            }
        }
        public static void SaveTiff8(string outputFilename, IBitmap<short> bitmap)
        {
            using (var tiffWriter = new TiffWriter(File.Create(outputFilename)))
            {
                var bitmap2 = bitmap.Convert((p) => (byte)((p >> 8) + 128));
                tiffWriter.WriteImageFile(bitmap2);
            }
        }

        public static void SaveTiff16(string outputFilename, IBitmap<short> bitmap)
        {
            using (var tiffWriter = new TiffWriter(File.Create(outputFilename)))
            {
                tiffWriter.WriteImageFile(bitmap);
            }
        }

        public static void SaveTiff16(string outputFilename, IBitmap<ushort> bitmap)
        {
            var bitmap2 = bitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
            SaveTiff16(outputFilename, bitmap2);
        }
    }
}