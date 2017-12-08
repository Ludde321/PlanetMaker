using System;
using System.IO;
using System.Linq;

namespace TiffExpress
{
    public class BinaryReader2 : IDisposable
    {
        private Stream _stream;
        private readonly bool _leaveOpen;

        private readonly byte[] _buffer;
        public bool StreamIsLittleEndian = true;
        public Stream Stream { get { return _stream; } }
        public BinaryReader2(Stream stream) : this(stream, false)
        {
        }
        public BinaryReader2(Stream stream, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException("Stream is null");
            if (!stream.CanRead)
                throw new ArgumentException("Stream not readable");

            _stream = stream;
            _leaveOpen = leaveOpen;
            _buffer = new byte[8];
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            var stream = _stream;
            _stream = null;
            if (stream != null && !_leaveOpen)
                stream.Close();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public bool ReadBoolean()
        {
            FillBuffer(1);
            return _buffer[0] != 0;
        }

        public byte ReadByte()
        {
            FillBuffer(1);
            return _buffer[0];
        }

        public sbyte ReadSByte()
        {
            FillBuffer(1);
            return (sbyte)_buffer[0];
        }

        public byte[] ReadBytes(int count)
        {
            var result = new byte[count];

            int numRead = 0;
            do
            {
                int n = _stream.Read(result, numRead, count);
                if (n == 0)
                    break;
                numRead += n;
            } while (numRead < count);

            if (numRead != result.Length)
            {
                var result2 = new byte[numRead];
                Buffer.BlockCopy(result, 0, result2, 0, count);
                return result2;
            }

            return result;
        }

        public sbyte[] ReadSBytes(int count)
        {
            var bytes = ReadBytes(count);
            return bytes.Cast<sbyte>().ToArray();
        }

        public short ReadInt16()
        {
            FillBuffer(2);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }
        public ushort ReadUInt16()
        {
            FillBuffer(2);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }

        public int ReadInt32()
        {
            FillBuffer(4);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }
        public uint ReadUInt32()
        {
            FillBuffer(4);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }

        public long ReadInt64()
        {
            FillBuffer(8);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }
        public ulong ReadUInt64()
        {
            FillBuffer(8);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }

        public float ReadSingle()
        {
            FillBuffer(4);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        public double ReadDouble()
        {
            FillBuffer(8);
            if (StreamIsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }

        private void FillBuffer(int numBytes)
        {
            int bytesRead = 0;
            do
            {
                int n = _stream.Read(_buffer, bytesRead, numBytes - bytesRead);
                if (n == 0)
                    throw new Exception("EOF reached");
                bytesRead += n;
            } while (bytesRead < numBytes);
        }


    }

}