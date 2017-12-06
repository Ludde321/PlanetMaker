using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiffExpress
{
    public class EnumerableBitmap<T> : IBitmap<T>
    {
        public int Width {get;}
        public int Height {get;}

        private IEnumerable<T[]> _rows;

        // protected EnumerableBitmap(int width, int height)
        // {
        //     Width = width;
        //     Height = height;
        // }

        public EnumerableBitmap(int width, int height, IEnumerable<T[]> rows)
        {
            Width = width;
            Height = height;
            _rows = rows;
        }

        public IEnumerable<T[]> GetRows()
        {
            return _rows;
        }

        public Bitmap<T> ToBitmap()
        {
            return new Bitmap<T>(Width, Height, _rows.ToArray());
        }


    }
}
