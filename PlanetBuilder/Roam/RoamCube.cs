using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public class RoamMaterialCube : RoamMaterial
    {
        public RoamMaterialCube()
        {
        }

        public override bool SubdivideTriangle(RoamTriangle triangle, bool split)
        {
            if(split)
                return triangle.Level < 4;
            else
                return false;
        }

        public override void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
        {
            vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes[0].LinearPosition, triangle.Vertexes[2].LinearPosition);
            vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

            vertex.Position = Vector3d.Multiply(vertex.Normal, 1);//vertex.Normal * (groundRadius + vertex.altitude);
        }
    }

    public class RoamCube : RoamBase
    {
        public void Init()
        {
            var m0 = new RoamMaterialCube();

            var v0 = new RoamVertex();
            var v1 = new RoamVertex();
            var v2 = new RoamVertex();
            var v3 = new RoamVertex();
            var v4 = new RoamVertex();
            var v5 = new RoamVertex();
            var v6 = new RoamVertex();
            var v7 = new RoamVertex();

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

            v0.InsertBefore(ActiveVertexes);
            v1.InsertBefore(ActiveVertexes);
            v2.InsertBefore(ActiveVertexes);
            v3.InsertBefore(ActiveVertexes);
            v4.InsertBefore(ActiveVertexes);
            v5.InsertBefore(ActiveVertexes);
            v6.InsertBefore(ActiveVertexes);
            v7.InsertBefore(ActiveVertexes);

            var t0 = new RoamTriangle();
            var t1 = new RoamTriangle();
            var t2 = new RoamTriangle();
            var t3 = new RoamTriangle();
            var t4 = new RoamTriangle();
            var t5 = new RoamTriangle();
            var t6 = new RoamTriangle();
            var t7 = new RoamTriangle();
            var t8 = new RoamTriangle();
            var t9 = new RoamTriangle();
            var t10 = new RoamTriangle();
            var t11 = new RoamTriangle();

            // Create front face vertices and triangles
            t0.Init(v0, v1, v2, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t1.Init(v2, v3, v0, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create back face vertices and triangles
            t2.Init(v4, v5, v6, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t3.Init(v6, v7, v4, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create left face vertices and triangles
            t4.Init(v7, v6, v1, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t5.Init(v1, v0, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create right face vertices and triangles
            t6.Init(v3, v2, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t7.Init(v5, v4, v3, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create top face vertices and triangles
            t8.Init(v7, v0, v3, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t9.Init(v3, v4, v7, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            // Create bottom face vertices and triangles
            t10.Init(v1, v6, v5, new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(1, 1), m0);
            t11.Init(v5, v2, v1, new Vector2d(1, 1), new Vector2d(1, 0), new Vector2d(0, 0), m0);

            t0.InsertBefore(ActiveTriangles);
            t1.InsertBefore(ActiveTriangles);
            t2.InsertBefore(ActiveTriangles);
            t3.InsertBefore(ActiveTriangles);
            t4.InsertBefore(ActiveTriangles);
            t5.InsertBefore(ActiveTriangles);
            t6.InsertBefore(ActiveTriangles);
            t7.InsertBefore(ActiveTriangles);
            t8.InsertBefore(ActiveTriangles);
            t9.InsertBefore(ActiveTriangles);
            t10.InsertBefore(ActiveTriangles);
            t11.InsertBefore(ActiveTriangles);


            InitTriangles();
        }
    }
}