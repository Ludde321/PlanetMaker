using System;

namespace Common
{
    public static class MathHelper
    {
        public static double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }

        public static double ToDegrees(double val)
        {
            return (180 / Math.PI) * val;
        }

        public static float ToRadians(float val)
        {
            return (float)(Math.PI / 180) * val;
        }

        public static float ToDegrees(float val)
        {
            return (float)(180 / Math.PI) * val;
        }

        public static Vector2d SphericalToTextureCoords(double lat, double lon)
        {
            double tx = (Math.PI + lon) / (Math.PI * 2);
            double ty = (Math.PI * 0.5 - lat) / Math.PI;

            return new Vector2d(tx, ty);
        }

        public static Vector2d SphericalToTextureCoords(Vector3d n)
        {
            double lat = Math.Asin(n.z);
            double lon = Math.Atan2(n.y, n.x);

            return SphericalToTextureCoords(lat, lon);
        }
    }
}