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

    }
}
