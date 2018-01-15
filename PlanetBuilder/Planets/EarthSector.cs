
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

        public bool UseAster;

        public EarthSector()
        {
            PlanetRadius = 6371000;
            ElevationScale = 1.5;
            NumSegmentsLon = 1200;
            NumSegmentsLat = 1200;
        }

        public void Create()
        {
            var targetPath = $@"Generated\Planets\Earth\{Name}";
            Directory.CreateDirectory(targetPath);

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

            if (UseAster)
            {
                using (var demReader = new DemZipTiffReader(@"Datasets\Planets\Earth\ASTER.zip", "ASTGTM2_{0}_dem.tif", 3601, 3601))
                {
                    var elevationSectorBitmap = demReader.LoadBitmap(lat0deg, lon0deg, lat1deg, lon1deg);
                    //_elevationSectorBitmap = elevationSectorBitmap.ToBitmap();
                    _elevationSectorBitmap = Resampler.Resample(elevationSectorBitmap, NumSegmentsLon, NumSegmentsLat).ToBitmap();
                }
            }
            else
            {
                using (var demReader = new DemZipRawReader(@"Datasets\Planets\Earth\SRTM.zip", "{0}.hgt", 3601, 3601))
                {
                    var elevationSectorBitmap = demReader.LoadBitmap(lat0deg, lon0deg, lat1deg, lon1deg);
                    _elevationSectorBitmap = elevationSectorBitmap.ToBitmap();
                }
            }
            Console.WriteLine($"Loading image sector {_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height} used {sw.Elapsed}");

            using (var tiffWriter = new TiffWriter(File.Create(Path.Combine(targetPath, $"Earth{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.tif"))))
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

            SaveSTL(Path.Combine(targetPath, $"{Name}{NumSegmentsLon}_{ElevationScale}x.stl"));
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

            //if (h > 0)
                r += h * ElevationScale;
            // else
            //     r -= 50 * ElevationScale;

            return r * 0.0005;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            double r = PlanetRadius - 1000 * ElevationScale;

            return r * 0.0005;
        }
    }
}