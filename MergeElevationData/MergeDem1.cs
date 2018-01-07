using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiffExpress;

namespace MergeElevationData
{
    public class MergeDem1
    {
        public readonly List<string> InputPaths = new List<string>();
        public string OutputPath;

        public const int BitmapWidth = 3601;
        public const int BitmapHeight = 3601;

        private readonly Bitmap<short> _oceanBitmap = new Bitmap<short>(BitmapWidth, BitmapHeight);

        public MergeDem1()
        {

        }

        public void Join(int lat0, int lon0, int lat1, int lon1)
        {
            int dlat = lat0 - lat1;
            int dlon = lon1 - lon0;

            var rawReaders = new List<RawReader>();
            var tiffReaders = new List<TiffReader>();

            var latBitmaps = new List<IBitmap<short>>();
            for(int lat = lat0;lat>=lat1;lat--)
            {
                var lonBitmaps = new List<IBitmap<short>>();
                for(int lon = lon0;lon<=lon1;lon++)
                {
                    string bitmapPath = LocateMapGranulate(lat, lon);

                    if (bitmapPath == null)
                    {
                        lonBitmaps.Add((IBitmap<short>)_oceanBitmap);
                    }
                    else
                    {
                        string ext = Path.GetExtension(bitmapPath);
                        if (string.Equals(ext, ".hgt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var rawReader = new RawReader(File.OpenRead(bitmapPath));
                            var bitmap = rawReader.ReadBitmap<short>(BitmapWidth, BitmapHeight);
                            lonBitmaps.Add(bitmap);
                            rawReaders.Add(rawReader);
                        }
                    }
                }
                var lonBitmap = BitmapTools.Concatenate(lonBitmaps.ToArray());

                latBitmaps.Add(lonBitmap);
            }

            var theBitmap = BitmapTools.Append(latBitmaps.ToArray());

            using (var tiffWriter = new TiffWriter(File.Create(string.Format(OutputPath, MapGranulateName(lat0, lon0), MapGranulateName(lat1, lon1)))))
            {
                tiffWriter.BigTiff = true;
                theBitmap = theBitmap.Convert((p) =>
                {
                    return (short)(50 + (((p >> 8) & 0xff) | ((p << 8) & 0xff00)));
                });
                tiffWriter.WriteImageFile(theBitmap);
            }

            foreach (var rawReader in rawReaders)
                rawReader.Dispose();
        }

        private string LocateMapGranulate(int lat, int lon)
        {
            foreach (string inputPath in InputPaths)
            {
                string bitmapPath = string.Format(inputPath, MapGranulateName(lat, lon));
                Console.WriteLine(bitmapPath);
                if (File.Exists(bitmapPath))
                    return bitmapPath;
            }
            return null;
        }

        private static string MapGranulateName(int lat, int lon)
        {
            return string.Format("{0}{1:D2}{2}{3:D3}", lat >= 0 ? 'N' : 'S', Math.Abs(lat), lon >= 0 ? 'E' : 'W', Math.Abs(lon));
        }

    }
}