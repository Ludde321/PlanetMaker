using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class MountCook : EarthSector
    {
        public MountCook()
        {
            ElevationScale = 4;
            NumSegmentsLon = 1500;
            NumSegmentsLat = 1500;
            ElevationBottom = -200;

            // -43.595056, 170.142139
            Name = "MountCook";
            UseAster = false;
            Lat0 = MathHelper.ToRadians(-43.595056 + 0.3);
            Lon0 = MathHelper.ToRadians(170.142139 - 0.4);
            Lat1 = MathHelper.ToRadians(-43.595056 - 0.4);
            Lon1 = MathHelper.ToRadians(170.142139 + 0.5);
        }
    }
}