using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder
{
    public abstract class Planet
    {
        public double PlanetRadius;
        public double ElevationScale;

        public Projection PlanetProjection = Projection.Equirectangular;

        private List<Vector3d> _planetVertexes;
        private List<Triangle> _planetTriangles;

        protected void CreatePlanetVertexes(int recursionLevel)
        {
            var icosphere = new Icosphere();
            icosphere.Create(recursionLevel);

            _planetVertexes = icosphere.mVertexes.Select(v => ComputeModelElevation(v)).ToList();
            _planetTriangles = icosphere.mTriangles.ToList();
        }

        protected abstract Vector3d ComputeModelElevation(Vector3d v);

        protected short ReadBilinearPixel(Texture<short> texture, double tx, double ty)
        {
            uint width = (uint)texture.Width;
            uint height = (uint)texture.Height;

            tx *= width - 1;
            ty *= height - 1;

            uint ix0 = (uint)tx;
            uint iy0 = (uint)ty;

            double fx = tx - ix0;
            double fy = ty - iy0;

            uint ix1 = (ix0 + 1) % (width - 1); // wrap width
            uint iy1 = iy0 + 1;
            if (iy1 >= height) iy1 = height - 1; // clamp height

            short p00 = texture.Data[iy0][ix0];  // p00......p01
            short p10 = texture.Data[iy1][ix0];  // .        .
            short p01 = texture.Data[iy0][ix1];  // .        .
            short p11 = texture.Data[iy1][ix1];  // p10......p11

            double p00p01 = p00 + (p01 - p00) * fx;
            double p10p11 = p10 + (p11 - p10) * fx;

            return (short)(p00p01 + (p10p11 - p00p01) * fy);
        }

        protected float ReadBilinearPixel(Texture<float> texture, double tx, double ty)
        {
            uint width = (uint)texture.Width;
            uint height = (uint)texture.Height;

            tx *= width - 1;
            ty *= height - 1;

            uint ix0 = (uint)tx;
            uint iy0 = (uint)ty;

            double fx = tx - ix0;
            double fy = ty - iy0;

            uint ix1 = (ix0 + 1) % (width - 1); // wrap width
            uint iy1 = iy0 + 1;
            if (iy1 >= height) iy1 = height - 1; // clamp height

            float p00 = texture.Data[iy0][ix0];  // p00......p01
            float p10 = texture.Data[iy1][ix0];  // .        .
            float p01 = texture.Data[iy0][ix1];  // .        .
            float p11 = texture.Data[iy1][ix1];  // p10......p11

            double p00p01 = p00 + (p01 - p00) * fx;
            double p10p11 = p10 + (p11 - p10) * fx;

            return (float)(p00p01 + (p10p11 - p00p01) * fy);
        }

        protected void SaveX3d(string outputFilename)
        {
            var sw = Stopwatch.StartNew();

            string template = ReadTemplateX3d();

            String indexes = string.Join(" ", _planetTriangles.Select(v => string.Format("{0} {1} {2} -1", v.i1, v.i2, v.i3)));
            String points = string.Join(" ", _planetVertexes.Select(v => string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", v.x, v.y, v.z)));

            File.WriteAllText(outputFilename, string.Format(template, indexes, points));

            Console.WriteLine($"Time used saving {outputFilename}: {sw.Elapsed}");
        }

        private string ReadTemplateX3d()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("PlanetBuilder.Data.template.x3d");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        protected void SaveSTL(string outputFilename)
        {
            var sw = Stopwatch.StartNew();

            using (var binaryWriter = new BinaryWriter(File.OpenWrite(outputFilename)))
            {
                binaryWriter.Write(new byte[80]); // Header 80 bytes
                binaryWriter.Write(_planetTriangles.Count);
                foreach (var triangle in _planetTriangles)
                {
                    var v1 = _planetVertexes[triangle.i1];
                    var v2 = _planetVertexes[triangle.i2];
                    var v3 = _planetVertexes[triangle.i3];

                    // Triangle Normal
                    binaryWriter.Write(0f);
                    binaryWriter.Write(0f);
                    binaryWriter.Write(0f);

                    // Vertex 1
                    binaryWriter.Write((float)v1.x);
                    binaryWriter.Write((float)v1.y);
                    binaryWriter.Write((float)v1.z);

                    // Vertex 2
                    binaryWriter.Write((float)v2.x);
                    binaryWriter.Write((float)v2.y);
                    binaryWriter.Write((float)v2.z);

                    // Vertex 3
                    binaryWriter.Write((float)v3.x);
                    binaryWriter.Write((float)v3.y);
                    binaryWriter.Write((float)v3.z);

                    binaryWriter.Write((short)0); // Attribute byte count
                }
            }
            Console.WriteLine($"Time used saving {outputFilename}: {sw.Elapsed}");
        }
    }
}
