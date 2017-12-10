using System;
using System.Collections.Generic;
using System.Linq;

namespace TiffExpress
{
    public class Bitmap<T> : IBitmap<T>
    {
        public T[][] Rows;

        public Bitmap(int width, int height, int samplesPerPixel = 1)
        {
            Width = width;
            Height = height;
            SamplesPerPixel = samplesPerPixel;

            Rows = new T[height][];
            for (int y = 0; y < height; y++)
                Rows[y] = new T[width * samplesPerPixel];
        }

        public Bitmap(int width, int height, int samplesPerPixel, T[][] rows)
        {
            Width = width;
            Height = height;
            SamplesPerPixel = samplesPerPixel;
            Rows = rows;
        }

        public override IEnumerable<T[]> GetRows()
        {
            return Rows;
        }
    }
}
