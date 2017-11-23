using System;

namespace PlanetBuilder
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
    }
}