using System;

namespace PlanetBuilder.Roam
{
    public class RoamDiamond : SimpleList<RoamDiamond>
    {
        public RoamTriangle Triangles0;
        public RoamTriangle Triangles1;
        public RoamTriangle Triangles2;
        public RoamTriangle Triangles3;

        public void SetTriangles(RoamTriangle t0, RoamTriangle t1, RoamTriangle t2, RoamTriangle t3)
        {
            Triangles0 = t0;
            Triangles1 = t1;
            Triangles2 = t2;
            Triangles3 = t3;

            t0.Diamond = this;
            t1.Diamond = this;
            t2.Diamond = this;
            t3.Diamond = this;
        }

        public void ReleaseTriangles()
        {
            Triangles0.Diamond = null;
            Triangles1.Diamond = null;
            Triangles2.Diamond = null;
            Triangles3.Diamond = null;
        }
    }
}