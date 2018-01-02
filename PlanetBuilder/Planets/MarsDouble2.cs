
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageMagick;
using PlanetBuilder.Roam;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class MarsDouble2 : RoamSquare
    {
        public ushort MaxLevels;
        private Bitmap<short> _topElevationBitmap;
        private Bitmap<short> _bottomElevationBitmap;

        private int _topSectorOffsetX;
        private int _topSectorWidth;
        private int _topSectorHeight;
        private int _topSectorOffsetY;
        private int _bottomSectorOffsetX;
        private int _bottomSectorWidth;
        private int _bottomSectorHeight;
        private int _bottomSectorOffsetY;
        public double LatT0;
        public double LonT0;
        public double LatT1;
        public double LonT1;
        public double LatB0;
        public double LonB0;
        public double LatB1;
        public double LonB1;

        public MarsDouble2()
        {
            MaxLevels = 21;

            // Gale crater 5.4°S 137.8°E
            LatT0 = MathHelper.ToRadians(-5.4 + 3.0);
            LonT0 = MathHelper.ToRadians(137.8 - 3.0);
            LatT1 = MathHelper.ToRadians(-5.4 - 3.0);
            LonT1 = MathHelper.ToRadians(137.8 + 3.0);

            // NE Syrtis 18°N 77°E 
            LatB0 = MathHelper.ToRadians(18.0 + 3.0);
            LonB0 = MathHelper.ToRadians(77.0 - 3.0);
            LatB1 = MathHelper.ToRadians(18.0 - 3.0);
            LonB1 = MathHelper.ToRadians(77.0 + 3.0);
        }

        public void Create()
        {
            Stopwatch sw;

            sw = Stopwatch.StartNew();
            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Mars\Mars_HRSC_MOLA_BlendDEM_Global_200mp.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];

                _topSectorOffsetY = (int)(ifd.ImageHeight * (Math.PI / 2 - LatT0) / Math.PI);
                _topSectorOffsetX = (int)(ifd.ImageWidth * (Math.PI + LonT0) / (Math.PI * 2));

                _topSectorWidth = (int)Math.Ceiling(ifd.ImageHeight * (LatT0 - LatT1) / Math.PI);
                _topSectorHeight = (int)Math.Ceiling(ifd.ImageWidth * (LonT1 - LonT0) / (Math.PI * 2));

                var topElevationBitmap = tiffReader.ReadImageFile<short>(ifd, _topSectorOffsetX, _topSectorOffsetY, _topSectorWidth, _topSectorHeight);
                _topElevationBitmap = topElevationBitmap/*Resampler.Resample(topElevationBitmap, 1024, 1024)*/.ToBitmap();
                Console.WriteLine($"Loading image top {_topElevationBitmap.Width}x{_topElevationBitmap.Height} sector used {sw.Elapsed}");

                using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\MarsDouble2\MarsTop.tif")))
                {
                    var bitmap = _topElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                    tiffWriter.WriteImageFile(bitmap);
                }

                //--
                _bottomSectorOffsetY = (int)(ifd.ImageHeight * (Math.PI / 2 - LatB0) / Math.PI);
                _bottomSectorOffsetX = (int)(ifd.ImageWidth * (Math.PI + LonB0) / (Math.PI * 2));

                _bottomSectorWidth = (int)Math.Ceiling(ifd.ImageHeight * (LatB0 - LatB1) / Math.PI);
                _bottomSectorHeight = (int)Math.Ceiling(ifd.ImageWidth * (LonB1 - LonB0) / (Math.PI * 2));

                var bottomElevationBitmap = tiffReader.ReadImageFile<short>(ifd, _bottomSectorOffsetX, _bottomSectorOffsetY, _bottomSectorWidth, _bottomSectorHeight);
                _bottomElevationBitmap = bottomElevationBitmap/*Resampler.Resample(bottomElevationBitmap, 1024, 1024)*/.ToBitmap();
                Console.WriteLine($"Loading image bottom {_bottomElevationBitmap.Width}x{_bottomElevationBitmap.Height} sector used {sw.Elapsed}");

                using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\MarsDouble2\MarsBottom.tif")))
                {
                    var bitmap = _bottomElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                    tiffWriter.WriteImageFile(bitmap);
                }

            }

            Init();

            sw = Stopwatch.StartNew();
            Split();
            Console.WriteLine($"Time used to split planet geometry: {sw.Elapsed}");
            PrintSummary();

            sw = Stopwatch.StartNew();
            Merge();
            Console.WriteLine($"Time used to merge planet geometry: {sw.Elapsed}");
            PrintSummary();

            SaveStl("Generated/Planets/MarsDouble2/MarsDouble2.stl");
        }

        private readonly double _maxCos2Angle = Math.Pow(Math.Cos(MathHelper.ToRadians(4)), 2);

        protected override bool MergeDiamond(RoamDiamond diamond)
        {
            var n1 = diamond.Triangles1.Vertexes2.Position - diamond.Triangles0.Vertexes0.Position;
            var n2 = diamond.Triangles0.Vertexes1.Position - diamond.Triangles0.Vertexes0.Position;

            double dot12 = Vector3d.Dot(n1, n2);
            double cos2A = (dot12 * dot12) / (n1.Abs2() * n2.Abs2());

            return cos2A >= _maxCos2Angle;
        }

        protected override bool SubdivideTriangle(RoamTriangle triangle)
        {
            if (triangle.Material > 1)
                return false;
            return triangle.Level < MaxLevels;
        }

        protected override void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
        {
            vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes0.LinearPosition, triangle.Vertexes2.LinearPosition);
            //vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

            var tex = Vector2d.MiddlePoint(triangle.TextureCoords0, triangle.TextureCoords2);

            double z = vertex.LinearPosition.z;
            if (triangle.Material == 0)
            {
                z += 0.17 + 0.000030 * _topElevationBitmap.ReadBilinearPixel(tex.x, tex.y, false, false);
            }
            else if (triangle.Material == 1)
            {
                z -= 0.22 + 0.000034 * _bottomElevationBitmap.ReadBilinearPixel(tex.x, tex.y, false, false);
            }

            vertex.Position = new Vector3d(vertex.LinearPosition.x, vertex.LinearPosition.y, z);
        }

        protected override void ComputeVertexAltitude(RoamVertex vertex, Vector3d position)
        {
            vertex.LinearPosition = position;
            //vertex.Normal = Vector3d.Normalize(position);
            vertex.Position = position;
        }
    }
}