using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetBuilder
{
    // public class TiffBitmap<T> : IDisposable
    // {
    //     private Stream _stream;
    //     private readonly TiffLoader _tiffLoader;

    //     private readonly TiffLoader.ImageFileDirectory[] _ifds;

    //     public TiffBitmap(string fileName)
    //     {
    //         _stream = File.OpenRead(fileName);
    //         _tiffLoader = new TiffLoader(_stream);

    //         _ifds = _tiffLoader.ReadImageFileDirectories();

    //         Data = _tiffLoader.ReadImageFileAs<T>(_ifds[0]);
    //     }

    //     public void Close()
    //     {
    //         Dispose();
    //     }

    //     public void Dispose()
    //     {
    //         var stream = _stream;
    //         _stream = null;
    //         stream.Dispose();
    //     }
    // }
}
