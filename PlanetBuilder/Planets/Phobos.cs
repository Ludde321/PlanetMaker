
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Phobos : Planet
    {

        public Phobos()
        {
            PlanetRadius = 11100;
            ElevationScale = 5;
            RecursionLevel = 7;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            sw = Stopwatch.StartNew();
           using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Phobos\Phobos_ME_HRSC_DEM_Global_2ppd.tif")))
           {
                _elevationTexture = tiffReader.ReadImageFile<short>().ToBitmap();
                Console.WriteLine($"Loading texture used {sw.Elapsed}");
           }

            BitmapHelper.SaveTiff8($@"Generated\Planets\Phobos\Phobos{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            sw = Stopwatch.StartNew();
            var blurFilter = new BlurFilter(PlanetProjection);
            _elevationTexture = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(2));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Phobos\Phobos{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double h = _elevationTexture.ReadBilinearPixel(t.x, t.y, true, false);

            double r = PlanetRadius + h;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}