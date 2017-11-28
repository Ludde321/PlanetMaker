using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetBuilder
{
    public class TiffTexture<T> : IDisposable
    {
        private Stream _stream;
        private readonly TiffLoader _tiffLoader;

        private readonly TiffLoader.ImageFileDirectory[] _ifds;

        public IEnumerable<T[]> Data;
        public int Width;
        public int Height;

        public TiffTexture(string fileName)
        {
            _stream = File.OpenRead(fileName);
            _tiffLoader = new TiffLoader(_stream);

            _ifds = _tiffLoader.ReadImageFileDirectories();

            Width = _ifds[0].ImageWidth;
            Height = _ifds[0].ImageHeight;
            Data = _tiffLoader.ReadImageFileAs<T>(_ifds[0]);
        }

        public TiffTexture(int width, int height, IEnumerable<T[]> data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public Texture<T> ToTexture()
        {
            return new Texture<T>(Width, Height, Data);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            var stream = _stream;
            _stream = null;
            stream.Dispose();
        }
    }
}
