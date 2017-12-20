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
            return new EnumerableBitmap<B>(inputTexture.Width, inputTexture.Height, inputTexture.SamplesPerPixel, rows);
        }

        public static void Process<A>(this IBitmap<A> inputTexture, Func<A, A> func)
        {
            foreach(var row in inputTexture.GetRows())
                for(int x =0;x<row.Length;x++)
                    row[x] = func(row[x]);
        }

        public static byte ReadBilinearPixel(this Bitmap<byte> texture, double tx, double ty, bool repeatX, bool repeatY)
        {
            uint width = (uint)texture.Width;
            uint height = (uint)texture.Height;

            tx *= width - 1;
            ty *= height - 1;

            uint ix0 = (uint)tx;
            uint iy0 = (uint)ty;

            double fx = tx - ix0;
            double fy = ty - iy0;

            uint ix1 = ix0 + 1;
            if(repeatX)
                ix1 = ix1 % (width - 1); // wrap
            else
            {
                if (ix1 >= width) // clamp
                    ix1 = width - 1;
            }

            uint iy1 = iy0 + 1;
            if(repeatY)
                iy1 = iy1 % (height - 1); // wrap
            else
            {
                if (iy1 >= height) // clamp
                    iy1 = height - 1; 
            }

            byte p00 = texture.Rows[iy0][ix0];  // p00......p01
            byte p10 = texture.Rows[iy1][ix0];  // .        .
            byte p01 = texture.Rows[iy0][ix1];  // .        .
            byte p11 = texture.Rows[iy1][ix1];  // p10......p11

            double p00p01 = p00 + (p01 - p00) * fx;
            double p10p11 = p10 + (p11 - p10) * fx;

            return (byte)(p00p01 + (p10p11 - p00p01) * fy);
        }


        public static short ReadBilinearPixel(this Bitmap<short> texture, double tx, double ty, bool repeatX, bool repeatY)
        {
            uint width = (uint)texture.Width;
            uint height = (uint)texture.Height;

            tx *= width - 1;
            ty *= height - 1;

            uint ix0 = (uint)tx;
            uint iy0 = (uint)ty;

            double fx = tx - ix0;
            double fy = ty - iy0;

            uint ix1 = ix0 + 1;
            if(repeatX)
                ix1 = ix1 % (width - 1); // wrap
            else
            {
                if (ix1 >= width) // clamp
                    ix1 = width - 1;
            }

            uint iy1 = iy0 + 1;
            if(repeatY)
                iy1 = iy1 % (height - 1); // wrap
            else
            {
                if (iy1 >= height) // clamp
                    iy1 = height - 1; 
            }

            short p00 = texture.Rows[iy0][ix0];  // p00......p01
            short p10 = texture.Rows[iy1][ix0];  // .        .
            short p01 = texture.Rows[iy0][ix1];  // .        .
            short p11 = texture.Rows[iy1][ix1];  // p10......p11

            double p00p01 = p00 + (p01 - p00) * fx;
            double p10p11 = p10 + (p11 - p10) * fx;

            return (short)(p00p01 + (p10p11 - p00p01) * fy);
        }

        public static float ReadBilinearPixel(this Bitmap<float> texture, double tx, double ty, bool repeatX, bool repeatY)
        {
            uint width = (uint)texture.Width;
            uint height = (uint)texture.Height;

            tx *= width - 1;
            ty *= height - 1;

            uint ix0 = (uint)tx;
            uint iy0 = (uint)ty;

            double fx = tx - ix0;
            double fy = ty - iy0;

            uint ix1 = ix0 + 1;
            if(repeatX)
                ix1 = ix1 % (width - 1); // wrap
            else
            {
                if (ix1 >= width) // clamp
                    ix1 = width - 1;
            }

            uint iy1 = iy0 + 1;
            if(repeatY)
                iy1 = iy1 % (height - 1); // wrap
            else
            {
                if (iy1 >= height) // clamp
                    iy1 = height - 1; 
            }

            float p00 = texture.Rows[iy0][ix0];  // p00......p01
            float p10 = texture.Rows[iy1][ix0];  // .        .
            float p01 = texture.Rows[iy0][ix1];  // .        .
            float p11 = texture.Rows[iy1][ix1];  // p10......p11

            double p00p01 = p00 + (p01 - p00) * fx;
            double p10p11 = p10 + (p11 - p10) * fx;

            return (float)(p00p01 + (p10p11 - p00p01) * fy);
        }


    }
}