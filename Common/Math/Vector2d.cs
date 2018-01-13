using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct Vector2d
    {
        public double x;
        public double y;

        public Vector2d(double x0, double y0)
        {
            x = x0;
            y = y0;
        }

        public double Abs()
        {
            return Math.Sqrt(x*x + y*y);
        }

        public double Abs2()
        {
            return x * x + y * y;
        }

        public static Vector2d MiddlePoint(Vector2d v1, Vector2d v2)
        {
            return new Vector2d((v1.x + v2.x)*0.5, (v1.y + v2.y)*0.5);
        }

        public static Vector2d Normalize(Vector2d v)
        {
            double length2 = v.x * v.x + v.y * v.y;
            if (length2 > 0)
            {
                double _length = 1/Math.Sqrt(length2);

                return new Vector2d(v.x * _length, v.y * _length);
            }
    
            return new Vector2d();
        }

        public static Vector2d Multiply(Vector2d v, double s)
        {
            return new Vector2d(v.x * s, v.y * s);
        }

        public static double Dot(Vector2d v0, Vector2d v1)
        {
            return v0.x * v1.x + v0.y * v1.y;
        }

        public static Vector2d operator+(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator-(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x - b.x, a.y - b.y);
        }

    }
}
