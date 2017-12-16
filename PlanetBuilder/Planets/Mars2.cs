
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;
using PlanetBuilder.Roam;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class Mars2 : RoamPlanet
    {
        private Bitmap<short> _elevationTexture;
        private Bitmap<short> _elevationTextureBlur;

        public Mars2()
        {
            PlanetRadius = 3396190;
            ElevationScale = 10;
            MaxLevels = 19;
        }

        public void Create()
        {
            Stopwatch sw;

            int width = 2880;
            int height = 1440;
            string elevationTextureFilename = $@"Generated\Planets\Mars2\Mars{width}x{height}.raw";
            if (!File.Exists(elevationTextureFilename))
            {
                sw = Stopwatch.StartNew();
                using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Mars\Mars_HRSC_MOLA_BlendDEM_Global_200mp.tif")))
                {
                    // Right-most pixel column in the Mars dataset is broken. This trick will skip it.
                    var ifd = tiffReader.ImageFileDirectories[0];
                    var elevationTexture = tiffReader.ReadImageFile<short>(0, 0, ifd.ImageWidth - 1, ifd.ImageHeight);

                    _elevationTexture = Resampler.Resample(elevationTexture, width, height).ToBitmap();
                    Console.WriteLine($"Resampling used {sw.Elapsed}");
                }

                BitmapHelper.SaveRaw16($@"Generated\Planets\Mars2\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.raw", _elevationTexture);
            }
            else
            {
                _elevationTexture = BitmapHelper.LoadRaw16(elevationTextureFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Mars2\Mars{_elevationTexture.Width}x{_elevationTexture.Height}.tif", _elevationTexture);

            // Blur
            string elevationTextureBlurFilename = $@"Generated\Planets\Mars2\MarsBlur{width}x{height}.raw";
            if (!File.Exists(elevationTextureBlurFilename))
            {
                sw = Stopwatch.StartNew();
                var blurFilter = new BlurFilter(Projection.Equirectangular);
                _elevationTextureBlur = blurFilter.Blur3(_elevationTexture, MathHelper.ToRadians(10));
                Console.WriteLine($"Blur used {sw.Elapsed}");

                BitmapHelper.SaveRaw16($@"Generated\Planets\Mars2\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.raw", _elevationTextureBlur);
            }
            else
            {
                _elevationTextureBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
            }
            BitmapHelper.SaveTiff8($@"Generated\Planets\Mars2\MarsBlur{_elevationTextureBlur.Width}x{_elevationTextureBlur.Height}.tif", _elevationTextureBlur);

            Init();

            sw = Stopwatch.StartNew();
            Split();
            Console.WriteLine($"Time used to split planet geometry: {sw.Elapsed}");
            PrintSummary();

            sw = Stopwatch.StartNew();
            Merge();
            Console.WriteLine($"Time used to merge planet geometry: {sw.Elapsed}");
            PrintSummary();

            SaveStl("Generated/Planets/Mars2/Mars2.stl");
        }

        public void Init()
        {
            var material = new RoamMaterialMars(this);
            base.Init(material);
        }

        protected override bool SubdivideTriangle(RoamTriangle triangle)
        {
            return triangle.Level < MaxLevels;
        }

        private readonly double _maxCos2Angle = Math.Pow(Math.Cos(MathHelper.ToRadians(5)), 2);

        protected override bool MergeDiamond(RoamDiamond diamond)
        {
            var n1 = diamond.Triangles[1].Vertexes[2].Position - diamond.Triangles[0].Vertexes[0].Position;
            var n2 = diamond.Triangles[0].Vertexes[1].Position - diamond.Triangles[0].Vertexes[0].Position;

            double dot12 = Vector3d.Dot(n1, n2);
            double cos2A = (dot12 * dot12) / (n1.Abs2() * n2.Abs2());

            return cos2A >= _maxCos2Angle;
        }

        public class RoamMaterialMars : RoamMaterial
        {
            public readonly Mars2 Planet;

            public RoamMaterialMars(Mars2 planet)
            {
                Planet = planet;
            }

            public override void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
            {
                vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes[0].LinearPosition, triangle.Vertexes[2].LinearPosition);
                vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

                var t = MathHelper.SphericalToTextureCoords(vertex.Normal);

                short h = Planet._elevationTexture.ReadBilinearPixel(t.x, t.y);
                short hAvg = Planet._elevationTextureBlur.ReadBilinearPixel(t.x, t.y);

                double r = Planet.PlanetRadius + (h - hAvg) * Planet.ElevationScale + hAvg;

                vertex.Position = Vector3d.Multiply(vertex.Normal, r);
            }

            public override void ComputeVertexAltitude(RoamVertex vertex, Vector3d normal)
            {
                var t = MathHelper.SphericalToTextureCoords(vertex.Normal);

                short h = Planet._elevationTexture.ReadBilinearPixel(t.x, t.y);
                short hAvg = Planet._elevationTextureBlur.ReadBilinearPixel(t.x, t.y);

                double r = Planet.PlanetRadius + (h - hAvg) * Planet.ElevationScale + hAvg;

                vertex.Normal = normal;
                vertex.Position = Vector3d.Multiply(vertex.Normal, r);
                vertex.LinearPosition = vertex.Position;
            }

        }

    }


}