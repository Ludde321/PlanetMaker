
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class EarthSector : Planet
    {
        public int NumSegments;
        private Bitmap<short> _elevationSectorBitmap;
        private Bitmap<byte> _landcoverSectorBitmap;

        private int _elevationWidth;
        private int _elevationHeight;

        private int _sectorOffsetX;
        private int _sectorWidth;
        private int _sectorHeight;
        private int _sectorOffsetY;
        public double Lat0;
        public double Lon0;
        public double Lat1;
        public double Lon1;

        private double _sx, _sy, _sx0, _sy0;

        public EarthSector()
        {
            PlanetRadius = 6371000;
            ElevationScale = 5;
            NumSegments = 800;
            PlanetProjection = Projection.Equirectangular;

            // Bylot Island 73.25N, 78.68W
            Lat0 = MathHelper.ToRadians(73.25 + 3.0);
            Lon0 = MathHelper.ToRadians(-78.68 - 3.0);
            Lat1 = MathHelper.ToRadians(73.25 - 3.0);
            Lon1 = MathHelper.ToRadians(-78.68 + 3.0);

            // Disco Island 69.81°N 53.47°W 
            // Lat0 = MathHelper.ToRadians(69.81 + 3.0);
            // Lon0 = MathHelper.ToRadians(-53.47 - 3.0);
            // Lat1 = MathHelper.ToRadians(69.81 - 3.0);
            // Lon1 = MathHelper.ToRadians(-53.47 + 3.0);

        }

        public void Create()
        {
            double dLat = Lat0 - Lat1;
            double dLon = Lon1 - Lon0;

            // Calculate sector transform
            _sx = Math.PI * 2 / dLon;
            _sy = Math.PI / dLat;
            _sx0 = (Math.PI + Lon0) / (Math.PI * 2) * _sx;
            _sy0 = (Math.PI / 2 - Lat0) / Math.PI * _sy;

            Stopwatch sw;

            sw = Stopwatch.StartNew();

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

            _elevationWidth = elevationTextureLarge.Width;
            _elevationHeight = elevationTextureLarge.Height;

            // --
            _sectorOffsetY = (int)(_elevationHeight * (Math.PI / 2 - Lat0) / Math.PI);
            _sectorOffsetX = (int)(_elevationWidth * (Math.PI + Lon0) / (Math.PI * 2));

            _sectorWidth = (int)Math.Ceiling(_elevationHeight * dLat / Math.PI);
            _sectorHeight = (int)Math.Ceiling(_elevationWidth * dLon / (Math.PI * 2));

            _elevationSectorBitmap = BitmapTools.Crop(elevationTextureLarge, _sectorOffsetX, _sectorOffsetY, _sectorWidth, _sectorHeight).ToBitmap();
            _landcoverSectorBitmap = BitmapTools.Crop(landcoverTextureLarge, _sectorOffsetX, _sectorOffsetY, _sectorWidth, _sectorHeight).ToBitmap();
            Console.WriteLine($"Loading image sector used {sw.Elapsed}");

            // _elevationSectorBitmap = Resampler.Resample(elevationBitmap, width, height).ToBitmap();
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\EarthSector\Earth{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.tif")))
            {
                var bitmap = _elevationSectorBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                tiffWriter.WriteImageFile(bitmap);
            }

            sw = Stopwatch.StartNew();

            var sphericalSector = new SphericalSector();
            sphericalSector.ComputeRadiusTop = ComputeModelElevationTop;
            sphericalSector.ComputeRadiusBottom = ComputeModelElevationBottom;

            sphericalSector.Create(Lat0, Lon0, Lat1, Lon1, NumSegments, NumSegments);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\EarthSector\EarthSector{NumSegments}.stl");
        }

        private double ComputeModelElevationTop(Vector3d v, double lat, double lon)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double sy = t.y * _sy - _sy0;
            double sx = t.x * _sx - _sx0;

            double h = _elevationSectorBitmap.ReadBilinearPixel(sx, sy, true, false);

            double r = PlanetRadius + h * ElevationScale;

            return r * 0.00001;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            double r = PlanetRadius - 50000;

            return r * 0.00001;
        }
    }
}