using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TiffExpress.Tiff;

namespace TiffExpress
{
    // https://www.itu.int/itudoc/itu-t/com16/tiff-fx/docs/tiff6.pdf
    // https://www.awaresystems.be/imaging/tiff/bigtiff.html


    public class TiffWriter : IDisposable
    {
        private Stream _stream;
        private BinaryWriter _writer;

        private bool _bigTiff = false;
        private long _lastReferencePosition;

        public ImageFileDirectory[] ImageFileDirectories { get; private set; }

        public TiffWriter(Stream stream)
        {
            _stream = stream;
            _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
            WriteImageFileHeader();
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

        private void WriteImageFileHeader()
        {
            if (BitConverter.IsLittleEndian)
                _writer.Write((ushort)0x4949);
            else
                _writer.Write((ushort)0x4d4d);

            if (_bigTiff)
            {
                _writer.Write((ushort)43);
                _writer.Write((ushort)8);
                _writer.Write((ushort)0);
            }
            else
                _writer.Write((ushort)42);

            _lastReferencePosition = _writer.BaseStream.Position;

            WriteReference(0);
        }

        private ImageFileDirectory WriteImageFileDirectory(ImageFileDirectory ifd)
        {
            UpdateLastReference();

            if (_bigTiff)
                _writer.Write((long)ifd.Entries.Count);
            else
                _writer.Write((ushort)ifd.Entries.Count);

            var entries = ifd.Entries.Values.OrderBy(v => v.Tag);
            foreach (var entry in entries)
            {
                _writer.Write((ushort)entry.Tag);
                _writer.Write((ushort)entry.FieldType);
                if (_bigTiff)
                {
                    _writer.Write((long)entry.NumValues);
                    _writer.Write((long)entry.ValueOffset);
                }
                else
                {
                    _writer.Write((uint)entry.NumValues);
                    _writer.Write((uint)entry.ValueOffset);
                }
            }

            WriteReference(0);

            return ifd;
        }

        private void WriteReference(long position)
        {
            if (_bigTiff)
                _writer.Write((long)position);
            else
                _writer.Write((uint)position);
        }
        private void UpdateLastReference()
        {
            long currentPosition = _writer.BaseStream.Position;
            _writer.BaseStream.Position = _lastReferencePosition;

            WriteReference(currentPosition);

            _writer.BaseStream.Position = currentPosition;
        }
        private void WriteArrayField(IfdEntry entry, long[] array)
        {
            entry.ValueOffset = _writer.BaseStream.Position;
            entry.NumValues = array.Length;

            if (entry.FieldType == FieldType.UInt64)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    _writer.Write((long)array[i]);
            }
            else if (entry.FieldType == FieldType.UInt32)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    _writer.Write((uint)array[i]);
            }
            else if (entry.FieldType == FieldType.UInt16)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    _writer.Write((ushort)array[i]);
            }
            else
                throw new Exception($"Unknown field type {entry.FieldType} for tag {entry.Tag}");
        }

        public void WriteImageFile<T>(IBitmap<T> bitmap)
        {
            var ifd = new ImageFileDirectory();
            WriteImageFile<T>(ifd, bitmap);
        }

        public void WriteImageFile<T>(ImageFileDirectory ifd, IBitmap<T> bitmap)
        {
            ifd.ImageWidth = bitmap.Width;
            ifd.ImageHeight = bitmap.Height;
            WriteImageFileInternal(ifd, bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void WriteImageFile<T>(ImageFileDirectory ifd, IBitmap<T> bitmap, int offsetX, int offsetY, int imageWidth, int imageHeight)
        {
            WriteImageFileInternal(ifd, bitmap, offsetX, offsetY, imageWidth, imageHeight);
        }

        private void WriteImageFileInternal<T>(ImageFileDirectory ifd, IBitmap<T> bitmap, int offsetX, int offsetY, int imageWidth, int imageHeight)
        {
            if (ifd.PhotometricInterpretation != PhotometricInterpretation.BlackIsZero)
                throw new NotSupportedException($"PhotometricInterpretation must be BlackIsZero: {ifd.PhotometricInterpretation}");
            if (ifd.Compression != Compression.NoCompression)
                throw new NotSupportedException($"Compression not supported: {ifd.Compression}");
            if (ifd.SamplesPerPixel != 1)
                throw new NotSupportedException($"SamplesPerPixel must be 1: {ifd.SamplesPerPixel}");
            if (ifd.SamplesPerPixel > 1 && ifd.PlanarConfiguration != 1)
                throw new NotSupportedException($"PlanarConfiguration must be ChunkyFormat: {ifd.PlanarConfiguration}");

            int bytesPerPixel = ifd.SamplesPerPixel * (ifd.BitsPerSample >> 3);

            var buffer = new byte[bytesPerPixel * imageWidth];

            var stripOffsets = new List<long>();
            var stripByteCounts = new List<long>();
            foreach(var row in bitmap.GetRows())
            {
                stripOffsets.Add(_writer.BaseStream.Position);
                stripByteCounts.Add(buffer.Length);

                Buffer.BlockCopy(row, offsetX * bytesPerPixel, buffer, 0, buffer.Length);
                _writer.BaseStream.Write(buffer, 0, buffer.Length);
            }

            ifd.StripOffsets = stripOffsets.ToArray();
            ifd.StripByteCounts = stripByteCounts.ToArray();
        }


    }
}