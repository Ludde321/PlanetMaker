using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiffExpress;

namespace MergeElevationData
{
    public abstract class DemReader<T>
    {
        public EnumerableBitmap<T> LoadBitmap(int lat0, int lon0, int lat1, int lon1)
        {
            int dlat = lat0 - lat1;
            int dlon = lon1 - lon0;

            var latBitmaps = new List<IBitmap<T>>();
            for (int lat = lat0; lat >= lat1; lat--)
            {
                var lonBitmaps = new List<IBitmap<T>>();
                for (int lon = lon0; lon <= lon1; lon++)
                {
                    var bitmap = LoadMapGranulate(lat, lon);
                    lonBitmaps.Add(bitmap);
                }
                var lonBitmap = BitmapTools.Concatenate(lonBitmaps.ToArray());

                latBitmaps.Add(lonBitmap);
            }

            return BitmapTools.Append(latBitmaps.ToArray());
        }

        protected abstract IBitmap<T> LoadMapGranulate(int lat, int lon);
    }
}