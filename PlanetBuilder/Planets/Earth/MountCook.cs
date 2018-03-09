using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class MountCook : EarthSector
    {
        public MountCook()
        {
            ElevationScale = 2.5;
            NumSegmentsLon = 1600;
            NumSegmentsLat = 1600;
            ElevationBottom = -50;

            // -43.595056, 170.142139
            Name = "MountCook";
            UseAster = false;
            Lat0 = MathHelper.ToRadians(-43.595056 + 0.15);
            Lon0 = MathHelper.ToRadians(170.142139 - 0.2);
            Lat1 = MathHelper.ToRadians(-43.595056 - 0.175);
            Lon1 = MathHelper.ToRadians(170.142139 + 0.25);
        }
    }
}