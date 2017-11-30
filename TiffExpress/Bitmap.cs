using System;
using System.Collections.Generic;
using System.Linq;

namespace TiffExpress
{
    public class Bitmap<T> : IBitmap<T>
    {
        public int Width {get;}
        public int Height {get;}

        public T[][] Rows;

        public Bitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Rows = new T[height][];
            for (int y = 0; y < height; y++)
                Rows[y] = new T[width];
        }

        public Bitmap(int width, int height, T[][] rows)
        {
            Width = width;
            Height = height;
            Rows = rows;
        }

        public IEnumerable<T[]> GetRows()
        {
            return Rows;
        }
    }
}
