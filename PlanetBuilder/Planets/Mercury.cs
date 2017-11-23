
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder.Planets
{
    public class Mercury : Planet
    {
        public int RecursionLevel;
        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Mercury()
        {
            PlanetRadius = 2439400;
            ElevationScale = 2.5;
            RecursionLevel = 9;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureSmallFilename = $@"Generated\Planets\Mercury\Mercury{width}x{height}.raw";
            if(!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = TextureHelper.LoadTiff16(@"Datasets\Planets\Mercury\Mercury_Messenger_USGS_DEM_Global_665m_v2.tif");
                Console.WriteLine($"Loading texture used {sw.Elapsed}");
                
                sw = Stopwatch.StartNew();
                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height);
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\Mercury\Mercury{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = TextureHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
                TextureHelper.SavePng8($@"Generated\Planets\Mercury\Mercury{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            string elevationTextureBlurFilename = $@"Generated\Planets\Mercury\MercuryBlur{width}x{height}.raw";
            if(!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\Mercury\MercuryBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
                TextureHelper.SavePng8($@"Generated\Planets\Mercury\MercuryBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Mercury\Mercury{RecursionLevel}.stl");
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