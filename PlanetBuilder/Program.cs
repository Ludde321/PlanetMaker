using System;
using System.Diagnostics;
using System.IO;
using PlanetBuilder.Planets;

namespace PlanetBuilder
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to PlanetBuilder!");

            Directory.SetCurrentDirectory(@"c:\Ludde\FractalWorlds");

            // var ceres = new Ceres();
            // ceres.Create();

            // var vesta = new Vesta();
            // vesta.Create();

            //var pluto = new Pluto();
            //pluto.Create();

            // var charon = new Charon();
            // charon.Create();

            // var moon = new Moon();
            // moon.Create();

            // var mercury = new Mercury();
            // mercury.Create();

            // var mars = new MarsSector();
            // mars.Create();

            // var phobos = new Phobos();
            // phobos.Create();

            // var venus = new Venus();
            // venus.Create();

            // var earth = new Earth();
            // earth.Create();

            // var sw = Stopwatch.StartNew();
            // using(var stream = File.OpenRead(@"Datasets\Planets\Mars\Mars_MGS_MOLA_DEM_mosaic_global_463m.tif"))
            // {
            //     var loader = new TiffLoader(stream);
            //     var ifds = loader.ReadImageFileDirectories();

            //     var rows = loader.ReadImageFileAsInt16(ifds[0]);

            //     foreach(var row in rows)
            //     {
            //         //Console.WriteLine(row.Length);
            //     }
            // }
            // Console.WriteLine($"Used: {sw.Elapsed}");


            // var sw = Stopwatch.StartNew();
            // using(var stream = File.OpenRead(@"Datasets\Planets\Vesta\Vesta_Dawn_HAMO_DTM_DLR_Global_48ppd.tif"))
            // {
            //     var loader = new TiffLoader(stream);
            //     var ifds = loader.ReadImageFileDirectories();

            //     var rows = loader.ReadImageFileAsFloat(ifds[0]);

            //     foreach(var row in rows)
            //     {
            //         Console.WriteLine(row.Length);
            //     }
            // }
            // Console.WriteLine($"Used: {sw.Elapsed}");


            // var sw = Stopwatch.StartNew();
            // using(var stream = File.OpenRead(@"Datasets\Planets\Moon\Lunar_LRO_LOLA_Global_LDEM_118m_Mar2014.tif"))
            // {
            //     var loader = new TiffLoader(stream);
            //     var ifds = loader.ReadImageFileDirectories();

            //     var rows = loader.ReadImageFileAsFloat(ifds[0]);

            //     foreach(var row in rows)
            //     {
            //         //Console.WriteLine(row.Length);
            //     }
            // }
            // Console.WriteLine($"Used: {sw.Elapsed}");




            //convert -size 360x180 -depth 16 -endian MSB gray:Ceres360x180Blur.raw Ceres360x180Blur.png

            Console.WriteLine("Done");
        }
    }
}
