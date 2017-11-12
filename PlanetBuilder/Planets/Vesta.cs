
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Vesta : Planet
    {
        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Vesta()
        {
            PlanetRadius = 255000;
            ElevationScale = 1.5;
            RecursionLevel = 8;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            var sw = Stopwatch.StartNew();
            var elevationTextureLarge = TextureHelper.LoadRaw16(@"Planets\Vesta\Datasets\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd_16bit_msb_cropped.raw", 17280, 8640);
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            var resampler = new Resampler();
            _elevationTextureSmall = resampler.Resample(elevationTextureLarge, 1200, 600);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            TextureHelper.SaveFile16($@"Planets\Vesta\Generated\Vesta{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            TextureHelper.SavePng8($@"Planets\Vesta\Generated\Vesta{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            var blurFilter = new BlurFilter(_elevationTextureSmall, PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            TextureHelper.SaveFile16($@"Planets\Vesta\Generated\VestaBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            TextureHelper.SavePng8($@"Planets\Vesta\Generated\VestaBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes();
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveX3d($@"Planets\Vesta\Generated\Vesta{RecursionLevel}.x3d");
        }

        // protected override Vector3d ComputeModelElevation(Vector3d v)
        // {
        //     double lon = Math.Atan2(v.x, v.z);

        //     double ty = (1 - v.y) * 0.5;
        //     double tx = (Math.PI + lon) / (Math.PI * 2);

        //     short h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
        //     short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

        //     double r = PlanetRadius + h;//(h - hAvg) * ElevationScale + hAvg;

        //     return Vector3d.Multiply(v, r * 0.00001);
        // }

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