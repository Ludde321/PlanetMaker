
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Common;
using Common.Dem;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class EarthSector : Planet
    {
        public int NumSegmentsLon;
        public int NumSegmentsLat;
        private Bitmap<short> _elevationSectorBitmap;

        public double Lat0;
        public double Lon0;
        public double Lat1;
        public double Lon1;

        private double _sx, _sy, _sx0, _sy0;

        public EarthSector()
        {
            PlanetRadius = 6371000;
            ElevationScale = 1.5;
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
            Lat1 = MathHelper.ToRadians(42.677 - 0.160);
            Lon0 = MathHelper.ToRadians(44.590 - 0.225);
            Lon1 = MathHelper.ToRadians(44.590 + 0.225);
            // Lat0 = MathHelper.ToRadians(42.696 + 0.5);
            // Lon0 = MathHelper.ToRadians(44.514 - 0.5);
            // Lat1 = MathHelper.ToRadians(42.696 - 0.5);
            // Lon1 = MathHelper.ToRadians(44.514 + 0.5);
        }

        public void Create()
        {
            int lat0deg = (int)Math.Floor(MathHelper.ToDegrees(Lat0));
            int lat1deg = (int)Math.Floor(MathHelper.ToDegrees(Lat1));
            int lon0deg = (int)Math.Floor(MathHelper.ToDegrees(Lon0));
            int lon1deg = (int)Math.Floor(MathHelper.ToDegrees(Lon1));

            double lat0 = MathHelper.ToRadians(lat0deg + 1);
            double lat1 = MathHelper.ToRadians(lat1deg);
            double lon0 = MathHelper.ToRadians(lon0deg);
            double lon1 = MathHelper.ToRadians(lon1deg + 1);

            // Calculate sector transform
            _sx = Math.PI * 2 / (lon1 - lon0);
            _sy = Math.PI / (lat0 - lat1);
            _sx0 = (Math.PI + lon0) / (Math.PI * 2) * _sx;
            _sy0 = (Math.PI / 2 - lat0) / Math.PI * _sy;

            // --

            int totalWidth = 3601 * 360;
            int totalHeight = 3601 * 180;

            int sectorHeight = (int)Math.Round(totalHeight * (Lat0 - Lat1) / Math.PI);
            int sectorWidth = (int)Math.Round(totalWidth * (Lon1 - Lon0) / (Math.PI * 2));

            Console.WriteLine($"Using image sector {sectorWidth}x{sectorHeight}");

            var sw = Stopwatch.StartNew();

                        // using (var demReader = new DemZipTiffReader(@"\\luddepc\Earth2\ASTER.zip", "ASTGTM2_{0}_dem.tif", 3601, 3601))
            using (var demReader = new DemZipRawReader(@"\\luddepc\Earth2\SRTM.zip", "{0}.hgt", 3601, 3601))
            {
                var elevationSectorBitmap = demReader.LoadBitmap(lat0deg, lon0deg, lat1deg, lon1deg);
                _elevationSectorBitmap = elevationSectorBitmap.ToBitmap();

                Console.WriteLine($"Loading image sector {_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height} used {sw.Elapsed}");
            }

            // _elevationSectorBitmap = Resampler.Resample(elevationBitmap, width, height).ToBitmap();
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            // using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\EarthSector\Earth{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.tif")))
            // {
            //     var bitmap = _elevationSectorBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
            //     tiffWriter.WriteImageFile(bitmap);
            // }

            // short pMin = short.MaxValue;
            // short pMax = short.MinValue;
            // _elevationSectorBitmap.Process(p => 
            // { 
            //     pMin = Math.Min(pMin, p);
            //     pMax = Math.Max(pMax, p);
            //     return p;
            // });

            // Console.WriteLine($"Min: {pMin}, Max: {pMax}");

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

            using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\EarthSector\Earth{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.tif")))
            {
                var bitmap = _elevationSectorBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                tiffWriter.WriteImageFile(bitmap);
            }

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

            // _elevationSectorBitmap.Rows[(int)(sy * _elevationSectorBitmap.Height)][(int)(sx * _elevationSectorBitmap.Width)] = 0x7fff;

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