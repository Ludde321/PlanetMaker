
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets.Mars
{
    public class PlanetMars : Planet
    {

        public PlanetMars()
        {
            PlanetRadius = 3396190;
            ElevationScale = 20;
            RecursionLevel = 9;
            Name = "PlanetMars";
        }

        public void Create()
        {
            var targetPath = $@"Generated\Planets\Mars\{Name}";
            Directory.CreateDirectory(targetPath);

             Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureFilename = $@"Generated\Planets\Mars\{Name}\Mars{width}x{height}.tif";
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

                BitmapHelper.SaveTiff16(elevationTextureFilename, _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadTiff16(elevationTextureFilename);
            }

            string elevationTextureBlurFilename = $@"Generated\Planets\Mars\{Name}\MarsBlur{width}x{height}.raw";
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
                _elevationTextureBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Mars\{Name}\Mars{RecursionLevel}_{(int)ElevationScale}x.stl");
        }

    }
}