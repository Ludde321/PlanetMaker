
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Charon : Planet
    {
        public int RecursionLevel;

        private Texture<short> _elevationTextureSmall;
        private Texture<short> _elevationTextureBlur;

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
            var elevationTextureLarge = TextureHelper.LoadTiff16(@"Planets\Charon\Datasets\Charon_NewHorizons_Global_DEM_300m_Jul2017_16bit.tif");
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, 1200, 600);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            TextureHelper.SaveFile16($@"Planets\Charon\Generated\Charon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            TextureHelper.SavePng8($@"Planets\Charon\Generated\Charon{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            var blurFilter = new BlurFilter(PlanetProjection);
            sw = Stopwatch.StartNew();
            _elevationTextureBlur = blurFilter.Blur2(_elevationTextureSmall, 10 * (Math.PI / 180), (h) => { return h != -32640 ? (short?)h : null;});
            Console.WriteLine($"Blur used {sw.Elapsed}");

            TextureHelper.SaveFile16($@"Planets\Charon\Generated\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            TextureHelper.SavePng8($@"Planets\Charon\Generated\CharonBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Planets\Charon\Generated\Charon{RecursionLevel}.stl");
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