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
        public RoamVertex Vertexes0;
        public RoamVertex Vertexes1;
        public RoamVertex Vertexes2;

        public Vector2d TextureCoords0;
        public Vector2d TextureCoords1;
        public Vector2d TextureCoords2;

        public RoamTriangleFlags Flags = RoamTriangleFlags.Modified;
        public ushort Level;
        public ushort Material;

        // public Vector3d Center;
        // public double TwoDivArea;      // 2/area
        // public double Radius;           // radius

        public void Init(RoamVertex v0, RoamVertex v1, RoamVertex v2, Vector2d tex0, Vector2d tex1, Vector2d tex2, ushort material)
        {
            Material = material;

            SetVertexes(v0, v1, v2);

            TextureCoords0 = tex0;
            TextureCoords1 = tex1;
            TextureCoords2 = tex2;
        }

        public void Set(RoamTriangle parent, RoamVertex v0, RoamVertex v1, RoamVertex v2, Vector2d texCoord0, Vector2d texCoord1, Vector2d texCoord2)
        {
            SetVertexes(v0, v1, v2);

            TextureCoords0 = texCoord0;
            TextureCoords1 = texCoord1;
            TextureCoords2 = texCoord2;

            Parent = parent;

            Level = (ushort)(parent.Level + 1);
            Flags = RoamTriangleFlags.Modified;
            Material = parent.Material;
        }

        void SetVertexes(RoamVertex v0, RoamVertex v1, RoamVertex v2)
        {
            Vertexes0 = v0;
            Vertexes1 = v1;
            Vertexes2 = v2;

            //Center = Vector3d.Center(v0.Position, v1.Position, v2.Position);

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
            return Vertexes0 == v || Vertexes1 == v || Vertexes2 == v;
        }

    }
}