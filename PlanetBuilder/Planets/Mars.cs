
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Mars : Planet
    {

        public Mars()
        {
            PlanetRadius = 3396190;
            ElevationScale = 20;
            RecursionLevel = 9;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
             Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureFilename = $@"Generated\Planets\Mars\Mars{width}x{height}.raw";
            if (!File.Exists(elevationTextureFilename))
            {
                sw = Stopwatch.StartNew();
                using(var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Mars\Mars_HRSC_MOLA_BlendDEM_Global_200mp.tif")))
                {
                    // Right-most pixel column in the Mars dataset is broken. This trick will skip it.
                    var ifd = tiffReader.ImageFileDirectories[0];
                    var elevationTextureLarge = tiffReader.ReadImageFile<short>(0, 0, ifd.ImageWidth - 1, ifd.ImageHeight);

                    _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }

                BitmapHelper.SaveRaw16($@"Generated\Planets\Mars\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadRaw16(elevationTextureFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Mars\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            // var sw = Stopwatch.StartNew();
            // var elevationTextureLarge = TextureHelper.LoadTiff16(@"Datasets\Planets\Mars\Mars_MGS_MOLA_DEM_mosaic_global_463m.tif");
            // Console.WriteLine($"Loading texture used {sw.Elapsed}");
            
            // sw = Stopwatch.StartNew();
            // _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 2400, 1200);
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            // TextureHelper.SaveRaw16($@"Generated\Planets\Mars\Mars{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            // TextureHelper.SaveTiff8($@"Generated\Planets\Mars\Mars{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.tif", _elevationTextureSmall);

            string elevationTextureBlurFilename = $@"Generated\Planets\Mars\MarsBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Mars\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Mars\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            // var blurFilter = new BlurFilter(PlanetProjection);
            // sw = Stopwatch.StartNew();
            // _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
            // Console.WriteLine($"Blur used {sw.Elapsed}");

            // TextureHelper.SaveRaw16($@"Generated\Planets\Mars\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            // TextureHelper.SaveTiff8($@"Generated\Planets\Mars\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Mars\Mars{RecursionLevel}_{(int)ElevationScale}x.stl");
        }


    }
}