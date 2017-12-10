using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiffExpress
{
    public class EnumerableBitmap<T> : IBitmap<T>
    {
        private IEnumerable<T[]> _rows;

        public EnumerableBitmap(int width, int height, int samplesPerPixel, IEnumerable<T[]> rows)
        {
            Width = width;
            Height = height;
            SamplesPerPixel = samplesPerPixel;
            _rows = rows;
        }

        public override IEnumerable<T[]> GetRows()
        {
            return _rows;
        }

        public Bitmap<T> ToBitmap()
        {
            return new Bitmap<T>(Width, Height, SamplesPerPixel, _rows.ToArray());
        }


    }
}
