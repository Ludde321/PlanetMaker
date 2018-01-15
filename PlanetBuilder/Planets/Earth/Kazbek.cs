using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class Kazbek : EarthSector
    {
        public Kazbek()
        {
            ElevationScale = 1.5;
            NumSegmentsLon = 1200;
            NumSegmentsLat = 1200;

            // Kazbek 42°41′57″N 44°31′06″ECoordinates: 42°41′57″N 44°31′06″E [1]
            Name = "Kazbek";
            Lat0 = MathHelper.ToRadians(42.677 + 0.160);
            Lat1 = MathHelper.ToRadians(42.677 - 0.160);
            Lon0 = MathHelper.ToRadians(44.590 - 0.225);
            Lon1 = MathHelper.ToRadians(44.590 + 0.225);
        }
    }
}