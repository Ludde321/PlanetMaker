
using System;
using System.Diagnostics;

namespace PlanetBuilder
{
    public class Ceres : Planet
    {
        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Ceres()
        {
            PlanetRadius = 470000;
            ElevationScale = 2;
            RecursionLevel = 8;
        }

        public void Create()
        {
            var sw = Stopwatch.StartNew();
            var elevationTextureLarge = TextureHelper.LoadRaw16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres_Dawn_FC_HAMO_DTM_DLR_Global_60ppd_Oct2016.raw", 21600, 10800);
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            var resampler = new Resampler();
            _elevationTextureSmall = resampler.Resample(elevationTextureLarge, 1200, 600);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

//            TextureHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres360x180.raw", _elevationTextureSmall);

            var blurFilter = new BlurFilter(_elevationTextureSmall);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

//            TextureHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\CeresBlur360x180.raw", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes();
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + h;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}