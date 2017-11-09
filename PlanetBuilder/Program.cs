using System;
using System.Diagnostics;

namespace PlanetBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to PlanetBuilder!");

            var sw = Stopwatch.StartNew();
            var texture = FileHelper.LoadRaw16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres_Dawn_FC_HAMO_DTM_DLR_Global_60ppd_Oct2016.raw", 21600, 10800);
            Console.WriteLine($"Loading texture used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            var resampler = new Resampler();
            //resampler.Test(10, 4);
            var texture2 = resampler.Resample(texture, 360, 180);
            Console.WriteLine($"Resampling used {sw.Elapsed}");

            FileHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres360x180.raw", texture2);

            var blurFilter = new BlurFilter(texture2);
            sw = Stopwatch.StartNew();
            var texture3a = blurFilter.Blur(10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            sw = Stopwatch.StartNew();
            var texture3b = blurFilter.Blur2(10 * (Math.PI / 180));
            Console.WriteLine($"Blur used {sw.Elapsed}");

            for(int i=0;i<texture3a.Data.Length;i++)
                if(texture3a.Data[i] != texture3b.Data[i])
                    {
                        Console.WriteLine($"{texture3a.Data[i]} != {texture3b.Data[i]}");
                    }

            FileHelper.SaveFile16(@"c:\Ludde\FractalWorlds\Planets\Ceres\Ceres360x180Blur3.raw", texture3a);

//convert -size 360x180 -depth 16 -endian MSB gray:Ceres360x180Blur.raw Ceres360x180Blur.png

            Console.WriteLine("Done");
        }
    }
}
