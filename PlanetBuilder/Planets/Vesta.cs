
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Vesta : Planet
    {
        private Bitmap<float> _elevationTextureFloat;
        private Bitmap<float> _elevationTextureBlurFloat;

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

                _elevationTextureFloat = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            // var elevationTextureLarge = BitmapHelper.LoadRaw32f(@"Datasets\Planets\Vesta\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd.raw", 17280, 8640);
            // Console.WriteLine($"Loading texture used {sw.Elapsed}");

            // sw = Stopwatch.StartNew();
            // _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            var textureSmall = _elevationTextureFloat.Convert((h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Generated\Planets\Vesta\Vesta{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Vesta\Vesta{textureSmall.Width}x{textureSmall.Height}.tif", textureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlurFloat = blurFilter.Blur2(_elevationTextureFloat, MathHelper.ToRadians(10));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            var textureBlur = _elevationTextureBlur.Convert((h) => {return (short)(h-PlanetRadius);});
            // TextureHelper.SaveFile16($@"Generated\Planets\Vesta\VestaBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Vesta\VestaBlur{textureBlur.Width}x{textureBlur.Height}.tif", textureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Vesta\Vesta{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double h = _elevationTextureFloat.ReadBilinearPixel(t.x, t.y, true, false);
            double hAvg = _elevationTextureBlurFloat.ReadBilinearPixel(t.x, t.y, true, false);

            double r = (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}