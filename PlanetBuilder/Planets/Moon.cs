
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Moon : Planet
    {
        public int RecursionLevel;
        private Bitmap<short> _elevationTextureSmall;
        private Bitmap<short> _elevationTextureBlur;

        public Moon()
        {
            PlanetRadius = 1737400;
            ElevationScale = 2.5;
            RecursionLevel = 9;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureSmallFilename = $@"Generated\Planets\Moon\Moon{width}x{height}.raw";
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                using(var tiffFile = new TiffFile(File.OpenRead(@"Datasets\Planets\Moon\Lunar_LRO_LOLA_Global_LDEM_118m_Mar2014.tif")))
                {
                    var elevationTextureLarge = tiffFile.ReadImageFile<short>();
                    
                    _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }

                TextureHelper.SaveRaw16($@"Generated\Planets\Moon\Moon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = TextureHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            TextureHelper.SavePng8($@"Generated\Planets\Moon\Moon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            string elevationTextureBlurFilename = $@"Generated\Planets\Moon\MoonBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\Moon\MoonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
            TextureHelper.SavePng8($@"Generated\Planets\Moon\MoonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Moon\Moon{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}