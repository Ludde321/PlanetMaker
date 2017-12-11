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

    public class RoamTriangle
    {
        //        private static byte[] _textureCodeTable;
        //        private static byte[] _textureCodeNext;

        public RoamTriangle BaseNeighbor;
        public RoamTriangle LeftNeighbor;
        public RoamTriangle RightNeighbor;

        public RoamTriangle Parent;
        public readonly RoamTriangle[] Children = new RoamTriangle[2];

        public RoamDiamond Diamond;
        public readonly RoamVertex[] Vertexes = new RoamVertex[3];

        public readonly Vector2d[] TextureCoords = new Vector2d[3];

        public RoamMaterial Material;
        public RoamTriangleFlags Flags;
        public ushort Level;

        public double TwoDivArea;      // 2/area
        public double Radius;           // radius

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