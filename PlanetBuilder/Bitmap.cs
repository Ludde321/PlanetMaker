using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanetBuilder
{
    public class Bitmap<T> : BitmapBase<T>
    {
        public T[][] Data;

        public Bitmap(int width, int height) : base(width, height)
        {
            Data = new T[height][];
            for (int y = 0; y < height; y++)
                Data[y] = new T[width];
        }

        public Bitmap(int width, int height, IEnumerable<T[]> data) : base(width, height)
        {
            Data = data.ToArray();
        }

        public override IEnumerable<T[]> GetRows()
        {
            return Data;
        }
    }
}
