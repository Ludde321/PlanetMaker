using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common;
using TiffExpress;

namespace PlanetBuilder.Planets
{
    public abstract class Planet
    {
        public double PlanetRadius;
        public double ElevationScale;
        public double ModelScale = 0.0005;
        public int RecursionLevel = 8;

        public string Name;

        public Projection PlanetProjection = Projection.Equirectangular;

        public List<Vector3d> PlanetVertexes;
        public List<Triangle> PlanetTriangles;

        protected Bitmap<short> _elevationTexture;
        protected Bitmap<short> _elevationTextureBlur;


        protected void CreatePlanetVertexes(int recursionLevel)
        {
            var icosphere = new Icosphere();
            icosphere.Create(recursionLevel);

            PlanetVertexes = icosphere.Vertexes.Select(v => ComputeModelElevation(v)).ToList();
            PlanetTriangles = icosphere.Triangles.ToList();
        }

        protected virtual Vector3d ComputeModelElevation(Vector3d v)
        {
            var t = MathHelper.SphericalToTextureCoords(v);

            double h = _elevationTexture.ReadBilinearPixel(t.x, t.y, true, false);
            double hAvg = _elevationTextureBlur.ReadBilinearPixel(t.x, t.y, true, false);

            double r = PlanetRadius + (h - hAvg) * ElevationScale + hAvg;

            return Vector3d.Multiply(v, r * ModelScale);
        }

        protected void SaveStl(string outputFilename)
        {
            Console.WriteLine($"Vertexes: {PlanetVertexes.Count}");
            Console.WriteLine($"Triangles: {PlanetTriangles.Count}");

            StlWriter.Create(outputFilename, PlanetTriangles, PlanetVertexes);
        }

    }
}
