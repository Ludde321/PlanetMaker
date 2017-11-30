using System.Collections.Generic;
using System.Linq;

namespace PlanetBuilder
{
    public interface IBitmap<T>
    {
        int Width {get;}
        int Height { get;}

        IEnumerable<T[]> GetRows();
    }
}