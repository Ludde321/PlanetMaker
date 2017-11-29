using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetBuilder
{
    public class EnumerableBitmap<T> : BitmapBase<T>
    {
        public IEnumerable<T[]> Data;

        protected EnumerableBitmap(int width, int height) : base(width, height)
        {
        }

        public EnumerableBitmap(int width, int height, IEnumerable<T[]> data) : base(width, height)
        {
            Data = data;
        }

        public override IEnumerable<T[]> GetRows()
        {
            return Data;
        }

        public Bitmap<T> ToBitmap()
        {
            return new Bitmap<T>(Width, Height, Data);
        }


    }
}
