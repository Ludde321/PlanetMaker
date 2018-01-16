using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class Matterhorn : EarthSector
    {
        public Matterhorn()
        {
            ElevationScale = 2;
            NumSegmentsLon = 1000;
            NumSegmentsLat = 1000;

            // Matterhorn 45.976389, 7.658333
            // 59.0120847,6.3631133
            Name = "Matterhorn";
            UseAster = false;
            Lat0 = MathHelper.ToRadians(45.976389 + 0.15);
            Lon0 = MathHelper.ToRadians(7.658333 - 0.15);
            Lat1 = MathHelper.ToRadians(45.976389 - 0.15);
            Lon1 = MathHelper.ToRadians(7.658333 + 0.15);
        }
    }
}