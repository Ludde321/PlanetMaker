using System;
using System.Collections.Generic;
using System.Linq;

namespace TiffExpress
{
    public static class BitmapExtensions
    {
        public static EnumerableBitmap<B> Convert<A, B>(this IBitmap<A> inputTexture, Func<A, B> func)
        {
            var rows = inputTexture.GetRows().Select(row => row.Select(p => func(p)).ToArray());
            return new EnumerableBitmap<B>(inputTexture.Width, inputTexture.Height, rows);
        }

        public static void Process<A>(this IBitmap<A> inputTexture, Func<A, A> func)
        {
            foreach(var row in inputTexture.GetRows())
                for(int x =0;x<row.Length;x++)
                    row[x] = func(row[x]);
        }
    }
}