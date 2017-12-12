using System;

namespace PlanetBuilder.Roam
{
    public class RoamDiamond : SimpleList<RoamDiamond>
    {
        public readonly RoamTriangle[] Triangles = new RoamTriangle[4];

        public void SetTriangles(RoamTriangle t0, RoamTriangle t1, RoamTriangle t2, RoamTriangle t3)
        {
            Triangles[0] = t0;
            Triangles[1] = t1;
            Triangles[2] = t2;
            Triangles[3] = t3;

            t0.Diamond = this;
            t1.Diamond = this;
            t2.Diamond = this;
            t3.Diamond = this;
        }

        public void ReleaseTriangles()
        {
            Triangles[0].Diamond = null;
            Triangles[1].Diamond = null;
            Triangles[2].Diamond = null;
            Triangles[3].Diamond = null;
        }
    }
}