using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TiffExpress
{
    public static class Resampler
    {
        public static void BresenhamTest(int dx, int dy)
        {
            int eps = 0;

            int y = 0;
            for (int x = 0; x <= dx; x++)
            {
                Console.WriteLine(x + ", " + y);

                eps += dy;

                if (2 * eps >= dx)
                {
                    Console.WriteLine(y);

                    y++;
                    eps -= dx;
                }
            }
            Console.WriteLine($"y = {y}");
        }
/*
        public static Bitmap<byte> Resample(Bitmap<byte> inputTexture, int width, int height)
        {
            var outputTexture = new Bitmap<byte>(width, height);

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

                var inputLine = inputTexture.Rows[offsetY0];

                for (int x0 = 0; x0 <= dx0; x0++)
                {
                    accumLine[x1] += inputLine[x0];
                    countLine[x1]++;

                    epsx += dx1;

                    if (2 * epsx >= dx0)
                    {
                        x1++;
                        epsx -= dx0;
                    }
                }
                offsetY0++;

                epsy += dy1;

                if (2 * epsy >= dy0)
                {
                    var outputLine = outputTexture.Rows[offsetY1];
                    for (int x = 0; x < outputTexture.Width; x++)
                    {
                        outputLine[x] = (byte)(accumLine[x] / countLine[x]);
                        accumLine[x] = 0;
                        countLine[x] = 0;
                    }
                    offsetY1++;

                    y1++;
                    epsy -= dy0;
                }
            }

            if (y1 == dy1)
            {
                var outputLine = outputTexture.Rows[offsetY1];
                for (int x = 0; x < outputTexture.Width; x++)
                    outputLine[x] = (byte)(accumLine[x] / countLine[x]);
            }
            return outputTexture;
        }

        public static Bitmap<short> Resample(Bitmap<short> inputTexture, int width, int height)
        {
            var outputTexture = new Bitmap<short>(width, height);

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

                var inputLine = inputTexture.Rows[offsetY0];

                for (int x0 = 0; x0 <= dx0; x0++)
                {
                    accumLine[x1] += inputLine[x0];
                    countLine[x1]++;

                    epsx += dx1;

                    if (2 * epsx >= dx0)
                    {
                        x1++;
                        epsx -= dx0;
                    }
                }
                offsetY0++;

                epsy += dy1;

                if (2 * epsy >= dy0)
                {
                    var outputLine = outputTexture.Rows[offsetY1];
                    for (int x = 0; x < outputTexture.Width; x++)
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

            if (y1 == dy1)
            {
                var outputLine = outputTexture.Rows[offsetY1];
                for (int x = 0; x < outputTexture.Width; x++)
                    outputLine[x] = (short)(accumLine[x] / countLine[x]);
            }
            return outputTexture;
        }

        public static Bitmap<float> Resample(Bitmap<float> inputTexture, int width, int height)
        {
            var outputTexture = new Bitmap<float>(width, height);

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

                var inputLine = inputTexture.Rows[offsetY0];

                for (int x0 = 0; x0 <= dx0; x0++)
                {
                    accumLine[x1] += inputLine[x0];
                    countLine[x1]++;

                    epsx += dx1;

                    if (2 * epsx >= dx0)
                    {
                        x1++;
                        epsx -= dx0;
                    }
                }
                offsetY0++;

                epsy += dy1;

                if (2 * epsy >= dy0)
                {
                    var outputLine = outputTexture.Rows[offsetY1];
                    for (int x = 0; x < outputTexture.Width; x++)
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

            if (y1 == dy1)
            {
                var outputLine = outputTexture.Rows[offsetY1];
                for (int x = 0; x < outputTexture.Width; x++)
                    outputLine[x] = (float)(accumLine[x] / countLine[x]);
            }
            return outputTexture;
        }
*/
        public static EnumerableBitmap<T> Resample<T>(IBitmap<T> inputTexture, int outputWidth, int outputHeight)
        {
            int inputWidth = inputTexture.Width;

            var inputRowGroups = ResampleY(inputTexture, outputHeight);

            var blockingQueue = new BlockingCollection<Task<T[]>>(16);
            
            ResampleXTask(blockingQueue, inputRowGroups, inputWidth, outputWidth);

            var rows = EnumerateQueue(blockingQueue);
            return new EnumerableBitmap<T>(outputWidth, outputHeight, rows);
        }

        private static Task ResampleXTask<T>(BlockingCollection<Task<T[]>> blockingQueue, IEnumerable<T[][]> inputRowGroups, int inputWidth, int outputWidth)
        {
            return Task.Run(() => 
            {
                foreach(var inputRows in inputRowGroups)
                    blockingQueue.Add(Task<T[]>.Run(() => ResampleX(inputRows, inputWidth, outputWidth)));
                blockingQueue.Add(null); // EOF mark
            });
        }

        private static IEnumerable<T[]> EnumerateQueue<T>(BlockingCollection<Task<T[]>> blockingQueue)
        {
            while(true)
            {
                var task = blockingQueue.Take();
                if(task == null)
                    break;
                yield return task.Result;
            }
        }

        private static IEnumerable<T[][]> ResampleY<T>(IBitmap<T> inputTexture, int outputHeight)
        {
            var outputRows = new List<T[]>();

            int epsy = 0;

            int dy0 = inputTexture.Height - 1;
            int dy1 = outputHeight - 1;
            int y1 = 0;
            foreach (var inputRow in inputTexture.GetRows())
            {
                outputRows.Add(inputRow);

                epsy += dy1;

                if (2 * epsy >= dy0)
                {
                    yield return outputRows.ToArray();
                    outputRows.Clear();

                    y1++;
                    epsy -= dy0;
                }
            }

            if (y1 == dy1)
            {
                yield return outputRows.ToArray();
            }
        }

        private static T[] ResampleX<T>(T[][] inputRows, int inputWidth, int outputWidth)
        {
            if(typeof(T) == typeof(short))
                return (T[])(object)ResampleXAsInt16((short[][])(object)inputRows, inputWidth, outputWidth);
            return null;
        }
        private static short[] ResampleXAsInt16(short[][] inputRows, int inputWidth, int outputWidth)
        {
            var outputRow = new short[outputWidth];

            long accum = 0;
            int count = 0;

            int epsx = 0;

            int dx0 = inputWidth - 1;
            int dx1 = outputWidth - 1;
            int x1 = 0;

            for (int x0 = 0; x0 <= dx0; x0++)
            {
                foreach (var inputRow in inputRows)
                    accum += inputRow[x0];
                count += inputRows.Length;

                epsx += dx1;

                if (2 * epsx >= dx0)
                {
                    outputRow[x1] = (short)(accum / count);
                    accum = 0;
                    count = 0;

                    x1++;
                    epsx -= dx0;
                }
            }

            if (x1 == dx1)
                outputRow[x1] = (short)(accum / count);

            return outputRow;
        }

    }
}