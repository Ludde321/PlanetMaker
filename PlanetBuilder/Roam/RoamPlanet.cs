using PlanetBuilder.Roam;

namespace PlanetBuilder.Roam
{
    public abstract class RoamPlanet : RoamIcosphere
    {
        public ushort MaxLevels;
        public double PlanetRadius;
        public double ElevationScale;
    }
}