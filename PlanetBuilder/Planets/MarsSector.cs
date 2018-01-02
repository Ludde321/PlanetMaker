
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageMagick;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public class MarsSector : Planet
    {
        public int NumSegments;
        private Bitmap<short> _elevationSectorBitmap;
        private Bitmap<short> _elevationBitmapBlur;

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

        public MarsSector()
        {
            PlanetRadius = 3396190;
            ElevationScale = 2.5;
            NumSegments = 800;
            PlanetProjection = Projection.Equirectangular;

            // Gale crater 5.4°S 137.8°E
            // Lat0 = MathHelper.ToRadians(-5.4 + 3.0);
            // Lon0 = MathHelper.ToRadians(137.8 - 3.0);
            // Lat1 = MathHelper.ToRadians(-5.4 - 3.0);
            // Lon1 = MathHelper.ToRadians(137.8 + 3.0);

            // Orcus Patera
            // Lat0 = MathHelper.ToRadians(18);
            // Lon0 = MathHelper.ToRadians(170);
            // Lat1 = MathHelper.ToRadians(8);
            // Lon1 = MathHelper.ToRadians(180);

            // Jezero crater 18.855°N 77.519°E 
            // Lat0 = MathHelper.ToRadians(18.855 + 5.0);
            // Lon0 = MathHelper.ToRadians(77.519 - 5.0);
            // Lat1 = MathHelper.ToRadians(18.855 - 5.0);
            // Lon1 = MathHelper.ToRadians(77.519 + 5.0);

            // Jezero crater AREA 18.855°N 77.519°E 
            // Lat0 = MathHelper.ToRadians(21.855 + 5.0);
            // Lon0 = MathHelper.ToRadians(76.519 - 5.0);
            // Lat1 = MathHelper.ToRadians(21.855 - 5.0);
            // Lon1 = MathHelper.ToRadians(76.519 + 5.0);

            // NE Syrtis 18°N 77°E 
            Lat0 = MathHelper.ToRadians(18.0 + 3.0);
            Lon0 = MathHelper.ToRadians(77.0 - 3.0);
            Lat1 = MathHelper.ToRadians(18.0 - 3.0);
            Lon1 = MathHelper.ToRadians(77.0 + 3.0);


            // Gusev crater 14.5°S 175.4°E (blurry area)
            // Lat0 = MathHelper.ToRadians(-14.5 + 2.5);
            // Lon0 = MathHelper.ToRadians(175.4 - 2.5);
            // Lat1 = MathHelper.ToRadians(-14.5 - 2.5);
            // Lon1 = MathHelper.ToRadians(175.4 + 2.5);
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
            using (var tiffReader = new TiffReader(File.OpenRead(@"Datasets\Planets\Mars\Mars_HRSC_MOLA_BlendDEM_Global_200mp.tif")))
            {
                var ifd = tiffReader.ImageFileDirectories[0];

                _elevationWidth = ifd.ImageWidth;
                _elevationHeight = ifd.ImageHeight;

                _sectorOffsetY = (int)(_elevationHeight * (Math.PI / 2 - Lat0) / Math.PI);
                _sectorOffsetX = (int)(_elevationWidth * (Math.PI + Lon0) / (Math.PI * 2));

                _sectorHeight = (int)Math.Ceiling(_elevationHeight * dLat / Math.PI);
                _sectorWidth = (int)Math.Ceiling(_elevationWidth * dLon / (Math.PI * 2));

                _elevationSectorBitmap = tiffReader.ReadImageFile<short>(ifd, _sectorOffsetX, _sectorOffsetY, _sectorWidth, _sectorHeight).ToBitmap();
                Console.WriteLine($"Loading image sector used {sw.Elapsed}");

                // _elevationSectorBitmap = Resampler.Resample(elevationBitmap, width, height).ToBitmap();
                // Console.WriteLine($"Resampling used {sw.Elapsed}");

                using (var tiffWriter = new TiffWriter(File.Create($@"Generated\Planets\MarsSector\Mars{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.tif")))
                {
                    var bitmap = _elevationSectorBitmap.Convert((p) => { return (ushort)(p - short.MinValue); });
                    tiffWriter.WriteImageFile(bitmap);
                }

                BitmapHelper.SavePng8($@"Generated\Planets\MarsSector\Mars{_elevationSectorBitmap.Width}x{_elevationSectorBitmap.Height}.png", _elevationSectorBitmap);

                int width = 2880;
                int height = 1440;
                string elevationTextureBlurFilename = $@"Generated\Planets\MarsSector\MarsBlur{width}x{height}.raw";
                if (!File.Exists(elevationTextureBlurFilename))
                {
                    var elevationTextureSmall = Resampler.Resample(_elevationSectorBitmap, width, height).ToBitmap();

                    sw = Stopwatch.StartNew();
                    var blurFilter = new BlurFilter(PlanetProjection);
                    _elevationBitmapBlur = blurFilter.Blur3(elevationTextureSmall, MathHelper.ToRadians(10));
                    Console.WriteLine($"Blur used {sw.Elapsed}");

                    BitmapHelper.SaveRaw16($@"Generated\Planets\MarsSector\MarsBlur{_elevationBitmapBlur.Width}x{_elevationBitmapBlur.Height}.raw", _elevationBitmapBlur);
                    BitmapHelper.SavePng8($@"Generated\Planets\MarsSector\MarsBlur{_elevationBitmapBlur.Width}x{_elevationBitmapBlur.Height}.png", _elevationBitmapBlur);
                }
                else
                {
                    _elevationBitmapBlur = BitmapHelper.LoadRaw16(elevationTextureBlurFilename, width, height);
                }
            }

            sw = Stopwatch.StartNew();

            var sphericalSector = new SphericalSector();
            sphericalSector.ComputeRadiusTop = ComputeModelElevationTop;
            sphericalSector.ComputeRadiusBottom = ComputeModelElevationBottom;

            sphericalSector.Create(Lat0, Lon0, Lat1, Lon1, NumSegments, NumSegments);

            PlanetVertexes = sphericalSector.Vertexes;
            PlanetTriangles = sphericalSector.Triangles;

            Console.WriteLine($"Time used to create planet vertexes: {sw.Elapsed}");

            SaveSTL($@"Generated\Planets\MarsSector\MarsSector{NumSegments}.stl");
        }




        private double ComputeModelElevationTop(Vector3d v, double lat, double lon)
        {
            var t = MathHelper.SphericalToTextureCoords(v);//lat, lon);

            double sy = t.y * _sy - _sy0;
            double sx = t.x * _sx - _sx0;

            double h = _elevationSectorBitmap.ReadBilinearPixel(sx, sy, true, false);
            double hAvg = _elevationBitmapBlur.ReadBilinearPixel(t.x, t.y, true, false);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + hAvg;

            return r * 0.00001;
        }
        private double ComputeModelElevationBottom(Vector3d v, double lat, double lon)
        {
            var t = MathHelper.SphericalToTextureCoords(lat, lon);

            double hAvg = _elevationBitmapBlur.ReadBilinearPixel(t.x, t.y, true, false);

            double r = PlanetRadius - 50000 + hAvg;

            return r * 0.00001;
        }
    }
}