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

        public bool BigTiff = false;
        private long _lastReferencePosition;

        private bool _writeFileHeader = true;

        public ImageFileDirectory[] ImageFileDirectories { get; private set; }

        public TiffWriter(Stream stream)
        {
            _stream = stream;
            _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
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
            if (_writeFileHeader)
            {
                if (BitConverter.IsLittleEndian)
                    _writer.Write((ushort)0x4949);
                else
                    _writer.Write((ushort)0x4d4d);

                if (BigTiff)
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
            _writeFileHeader = false;
        }

        private void WriteImageFileDirectory(ImageFileDirectory ifd)
        {
            if (ifd.Entries.TryGetValue(IfdTag.StripOffsets, out var stripOffsetsEntry))
                WriteArrayField(stripOffsetsEntry, ifd.StripOffsets);

            if (ifd.Entries.TryGetValue(IfdTag.StripByteCounts, out var stripByteCountsEntry))
                WriteArrayField(stripByteCountsEntry, ifd.StripByteCounts);

            UpdateLastReference();

            var entries = ifd.Entries.Values.OrderBy(v => v.Tag).ToArray();

            if (BigTiff)
                _writer.Write((long)entries.Length);
            else
                _writer.Write((ushort)entries.Length);

            foreach (var entry in entries)
            {
                _writer.Write((ushort)entry.Tag);
                _writer.Write((ushort)entry.FieldType);
                if (BigTiff)
                    _writer.Write((long)entry.NumValues);
                else
                    _writer.Write((uint)entry.NumValues);
                
                if(entry.Value != null)
                    WriteFieldValue(entry.FieldType, entry.Value);
                else
                    WriteFieldValue(entry.FieldType, entry.Offset);
            }

            _lastReferencePosition = _writer.BaseStream.Position;
            WriteReference(0);
        }

        private void WriteFieldValue(FieldType fieldType, object value)
        {
            int paddingBytes = (BigTiff ? 8 : 4);
            switch (fieldType)
            {
                case FieldType.Int64:
                    _writer.Write((long)Convert.ChangeType(value, typeof(long)));
                    paddingBytes -= 8;
                    break;
                case FieldType.UInt64:
                    _writer.Write((ulong)Convert.ChangeType(value, typeof(ulong)));
                    paddingBytes -= 8;
                    break;
                case FieldType.Int32:
                    _writer.Write((int)Convert.ChangeType(value, typeof(int)));
                    paddingBytes -= 4;
                    break;
                case FieldType.UInt32:
                    _writer.Write((uint)Convert.ChangeType(value, typeof(uint)));
                    paddingBytes -= 4;
                    break;
                case FieldType.Int16:
                    _writer.Write((short)Convert.ChangeType(value, typeof(short)));
                    paddingBytes -= 2;
                    break;
                case FieldType.UInt16:
                    _writer.Write((ushort)Convert.ChangeType(value, typeof(ushort)));
                    paddingBytes -= 2;
                    break;
                case FieldType.Byte:
                    _writer.Write((byte)Convert.ChangeType(value, typeof(byte)));
                    paddingBytes -= 1;
                    break;
                case FieldType.SByte:
                    _writer.Write((sbyte)Convert.ChangeType(value, typeof(sbyte)));
                    paddingBytes -= 1;
                    break;
                case FieldType.Rational:
                    {
                        var rValue = (Rational)value;
                        _writer.Write(rValue.Numerator);
                        _writer.Write(rValue.Denominator);
                        paddingBytes -= 8;
                    }
                    break;
                case FieldType.SRational:
                    {
                        var rValue = (SRational)value;
                        _writer.Write(rValue.Numerator);
                        _writer.Write(rValue.Denominator);
                        paddingBytes -= 8;
                    }
                    break;
                default:
                    throw new Exception($"Unknown field type {fieldType}");
            }
            _writer.Write(new byte[paddingBytes]);
        }

        private void PrepareImageFileDirectory(ImageFileDirectory ifd)
        {
            ifd.Entries[IfdTag.ImageWidth] = new IfdEntry { Tag = IfdTag.ImageWidth, FieldType = FieldType.UInt32, NumValues = 1, Value = ifd.ImageWidth };
            ifd.Entries[IfdTag.ImageHeight] = new IfdEntry { Tag = IfdTag.ImageHeight, FieldType = FieldType.UInt32, NumValues = 1, Value = ifd.ImageHeight };
            ifd.Entries[IfdTag.BitsPerSample] = new IfdEntry { Tag = IfdTag.BitsPerSample, FieldType = FieldType.UInt16, NumValues = 1, Value = ifd.BitsPerSample };
            ifd.Entries[IfdTag.PhotometricInterpretation] = new IfdEntry { Tag = IfdTag.PhotometricInterpretation, FieldType = FieldType.UInt16, NumValues = 1, Value = (ushort)ifd.PhotometricInterpretation };
            ifd.Entries[IfdTag.Compression] = new IfdEntry { Tag = IfdTag.Compression, FieldType = FieldType.UInt16, NumValues = 1, Value = (ushort)ifd.Compression };
            ifd.Entries[IfdTag.SamplesPerPixel] = new IfdEntry { Tag = IfdTag.SamplesPerPixel, FieldType = FieldType.UInt16, NumValues = 1, Value = (ushort)ifd.SamplesPerPixel };
            ifd.Entries[IfdTag.RowsPerStrip] = new IfdEntry { Tag = IfdTag.RowsPerStrip, FieldType = FieldType.UInt32, NumValues = 1, Value = (uint)ifd.RowsPerStrip };
            ifd.Entries[IfdTag.PlanarConfiguration] = new IfdEntry { Tag = IfdTag.PlanarConfiguration, FieldType = FieldType.UInt16, NumValues = 1, Value = (uint)ifd.PlanarConfiguration };
            ifd.Entries[IfdTag.SampleFormat] = new IfdEntry { Tag = IfdTag.SampleFormat, FieldType = FieldType.UInt16, NumValues = 1, Value = (uint)ifd.SampleFormat };

            ifd.Entries[IfdTag.StripOffsets] = new IfdEntry { Tag = IfdTag.StripOffsets, FieldType = BigTiff ? FieldType.UInt64 : FieldType.UInt32 };
            ifd.Entries[IfdTag.StripByteCounts] = new IfdEntry { Tag = IfdTag.StripByteCounts, FieldType = BigTiff ? FieldType.UInt64 : FieldType.UInt32 };
        }


        private void WriteReference(long position)
        {
            if (BigTiff)
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
            entry.Offset = _writer.BaseStream.Position;
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

        public ImageFileDirectory WriteImageFile<T>(IBitmap<T> bitmap)
        {
            var ifd = new ImageFileDirectory();
            WriteImageFile<T>(ifd, bitmap);
            return ifd;
        }

        public void WriteImageFile<T>(ImageFileDirectory ifd, IBitmap<T> bitmap)
        {
            WriteImageFile(ifd, bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void WriteImageFile<T>(ImageFileDirectory ifd, IBitmap<T> bitmap, int offsetX, int offsetY, int imageWidth, int imageHeight)
        {
            if (ifd.PhotometricInterpretation != PhotometricInterpretation.BlackIsZero)
                throw new NotSupportedException($"PhotometricInterpretation must be BlackIsZero: {ifd.PhotometricInterpretation}");
            if (ifd.Compression != Compression.NoCompression)
                throw new NotSupportedException($"Compression not supported: {ifd.Compression}");
            if (ifd.SamplesPerPixel != 1)
                throw new NotSupportedException($"SamplesPerPixel must be 1: {ifd.SamplesPerPixel}");
            if (ifd.SamplesPerPixel > 1 && ifd.PlanarConfiguration != PlanarConfiguration.Chunky)
                throw new NotSupportedException($"PlanarConfiguration must be ChunkyFormat: {ifd.PlanarConfiguration}");

            WriteImageFileHeader();

            ifd.ImageWidth = imageWidth;
            ifd.ImageHeight = imageHeight;
            ifd.SamplesPerPixel = (ushort)bitmap.SamplesPerPixel;
            if (bitmap.SamplesPerPixel == 1)
                ifd.PhotometricInterpretation = PhotometricInterpretation.BlackIsZero;
            else if (bitmap.SamplesPerPixel == 3)
                ifd.PhotometricInterpretation = PhotometricInterpretation.RGB;
            ifd.Compression = Compression.NoCompression;

            if (typeof(T) == typeof(byte))
            {
                ifd.BitsPerSample = 8;
                ifd.SampleFormat = SampleFormat.Unsigned;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                ifd.BitsPerSample = 8;
                ifd.SampleFormat = SampleFormat.Signed;
            }
            if (typeof(T) == typeof(ushort))
            {
                ifd.BitsPerSample = 16;
                ifd.SampleFormat = SampleFormat.Unsigned;
            }
            else if (typeof(T) == typeof(short))
            {
                ifd.BitsPerSample = 16;
                ifd.SampleFormat = SampleFormat.Signed;
            }
            else if (typeof(T) == typeof(float))
            {
                ifd.BitsPerSample = 32;
                ifd.SampleFormat = SampleFormat.FloatingPoint;
            }
            WriteBitmap(ifd, bitmap, offsetX, offsetY, imageWidth, imageHeight);

            PrepareImageFileDirectory(ifd);

            WriteImageFileDirectory(ifd);
        }

        private void WriteBitmap<T>(ImageFileDirectory ifd, IBitmap<T> bitmap, int offsetX, int offsetY, int imageWidth, int imageHeight)
        {
            int bytesPerPixel = ifd.SamplesPerPixel * (ifd.BitsPerSample >> 3);

            var buffer = new byte[bytesPerPixel * imageWidth];

            var stripOffsets = new List<long>();
            var stripByteCounts = new List<long>();
            foreach (var row in bitmap.GetRows())
            {
                stripOffsets.Add(_writer.BaseStream.Position);
                stripByteCounts.Add(buffer.Length);

                Buffer.BlockCopy(row, offsetX * bytesPerPixel, buffer, 0, buffer.Length);
                _writer.BaseStream.Write(buffer, 0, buffer.Length);
            }

            ifd.RowsPerStrip = 1;
            ifd.StripOffsets = stripOffsets.ToArray();
            ifd.StripByteCounts = stripByteCounts.ToArray();
        }


    }
}