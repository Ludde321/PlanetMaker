using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public abstract class RoamIcosphere : RoamBase
    {
        protected void Init(RoamMaterial material)
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
            AllocTriangle().Init(v0, v11, v5, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v0, v5, v1, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v0, v1, v7, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v0, v7, v10, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v0, v10, v11, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);

            // 5 adjacent faces
            AllocTriangle().Init(v1, v5, v9, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v5, v11, v4, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v11, v10, v2, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v10, v7, v6, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v7, v1, v8, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);

            // 5 faces around point 3
            AllocTriangle().Init(v3, v9, v4, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v3, v4, v2, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v3, v2, v6, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v3, v6, v8, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v3, v8, v9, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);

            // 5 adjacent faces
            AllocTriangle().Init(v4, v9, v5, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v2, v4, v11, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v6, v2, v10, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v8, v6, v7, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);
            AllocTriangle().Init(v9, v8, v1, new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 0), material);

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