using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TiffExpress;

namespace MergeElevationData
{
    public class DemZipRawReader : DemReader<short>, IDisposable
    {
        private readonly List<RawReader> _rawReaders = new List<RawReader>();

        public readonly string InputPath;

        public readonly int BitmapWidth;
        public readonly int BitmapHeight;

        private readonly Bitmap<short> _oceanBitmap;

        private ZipArchive _zipArchive;

        public DemZipRawReader(string zipArchivePath, string inputPath, int bitmapWidth, int bitmapHeight)
        {
            InputPath = inputPath;
            BitmapWidth = bitmapWidth;
            BitmapHeight = bitmapHeight;
            _oceanBitmap = new Bitmap<short>(BitmapWidth, BitmapHeight);

            _zipArchive = ZipFile.OpenRead(zipArchivePath);
        }

        public void Dispose()
        {
            foreach (var rawReader in _rawReaders)
                rawReader.Dispose();
            _rawReaders.Clear();

            var zipArchive = _zipArchive;
            _zipArchive = null;
            if(zipArchive != null)
                zipArchive.Dispose();
        }

        protected override IBitmap<short> LoadMapGranulate(int lat, int lon)
        {
            string bitmapPath = string.Format(InputPath, MapGranulateName(lat, lon));
            var bitmapEntry = _zipArchive.GetEntry(bitmapPath);

            if (bitmapEntry == null)
                return _oceanBitmap;

            Console.WriteLine($"Found: {bitmapPath}");

            var rawReader = new RawReader(new BufferedStream(bitmapEntry.Open(), 1024*1024));
            var bitmap = rawReader.ReadBitmap<short>(BitmapWidth, BitmapHeight);
            bitmap = bitmap.Convert((p) =>
                {
                    return (short)(((p >> 8) & 0xff) | ((p << 8) & 0xff00));
                });

            _rawReaders.Add(rawReader);

            return bitmap;
        }

        public virtual string MapGranulateName(int lat, int lon)
        {
            return string.Format("{0}{1:D2}{2}{3:D3}", lat >= 0 ? 'N' : 'S', Math.Abs(lat), lon >= 0 ? 'E' : 'W', Math.Abs(lon));
        }

    }
}