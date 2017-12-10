using System;
using Xunit;
using TiffExpress;
using System.IO;
using TiffExpress.Tiff;

namespace Test.TiffExpress
{
    public class ReadImageTest
    {
        [Fact]
        public void TestReadGray8()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\gray8.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(64, ifd.ImageWidth);
                Assert.Equal(64, ifd.ImageHeight);
                Assert.Equal(8, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
                var bitmap = tiffReader.ReadImageFile<byte>(ifd).ToBitmap();
            }
        }

        [Fact]
        public void TestReadGray16()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\gray16.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(64, ifd.ImageWidth);
                Assert.Equal(64, ifd.ImageHeight);
                Assert.Equal(16, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
                var bitmap = tiffReader.ReadImageFile<short>(ifd).ToBitmap();
            }
        }

        [Fact]
        public void TestReadGray16Msb()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\gray16_msb.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(64, ifd.ImageWidth);
                Assert.Equal(64, ifd.ImageHeight);
                Assert.Equal(16, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
                var bitmap = tiffReader.ReadImageFile<short>(ifd).ToBitmap();
            }
        }

        [Fact]
        public void TestReadGray32()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\gray32.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(64, ifd.ImageWidth);
                Assert.Equal(64, ifd.ImageHeight);
                Assert.Equal(32, ifd.BitsPerSample);
                Assert.Equal(1, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
                var bitmap = tiffReader.ReadImageFile<float>(ifd).ToBitmap();
            }
        }

        [Fact]
        public void TestReadRgbChunky()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\rgb_chunky.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(64, ifd.ImageWidth);
                Assert.Equal(64, ifd.ImageHeight);
                Assert.Equal(8, ifd.BitsPerSample);
                Assert.Equal(3, ifd.SamplesPerPixel);
                Assert.Equal(Compression.NoCompression, ifd.Compression);
                var bitmap = tiffReader.ReadImageFile<byte>(ifd).ToBitmap();
            }
        }
    }
}
