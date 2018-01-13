using Common;
using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public abstract class RoamIcosphere : RoamBase
    {
        protected void Init()
        {
            double t = (1.0 + System.Math.Sqrt(5.0)) / 2.0;

            var v0 = AddVertex(-1, t, 0);
            var v1 = AddVertex(1, t, 0);
            var v2 = AddVertex(-1, -t, 0);
            var v3 = AddVertex(1, -t, 0);
            var v4 = AddVertex(0, -1, t);
            var v5 = AddVertex(0, 1, t);
            var v6 = AddVertex(0, -1, -t);
            var v7 = AddVertex(0, 1, -t);
            var v8 = AddVertex(t, 0, -1);
            var v9 = AddVertex(t, 0, 1);
            var v10 = AddVertex(-t, 0, -1);
            var v11 = AddVertex(-t, 0, 1);

            // 5 faces around point 0
            AllocTriangle().Init(v5, v0, v11, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 0);
            AllocTriangle().Init(v1, v0, v5, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 1);
            AllocTriangle().Init(v7, v0, v1, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 2);
            AllocTriangle().Init(v10, v0, v7, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 3);
            AllocTriangle().Init(v11, v0, v10, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 4);

            // 5 adjacent faces
            AllocTriangle().Init(v11, v4, v5, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 5);
            AllocTriangle().Init(v5, v9, v1, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 6);
            AllocTriangle().Init(v1, v8, v7, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 7);
            AllocTriangle().Init(v7, v6, v10, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 8);
            AllocTriangle().Init(v10, v2, v11, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 9);

            // 5 faces around point 3
            AllocTriangle().Init(v4, v3, v9, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 10);
            AllocTriangle().Init(v2, v3, v4, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 11);
            AllocTriangle().Init(v6, v3, v2, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 12);
            AllocTriangle().Init(v8, v3, v6, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 13);
            AllocTriangle().Init(v9, v3, v8, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 14);

            // 5 adjacent faces
            AllocTriangle().Init(v9, v5, v4, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 15);
            AllocTriangle().Init(v4, v11, v2, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 16);
            AllocTriangle().Init(v2, v10, v6, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 17);
            AllocTriangle().Init(v6, v7, v8, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 18);
            AllocTriangle().Init(v8, v1, v9, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), 19);

            InitTriangles();
        }

        private RoamVertex AddVertex(double x, double y, double z)
        {
            var n = Vector3d.Normalize(x, y, z);
            var v = AllocVertex();
            ComputeVertexAltitude(v, n);
            return v;
        }
    }
}