using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public class RoamMaterialCube : RoamMaterial
    {
    }

    public abstract class RoamCube : RoamBase
    {
        public void Init(RoamMaterial material)
        {
            var v0 = AddVertex(-1, 1, 1);
            var v1 = AddVertex(-1, -1, 1);
            var v2 = AddVertex(1, -1, 1);
            var v3 = AddVertex(1, 1, 1);
            var v4 = AddVertex(1, 1, -1);
            var v5 = AddVertex(1, -1, -1);
            var v6 = AddVertex(-1, -1, -1);
            var v7 = AddVertex(-1, 1, -1);

            // Create front face vertices and triangles
            AllocTriangle().Init(v0, v1, v2, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v2, v3, v0, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

            // Create back face vertices and triangles
            AllocTriangle().Init(v4, v5, v6, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v6, v7, v4, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

            // Create left face vertices and triangles
            AllocTriangle().Init(v7, v6, v1, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v1, v0, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

            // Create right face vertices and triangles
            AllocTriangle().Init(v3, v2, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v5, v4, v3, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

            // Create top face vertices and triangles
            AllocTriangle().Init(v7, v0, v3, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v3, v4, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

            // Create bottom face vertices and triangles
            AllocTriangle().Init(v1, v6, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), material);
            AllocTriangle().Init(v5, v2, v1, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), material);

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