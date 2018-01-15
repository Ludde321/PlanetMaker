using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class DiscoIsland : EarthSector
    {
        public DiscoIsland()
        {
            ElevationScale = 1.5;
            NumSegmentsLon = 1200;
            NumSegmentsLat = 1200;

            // Disco Island 69.81°N 53.47°W 
            Name = "DiscoIsland";
            UseAster = true;
            Lat0 = MathHelper.ToRadians(69.81 + 0.75);
            Lon0 = MathHelper.ToRadians(-53.47 - 2.0);
            Lat1 = MathHelper.ToRadians(69.81 - 0.75);
            Lon1 = MathHelper.ToRadians(-53.47 + 2.0);
        }
    }
}