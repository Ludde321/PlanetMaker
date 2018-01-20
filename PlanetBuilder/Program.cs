using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using PlanetBuilder.Planets;
using PlanetBuilder.Planets.Earth;
using PlanetBuilder.Planets.Mars;
using PlanetBuilder.Roam;

namespace PlanetBuilder
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to PlanetBuilder!");

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Directory.SetCurrentDirectory(@"D:\Ludde\FractalWorlds");

            // var ceres = new Ceres();
            // ceres.Create();

            // var vesta = new Vesta();
            // vesta.Create();

            // var pluto = new Pluto();
            // pluto.Create();

            // var charon = new Charon();
            // charon.Create();

            // var moon = new Moon();
            // moon.Create();

            // var mercury = new Mercury();
            // mercury.Create();

            var mars = new PlanetMars();
            mars.Create();

            // var mars2 = new Mars2();
            // mars2.Create();

            // var marsSector = new MarsSector();
            // marsSector.Create();

            // var marsDouble = new MarsDouble();
            // marsDouble.Create();

            // var marsDouble2 = new MarsDouble2();
            // marsDouble2.Create();

            // var phobos = new Phobos();
            // phobos.Create();

            // var venus = new Venus();
            // venus.Create();

            // var earth = new Earth();
            // earth.Create();

            // var kazbek = new Kazbek();
            // kazbek.Create();

            // var discoIsland = new DiscoIsland();
            // discoIsland.Create();

            // var bylotIsland = new BylotIsland();
            // bylotIsland.Create();

            // var newZealand = new NewZealand();
            // newZealand.Create();

            // var mountCook = new MountCook();
            // mountCook.Create();

            // var matterhorn = new Matterhorn();
            // matterhorn.Create();

            // var lysefjord = new Lysefjord();
            // lysefjord.Create();

            // var earthBoylotIsland = new EarthBylotIsland();
            // earthBoylotIsland.Create();

            // var joinBitmaps = new JoinViewFinderPanoramas();
            // joinBitmaps.Join();

            // var joinEarthDem3 = new JoinEarthDem3();
            // joinEarthDem3.Join(75, -85, 65, -50);

            Console.WriteLine("Done");
        }
    }
}
