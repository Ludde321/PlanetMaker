﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PlanetBuilder.Planets;
using PlanetBuilder.Roam;

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

            // var pluto = new Pluto();
            // pluto.Create();

            // var charon = new Charon();
            // charon.Create();

            // var moon = new Moon();
            // moon.Create();

            // var mercury = new Mercury();
            // mercury.Create();

            // var mars = new Mars();
            // mars.Create();

            // var mars2 = new Mars2();
            // mars2.Create();

            // var marsSector = new MarsSector();
            // marsSector.Create();

            // var marsDouble = new MarsDouble();
            // marsDouble.Create();

            var marsDouble2 = new MarsDouble2();
            marsDouble2.Create();

            // var phobos = new Phobos();
            // phobos.Create();

            // var venus = new Venus();
            // venus.Create();

            // var earth = new Earth();
            // earth.Create();

            // var roamCube = new RoamCube();
            // roamCube.Init();
            // roamCube.Split();
            // //roamCube.Merge();

            // Console.WriteLine($"NumVertexes: {roamCube.ActiveVertexes.Count()}");
            // Console.WriteLine($"NumTriangles: {roamCube.ActiveTriangles.Count()}");
            // Console.WriteLine($"NumDiamonds: {roamCube.ActiveDiamonds.Count()}");

            // using(var stlWriter = new StlWriter(File.Create("Generated/Roam.stl")))
            // {
            //     var triangle = roamCube.ActiveTriangles.NextNode;
            //     for (; triangle != roamCube.ActiveTriangles;triangle = triangle.NextNode)
            //     {
            //         stlWriter.AddTriangle(triangle.Vertexes[0].Position, triangle.Vertexes[1].Position, triangle.Vertexes[2].Position);
            //     }
            // }


            Console.WriteLine("Done");
        }
    }
}
