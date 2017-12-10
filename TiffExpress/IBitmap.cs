using System.Collections.Generic;
using System.Linq;

namespace TiffExpress
{
    public abstract class IBitmap<T>
    {
        public int Width;
        public int Height;
        public int SamplesPerPixel;

        public abstract IEnumerable<T[]> GetRows();
    }
}