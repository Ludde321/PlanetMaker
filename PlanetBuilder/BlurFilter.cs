using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder
{
    public class BlurFilter
    {
        private readonly Texture16 _inputTexture;
        private readonly Vector3d[] _vectorMap;

        public BlurFilter(Texture16 inputTexture)
        {
            _inputTexture = inputTexture;

            _vectorMap = CreateVectorMap();
        }

        private Vector3d[] CreateVectorMap()
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;

            var vmap = new Vector3d[width*height];

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                double lat = Math.PI * y / (height - 1);

                double sinLat = Math.Sin(lat);
                double cosLat = Math.Cos(lat);

                for (int x = 0; x < width; x++)
                {
                    double lon = Math.PI*2*x/width;

                    vmap[i++] = new Vector3d(
                        Math.Cos(lon)*sinLat,
                        Math.Sin(lon)*sinLat,
                        cosLat);
                }
            }
            return vmap;
        }

        private Vector2s[] CreateLookup(int y, double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;

            var cosBlurAngle = Math.Cos(blurAngle);

            var lookup = new List<Vector2s>();

            var v0 = _vectorMap[y*width];

            int j = 0;
            for (short y1 = 0; y1 < height; y1++)
            {
                for (short x1 = 0; x1 < width; x1++)
                {
                    var v1 = _vectorMap[j++];

                    var cosAngle = Vector3d.Dot(v0, v1);

                    if (cosAngle >= cosBlurAngle)
                        lookup.Add(new Vector2s(x1, y1));
                }
            }

            return lookup.ToArray();
        }

        public Texture16 Blur(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture16(width, height);

            var cosBlurAngle = Math.Cos(blurAngle);

            int size = width*height;

            Parallel.For(0, size, i =>
                                  {
                                      var v0 = _vectorMap[i];
                                      long avgElevation = 0;
                                      int countElevation = 0;

                                      for (int j = 0; j < size; j++)
                                      {
                                          var v1 = _vectorMap[j];

                                          var cosAngle = Vector3d.Dot(v0, v1);

                                          if (cosAngle >= cosBlurAngle)
                                          {
                                              avgElevation += _inputTexture.Data[j];
                                              countElevation++;
                                          }
                                      }

                                      if (countElevation == 0)
                                          throw new Exception("This should never happen. Count = 0");

                                      outputTexture.Data[i] = (short) (avgElevation/countElevation);
                                  });

            return outputTexture;
        }

        public Texture16 Blur2(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture16(width, height);

            //for (short y = 0; y < Height; y++)
            Parallel.For(0, height, y =>
            {
                var lookup = CreateLookup(y, blurAngle);

                //Console.WriteLine(lookup.Length);

                for (short x = 0; x < width; x++)
                {
                    long avgElevation = 0;

                    foreach (var v in lookup)
                    {
                        int x0 = (x + v.x) % width;
                        avgElevation += _inputTexture.Data[v.y * width + x0];
                    }

                    outputTexture.Data[y * width + x] = (short)(avgElevation / lookup.Length);
                }
            });

            return outputTexture;
        }


        public Texture16 Blur3(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture16(width, height);

            //for (short y = 0; y < Height; y++)
            Parallel.For(0, height, y =>
                                    {
                                        var lookup1 = CreateLookup(y, blurAngle);
                                        var lookup2 = lookup1.Select(v => new Vector2s((short)((v.x + 1)%width), v.y)).ToArray();

                                        var subs = lookup1.Except(lookup2).ToArray();
                                        var adds = lookup2.Except(lookup1).ToArray();

                                        long avgElevation = 0;

                                        foreach (var v in lookup1)
                                            avgElevation += _inputTexture.Data[v.y * width + v.x];

                                        outputTexture.Data[y * width + 0] = (short)(avgElevation / lookup1.Length);

                                        for (short x = 1; x < width; x++)
                                        {
                                            foreach (var v in adds)
                                            {
                                                int x0 = (x + v.x) % width;
                                                avgElevation += _inputTexture.Data[v.y * width + x0];
                                            }
                                            foreach (var v in subs)
                                            {
                                                int x0 = (x + v.x) % width;
                                                avgElevation -= _inputTexture.Data[v.y * width + x0];
                                            }

                                            outputTexture.Data[y * width + x] = (short) (avgElevation/lookup1.Length);
                                        }
                                    });

            return outputTexture;
        }

       

    }
}
