
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Vesta : Planet
    {
        private Texture<float> _elevationTextureSmall;
        private Texture<float> _elevationTextureBlur;

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
            var elevationTextureLarge = TextureHelper.LoadRaw32f(@"Planets\Vesta\Datasets\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd.raw", 17280, 8640);
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            var textureSmall = TextureHelper.Convert(_elevationTextureSmall, (h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Planets\Vesta\Generated\Vesta{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            TextureHelper.SavePng8($@"Planets\Vesta\Generated\Vesta{textureSmall.Width}x{textureSmall.Height}.png", textureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTextureSmall, 10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            var textureBlur = TextureHelper.Convert(_elevationTextureBlur, (h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Planets\Vesta\Generated\VestaBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            TextureHelper.SavePng8($@"Planets\Vesta\Generated\VestaBlur{textureBlur.Width}x{textureBlur.Height}.png", textureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes();
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveX3d($@"Planets\Vesta\Generated\Vesta{RecursionLevel}.x3d");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            float h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
            float hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}