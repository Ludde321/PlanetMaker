
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
            ElevationScale = 2;
            NumSegmentsLon = 1200;
            NumSegmentsLat = 1200;
            PlanetProjection = Projection.Equirectangular;

            // Bylot Island 73.25N, 78.68W
            // Name = "Bylot";
            // Lat0 = MathHelper.ToRadians(73.25 + 0.80);
            // Lon0 = MathHelper.ToRadians(-78.65 - 2.85);
            // Lat1 = MathHelper.ToRadians(73.25 - 0.80);
            // Lon1 = MathHelper.ToRadians(-78.65 + 2.85);

            // Disco Island 69.81°N 53.47°W 
            // Name = "Disco";
            // Lat0 = MathHelper.ToRadians(69.81 + 0.75);
            // Lon0 = MathHelper.ToRadians(-53.47 - 2.0);
            // Lat1 = MathHelper.ToRadians(69.81 - 0.75);
            // Lon1 = MathHelper.ToRadians(-53.47 + 2.0);

            // Kazbek 42°41′57″N 44°31′06″ECoordinates: 42°41′57″N 44°31′06″E [1]
            Name = "Kazbek";
            Lat0 = MathHelper.ToRadians(42.677 + 0.160);
            Lon0 = MathHelper.ToRadians(44.565 - 0.20);
            Lat1 = MathHelper.ToRadians(42.677 - 0.160);
            Lon1 = MathHelper.ToRadians(44.615 + 0.20);
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
            // using (var tiffElevationReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Earth\ViewFinderPanoramas\dem15.tif")))
            //using (var tiffElevationReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Earth\ViewFinderPanoramas\dem3_N75W085_N65W050.tif")))
            using (var tiffElevationReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Earth\SRTM_N48E040_N40E048.tif")))
            {
                var ifd = tiffElevationReader.ImageFileDirectories[0];

                _elevationWidth = 3601 * 360;//ifd.ImageWidth;
                _elevationHeight = 3601 * 180;//ifd.ImageHeight;

                // _elevationWidth = ifd.ImageWidth;
                // _elevationHeight = ifd.ImageHeight;


                double latOffset = MathHelper.ToRadians(89 - 48);
                double lonOffset = MathHelper.ToRadians(180 + 40);

                // --
                _sectorOffsetY = (int)(_elevationHeight * (Math.PI / 2 - Lat0 - latOffset) / Math.PI);
                _sectorOffsetX = (int)(_elevationWidth * (Math.PI + Lon0 - lonOffset) / (Math.PI * 2));

                _sectorHeight = (int)Math.Ceiling(_elevationHeight * dLat / Math.PI);
                _sectorWidth = (int)Math.Ceiling(_elevationWidth * dLon / (Math.PI * 2));

                _elevationSectorBitmap = tiffElevationReader.ReadImageFile<short>(_sectorOffsetX, _sectorOffsetY, _sectorWidth, _sectorHeight).ToBitmap();
                Console.WriteLine($"Loading image sector {_sectorWidth}x{_sectorHeight} used {sw.Elapsed}");
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

            sphericalSector.Create(Lat0, Lon0, Lat1, Lon1, NumSegmentsLat, NumSegmentsLon, 0.0005 * (PlanetRadius - 10));

            CenterVertexes(sphericalSector.Vertexes);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\EarthSector\{Name}{NumSegmentsLon}_{ElevationScale}x.stl");
        }

        private void CenterVertexes(List<Vector3d> vertexes)
        {
            var vMin = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            var vMax = new Vector3d(double.MinValue, double.MinValue, double.MinValue);

            foreach (var v in vertexes)
            {
                vMin = Vector3d.Min(vMin, v);
                vMax = Vector3d.Max(vMax, v);
            }

            var vCenter = Vector3d.MiddlePoint(vMin, vMax);

            for (int i = 0; i < vertexes.Count; i++)
                vertexes[i] -= vCenter;
        }

        private double ComputeModelElevationTop(Vector3d v, double lat, double lon)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double sy = t.y * _sy - _sy0;
            double sx = t.x * _sx - _sx0;

            double h = _elevationSectorBitmap.ReadBilinearPixel(sx, sy, true, false);

            double r = PlanetRadius;
            if (h > 0)
                r += h * ElevationScale;
            // else
            //     r -= 50 * ElevationScale;

            return r * 0.0005;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            double r = PlanetRadius + 1000;

            return r * 0.0005;
        }
    }
}