using Common;
using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public abstract class RoamSquare : RoamBase
    {
        public virtual void Init()
        {
            var v0 = AddVertex(-1, 1, 0);
            var v1 = AddVertex(-1, -1, 0);
            var v2 = AddVertex(1, -1, 0);
            var v3 = AddVertex(1, 1, 0);

            // Create top face vertices and triangles
            AllocTriangle().Init(v0, v1, v2, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), 0);
            AllocTriangle().Init(v2, v3, v0, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), 0);

            // Create bottom face vertices and triangles
            AllocTriangle().Init(v3, v2, v1, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), 1);
            AllocTriangle().Init(v1, v0, v3, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), 1);

            InitTriangles();
        }

        private RoamVertex AddVertex(double x, double y, double z)
        {
            var v = AllocVertex();
            ComputeVertexAltitude(v, new Vector3d(x, y, z));
            return v;
        }
    }
}