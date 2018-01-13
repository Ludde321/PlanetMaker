
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Ceres : Planet
    {

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
            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Ceres\Ceres_Dawn_FC_HAMO_DTM_DLR_Global_60ppd_Oct2016.tif")))
            {
                var elevationTextureLarge = tiffReader.ReadImageFile<short>();

                _elevationTexture = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            BitmapHelper.SaveRaw16($@"Generated\Planets\Ceres\Ceres{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Ceres\Ceres{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            BitmapHelper.SaveRaw16($@"Generated\Planets\Ceres\CeresBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Ceres\CeresBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Ceres\Ceres{RecursionLevel}.stl");
        }

    }
}