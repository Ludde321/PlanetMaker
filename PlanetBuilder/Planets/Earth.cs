
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder
{
    public class Earth : Planet
    {
        public int RecursionLevel;
        private Texture<short> _elevationTextureSmall;
        private Texture<byte> _landcoverTextureSmall;
        private Texture<short> _elevationTextureBlur;

        public Earth()
        {
            PlanetRadius = 6371000;
            ElevationScale = 8;
            RecursionLevel = 10;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;

            // Topo Bathymetry
            string elevationTextureSmallFilename = $@"Planets\Earth\Generated\topo.bathymetry.{width}x{height}.raw";
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = TextureHelper.LoadRaw16(@"Planets\Earth\Datasets\Blue Marble\topo.bathymetry.43200x21600.raw", 43200, 21600);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                TextureHelper.Process(elevationTextureLarge, (p) => { return (short)(p - 32768); });

                sw = Stopwatch.StartNew();
                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height);
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Planets\Earth\Generated\topo.bathymetry.{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = TextureHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            TextureHelper.SavePng8($@"Planets\Earth\Generated\topo.bathymetry.{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            // Landcover
            string landcoverTextureSmallFilename = $@"Planets\Earth\Generated\landcover.{width}x{height}.raw";
            if (!File.Exists(landcoverTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var landcoverTextureLarge = TextureHelper.LoadRaw8(@"Planets\Earth\Datasets\Blue Marble\landcover.43200x21600.raw", 43200, 21600);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                var histo = new long[256];

                TextureHelper.Process(landcoverTextureLarge, (p) => { histo[p]++; return p != 0 ? (byte)0xff : (byte)0x00; });

                sw = Stopwatch.StartNew();
                _landcoverTextureSmall = Resampler.Resample(landcoverTextureLarge, width, height);
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                TextureHelper.SaveRaw8($@"Planets\Earth\Generated\landcover.{width}x{height}.raw", _landcoverTextureSmall);
            }
            else
            {
                _landcoverTextureSmall = TextureHelper.LoadRaw8(landcoverTextureSmallFilename, width, height);
            }
            TextureHelper.SavePng8($@"Planets\Earth\Generated\landcover.{width}x{height}.png", _landcoverTextureSmall);



            // string elevationTextureBlurFilename = $@"Planets\Earth\Generated\EarthBlur{width}x{height}.raw";
            // if(!File.Exists(elevationTextureBlurFilename))
            // {
            //     sw = Stopwatch.StartNew();
            //     var blurFilter = new BlurFilter(PlanetProjection);
            //     _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, 10 * (Math.PI / 180));
            //     Console.WriteLine($"Blur used {sw.Elapsed}");

            //     TextureHelper.SaveRaw16($@"Planets\Earth\Generated\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            // }
            // else
            // {
            //     _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            // }
            //     TextureHelper.SavePng8($@"Planets\Earth\Generated\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            // sw = Stopwatch.StartNew();
            // CreatePlanetVertexes(RecursionLevel);
            // Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            // SaveSTL($@"Planets\Earth\Generated\Earth{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            double lat = Math.PI / 2 - Math.Acos(v.y);
            double lon = Math.Atan2(v.x, v.z);

            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTextureSmall, tx, ty);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}