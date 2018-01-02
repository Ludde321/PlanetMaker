using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder.Planets
{
    public class SphericalSector
    {
        public List<Vector3d> Vertexes;
        public List<Triangle> Triangles;

        public Func<Vector3d, double, double, double> ComputeRadiusTop = ComputeRadiusTopDefault;
        public Func<Vector3d, double, double, double> ComputeRadiusBottom = ComputeRadiusBottomDefault;

        private static double ComputeRadiusTopDefault(Vector3d v, double lat, double lon) { return 1; }
        private static double ComputeRadiusBottomDefault(Vector3d v, double lat, double lon) { return 0.8; }

        public void Create(double latitudeOffset0, double longitudeOffset0, double latitudeOffset1, double longitudeOffset1, int latitudeSegments, int longitudeSegments)
        {
            Vertexes = new List<Vector3d>();
            Triangles = new List<Triangle>();

            double latitudeAngle = latitudeOffset1 - latitudeOffset0;
            double longitudeAngle = longitudeOffset1 - longitudeOffset0;

            double dLat = latitudeAngle / (latitudeSegments - 1);
            for (int y = 0; y < latitudeSegments; y++)
            {
                double lat = (latitudeOffset0 + dLat * y);

                double sinLat = Math.Sin(lat);
                double cosLat = Math.Cos(lat);

                double dLon = longitudeAngle / (longitudeSegments - 1);

                for (int x = 0; x < longitudeSegments; x++)
                {
                    double lon = longitudeOffset0 + dLon * x;

                    double nx = Math.Cos(lon) * cosLat;
                    double ny = Math.Sin(lon) * cosLat;
                    double nz = sinLat;

                    var v = new Vector3d(nx, ny, nz);
                    double rTop = ComputeRadiusTop(v, lat, lon);
                    double rBottom = ComputeRadiusBottom(v, lat, lon);
                    Vertexes.Add(Vector3d.Multiply(v, rTop));
                    Vertexes.Add(Vector3d.Multiply(v, rBottom));
                }
            }

            int longitudeSegments2 = longitudeSegments + longitudeSegments;
            int ofsY = 0;
            for (int y = 0; y < latitudeSegments - 1; y++)
            {
                Triangles.Add(new Triangle(ofsY, ofsY + 1, ofsY + longitudeSegments2 + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + longitudeSegments2 + 1, ofsY + longitudeSegments2));

                for (int x = 0; x < longitudeSegments - 1; x++)
                {
                    int ofsY0 = ofsY;
                    int ofsY1 = ofsY0 + longitudeSegments2;
                    Triangles.Add(new Triangle(ofsY0, ofsY1, ofsY1 + 2));
                    Triangles.Add(new Triangle(ofsY1 + 2, ofsY0 + 2, ofsY0));

                    Triangles.Add(new Triangle(ofsY0 + 1, ofsY1 + 3, ofsY1 + 1));
                    Triangles.Add(new Triangle(ofsY1 + 3, ofsY0 + 1, ofsY0 + 3));

                    ofsY += 2;
                }

                Triangles.Add(new Triangle(ofsY, ofsY + longitudeSegments2 + 1, ofsY + 1));
                Triangles.Add(new Triangle(ofsY, ofsY + longitudeSegments2, ofsY + longitudeSegments2 + 1));

                ofsY += 2;
            }

            int ofsYT = 0;
            int ofsYB = longitudeSegments2 * (latitudeSegments - 1);
            for (int x = 0; x < longitudeSegments - 1; x++)
            {
                Triangles.Add(new Triangle(ofsYT, ofsYT + 3, ofsYT + 1));
                Triangles.Add(new Triangle(ofsYT, ofsYT + 2, ofsYT + 3));

                Triangles.Add(new Triangle(ofsYB, ofsYB + 1, ofsYB + 3));
                Triangles.Add(new Triangle(ofsYB, ofsYB + 3, ofsYB + 2));

                ofsYT += 2;
                ofsYB += 2;
            }
        }

    }
}