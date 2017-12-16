using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public class RoamMaterialCube : RoamMaterial
    {
        public RoamMaterialCube()
        {
        }


        public override void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
        {
            vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes0.LinearPosition, triangle.Vertexes2.LinearPosition);
            vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

            vertex.Position = Vector3d.Multiply(vertex.Normal, 1);//vertex.Normal * (groundRadius + vertex.altitude);
        }
    }

    public class RoamCube : RoamBase
    {
        public void Init()
        {
            var m0 = new RoamMaterialCube();

            var v0 = AllocVertex();
            var v1 = AllocVertex();
            var v2 = AllocVertex();
            var v3 = AllocVertex();
            var v4 = AllocVertex();
            var v5 = AllocVertex();
            var v6 = AllocVertex();
            var v7 = AllocVertex();

            var n0 = Vector3d.Normalize(new Vector3d(-1, 1, 1));
            var n1 = Vector3d.Normalize(new Vector3d(-1, -1, 1));
            var n2 = Vector3d.Normalize(new Vector3d(1, -1, 1));
            var n3 = Vector3d.Normalize(new Vector3d(1, 1, 1));
            var n4 = Vector3d.Normalize(new Vector3d(1, 1, -1));
            var n5 = Vector3d.Normalize(new Vector3d(1, -1, -1));
            var n6 = Vector3d.Normalize(new Vector3d(-1, -1, -1));
            var n7 = Vector3d.Normalize(new Vector3d(-1, 1, -1));

            m0.ComputeVertexAltitude(v0, n0);
            m0.ComputeVertexAltitude(v1, n1);
            m0.ComputeVertexAltitude(v2, n2);
            m0.ComputeVertexAltitude(v3, n3);
            m0.ComputeVertexAltitude(v4, n4);
            m0.ComputeVertexAltitude(v5, n5);
            m0.ComputeVertexAltitude(v6, n6);
            m0.ComputeVertexAltitude(v7, n7);

            // Create front face vertices and triangles
            AllocTriangle().Init(v0, v1, v2, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v2, v3, v0, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create back face vertices and triangles
            AllocTriangle().Init(v4, v5, v6, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v6, v7, v4, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create left face vertices and triangles
            AllocTriangle().Init(v7, v6, v1, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v1, v0, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create right face vertices and triangles
            AllocTriangle().Init(v3, v2, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v5, v4, v3, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create top face vertices and triangles
            AllocTriangle().Init(v7, v0, v3, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v3, v4, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create bottom face vertices and triangles
            AllocTriangle().Init(v1, v6, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            AllocTriangle().Init(v5, v2, v1, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            InitTriangles();
        }

        protected override bool SubdivideTriangle(RoamTriangle triangle)
        {
            return triangle.Level < 4;
        }

        protected override bool MergeDiamond(RoamDiamond diamond)
        {
            return true;
        }
    }
}