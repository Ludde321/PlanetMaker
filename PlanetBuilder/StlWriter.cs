using System;
using System.IO;
using Common;

namespace PlanetBuilder
{
    public class StlWriter : IDisposable
    {
        private Stream _stream;
        private BinaryWriter _writer;

        private int _triangleCount = 0;

        public StlWriter(Stream stream)
        {
            _stream = stream;
            _writer = new BinaryWriter(_stream);

            _writer.Write(new byte[80]); // Header 80 bytes
            _writer.Write(_triangleCount);
        }

        public void AddTriangle(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            // Triangle Normal
            _writer.Write((float)0);
            _writer.Write((float)0);
            _writer.Write((float)0);

            // Vertex 1
            _writer.Write((float)v0.x);
            _writer.Write((float)v0.y);
            _writer.Write((float)v0.z);

            // Vertex 2
            _writer.Write((float)v1.x);
            _writer.Write((float)v1.y);
            _writer.Write((float)v1.z);

            // Vertex 3
            _writer.Write((float)v2.x);
            _writer.Write((float)v2.y);
            _writer.Write((float)v2.z);

            _writer.Write((short)0); // Attribute byte count

            _triangleCount++;
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            _stream.Position = 80;
            _writer.Write(_triangleCount);

            var stream = _stream;
            _stream = null;
            stream.Dispose();
        }
    }
}