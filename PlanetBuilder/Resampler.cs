using System;

namespace PlanetBuilder
{
    public static class Resampler
    {
        public static void Test(int dx, int dy)
        {
            int eps = 0;

            int y = 0;
            for (int x = 0; x <= dx; x++)
            {
                Console.WriteLine(x + ", " + y);

                eps += dy;

                if (2*eps >= dx)
                {
                    Console.WriteLine(y);

                    y++;
                    eps -= dx;
                }
            }
            Console.WriteLine($"y = {y}");

        }

        public static Texture<short> Resample(Texture<short> inputTexture, int width, int height)
        {
            var outputTexture = new Texture<short>(width, height);

            var accumLine = new long[width];
            var countLine = new int[width];

            int offsetY0 = 0;
            int offsetY1 = 0;

            int epsy = 0;

            int dy0 = inputTexture.Height - 1;
            int dy1 = outputTexture.Height - 1;
            int y1 = 0;
            for (int y0 = 0; y0 <= dy0; y0++)
            {
                int epsx = 0;

                int dx0 = inputTexture.Width - 1;
                int dx1 = outputTexture.Width - 1;
                int x1 = 0;

                var inputLine = inputTexture.Data[offsetY0];

                for (int x0 = 0; x0 <= dx0; x0++)
                {
                    accumLine[x1] += inputLine[x0];
                    countLine[x1]++;

                    epsx += dx1;

                    if (2*epsx >= dx0)
                    {
                        x1++;
                        epsx -= dx0;
                    }
                }
                offsetY0++;

                epsy += dy1;

                if (2*epsy >= dy0)
                {
                    var outputLine = outputTexture.Data[offsetY1];
                    for(int x=0;x<outputTexture.Width;x++)
                    {
                        outputLine[x] = (short)(accumLine[x] / countLine[x]);
                        accumLine[x] = 0;
                        countLine[x] = 0;
                    }
                    offsetY1++;
                    
                    y1++;
                    epsy -= dy0;
                }
            }
            
            if(y1 == dy1)
            {
                var outputLine = outputTexture.Data[offsetY1];
                for(int x=0;x<outputTexture.Width;x++)
                    outputLine[x] = (short)(accumLine[x] / countLine[x]);
            }
            return outputTexture;
        }

        public static Texture<float> Resample(Texture<float> inputTexture, int width, int height)
        {
            var outputTexture = new Texture<float>(width, height);

            var accumLine = new float[width];
            var countLine = new int[width];

            int offsetY0 = 0;
            int offsetY1 = 0;

            int epsy = 0;

            int dy0 = inputTexture.Height - 1;
            int dy1 = outputTexture.Height - 1;
            int y1 = 0;
            for (int y0 = 0; y0 <= dy0; y0++)
            {
                int epsx = 0;

                int dx0 = inputTexture.Width - 1;
                int dx1 = outputTexture.Width - 1;
                int x1 = 0;

                var inputLine = inputTexture.Data[offsetY0];

                for (int x0 = 0; x0 <= dx0; x0++)
                {
                    accumLine[x1] += inputLine[x0];
                    countLine[x1]++;

                    epsx += dx1;

                    if (2*epsx >= dx0)
                    {
                        x1++;
                        epsx -= dx0;
                    }
                }
                offsetY0++;

                epsy += dy1;

                if (2*epsy >= dy0)
                {
                    var outputLine = outputTexture.Data[offsetY1];
                    for(int x=0;x<outputTexture.Width;x++)
                    {
                        outputLine[x] = (float)(accumLine[x] / countLine[x]);
                        accumLine[x] = 0;
                        countLine[x] = 0;
                    }
                    offsetY1++;
                    
                    y1++;
                    epsy -= dy0;
                }
            }
            
            if(y1 == dy1)
            {
                var outputLine = outputTexture.Data[offsetY1];
                for(int x=0;x<outputTexture.Width;x++)
                    outputLine[x] = (float)(accumLine[x] / countLine[x]);
            }
            return outputTexture;
        }


    }
}