
namespace PlanetBuilder
{
    public class Texture16
    {
        public short[] Data;
        public int Width;
        public int Height;

        public Texture16(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new short[width * height];
        }

        public Texture16(short[] data, int width, int height)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
}
