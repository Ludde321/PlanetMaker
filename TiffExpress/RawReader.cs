using System;
using System.Collections.Generic;
using System.IO;

namespace TiffExpress
{
    public class RawReader : IDisposable
    {
        private Stream _stream;

        public RawReader(Stream stream)
        {
            _stream = stream;
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

        public static Bitmap<T> LoadBitmap<T>(Stream stream, int inputWidth, int inputHeight)
        {
            using (var rawReader = new RawReader(stream))
            {
                return rawReader.ReadBitmap<T>(inputWidth, inputHeight).ToBitmap();
            }
        }

        public EnumerableBitmap<T> ReadBitmap<T>(int inputWidth, int inputHeight)
        {
            return ReadBitmap<T>(inputWidth, inputHeight, 0, 0, inputWidth, inputHeight);
        }

        public EnumerableBitmap<T> ReadBitmap<T>(int inputWidth, int inputHeight, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            if (offsetX < 0 || offsetX >= inputWidth)
                throw new ArgumentOutOfRangeException("offsetX");
            if (offsetY < 0 || offsetY >= inputHeight)
                throw new ArgumentOutOfRangeException("offsetY");
            if (outputWidth <= 0)
                throw new ArgumentOutOfRangeException("outputWidth");
            if (outputHeight <= 0)
                throw new ArgumentOutOfRangeException("outputHeight");

            outputWidth = Math.Min(outputWidth, inputWidth - offsetX);
            outputHeight = Math.Min(outputHeight, inputHeight - offsetY);

            var rows = ReadBitmapRows<T>(inputWidth, inputHeight, offsetX, offsetY, outputWidth, outputHeight);

            return new EnumerableBitmap<T>(outputWidth, outputHeight, 1, rows);
        }

        private IEnumerable<T[]> ReadBitmapRows<T>(int inputWidth, int inputHeight, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            if (typeof(T) == typeof(byte))
            {
                return (IEnumerable<T[]>)ReadBitmapRows8(inputWidth, inputHeight, offsetX, offsetY, outputWidth, outputHeight);
            }
            else if (typeof(T) == typeof(short))
            {
                return (IEnumerable<T[]>)ReadBitmapRows16(inputWidth, inputHeight, offsetX, offsetY, outputWidth, outputHeight);
            }
            else if (typeof(T) == typeof(float))
            {
                return (IEnumerable<T[]>)ReadBitmapRows32(inputWidth, inputHeight, offsetX, offsetY, outputWidth, outputHeight);
            }
            return null;
        }
        private IEnumerable<byte[]> ReadBitmapRows8(int inputWidth, int inputHeight, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            _stream.Position = 2 * (offsetY * inputWidth + offsetX);

            var row = new byte[outputWidth];

            for (int y = 0; y < outputHeight; y++)
            {
                _stream.Read(row, 0, row.Length);
                yield return row;

                _stream.Position += inputWidth - outputWidth;
            }
        }
        private IEnumerable<short[]> ReadBitmapRows16(int inputWidth, int inputHeight, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            _stream.Position = 2 * (offsetY * inputWidth + offsetX);

            var row = new short[outputWidth];
            var buffer = new byte[2 * outputWidth];

            for (int y = 0; y < outputHeight; y++)
            {
                _stream.Read(buffer, 0, buffer.Length);
                Buffer.BlockCopy(buffer, 0, row, 0, buffer.Length);
                yield return row;

                if (inputWidth != outputWidth)
                    _stream.Position += 2 * (inputWidth - outputWidth);
            }
        }

        private IEnumerable<float[]> ReadBitmapRows32(int inputWidth, int inputHeight, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            _stream.Position = 4 * (offsetY * inputWidth + offsetX);

            var row = new float[outputWidth];
            var buffer = new byte[4 * outputWidth];

            for (int y = 0; y < outputHeight; y++)
            {
                _stream.Read(buffer, 0, buffer.Length);
                Buffer.BlockCopy(buffer, 0, row, 0, buffer.Length);
                yield return row;

                if (inputWidth != outputWidth)
                    _stream.Position += 4 * (inputWidth - outputWidth);
            }
        }
    }
}