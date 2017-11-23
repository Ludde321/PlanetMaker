﻿using System;
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

            var mars = new MarsSector();
            mars.Create();

            // var venus = new Venus();
            // venus.Create();

            // var earth = new Earth();
            // earth.Create();

            // var sw = Stopwatch.StartNew();
            // var texture = TextureHelper.LoadRaw16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres_Dawn_FC_HAMO_DTM_DLR_Global_60ppd_Oct2016.raw", 21600, 10800);
            // Console.WriteLine($"Loading texture used {sw.Elapsed}");

            // sw = Stopwatch.StartNew();
            // var resampler = new Resampler();
            // //resampler.Test(10, 4);
            // var texture2 = resampler.Resample(texture, 360, 180);
            // Console.WriteLine($"Resampling used {sw.Elapsed}");

            // TextureHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres360x180.raw", texture2);

            // var blurFilter = new BlurFilter(texture2);
            // sw = Stopwatch.StartNew();
            // var texture3a = blurFilter.Blur2(MathHelper.ToRadians(10));
            // Console.WriteLine($"Blur used {sw.Elapsed}");

            // sw = Stopwatch.StartNew();
            // var texture3b = blurFilter.Blur3(MathHelper.ToRadians(10));
            // Console.WriteLine($"Blur used {sw.Elapsed}");

            // for(int y=0;y<texture3a.Height;y++)
            //     for(int x=0;x<texture3a.Width;x++)
            //         if(texture3a.Data[y][x] != texture3b.Data[y][x])
            //         {
            //             Console.WriteLine($"{texture3a.Data[y][x]} != {texture3b.Data[y][x]}");
            //         }

            // TextureHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres360x180Blur3.raw", texture3a);

            //convert -size 360x180 -depth 16 -endian MSB gray:Ceres360x180Blur.raw Ceres360x180Blur.png

            Console.WriteLine("Done");
        }
    }
}
