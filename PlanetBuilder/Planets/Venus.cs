
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder.Planets
{
    public class Venus : Planet
    {
        public int RecursionLevel;
        private Bitmap<short> _elevationTextureSmall;
        private Bitmap<short> _elevationTextureBlur;

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
            if(!File.Exists(elevationTextureSmallFilename))
            {
                sw = Stopwatch.StartNew();
                var elevationTextureLarge = TextureHelper.LoadRaw16(@"Datasets\Planets\Venus\Venus_Magellan_Topography_Global_4641m_v02_ca2.raw", 8192, 4096);
                Console.WriteLine($"Loading texture used {sw.Elapsed}");

                // sw = Stopwatch.StartNew();
                // var elevationTextureLarge8 = TextureHelper.LoadAny8(@"Datasets\Planets\Venus\4kVenus.png");
                // var elevationTextureLarge = TextureHelper.Convert(elevationTextureLarge8, (p) => {return (short)(p*64);});
                // Console.WriteLine($"Loading texture used {sw.Elapsed}");

                sw = Stopwatch.StartNew();
                _elevationTextureSmall = Resampler.Resample(elevationTextureLarge, width, height).ToBitmap();
                Console.WriteLine($"Resampling used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\Venus\Venus{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.raw", _elevationTextureSmall);
            }
            else
            {
                _elevationTextureSmall = TextureHelper.LoadRaw16(elevationTextureSmallFilename, width, height);
            }
                TextureHelper.SavePng8($@"Generated\Planets\Venus\Venus{_elevationTextureSmall.Width}x{_elevationTextureSmall.Height}.png", _elevationTextureSmall);

            string elevationTextureBlurFilename = $@"Generated\Planets\Venus\VenusBlur{width}x{height}.raw";
            if(!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTextureSmall, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\Venus\VenusBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
                TextureHelper.SavePng8($@"Generated\Planets\Venus\VenusBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);

            sw = Stopwatch.StartNew();
            CreatePlanetVertexes(RecursionLevel);
            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\Venus\Venus{RecursionLevel}.stl");
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