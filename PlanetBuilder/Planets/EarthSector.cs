
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
        public int NumSegmentsLon;
        public int NumSegmentsLat;
        private Bitmap<short> _elevationSectorBitmap;

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
            ElevationScale = 9;
            NumSegmentsLon = 800;
            NumSegmentsLat = 800;
            PlanetProjection = Projection.Equirectangular;

            // // Bylot Island 73.25N, 78.68W
            // Lat0 = MathHelper.ToRadians(73.25 + 1.0);
            // Lon0 = MathHelper.ToRadians(-78.68 - 3.0);
            // Lat1 = MathHelper.ToRadians(73.25 - 1.0);
            // Lon1 = MathHelper.ToRadians(-78.68 + 3.0);

            // Disco Island 69.81°N 53.47°W 
            Lat0 = MathHelper.ToRadians(69.81 + 1.0);
            Lon0 = MathHelper.ToRadians(-53.47 - 3.0);
            Lat1 = MathHelper.ToRadians(69.81 - 1.0);
            Lon1 = MathHelper.ToRadians(-53.47 + 3.0);
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
            using (var tiffElevationReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Earth\ViewFinderPanoramas\dem15.tif")))
            {
                var ifd = tiffElevationReader.ImageFileDirectories[0];

                _elevationWidth = ifd.ImageWidth;
                _elevationHeight = ifd.ImageHeight;

                // --
                _sectorOffsetY = (int)(_elevationHeight * (Math.PI / 2 - Lat0) / Math.PI);
                _sectorOffsetX = (int)(_elevationWidth * (Math.PI + Lon0) / (Math.PI * 2));

                _sectorHeight= (int)Math.Ceiling(_elevationHeight * dLat / Math.PI);
                _sectorWidth = (int)Math.Ceiling(_elevationWidth * dLon / (Math.PI * 2));

                _elevationSectorBitmap = tiffElevationReader.ReadImageFile<short>(_sectorOffsetX, _sectorOffsetY, _sectorWidth, _sectorHeight).ToBitmap();
                Console.WriteLine($"Loading image sector used {sw.Elapsed}");
            }

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

            sphericalSector.Create(Lat0, Lon0, Lat1, Lon1, NumSegmentsLat, NumSegmentsLon);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\EarthSector\Disco{NumSegmentsLon}.stl");
        }

        private double ComputeModelElevationTop(Vector3d v, double lat, double lon)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double sy = t.y * _sy - _sy0;
            double sx = t.x * _sx - _sx0;

            double h = _elevationSectorBitmap.ReadBilinearPixel(sx, sy, true, false);

            double r = PlanetRadius;
            if(h > 0)
                r += h * ElevationScale;
            // else
            //     r -= 50 * ElevationScale;

            return r * 0.00001;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            double r = PlanetRadius - 50000;

            return r * 0.00001;
        }
    }
}