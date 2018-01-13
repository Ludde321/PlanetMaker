
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class MarsDouble : Planet
    {
        public int NumSegments;
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

        public MarsDouble()
        {
            // PlanetRadius = 3396190;
            // ElevationScale = 2.5;
            NumSegments = 1150;
            // PlanetProjection = Projection.Equirectangular;

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
                // _topElevationBitmap = Resampler.Resample(topElevationBitmap, 899, 899).ToBitmap();
                _topElevationBitmap = topElevationBitmap.ToBitmap();
                Console.WriteLine($"Loading image top {_topElevationBitmap.Width}x{_topElevationBitmap.Height} sector used {sw.Elapsed}");

                using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\MarsDouble\MarsTop.tif")))
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
                // _bottomElevationBitmap = Resampler.Resample(bottomElevationBitmap, 899, 899).ToBitmap();
                _bottomElevationBitmap = bottomElevationBitmap.ToBitmap();
                Console.WriteLine($"Loading image bottom {_bottomElevationBitmap.Width}x{_bottomElevationBitmap.Height} sector used {sw.Elapsed}");

                using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\MarsDouble\MarsBottom.tif")))
                {
                    var bitmap = _bottomElevationBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                    tiffWriter.WriteImageFile(bitmap);
                }

            }

            sw = Stopwatch.StartNew();

            var sphericalSector = new Box();
            sphericalSector.ComputeRadiusTop = ComputeModelElevationTop;
            sphericalSector.ComputeRadiusBottom = ComputeModelElevationBottom;

            sphericalSector.Create(NumSegments, NumSegments);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            Console.WriteLine($"NumVertexes: {PlanetVertexes.Count()}");
            Console.WriteLine($"NumTriangles: {PlanetTriangles.Count()}");

            SaveSTL($@"Generated\Planets\MarsDouble\MarsDouble{NumSegments}.stl");
        }


        private double ComputeModelElevationTop(double tu, double tv)
        {
            return 0.07 + 0.000015 * _topElevationBitmap.ReadBilinearPixel(tu, tv, false, false);
        }
        private double ComputeModelElevationBottom(double tu, double tv)
        {
            return -0.07 - 0.000017 * _bottomElevationBitmap.ReadBilinearPixel(1 - tu, tv, false, false);
        }
    }
}