using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder
{
    public struct Vector3d
    {
        public double x;
        public double y;
        public double z;

        public Vector3d(double x0, double y0, double z0)
        {
            x = x0;
            y = y0;
            z = z0;
        }

        public double Abs()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double Abs2()
        {
            return x * x + y * y + z * z;
        }

        public static Vector3d MiddlePoint(Vector3d v1, Vector3d v2)
        {
            return new Vector3d((v1.x + v2.x) * 0.5, (v1.y + v2.y) * 0.5, (v1.z + v2.z) * 0.5);
        }
        public static Vector3d Center(Vector3d v1, Vector3d v2, Vector3d v3)
        {
            const double div3 = 1.0 / 3;
            return new Vector3d((v1.x + v2.x + v3.x) * div3, (v1.y + v2.y + v3.y) * div3, (v1.z + v2.z + v3.z) * div3);
        }

        public static Vector3d Normalize(Vector3d v)
        {
            double length2 = v.x * v.x + v.y * v.y + v.z * v.z;
            if (length2 > 0)
            {
                double _length = 1 / Math.Sqrt(length2);

                return new Vector3d(v.x * _length, v.y * _length, v.z * _length);
            }

            return new Vector3d();
        }

        public static Vector3d Normalize(double x, double y, double z)
        {
            double length2 = x * x + y * y + z * z;
            if (length2 > 0)
            {
                double _length = 1 / Math.Sqrt(length2);

                return new Vector3d(x * _length, y * _length, z * _length);
            }

            return new Vector3d();
        }


        public static double Distance2(Vector3d v1, Vector3d v2)
        {
            double dx = v2.x - v1.x;
            double dy = v2.y - v1.y;
            double dz = v2.z - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }
        public static double Distance(Vector3d v1, Vector3d v2)
        {
            double dx = v2.x - v1.x;
            double dy = v2.y - v1.y;
            double dz = v2.z - v1.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static Vector3d Multiply(Vector3d v, double s)
        {
            return new Vector3d(v.x * s, v.y * s, v.z * s);
        }

        public static double Dot(Vector3d v0, Vector3d v1)
        {
            return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
        }

        public static Vector3d CrossProduct(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static double TriangleArea(Vector3d a, Vector3d b, Vector3d c)
        {
            return CrossProduct(a - b, c - b).Abs() * .5;
        }

        public static Vector3d Min(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(Math.Min(v1.x, v2.x), Math.Min(v1.y, v2.y), Math.Min(v1.z, v2.z));
        }
        public static Vector3d Max(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y), Math.Max(v1.z, v2.z));
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

    }
}
