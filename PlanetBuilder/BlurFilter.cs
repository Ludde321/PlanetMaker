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
        private readonly Texture<short> _inputTexture;
        private readonly Texture<Vector3d> _vectorMap;

        private readonly Projection _projection;

        public BlurFilter(Texture<short> inputTexture, Projection projection)
        {
            _inputTexture = inputTexture;
            _projection = projection;

            _vectorMap = CreateVectorMap();
        }

        private Texture<Vector3d> CreateVectorMap()
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;

            var vmap = new Texture<Vector3d>(width, height);

            switch(_projection)
            {
                case Projection.Equirectangular:
                {
                    for (int y = 0; y < height; y++)
                    {
                        double lat = Math.PI * y / (height - 1);

                        double sinLat = Math.Sin(lat);
                        double cosLat = Math.Cos(lat);

                        for (int x = 0; x < width; x++)
                        {
                            double lon = Math.PI * 2 * x / width;

                            vmap.Data[y][x] = new Vector3d(
                                Math.Cos(lon) * sinLat,
                                cosLat,
                                Math.Sin(lon) * sinLat);
                        }
                    }
                } break;
                case Projection.SimpleCylindrical:
                {
                    for (int y = 0; y < height; y++)
                    {
                        double z = 1 - 2.0 * y / (height - 1);

                        for (int x = 0; x < width; x++)
                        {
                            double lon = Math.PI * 2 * x / width;

                            vmap.Data[y][x] = Vector3d.Normalize(new Vector3d(
                                Math.Cos(lon),
                                z,
                                Math.Sin(lon)));
                        }
                    }
                } break;
            }
            return vmap;
        }

        private Vector2us[] CreateLookup(int y, double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;

            var cosBlurAngle = Math.Cos(blurAngle);

            var lookup = new List<Vector2us>();

            var v0 = _vectorMap.Data[y][0];

            for (ushort y1 = 0; y1 < height; y1++)
            {
                for (ushort x1 = 0; x1 < width; x1++)
                {
                    var v1 = _vectorMap.Data[y1][x1];

                    var cosAngle = Vector3d.Dot(v0, v1);

                    if (cosAngle >= cosBlurAngle)
                        lookup.Add(new Vector2us(x1, y1));
                }
            }

            return lookup.ToArray();
        }

        public Texture<short> Blur(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture<short>(width, height);

            var cosBlurAngle = Math.Cos(blurAngle);

            Parallel.For(0, height, y0 =>
            {
                for (int x0 = 0; x0 < width; x0++)
                {
                    var v0 = _vectorMap.Data[y0][x0];
                    long avgElevation = 0;
                    int countElevation = 0;

                    for (int y1 = 0; y1 < height; y1++)
                    {
                        for (int x1 = 0; x1 < width; x1++)
                        {
                            var v1 = _vectorMap.Data[y1][x1];

                            var cosAngle = Vector3d.Dot(v0, v1);

                            if (cosAngle >= cosBlurAngle)
                            {
                                avgElevation += _inputTexture.Data[y1][x1];
                                countElevation++;
                            }

                        }
                    }
                    if (countElevation == 0)
                        throw new Exception("This should never happen. Count = 0");

                    outputTexture.Data[y0][x0] = (short)(avgElevation / countElevation);
                }
            });

            return outputTexture;
        }

        public Texture<short> Blur2(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture<short>(width, height);

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
                        avgElevation += _inputTexture.Data[v.y][x0];
                    }

                    outputTexture.Data[y][x] = (short)(avgElevation / lookup.Length);
                }
            });

            return outputTexture;
        }


        public Texture<short> Blur3(double blurAngle)
        {
            int width = _inputTexture.Width;
            int height = _inputTexture.Height;
            var outputTexture = new Texture<short>(width, height);

            //for (short y = 0; y < Height; y++)
            Parallel.For(0, height, y =>
                                    {
                                        var lookup1 = CreateLookup(y, blurAngle);
                                        var lookup2 = lookup1.Select(v => new Vector2us((ushort)((v.x + 1) % width), v.y)).ToArray();

                                        var subs = lookup1.Except(lookup2).ToArray();
                                        var adds = lookup2.Except(lookup1).ToArray();

                                        long avgElevation = 0;

                                        foreach (var v in lookup1)
                                            avgElevation += _inputTexture.Data[v.y][v.x];

                                        outputTexture.Data[y][0] = (short)(avgElevation / lookup1.Length);

                                        for (short x = 1; x < width; x++)
                                        {
                                            foreach (var v in adds)
                                            {
                                                int x0 = (x + v.x - 1) % width;
                                                avgElevation += _inputTexture.Data[v.y][x0];
                                            }
                                            foreach (var v in subs)
                                            {
                                                int x0 = (x + v.x - 1) % width;
                                                avgElevation -= _inputTexture.Data[v.y][x0];
                                            }

                                            outputTexture.Data[y][x] = (short)(avgElevation / lookup1.Length);
                                        }
                                    });

            return outputTexture;
        }



    }
}
