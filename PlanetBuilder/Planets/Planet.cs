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

            return Vector3d.Multiply(v, r * 0.00001);
        }

        protected void SaveSTL(string outputFilename)
        {
            Console.WriteLine($"Vertexes: {PlanetVertexes.Count}");
            Console.WriteLine($"Triangles: {PlanetTriangles.Count}");

            using(var stlWriter = new StlWriter(File.Create(outputFilename)))
            {
                foreach (var triangle in PlanetTriangles)
                {
                    var v1 = PlanetVertexes[triangle.i1];
                    var v2 = PlanetVertexes[triangle.i2];
                    var v3 = PlanetVertexes[triangle.i3];

                    stlWriter.AddTriangle(v1, v2, v3);
                }
            }
            
        }

        // protected void SaveSTL(string outputFilename)
        // {
        //     var sw = Stopwatch.StartNew();

        //     using (var binaryWriter = new BinaryWriter(File.Create(outputFilename)))
        //     {
        //         binaryWriter.Write(new byte[80]); // Header 80 bytes
        //         binaryWriter.Write(PlanetTriangles.Count);
        //         foreach (var triangle in PlanetTriangles)
        //         {
        //             var v1 = PlanetVertexes[triangle.i1];
        //             var v2 = PlanetVertexes[triangle.i2];
        //             var v3 = PlanetVertexes[triangle.i3];

        //             // Triangle Normal
        //             binaryWriter.Write(0f);
        //             binaryWriter.Write(0f);
        //             binaryWriter.Write(0f);

        //             // Vertex 1
        //             binaryWriter.Write((float)v1.x);
        //             binaryWriter.Write((float)v1.y);
        //             binaryWriter.Write((float)v1.z);

        //             // Vertex 2
        //             binaryWriter.Write((float)v2.x);
        //             binaryWriter.Write((float)v2.y);
        //             binaryWriter.Write((float)v2.z);

        //             // Vertex 3
        //             binaryWriter.Write((float)v3.x);
        //             binaryWriter.Write((float)v3.y);
        //             binaryWriter.Write((float)v3.z);

        //             binaryWriter.Write((short)0); // Attribute byte count
        //         }
        //     }
        //     Console.WriteLine($"Time used saving {outputFilename}: {sw.Elapsed}");
        // }
    }
}
