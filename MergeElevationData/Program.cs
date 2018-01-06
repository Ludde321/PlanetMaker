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
            t.InputPaths.Add(@"Datasets\Planets\Earth\STRMv3Arc1\{0}.hgt");
            t.InputPaths.Add(@"Datasets\Planets\Earth\ASTERv2Arc1\{0}.tif");
            t.OutputPath = @"Datasets\Planets\Earth\Arc1_{0}_{1}.tif";

            t.Join(-60, 10, -55, 15);
        }
    }
}
