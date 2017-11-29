
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder.Planets
{
    public class Phobos : Planet
    {
        public int RecursionLevel;
        private Bitmap<short> _elevationTexture;

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
            _elevationTexture = TextureHelper.LoadTiff16(@"Datasets\Planets\Phobos\Phobos_ME_HRSC_DEM_Global_2ppd.tif");
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            TextureHelper.SavePng8($@"Generated\Planets\Phobos\Phobos{_elevationTexture.Width}x{_elevationTexture.Height}.png", _elevationTexture);

            sw = Stopwatch.StartNew();
            var blurFilter = new BlurFilter(PlanetProjection);
            _elevationTexture = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(2));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Phobos\Phobos{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTexture, tx, ty);

            double r = PlanetRadius + h;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}