using System;

namespace PlanetBuilder.Roam
{
    [Flags]
    public enum RoamTriangleFlags
    {
        ForceSplit = (1 << 0),
        Visible = (1 << 1),
        Modified = (1 << 2),
    };

    public class RoamTriangle : SimpleList<RoamTriangle>
    {
        //        private static byte[] _textureCodeTable;
        //        private static byte[] _textureCodeNext;

        public RoamTriangle BaseNeighbor;
        public RoamTriangle LeftNeighbor;
        public RoamTriangle RightNeighbor;

        public RoamTriangle Parent;

        public RoamDiamond Diamond;
        public readonly RoamVertex[] Vertexes = new RoamVertex[3];

        public readonly Vector2d[] TextureCoords = new Vector2d[3];

        public RoamMaterial Material;
        public RoamTriangleFlags Flags = RoamTriangleFlags.Modified;
        public ushort Level;

        public Vector3d Center;
        public double TwoDivArea;      // 2/area
        public double Radius;           // radius

        public void Set(RoamTriangle triangle, int i0, int i2, RoamVertex v1, Vector2d texCoord)
        {
            var v0 = triangle.Vertexes[i0];
            var v2 = triangle.Vertexes[i2];

            SetVertexes(v0, v1, v2);

            TextureCoords[0] = triangle.TextureCoords[i0];
            TextureCoords[1] = texCoord;
            TextureCoords[2] = triangle.TextureCoords[i2];

            Parent = triangle;

            Level = (ushort)(triangle.Level + 1);
            Flags = RoamTriangleFlags.Modified;
        }

        void SetVertexes(RoamVertex v0, RoamVertex v1, RoamVertex v2)
        {
            Vertexes[0] = v0;
            Vertexes[1] = v1;
            Vertexes[2] = v2;

            Center = Vector3d.Center(v0.Position, v1.Position, v2.Position);

            // Compute plane and area
            // Vector3d n = Vector3d.CrossProduct(v2.Position - v1.Position, v0.Position - v1.Position);
            // _area = 1 / n.Abs();
            // plane.x = n.x  _area;
            // plane.y = n.y  _area;
            // plane.z = n.z  _area;
            // plane.w = -(plane.x  center.x + plane.y  center.y + plane.z  center.z);

            // double r = center.distance2(v0.position);
            // double rt = center.distance2(v1.position);
            // if (rt > r) r = rt;
            // rt = center.distance2(v2.position);
            // if (rt > r) r = rt;

            // radius = sqrt(r);
        }

        public void SetNeighbors(RoamTriangle baseTriangle, RoamTriangle leftTriangle, RoamTriangle rightTriangle)
        {
            BaseNeighbor = baseTriangle;
            LeftNeighbor = leftTriangle;
            RightNeighbor = rightTriangle;
        }

        public void ReplaceNeighbor(RoamTriangle oldTriangle, RoamTriangle newTriangle)
        {
            if (BaseNeighbor == oldTriangle)
                BaseNeighbor = newTriangle;
            else
            if (LeftNeighbor == oldTriangle)
                LeftNeighbor = newTriangle;
            else
            if (RightNeighbor == oldTriangle)
                RightNeighbor = newTriangle;
        }

        public bool HasVertex(RoamVertex v)
        {
            return Vertexes[0] == v || Vertexes[1] == v || Vertexes[2] == v;
        }

    }
}