
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Moon : Planet
    {

        public Moon()
        {
            PlanetRadius = 1737400;
            ElevationScale = 2.5;
            RecursionLevel = 9;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureSmallFilename = $@"Generated\Planets\Moon\Moon{width}x{height}.tif";
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                using(var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Moon\Lunar_LRO_LOLA_Global_LDEM_118m_Mar2014.tif")))
                {
                    var elevationTextureLarge = tiffReader.ReadImageFile<short>();
                    
                    _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }

                BitmapHelper.SaveTiff16(elevationTextureSmallFilename, _elevationTexture);
                BitmapHelper.SaveTiff8($@"Generated\Planets\Moon\Moon{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadTiff16(elevationTextureSmallFilename);
            }

            string elevationTextureBlurFilename = $@"Generated\Planets\Moon\MoonBlur{width}x{height}.tif";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                BitmapHelper.SaveTiff16(elevationTextureBlurFilename, _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = BitmapHelper.LoadTiff16(elevationTextureBlurFilename);
            }
            //BitmapHelper.SaveTiff8($@"Generated\Planets\Moon\MoonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Moon\Moon{RecursionLevel}.stl");
        }


    }
}