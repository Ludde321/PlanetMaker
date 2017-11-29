using System.Collections.Generic;
using System.Linq;

namespace PlanetBuilder
{
    public abstract class BitmapBase<T>
    {
        public int Width;
        public int Height;

        protected BitmapBase(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public abstract IEnumerable<T[]> GetRows();
    }
}