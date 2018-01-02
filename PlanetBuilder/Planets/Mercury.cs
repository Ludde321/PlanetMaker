
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Mercury : Planet
    {

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
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Mercury\Mercury_Messenger_USGS_DEM_Global_665m_v2.tif")))
                {
                    var elevationTextureLarge = tiffReader.ReadImageFile<short>();
                    _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }
    
                BitmapHelper.SaveRaw16($@"Generated\Planets\Mercury\Mercury{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            BitmapHelper.SavePng8($@"Generated\Planets\Mercury\Mercury{_elevationTexture.Width}x{_elevationTexture.Height}.png", _elevationTexture);

            string elevationTextureBlurFilename = $@"Generated\Planets\Mercury\MercuryBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Mercury\MercuryBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
            BitmapHelper.SavePng8($@"Generated\Planets\Mercury\MercuryBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Mercury\Mercury{RecursionLevel}.stl");
        }

    }
}