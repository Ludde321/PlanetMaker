using System;
using System.IO;

namespace MergeElevationData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Directory.SetCurrentDirectory(@"c:\Ludde\FractalWorlds");

            var t = new MergeDem1();
            t.InputPaths.Add(@"\\luddepc\Earth\SRTMunzip\{0}.hgt");
            //t.InputPaths.Add(@"\\luddepc\Earth\ASTERunzip\ASTGTM2_{0}_dem.tif");
            
            // t.InputPaths.Add(@"Datasets\Planets\Earth\STRMv3Arc1\{0}.hgt");
            // t.InputPaths.Add(@"Datasets\Planets\Earth\ASTERv2Arc1\{0}.tif");
            t.OutputPath = @"Datasets\Planets\Earth\SRTM_{0}_{1}.tif";

            //t.Join(78, -83, 68, -73);
            // t.Join(64, 5, 54, 15);

            // Kazbek 42°41′57″N 44°31′06″ECoordinates: 42°41′57″N 44°31′06″E [1]
            t.Join(48, 40, 40, 48);

            Console.WriteLine("Done!");
        }
    }
}
