
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder.Planets
{
    public class Charon : Planet
    {
        public int RecursionLevel;

        private Bitmap<short> _elevationTextureSmall;
        private Bitmap<short> _elevationTextureBlur;

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
            var elevationTextureLarge = TextureHelper.LoadTiff16(@"Datasets\Planets\Charon\Charon_NewHorizons_Global_DEM_300m_Jul2017_16bit.tif");
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600).ToBitmap();
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            TextureHelper.SaveRaw16($@"Generated\Planets\Charon\Charon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            TextureHelper.SavePng8($@"Generated\Planets\Charon\Charon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTextureSmall, MathHelper.ToRadians(10), (h) => { return h != -32640 ? (short?)h : null;});
            Console.WriteLine($"Blur used {sw.Elapsed}");

            TextureHelper.SaveRaw16($@"Generated\Planets\Charon\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            TextureHelper.SavePng8($@"Generated\Planets\Charon\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Charon\Charon{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius;
            if(h != -32640)
                r += (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}