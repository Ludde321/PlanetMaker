
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Ceres : Planet
    {
        public int RecursionLevel;
        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Ceres()
        {
            PlanetRadius = 470000;
            ElevationScale = 2;
            RecursionLevel = 8;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            var sw = Stopwatch.StartNew();
            var elevationTextureLarge = TextureHelper.LoadTiff16(@"Datasets\Planets\Ceres\Ceres_Dawn_FC_HAMO_DTM_DLR_Global_60ppd_Oct2016.tif");
            Console.WriteLine($"Loading texture used {sw.Elapsed}");
            
            sw = Stopwatch.StartNew();
            _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            TextureHelper.SaveRaw16($@"Generated\Planets\Ceres\Ceres{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            TextureHelper.SavePng8($@"Generated\Planets\Ceres\Ceres{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTextureSmall, 10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            TextureHelper.SaveRaw16($@"Generated\Planets\Ceres\CeresBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            TextureHelper.SavePng8($@"Generated\Planets\Ceres\CeresBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Ceres\Ceres{RecursionLevel}.stl");
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