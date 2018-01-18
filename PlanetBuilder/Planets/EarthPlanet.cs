using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class EarthPlanet : Planet
    {
        private Bitmap<byte> _landcoverTextureSmall;

        public EarthPlanet()
        {
            PlanetRadius = 6371000;
            ElevationScale = 15;
            RecursionLevel = 8;
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
                var elevationTextureLargeW = BitmapHelper.LoadRaw16(@"Datasets\Planets\Earth\Blue Marble\topo.bathymetry.W.21600x21600.raw", 21600, 21600);
                var elevationTextureLargeE = BitmapHelper.LoadRaw16(@"Datasets\Planets\Earth\Blue Marble\topo.bathymetry.E.21600x21600.raw", 21600, 21600);
                var elevationTextureLarge = BitmapTools.Concatenate(elevationTextureLargeW, elevationTextureLargeE);

                elevationTextureLarge = elevationTextureLarge.Convert((p) =>
                {
                    int p2 = ((p >> 8) & 0xff) | ((p<< 8) & 0xff00);
                    return (short)(p2 - 32768);
                });

                Console.WriteLine($"Loading elevation texture used {sw.Elapsed}");

                sw = Stopwatch.StartNew();
                _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Earth\topo.bathymetry.{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Earth\topo.bathymetry.{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            // Landcover
            string landcoverTextureSmallFilename = $@"Generated\Planets\Earth\landcover.{width}x{height}.raw";
            if (!File.Exists(landcoverTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var landcoverTextureLargeW = BitmapHelper.LoadRaw8(@"Datasets\Planets\Earth\Blue Marble\landcover.W.21600x21600.raw", 21600, 21600);
                var landcoverTextureLargeE = BitmapHelper.LoadRaw8(@"Datasets\Planets\Earth\Blue Marble\landcover.E.21600x21600.raw", 21600, 21600);
                var landcoverTextureLarge = BitmapTools.Concatenate(landcoverTextureLargeW, landcoverTextureLargeE);
                Console.WriteLine($"Loading landcover texture used {sw.Elapsed}");

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
            BitmapHelper.SaveTiff8($@"Generated\Planets\Earth\landcover.{width}x{height}.tif", _landcoverTextureSmall);



            // string elevationTextureBlurFilename = $@"Generated\Planets\Earth\EarthBlur{width}x{height}.raw";
            // if(!File.Exists(elevationTextureBlurFilename))
            // {
            //     sw = Stopwatch.StartNew();
            //     var blurFilter = new BlurFilter(PlanetProjection);
            //     _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
            //     Console.WriteLine($"Blur used {sw.Elapsed}");

            //     TextureHelper.SaveRaw16($@"Generated\Planets\Earth\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            // }
            // else
            // {
            //     _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            // }
            //     TextureHelper.SaveTiff8($@"Generated\Planets\Earth\EarthBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveStl($@"Generated\Planets\Earth\Earth{RecursionLevel}.stl");
        }



        protected override Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double h = _elevationTexture.ReadBilinearPixel(t.x, t.y, true, false);
            byte landcover = (byte)_landcoverTextureSmall.ReadBilinearPixel(t.x, t.y, true, false);
            //            short hAvg = ReadBilinearPixel(_elevationTextureBlur, t.x, t.y);

            double r = PlanetRadius + h * ElevationScale;//(h - hAvg) * ElevationScale + hAvg;
            if (h < 0)
            {
                //    r = PlanetRadius - 8000 * (1 - landcover * 0.0078125);
                //   r -= 15000;
                r = PlanetRadius - 12000;
            }

            return Vector3d.Multiply(v, r * ModelScale);
        }

    }
}