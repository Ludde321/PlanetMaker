using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder
{
    public class Icosphere
    {
        public List<Vector3d> mVertexes;
        public List<Triangle> mTriangles;

        private Dictionary<ulong, int> mMiddlePointIndexCache;

        public void Create(int recursionLevel)
        {
            var sw = Stopwatch.StartNew();

            CreateIcosahedron();
            SubdivideTriangles(recursionLevel);

            Console.WriteLine($"Vertexes: {mVertexes.Count}");
            Console.WriteLine($"Triangles: {mTriangles.Count}");
            Console.WriteLine($"RecursionLevel: {recursionLevel}");
            Console.WriteLine($"Time used to create and subdivide icosphere: {sw.Elapsed}");
        }

        private void CreateIcosahedron()
        {
            mVertexes = new List<Vector3d>();
            mTriangles = new List<Triangle>();

            var t = (1.0 + System.Math.Sqrt(5.0)) / 2.0;

            AddVertex(new Vector3d(-1, t, 0));
            AddVertex(new Vector3d( 1,  t,  0));
            AddVertex(new Vector3d(-1, -t,  0));
            AddVertex(new Vector3d( 1, -t,  0));
            AddVertex(new Vector3d( 0, -1,  t));
            AddVertex(new Vector3d( 0,  1,  t));
            AddVertex(new Vector3d( 0, -1, -t));
            AddVertex(new Vector3d( 0,  1, -t));
            AddVertex(new Vector3d( t,  0, -1));
            AddVertex(new Vector3d( t,  0,  1));
            AddVertex(new Vector3d(-t,  0, -1));
            AddVertex(new Vector3d(-t,  0,  1));

            // 5 faces around point 0
            mTriangles.Add(new Triangle(0, 11, 5));
            mTriangles.Add(new Triangle(0, 5, 1));
            mTriangles.Add(new Triangle(0, 1, 7));
            mTriangles.Add(new Triangle(0, 7, 10));
            mTriangles.Add(new Triangle(0, 10, 11));
 
            // 5 adjacent faces
            mTriangles.Add(new Triangle(1, 5, 9));
            mTriangles.Add(new Triangle(5, 11, 4));
            mTriangles.Add(new Triangle(11, 10, 2));
            mTriangles.Add(new Triangle(10, 7, 6));
            mTriangles.Add(new Triangle(7, 1, 8));
 
            // 5 faces around point 3
            mTriangles.Add(new Triangle(3, 9, 4));
            mTriangles.Add(new Triangle(3, 4, 2));
            mTriangles.Add(new Triangle(3, 2, 6));
            mTriangles.Add(new Triangle(3, 6, 8));
            mTriangles.Add(new Triangle(3, 8, 9));
 
            // 5 adjacent faces
            mTriangles.Add(new Triangle(4, 9, 5));
            mTriangles.Add(new Triangle(2, 4, 11));
            mTriangles.Add(new Triangle(6, 2, 10));
            mTriangles.Add(new Triangle(8, 6, 7));
            mTriangles.Add(new Triangle(9, 8, 1));
        }

        private void SubdivideTriangles(int recursionLevel)
        {
            mMiddlePointIndexCache = new Dictionary<ulong, int>();

            var triangles = mTriangles;

            for (int i = 0; i < recursionLevel; i++)
            {
                var triangles2 = new List<Triangle>();
                foreach (var tri in triangles)
                {
                    // replace triangle by 4 triangles
                    int a = GetMiddlePoint(tri.i1, tri.i2);
                    int b = GetMiddlePoint(tri.i2, tri.i3);
                    int c = GetMiddlePoint(tri.i3, tri.i1);

                    triangles2.Add(new Triangle(tri.i1, a, c));
                    triangles2.Add(new Triangle(tri.i2, b, a));
                    triangles2.Add(new Triangle(tri.i3, c, b));
                    triangles2.Add(new Triangle(a, b, c));
                }
                triangles = triangles2;
            }

            mTriangles = triangles;
        }

        // MurmurHash3 https://code.google.com/p/smhasher/wiki/MurmurHash3
        private static ulong MurmurHash3(ulong key)
        {
            key ^= key >> 33;
            key *= 0xff51afd7ed558ccdL;
            key ^= key >> 33;
            key *= 0xc4ceb9fe1a85ec53L;
            key ^= key >> 33;
            return key;
        }

        private int GetMiddlePoint(int i1, int i2)
        {
            // first check if we have it already
            bool firstIsSmaller = i1 < i2;
            ulong smallerIndex = firstIsSmaller ? (ulong)i1 : (ulong)i2;
            ulong greaterIndex = firstIsSmaller ? (ulong)i2 : (ulong)i1;
            ulong key = (smallerIndex << 32) + greaterIndex;

            key = MurmurHash3(key);

            int ret;
            if (mMiddlePointIndexCache.TryGetValue(key, out ret))
                return ret;

            // not in cache, calculate it
            Vector3d v1 = mVertexes[i1];
            Vector3d v2 = mVertexes[i2];
            Vector3d vm = Vector3d.MiddlePoint(v1, v2);

            int im = AddVertex(vm);

            // store it, return index
            mMiddlePointIndexCache.Add(key, im);

            return im;
        }

        private int AddVertex(Vector3d v)
        {
            v = Vector3d.Normalize(v);

            mVertexes.Add(v);

            return mVertexes.Count - 1;
        }

    }
}
