using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PlanetBuilder.Planets
{
    public class Box
    {
        public List<Vector3d> Vertexes;
        public List<Triangle> Triangles;

        public Func<double, double, double> ComputeRadiusTop = ComputeRadiusTopDefault;
        public Func<double, double, double> ComputeRadiusBottom = ComputeRadiusBottomDefault;

        private static double ComputeRadiusTopDefault(double tu, double tv) { return 1; }
        private static double ComputeRadiusBottomDefault(double tu, double tv) { return -1; }

        public void Create(int xSegments, int ySegments)
        {
            Vertexes = new List<Vector3d>();
            Triangles = new List<Triangle>();

            double x0 = 0.0;
            double y0 = 1.0;
            double dx = 1.0 / (xSegments - 1);
            double dy = -1.0 / (ySegments - 1);

            double tx0 = 0.0;
            double ty0 = 0.0;
            double dtx = 1.0 / (xSegments - 1);
            double dty = 1.0 / (ySegments - 1);

            for (int z = 0; z < ySegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    double vx = x * dx + x0;
                    double vy = z * dy + y0;

                    double tx = x * dtx + tx0;
                    double ty = z * dty + ty0;

                    double vyT = ComputeRadiusTop(tx, ty);
                    double vyB = ComputeRadiusBottom(tx, ty);
                    Vertexes.Add(new Vector3d(vx, vy, vyT));
                    Vertexes.Add(new Vector3d(vx, vy, vyB));
                }
            }

            int xSegments2 = xSegments + xSegments;
            int ofsY = 0;
            for (int y = 0; y < ySegments - 1; y++)
            {
                Triangles.Add(new Triangle(ofsY, ofsY + 1, ofsY + xSegments2 + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2 + 1, ofsY + xSegments2));

                for (int x = 0; x < xSegments - 1; x++)
                {
                    int ofsY0 = ofsY;
                    int ofsY1 = ofsY0 + xSegments2;
                    Triangles.Add(new Triangle(ofsY0, ofsY1, ofsY1 + 2));
                    Triangles.Add(new Triangle(ofsY1 + 2, ofsY0 + 2, ofsY0));

                    Triangles.Add(new Triangle(ofsY0 + 1, ofsY1 + 3, ofsY1 + 1));
                    Triangles.Add(new Triangle(ofsY1 + 3, ofsY0 + 1, ofsY0 + 3));

                    ofsY += 2;
                }

                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2 + 1, ofsY + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2, ofsY + xSegments2 + 1));

                ofsY += 2;
            }

            int ofsYT = 0;
            int ofsYB = xSegments2 * (ySegments - 1);
            for (int x = 0; x < xSegments - 1; x++)
            {
                Triangles.Add(new Triangle(ofsYT, ofsYT + 3, ofsYT + 1));
                Triangles.Add(new Triangle(ofsYT, ofsYT + 2, ofsYT + 3));

                Triangles.Add(new Triangle(ofsYB, ofsYB + 1, ofsYB + 3));
                Triangles.Add(new Triangle(ofsYB, ofsYB + 3, ofsYB + 2));

                ofsYT += 2;
                ofsYB += 2;
            }
        }
/*            int xSegments2 = xSegments + xSegments;
            int ofsY = 0;
            for (int y = 0; y < zSegments - 1; y++)
            {
                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2 + 1, ofsY + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2, ofsY + xSegments2 + 1));

                for (int x = 0; x < xSegments - 1; x++)
                {
                    int ofsY0 = ofsY;
                    int ofsY1 = ofsY0 + xSegments2;
                    Triangles.Add(new Triangle(ofsY0, ofsY1 + 2, ofsY1));
                    Triangles.Add(new Triangle(ofsY1 + 2, ofsY0, ofsY0 + 2));

                    Triangles.Add(new Triangle(ofsY0 + 1, ofsY1 + 1, ofsY1 + 3));
                    Triangles.Add(new Triangle(ofsY1 + 3, ofsY0 + 3, ofsY0 + 1));

                    ofsY += 2;
                }

                Triangles.Add(new Triangle(ofsY, ofsY + 1, ofsY + xSegments2 + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + xSegments2 + 1, ofsY + xSegments2));

                ofsY += 2;
            }

            int ofsYT = 0;
            int ofsYB = xSegments2 * (zSegments - 1);
            for (int x = 0; x < xSegments - 1; x++)
            {
                Triangles.Add(new Triangle(ofsYT, ofsYT + 1, ofsYT + 3));
                Triangles.Add(new Triangle(ofsYT, ofsYT + 3, ofsYT + 2));

                Triangles.Add(new Triangle(ofsYB, ofsYB + 3, ofsYB + 1));
                Triangles.Add(new Triangle(ofsYB, ofsYB + 2, ofsYB + 3));

                ofsYT += 2;
                ofsYB += 2;
            }
        } */
    }
}