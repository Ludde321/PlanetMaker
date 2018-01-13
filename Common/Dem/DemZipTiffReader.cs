using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TiffExpress;

namespace Common.Dem
{
    public class DemZipTiffReader : DemTiffReader, IDisposable
    {
        private ZipArchive _zipArchive;

        public DemZipTiffReader(string zipArchivePath, string inputPath, int bitmapWidth, int bitmapHeight) : base(inputPath, bitmapWidth, bitmapHeight)
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

            var memoryStream = new MemoryStream();
            using(var tiffStream = bitmapEntry.Open())
                tiffStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var tiffReader = new TiffReader(memoryStream);
            var bitmap = tiffReader.ReadImageFile<short>();

            _tiffReaders.Add(tiffReader);

            return bitmap;
        }
    }
}