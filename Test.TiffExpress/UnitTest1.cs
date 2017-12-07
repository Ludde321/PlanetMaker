using System;
using Xunit;
using TiffExpress;
using System.IO;

namespace Test.TiffExpress
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using(var tiffReader = new TiffReader(File.OpenRead(@"TestData\gray8.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];
                Assert.Equal(ifd.ImageWidth, 64);
                Assert.Equal(ifd.ImageHeight, 64);
                Assert.Equal(ifd.BitsPerSample, 8);
                Assert.Equal(ifd.SamplesPerPixel, 1);
                var bitmap = tiffReader.ReadImageFile<byte>(ifd).ToBitmap();
            }
        }
    }
}
