using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiffExpress;

namespace PlanetBuilder
{
    public class JoinEarthDem3
    {
        private string _inputPath = @"Datasets\Planets\Earth\ViewFinderPanoramas\dem3\{0}.hgt";

        private readonly Bitmap<short> _oceanBitmap = new Bitmap<short>(1201, 1201);

        public JoinEarthDem3()
        {

        }

        public void Join(int lat0, int lon0, int lat1, int lon1)
        {
            int dlat = lat0-lat1;
            int dlon = lon1-lon0;

            var readers = new List<RawReader>();

            var concatBitmaps = new List<EnumerableBitmap<short>>();

            for(int lat = lat0;lat>=lat1;lat--)
            {
                var bitmapPaths = new List<string>();
                for(int lon = lon0;lon<=lon1;lon++)
                {
                    string bitmapPath = string.Format(_inputPath, MapPosition(lat, lon));
                    bitmapPaths.Add(bitmapPath);
                }
                var rawReaders = bitmapPaths.Select(p => File.Exists(p) ? new RawReader(File.OpenRead(p)) : null).ToArray();
                readers.AddRange(rawReaders);

                var rowBitmaps = rawReaders.Select(r => r != null ? r.ReadBitmap<short>(1201, 1201) : (IBitmap<short>)_oceanBitmap).ToArray();
                var rowBitmap = BitmapTools.Concatenate(rowBitmaps);

                concatBitmaps.Add(rowBitmap);
            }

            var theBitmap = BitmapTools.Append(concatBitmaps.ToArray());

            using (var tiffWriter = new TiffWriter(File.Create($@"Datasets\Planets\Earth\ViewFinderPanoramas\dem3_{MapPosition(lat0, lon0)}_{MapPosition(lat1, lon1)}.tif")))
            {
                tiffWriter.BigTiff = true;
                theBitmap = theBitmap.Convert((p) =>
                {
                    return (short)(((p >> 8) & 0xff) | ((p<< 8) & 0xff00));
                });
                tiffWriter.WriteImageFile(theBitmap);
            }

            foreach(var reader in readers)
                reader?.Dispose();
        }

        private static string MapPosition(int lat, int lon)
        {
            return string.Format("{0}{1:D2}{2}{3:D3}", lat >= 0 ? 'N' : 'S', Math.Abs(lat), lon >= 0 ? 'E' : 'W', Math.Abs(lon));
        }

    }
}