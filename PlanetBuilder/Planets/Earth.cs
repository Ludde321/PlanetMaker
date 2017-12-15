
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Earth : Planet
    {
        public int RecursionLevel;
        private Bitmap<short> _elevationTextureSmall;
        private Bitmap<byte> _landcoverTextureSmall;
        //private Texture<short> _elevationTextureBlur;

        public Earth()
        {
            PlanetRadius = 6371000;
            ElevationScale = 15;
            RecursionLevel = 9;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;

            // Topo Bathymetry
            string elevationTextureSmallFilename = $@"Generated\Planets\Earth\topo.bathymetry.{width}x{height}.raw";
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = BitmapHelper.LoadRaw16(@"Datasets\Planets\Earth\Blue Marble\topo.bathymetry.43200x21600.raw", 43200, 21600);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                elevationTextureLarge.Process((p) => { return (short)(p - 32768); });

                sw = Stopwatch.StartNew();
                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Earth\topo.bathymetry.{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = BitmapHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            BitmapHelper.SavePng8($@"Generated\Planets\Earth\topo.bathymetry.{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            // Landcover
            string landcoverTextureSmallFilename = $@"Generated\Planets\Earth\landcover.{width}x{height}.raw";
            if (!File.Exists(landcoverTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var landcoverTextureLarge = BitmapHelper.LoadRaw8(@"Datasets\Planets\Earth\Blue Marble\landcover.43200x21600.raw", 43200, 21600);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                var histo = new long[256];

                landcoverTextureLarge.Process((p) => { histo[p]++; return p != 0 ? (byte)0xff : (byte)0x00; });

                sw = Stopwatch.StartNew();
                _landcoverTextureSmall = Resampler.Resample(landcoverTextureLarge, width, height).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                BitmapHelper.SaveRaw8($@"Generated\Planets\Earth\landcover.{width}x{height}.raw", _landcoverTextureSmall);
            }
            else
            {
                _landcoverTextureSmall = BitmapHelper.LoadRaw8(landcoverTextureSmallFilename, width, height);
            }
            BitmapHelper.SavePng8($@"Generated\Planets\Earth\landcover.{width}x{height}.png", _landcoverTextureSmall);



            // string elevationTextureBlurFilename = $@"Generated\Planets\Earth\EarthBlur{width}x{height}.raw";
            // if(!File.Exists(elevationTextureBlurFilename))
            // {
            //     sw = Stopwatch.StartNew();
            //     var blurFilter = new BlurFilter(PlanetProjection);
            //     _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
            //     Console.WriteLine($"Blur used {sw.Elapsed}");

            //     TextureHelper.SaveRaw16($@"Generated\Planets\Earth\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            // }
            // else
            // {
            //     _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            // }
            //     TextureHelper.SavePng8($@"Generated\Planets\Earth\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Earth\Earth{RecursionLevel}.stl");
        }

        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            short h = _elevationTextureSmall.ReadBilinearPixel(t.x, t.y);
            byte landcover = _landcoverTextureSmall.ReadBilinearPixel(t.x, t.y);
//            short hAvg = ReadBilinearPixel(_elevationTextureBlur, t.x, t.y);

            double r = PlanetRadius + h * ElevationScale;//(h - hAvg) * ElevationScale + hAvg;
            if(h < 0)
            {
            //    r = PlanetRadius - 8000 * (1 - landcover * 0.0078125);
             //   r -= 15000;
                r = PlanetRadius - 12000;
            }

            return Vector3d.Multiply(v, r * 0.00001);
        }
    }
}