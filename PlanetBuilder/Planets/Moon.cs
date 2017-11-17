
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Moon : Planet
    {
        public int RecursionLevel;
        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Moon()
        {
            PlanetRadius = 1737400;
            ElevationScale = 2.5;
            RecursionLevel = 11;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureSmallFilename = $@"Planets\Moon\Generated\Moon{width}x{height}.raw";
            if(!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = TextureHelper.LoadRaw16(@"Planets\Moon\Datasets\Lunar_LRO_LOLA_Global_LDEM_118m_Mar2014.gray", 92160, 46080);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");
                
                sw = Stopwatch.StartNew();
                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height);
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Planets\Moon\Generated\Moon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = TextureHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
                TextureHelper.SavePng8($@"Planets\Moon\Generated\Moon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            string elevationTextureBlurFilename = $@"Planets\Moon\Generated\MoonBlur{width}x{height}.raw";
            if(!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, 10 * (Math.PI / 180));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Planets\Moon\Generated\MoonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
                TextureHelper.SavePng8($@"Planets\Moon\Generated\MoonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Planets\Moon\Generated\Moon{RecursionLevel}.stl");
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