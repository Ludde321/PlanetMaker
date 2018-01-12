using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TiffExpress;

namespace MergeElevationData
{
    public class DemZipRawReader : DemRawReader, IDisposable
    {
        private ZipArchive _zipArchive;

        public DemZipRawReader(string zipArchivePath, string inputPath, int bitmapWidth, int bitmapHeight) : base(inputPath, bitmapWidth, bitmapHeight)
        {
            _zipArchive = ZipFile.OpenRead(zipArchivePath);
        }

        public override void Dispose()
        {
            base.Dispose();

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
    }
}