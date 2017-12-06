
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Pluto : Planet
    {
        public int RecursionLevel;
        private Bitmap<short> _elevationTextureSmall;
        private Bitmap<short> _elevationTextureBlur;

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

                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            BitmapHelper.SaveTiff16($@"Generated\Planets\Pluto\Pluto{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.tif", _elevationTextureSmall);
            BitmapHelper.SavePng8($@"Generated\Planets\Pluto\Pluto{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            BitmapHelper.SaveTiff16($@"Generated\Planets\Pluto\PlutoBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);
            BitmapHelper.SavePng8($@"Generated\Planets\Pluto\PlutoBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Pluto\Pluto{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            short h = ReadBilinearPixel(_elevationTextureSmall, t.x, t.y);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, t.x, t.y);

            double r = PlanetRadius + h * ElevationScale;//(h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}