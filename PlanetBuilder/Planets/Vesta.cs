
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Vesta : Planet
    {
        public int RecursionLevel;
        private Bitmap<float> _elevationTextureSmall;
        private Bitmap<float> _elevationTextureBlur;

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

            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Vesta\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd.tif")))
            {
                var elevationTextureLarge = tiffReader.ReadImageFile<float>();

                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            // var elevationTextureLarge = BitmapHelper.LoadRaw32f(@"Datasets\Planets\Vesta\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd.raw", 17280, 8640);
            // Console.WriteLine($"Loading texture used {sw.Elapsed}");

            // sw = Stopwatch.StartNew();
            // _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            var textureSmall = _elevationTextureSmall.Convert((h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Generated\Planets\Vesta\Vesta{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            BitmapHelper.SavePng8($@"Generated\Planets\Vesta\Vesta{textureSmall.Width}x{textureSmall.Height}.png", textureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTextureSmall, MathHelper.ToRadians(10));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            var textureBlur = _elevationTextureBlur.Convert((h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Generated\Planets\Vesta\VestaBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            BitmapHelper.SavePng8($@"Generated\Planets\Vesta\VestaBlur{textureBlur.Width}x{textureBlur.Height}.png", textureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Vesta\Vesta{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            float h = _elevationTextureSmall.ReadBilinearPixel(t.x, t.y, true, false);
            float hAvg = _elevationTextureBlur.ReadBilinearPixel(t.x, t.y, true, false);

            double r = (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}