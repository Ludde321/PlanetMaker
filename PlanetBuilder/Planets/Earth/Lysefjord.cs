using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class Lysefjord : EarthSector
    {
        public Lysefjord()
        {
            ElevationScale = 2;
            NumSegmentsLon = 1000;
            NumSegmentsLat = 1000;

            // 59.0120847,6.3631133
            Name = "Lysefjord";
            UseAster = false;
            Lat0 = MathHelper.ToRadians(59.0120847 + 0.15);
            Lon0 = MathHelper.ToRadians(6.3631133 - 0.40);
            Lat1 = MathHelper.ToRadians(59.0120847 - 0.15);
            Lon1 = MathHelper.ToRadians(6.3631133 + 0.40);
        }
    }
}