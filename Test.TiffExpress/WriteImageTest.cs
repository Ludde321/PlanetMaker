using System;
using Xunit;
using TiffExpress;
using TiffExpress.Tiff;
using System.IO;

namespace Test.TiffExpress
{
    public class WriteImageTest
    {
        [Fact]
        public void TestWriteGray8()
        {
            var bitmap = new Bitmap<byte>(32, 32);
            CreateGradient(bitmap);

            var tempFile = Path.Combine(Path.GetTempPath(), "TestWriteGray8.tif");

            using (var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
            {
                var ifd = tiffWriter.WriteImageFile(bitmap);

                Assert.Equal(32, ifd.ImageWidth);
                Assert.Equal(32, ifd.ImageHeight);
                Assert.Equal(8, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
            }

            var bitmap2 = TiffReader.LoadBitmap<byte>(File.OpenRead(tempFile));
        }

        [Fact]
        public void TestWriteGray16()
        {
            var bitmap = new Bitmap<ushort>(32, 32);
            CreateGradient(bitmap);

            var tempFile = Path.Combine(Path.GetTempPath(), "TestWriteGray16.tif");

            using (var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
            {
                var ifd = tiffWriter.WriteImageFile(bitmap);

                Assert.Equal(32, ifd.ImageWidth);
                Assert.Equal(32, ifd.ImageHeight);
                Assert.Equal(16, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
            }

            var bitmap2 = TiffReader.LoadBitmap<short>(File.OpenRead(tempFile));
        }

        [Fact]
        public void TestWriteGray32()
        {
            var bitmap = new Bitmap<float>(32, 32);
            CreateGradient(bitmap);

            var tempFile = Path.Combine(Path.GetTempPath(), "TestWriteGray32.tif");

            using (var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
            {
                var ifd = tiffWriter.WriteImageFile(bitmap);

                Assert.Equal(32, ifd.ImageWidth);
                Assert.Equal(32, ifd.ImageHeight);
                Assert.Equal(32, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
            }

            var bitmap2 = TiffReader.LoadBitmap<float>(File.OpenRead(tempFile));
        }

        [Fact]
        public void TestWriteRGB8()
        {
            var bitmap = new Bitmap<byte>(32, 32, 3);
            CreateGradient(bitmap);

            var tempFile = Path.Combine(Path.GetTempPath(), "TestWriteRGB8.tif");

            using (var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
            {
                var ifd = tiffWriter.WriteImageFile(bitmap);

                Assert.Equal(32, ifd.ImageWidth);
                Assert.Equal(32, ifd.ImageHeight);
                Assert.Equal(8, ifd.BitsPerSample);
                Assert.Equal(3, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
            }

            var bitmap2 = TiffReader.LoadBitmap<short>(File.OpenRead(tempFile));
        }

        private void CreateGradient(Bitmap<byte> bitmap)
        {
            if (bitmap.SamplesPerPixel == 1)
            {
                int scale = 256 / (bitmap.Height + bitmap.Width);
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                        bitmap.Rows[y][x] = (byte)((x + y) * scale);
            }
            else if (bitmap.SamplesPerPixel == 3)
            {
                int scale = 256 / (bitmap.Height + bitmap.Width);
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int xxx = x+x+x;
                        bitmap.Rows[y][xxx++] = (byte)((x + y) * scale);
                        bitmap.Rows[y][xxx++] = (byte)((bitmap.Width - 1 - x + y) * scale);
                        bitmap.Rows[y][xxx++] = (byte)((bitmap.Height + bitmap.Width - x - y - 2) * scale);
                    }
            }
        }

        private void CreateGradient(Bitmap<ushort> bitmap)
        {
            int scale = 65536 / (bitmap.Height + bitmap.Width);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    bitmap.Rows[y][x] = (ushort)((x + y) * scale);
        }

        private void CreateGradient(Bitmap<float> bitmap)
        {
            float scale = 1f / (bitmap.Height + bitmap.Width);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    bitmap.Rows[y][x] = ((x + y) * scale);
        }
    }
}
