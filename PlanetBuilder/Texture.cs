using System.Collections.Generic;
using System.Linq;

namespace PlanetBuilder
{
    public class Texture<T>
    {
        public T[][] Data;
        public int Width;
        public int Height;

        public Texture(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new T[height][];
            for (int y = 0; y < height; y++)
                Data[y] = new T[width];
        }

        public Texture(int width, int height, IEnumerable<T[]> data)
        {
            Width = width;
            Height = height;
            Data = data.ToArray();
        }

    }
}
