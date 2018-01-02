using System;
using System.Collections.Generic;
using System.Linq;

namespace TiffExpress
{
    public static class BitmapTools
    {
        public static EnumerableBitmap<T> Crop<T>(IBitmap<T> inputBitmap, int offsetX, int offsetY, int width, int height)
        {
            var rows = CropRows(inputBitmap, offsetX, offsetY, width, height);
            return new EnumerableBitmap<T>(width, height, inputBitmap.SamplesPerPixel, rows);
        }

        private static IEnumerable<T[]> CropRows<T>(IBitmap<T> inputBitmap, int offsetX, int offsetY, int width, int height)
        {
            int y = 0;
            int rowCount = 0;
            foreach(var inputRow in inputBitmap.GetRows())
            {
                if(rowCount >= height)
                    break;

                if(y >= offsetY)
                {
                    var row = new T[width];
                    Array.Copy(inputRow, offsetX, row, 0, width);
                    yield return row;
                    rowCount++;
                }
                y++;
            }
        }

        public static EnumerableBitmap<T> Concatenate<T>(params IBitmap<T>[] inputBitmaps)
        {
            int outputWidth = inputBitmaps.Sum(t => t.Width);
            int outputHeight = inputBitmaps.Min(t => t.Height);
            int samplesPerPixel = inputBitmaps.Min(t => t.SamplesPerPixel);

            var inputRowsArray = inputBitmaps.Select(t => t.GetRows()).ToArray();
            var rows = ConcatenateRows(outputWidth, outputHeight, inputRowsArray);
            return new EnumerableBitmap<T>(outputWidth, outputHeight, samplesPerPixel, rows);
        }

        private static IEnumerable<T[]> ConcatenateRows<T>(int outputWidth, int outputHeight, params IEnumerable<T[]>[] inputBitmapsRows)
        {
            var enumerators = inputBitmapsRows.Select(t => t.GetEnumerator()).ToArray();

            for (int y = 0; y < outputHeight; y++)
            {
                var row = new T[outputWidth];
                int x = 0;
                foreach (var inputRowEnumerator in enumerators)
                {
                    inputRowEnumerator.MoveNext();

                    var inputRow = inputRowEnumerator.Current;
                    foreach (var pixel in inputRow)
                        row[x++] = pixel;
                }
                yield return row;
            }

            foreach(var enumerator in enumerators)
                enumerator.Dispose();
        }


    }
}
