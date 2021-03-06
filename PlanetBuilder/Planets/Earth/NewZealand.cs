using Common;

namespace PlanetBuilder.Planets.Earth
{
    public class NewZealand : EarthSector
    {
        public NewZealand()
        {
            ElevationScale = 6;
            NumSegmentsLon = 20;
            NumSegmentsLat = 20;
            ElevationBottom = -8000;

            // ,170.848834
            Name = "NewZealand";
            UseAster = false;
            Lat0 = MathHelper.ToRadians(-43.8733831 + 3.6);
            Lon0 = MathHelper.ToRadians(170.5 - 4.5);
            Lat1 = MathHelper.ToRadians(-43.8733831 - 3.6);
            Lon1 = MathHelper.ToRadians(170.5 + 4.5);
        }
    }
}