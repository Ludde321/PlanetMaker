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
        private readonly Icosphere _icosphere = new Icosphere();

        public double PlanetRadius;
        public double ElevationScale;

        public int RecursionLevel = 3;

        public Projection PlanetProjection = Projection.Equirectangular;

        private List<Vector3d> _planetVertexes;
        private List<Triangle> _planetTriangles;

        protected void CreatePlanetVertexes()
        {
            _icosphere.Create(RecursionLevel);

            _planetVertexes = _icosphere.mVertexes.Select(v => ComputeModelElevation(v)).ToList();
            _planetTriangles = _icosphere.mTriangles.ToList();
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

        protected void SaveX3d(string outputFilename)
        {
            var sw = Stopwatch.StartNew();

            string template = ReadTemplateX3d();

            String indexes = string.Join(" ", _planetTriangles.Select(v => string.Format("{0} {1} {2} -1", v.i1, v.i2, v.i3)));
            String points = string.Join(" ", _planetVertexes.Select(v => string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", v.x, v.y, v.z)));

            File.WriteAllText(outputFilename, string.Format(template, indexes, points));

            Console.WriteLine($"Time used saving: {sw.Elapsed}");
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
    }
}
