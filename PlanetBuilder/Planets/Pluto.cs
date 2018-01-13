
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Pluto : Planet
    {

        public Pluto()
        {
            PlanetRadius = 1188300;
            ElevationScale = 4;
            RecursionLevel = 8;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            var sw = Stopwatch.StartNew();

            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Pluto\Pluto_NewHorizons_Global_DEM_300m_Jul2017_16bit.tif")))
            {
                var elevationTextureLarge = tiffReader.ReadImageFile<short>();

                _elevationTexture = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            BitmapHelper.SaveTiff16($@"Generated\Planets\Pluto\Pluto{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            BitmapHelper.SaveTiff16($@"Generated\Planets\Pluto\PlutoBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Pluto\Pluto{RecursionLevel}.stl");
        }


    }
}