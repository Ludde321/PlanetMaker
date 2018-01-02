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
    public class EarthBylotIsland2 : RoamSquare
    {
        public ushort MaxLevels;
        private Bitmap<short> _topElevationBitmap;
        private Bitmap<short> _bottomElevationBitmap;
        private Bitmap<byte> _topLandcoverBitmap;
        private Bitmap<byte> _bottomLandcoverBitmap;

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

        public EarthBylotIsland2()
        {
            MaxLevels = 22;

            // Bylot Island 73.25N, 78.68W
            LatT0 = MathHelper.ToRadians(73.25 + 3.0);
            LonT0 = MathHelper.ToRadians(-78.68 - 3.0);
            LatT1 = MathHelper.ToRadians(73.25 - 3.0);
            LonT1 = MathHelper.ToRadians(-78.68 + 3.0);

            // Disco Island 69.81°N 53.47°W 
            LatB0 = MathHelper.ToRadians(69.81 + 3.0);
            LonB0 = MathHelper.ToRadians(-53.47 - 3.0);
            LatB1 = MathHelper.ToRadians(69.81 - 3.0);
            LonB1 = MathHelper.ToRadians(-53.47 + 3.0);
        }

        public void Create()
        {
            Stopwatch sw;

            // Topo Bathymetry
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

            // Landcover
            sw = Stopwatch.StartNew();
            var landcoverTextureLargeW = BitmapHelper.LoadRaw8(@"Datasets\Planets\Earth\Blue Marble\landcover.W.21600x21600.raw", 21600, 21600);
            var landcoverTextureLargeE = BitmapHelper.LoadRaw8(@"Datasets\Planets\Earth\Blue Marble\landcover.E.21600x21600.raw", 21600, 21600);
            var landcoverTextureLarge = BitmapTools.Concatenate(landcoverTextureLargeW, landcoverTextureLargeE);
            Console.WriteLine($"Loading landcover texture used {sw.Elapsed}");

            landcoverTextureLarge.Process((p) => { return p != 0 ? (byte)0xff : (byte)0x00; });

            int width = elevationTextureLarge.Width;
            int height = elevationTextureLarge.Height;

            // --
            _topSectorOffsetY = (int)(height * (Math.PI / 2 - LatT0) / Math.PI);
            _topSectorOffsetX = (int)(width * (Math.PI + LonT0) / (Math.PI * 2));

            _topSectorWidth = (int)Math.Ceiling(height * (LatT0 - LatT1) / Math.PI);
            _topSectorHeight = (int)Math.Ceiling(width * (LonT1 - LonT0) / (Math.PI * 2));

            var topElevationBitmap = BitmapTools.Crop(elevationTextureLarge, _topSectorOffsetX, _topSectorOffsetY, _topSectorWidth, _topSectorHeight);
            _topElevationBitmap = topElevationBitmap/*Resampler.Resample(topElevationBitmap, 1024, 1024)*/.ToBitmap();

            var topLandcoverBitmap = BitmapTools.Crop(landcoverTextureLarge, _topSectorOffsetX, _topSectorOffsetY, _topSectorWidth, _topSectorHeight);
            _topLandcoverBitmap = topLandcoverBitmap/*Resampler.Resample(topElevationBitmap, 1024, 1024)*/.ToBitmap();
            
            Console.WriteLine($"Loading image top {_topElevationBitmap.Width}x{_topElevationBitmap.Height} sector used {sw.Elapsed}");

            using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\EarthBylotIsland\BylotIslandTop.tif")))
            {
                var bitmap = _topElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                tiffWriter.WriteImageFile(bitmap);
            }

            //--
            _bottomSectorOffsetY = (int)(height * (Math.PI / 2 - LatB0) / Math.PI);
            _bottomSectorOffsetX = (int)(width * (Math.PI + LonB0) / (Math.PI * 2));

            _bottomSectorWidth = (int)Math.Ceiling(height * (LatB0 - LatB1) / Math.PI);
            _bottomSectorHeight = (int)Math.Ceiling(width * (LonB1 - LonB0) / (Math.PI * 2));

            var bottomElevationBitmap = BitmapTools.Crop(elevationTextureLarge, _bottomSectorOffsetX, _bottomSectorOffsetY, _bottomSectorWidth, _bottomSectorHeight);
            _bottomElevationBitmap = bottomElevationBitmap/*Resampler.Resample(bottomElevationBitmap, 1024, 1024)*/.ToBitmap();

            var bottomLandcoverBitmap = BitmapTools.Crop(landcoverTextureLarge, _bottomSectorOffsetX, _bottomSectorOffsetY, _bottomSectorWidth, _bottomSectorHeight);
            _bottomLandcoverBitmap = bottomLandcoverBitmap/*Resampler.Resample(bottomElevationBitmap, 1024, 1024)*/.ToBitmap();

            Console.WriteLine($"Loading image bottom {_bottomElevationBitmap.Width}x{_bottomElevationBitmap.Height} sector used {sw.Elapsed}");

            using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\EarthBylotIsland\DiscoIslandBottom.tif")))
            {
                var bitmap = _bottomElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                tiffWriter.WriteImageFile(bitmap);
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

            SaveStl("Generated/Planets/EarthBylotIsland/EarthBylotIsland.stl");
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