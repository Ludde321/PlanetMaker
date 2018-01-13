using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiffExpress;

namespace Common.Dem
{
    public class DemRawReader : DemReader<short>, IDisposable
    {
        protected readonly List<RawReader> _rawReaders = new List<RawReader>();

        public readonly string InputPath;

        public readonly int BitmapWidth;
        public readonly int BitmapHeight;

        protected readonly Bitmap<short> _oceanBitmap;

        public DemRawReader(string inputPath, int bitmapWidth, int bitmapHeight)
        {
            InputPath = inputPath;
            BitmapWidth = bitmapWidth;
            BitmapHeight = bitmapHeight;
            _oceanBitmap = new Bitmap<short>(BitmapWidth, BitmapHeight);
        }

        public virtual void Dispose()
        {
            foreach (var rawReader in _rawReaders)
                rawReader.Dispose();
            _rawReaders.Clear();
        }

        protected override IBitmap<short> LoadMapGranulate(int lat, int lon)
        {
            string bitmapPath = LocateMapGranulate(lat, lon);

            if (bitmapPath == null)
                return _oceanBitmap;

            var rawReader = new RawReader(File.OpenRead(bitmapPath));
            var bitmap = rawReader.ReadBitmap<short>(BitmapWidth, BitmapHeight);
            bitmap = bitmap.Convert((p) =>
                {
                    return (short)(((p >> 8) & 0xff) | ((p << 8) & 0xff00));
                });

            _rawReaders.Add(rawReader);

            return bitmap;
        }

        private string LocateMapGranulate(int lat, int lon)
        {
            string bitmapPath = string.Format(InputPath, MapGranulateName(lat, lon));
            if (File.Exists(bitmapPath))
            {
                Console.WriteLine($"Found: {bitmapPath}");
                return bitmapPath;
            }
            return null;
        }
    }
}