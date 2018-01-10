
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Venus : Planet
    {

        public Venus()
        {
            PlanetRadius = 6051000;
            ElevationScale = 14;
            RecursionLevel = 8;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
            Stopwatch sw;


            // var t1 = TextureHelper.LoadTiff16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02.tif");
            // for(int y = 0;y<t1.Height;y++)
            //     for(int x = 0;x<t1.Width;x++)
            //         {
            //             short h = t1.Data[y][x];
            //             if(h != -32678) // NoData
            //                 t1.Data[y][x] += 2951;
            //         }
            // TextureHelper.SaveRaw16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02.raw", t1);

            // var t1 = TextureHelper.LoadRaw16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02_ca2.raw", 8192, 4096);
            // for(int y = 0;y<t1.Height;y++)
            //     for(int x = 0;x<t1.Width;x++)
            //         t1.Data[y][x] -= 2951;
            // TextureHelper.SaveRaw16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02_ca2.raw", t1);

            int width = 2880;
            int height = 1440;
            string elevationTextureSmallFilename = $@"Generated\Planets\Venus\Venus{width}x{height}.raw";
            if (!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = BitmapHelper.LoadRaw16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02_ca2.raw", 8192, 4096);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                sw = Stopwatch.StartNew();
                _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Venus\Venus{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Venus\Venus{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            string elevationTextureBlurFilename = $@"Generated\Planets\Venus\VenusBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Venus\VenusBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Venus\VenusBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Venus\Venus{RecursionLevel}.stl");
        }

    }
}