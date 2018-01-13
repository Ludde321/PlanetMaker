using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiffExpress;

namespace Common.Dem
{
    public class DemTiffReader : DemReader<short>, IDisposable
    {
        protected readonly List<TiffReader> _tiffReaders = new List<TiffReader>();

        public readonly string InputPath;

        public readonly int BitmapWidth;
        public readonly int BitmapHeight;

        protected readonly Bitmap<short> _oceanBitmap;

        public DemTiffReader(string inputPath, int bitmapWidth, int bitmapHeight)
        {
            InputPath = inputPath;
            BitmapWidth = bitmapWidth;
            BitmapHeight = bitmapHeight;
            _oceanBitmap = new Bitmap<short>(BitmapWidth, BitmapHeight);
        }

        public virtual void Dispose()
        {
            foreach (var rawReader in _tiffReaders)
                rawReader.Dispose();
            _tiffReaders.Clear();
        }

        protected override IBitmap<short> LoadMapGranulate(int lat, int lon)
        {
            string bitmapPath = LocateMapGranulate(lat, lon);

            if (bitmapPath == null)
                return _oceanBitmap;

            var tiffReader = new TiffReader(File.OpenRead(bitmapPath));
            var bitmap = tiffReader.ReadImageFile<short>();

            _tiffReaders.Add(tiffReader);

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