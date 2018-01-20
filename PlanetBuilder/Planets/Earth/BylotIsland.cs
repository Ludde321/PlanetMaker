using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class BylotIsland : EarthSector
    {
        public BylotIsland()
        {
            ElevationScale = 6;
            NumSegmentsLon = 1500;
            NumSegmentsLat = 1500;
            ElevationBottom = -200;

            // Bylot Island 73.25N, 78.68W
            Name = "BylotIsland";
            UseAster = true;
            Lat0 = MathHelper.ToRadians(73.15 + 0.70);
            Lon0 = MathHelper.ToRadians(-78.65 - 2.85);
            Lat1 = MathHelper.ToRadians(73.15 - 0.70);
            Lon1 = MathHelper.ToRadians(-78.65 + 2.85);
        }
    }
}