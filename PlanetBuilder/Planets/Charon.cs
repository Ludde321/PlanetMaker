
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Charon : Planet
    {

        public Charon()
        {
            PlanetRadius = 606000;
            ElevationScale = 4;
            RecursionLevel = 8;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            var sw = Stopwatch.StartNew();
            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Charon\Charon_NewHorizons_Global_DEM_300m_Jul2017_16bit.tif")))
            {
                var elevationTextureLarge = tiffReader.ReadImageFile<short>();

                _elevationTexture = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");
            }

            BitmapHelper.SaveRaw16($@"Generated\Planets\Charon\Charon{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Charon\Charon{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTexture, MathHelper.ToRadians(10), (h) => { return h != -32640 ? (short?)h : null; });
            Console.WriteLine($"Blur used {sw.Elapsed}");

            BitmapHelper.SaveRaw16($@"Generated\Planets\Charon\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            BitmapHelper.SaveTiff8($@"Generated\Planets\Charon\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Charon\Charon{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double h = _elevationTexture.ReadBilinearPixel(t.x, t.y, true, false);
            double hAvg = _elevationTextureBlur.ReadBilinearPixel(t.x, t.y, true, false);

            double r = PlanetRadius;
            if (h != -32640)
                r += (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}