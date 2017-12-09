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

            var tempFile = Path.GetTempFileName();

            using(var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
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
            var bitmap = new Bitmap<short>(32, 32);

            var tempFile = Path.GetTempFileName();

            using(var tiffWriter = new TiffWriter(File.OpenWrite(tempFile)))
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

    }
}
