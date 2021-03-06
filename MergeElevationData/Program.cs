﻿using System;
using System.Diagnostics;
using System.IO;
using Common.Dem;
using TiffExpress;

namespace MergeElevationData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Directory.SetCurrentDirectory(@"c:\Ludde\FractalWorlds");

            var sw = Stopwatch.StartNew();

//            var demReader = new DemZipRawReader(@"\\luddepc\Earth2\SRTM.zip", "{0}.hgt", 3601, 3601);
            var demReader = new DemZipTiffReader(@"\\luddepc\Earth2\ASTER.zip", "ASTGTM2_{0}_dem.tif", 3601, 3601);
            //var demReader = new DemTiffReader(@"\\luddepc\Earth\ASTERunzip\ASTGTM2_{0}_dem.tif", 3601, 3601);

            //t.Join(78, -83, 68, -73);
            // t.Join(64, 5, 54, 15);

            // Kazbek 42°41′57″N 44°31′06″ECoordinates: 42°41′57″N 44°31′06″E [1]
            int lat0 = 48;
            int lon0 = 40;
            int lat1 = 40;
            int lon1 = 48;
            var theBitmap = demReader.LoadBitmap(lat0, lon0, lat1, lon1);

            using (var tiffWriter = new TiffWriter(File.Create(string.Format(@"Datasets\Planets\Earth\ASTER_{0}_{1}.tif", demReader.MapGranulateName(lat0, lon0), demReader.MapGranulateName(lat1, lon1)))))
            {
                tiffWriter.BigTiff = true;
                tiffWriter.WriteImageFile(theBitmap);
            }

            Console.WriteLine($"Saving bitmap in {sw.Elapsed}");

            Console.WriteLine("Done!");
        }
    }
}
