
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace PlanetBuilder.Planets
{
    public class MarsSector : Planet
    {
        public int NumSegments;
        private Texture<short> _elevationTexture;
        private Texture<short> _elevationTextureBlur;

        public MarsSector()
        {
            PlanetRadius = 3396190;
            ElevationScale = 4;
            NumSegments = 1200;
            PlanetProjection = Projection.Equirectangular;
        }

        public void Create()
        {
             Stopwatch sw;

            int width = 11520;
            int height = 5760;
            string elevationTextureFilename = $@"Generated\Planets\MarsSector\Mars{width}x{height}.raw";
            if (!File.Exists(elevationTextureFilename))
            {
                sw = Stopwatch.StartNew();
                using(var elevationTextureLarge = new TiffTexture<short>(@"Datasets\Planets\Mars\Mars_HRSC_MOLA_BlendDEM_Global_200mp.tif"))
                {
                    elevationTextureLarge.Width--; // Right-most pixel column in the Mars dataset is broken. This trick will skip it.

                    _elevationTexture = Resampler.Resample(elevationTextureLarge, width, height).ToTexture();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }

//                TextureHelper.SaveRaw16($@"Generated\Planets\MarsSector\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = TextureHelper.LoadRaw16(elevationTextureFilename, width, height);
            }
  //          TextureHelper.SavePng8($@"Generated\Planets\MarsSector\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.png", _elevationTexture);


            width = 2880;
            height = 1440;
            string elevationTextureBlurFilename = $@"Generated\Planets\MarsSector\MarsBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                var elevationTextureSmall = Resampler.Resample(_elevationTexture, width, height);

                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(PlanetProjection);
                _elevationTextureBlur = blurFilter.Blur3(elevationTextureSmall, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                TextureHelper.SaveRaw16($@"Generated\Planets\MarsSector\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
                TextureHelper.SavePng8($@"Generated\Planets\MarsSector\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.png", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = TextureHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }


            sw = Stopwatch.StartNew();

            var sphericalSector = new SphericalSector();
            sphericalSector.ComputeRadiusTop = ComputeModelElevationTop;
            sphericalSector.ComputeRadiusBottom = ComputeModelElevationBottom;

            sphericalSector.Create(MathHelper.ToRadians(-10), MathHelper.ToRadians(133), MathHelper.ToRadians(0), MathHelper.ToRadians(142), NumSegments, NumSegments);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\MarsSector\MarsSector{NumSegments}.stl");
        }

        private double ComputeModelElevationTop(Vector3d v, double lat, double lon)
        {
            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short h = ReadBilinearPixel(_elevationTexture, tx, ty);
            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + hAvg;

            return r * 0.00001;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            double ty = (Math.PI / 2 - lat) / Math.PI;
            double tx = (Math.PI + lon) / (Math.PI * 2);

            short hAvg = ReadBilinearPixel(_elevationTextureBlur, tx, ty);

            double r = PlanetRadius - 100000 + hAvg;

            return r * 0.00001;
        }
    }
}