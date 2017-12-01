using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TiffExpress
{
    // https://www.itu.int/itudoc/itu-t/com16/tiff-fx/docs/tiff6.pdf
    // https://www.awaresystems.be/imaging/tiff/bigtiff.html


    public class TiffFile : IDisposable
    {
        private Stream _stream;
        private BinaryReader2 _reader;

        private bool _bigTiff = false;

        public ImageFileDirectory[] ImageFileDirectories { get; private set; }

        public TiffFile(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader2(_stream, true);
            ImageFileDirectories = ReadImageFileDirectories();
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


        private ImageFileDirectory[] ReadImageFileDirectories()
        {
            _reader.Stream.Position = 0;

            short byteOrder = _reader.ReadInt16();
            if (byteOrder == 0x4949)
                _reader.StreamIsLittleEndian = true;
            else if (byteOrder == 0x4d4d)
                _reader.StreamIsLittleEndian = false;
            else
                throw new Exception($"Not a TIFF file: {byteOrder}");

            short meaningOfLife = _reader.ReadInt16();
            if (meaningOfLife == 42)
                _bigTiff = false;
            else if (meaningOfLife == 43)
                _bigTiff = true;
            else
                throw new Exception($"Not a TIFF file. Life: {meaningOfLife}");

            if (_bigTiff)
            {
                _reader.ReadUInt16(); // always 8
                _reader.ReadUInt16(); // always 0
            }

            // Read Image File Directories
            var ifdList = new List<ImageFileDirectory>();

            long ifdOffset = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();
            while (ifdOffset > 0)
            {
                _reader.Stream.Position = ifdOffset;

                var ifd = new ImageFileDirectory();

                long numFields = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt16();

                for (int i = 0; i < numFields; i++)
                {
                    ushort tag = _reader.ReadUInt16();
                    ushort type = _reader.ReadUInt16();
                    long numValues = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();
                    long valueOffset = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();

                    switch ((IfdTag)tag)
                    {
                        case IfdTag.ImageWidth:
                            {
                                ifd.ImageWidth = (int)valueOffset;
                            }
                            break;
                        case IfdTag.ImageHeight:
                            {
                                ifd.ImageHeight = (int)valueOffset;
                            }
                            break;
                        case IfdTag.BitsPerSample:
                            {
                                ifd.BitsPerSample = (ushort)valueOffset;
                            }
                            break;
                        case IfdTag.PhotometricInterpretation:
                            {
                                ifd.PhotometricInterpretation = (ushort)valueOffset;
                            }
                            break;
                        case IfdTag.Compression:
                            {
                                ifd.Compression = (ushort)valueOffset;
                            }
                            break;
                        case IfdTag.StripOffsets:
                            {
                                ifd.StripOffsetsType = type;
                                ifd.NumStripOffsets = (uint)numValues;
                                ifd.StripOffsets = valueOffset;
                            }
                            break;
                        case IfdTag.SamplesPerPixel:
                            {
                                ifd.SamplesPerPixel = (ushort)valueOffset;
                            }
                            break;
                        case IfdTag.RowsPerStrip:
                            {
                                ifd.RowsPerStrip = (uint)valueOffset;
                            }
                            break;
                        case IfdTag.StripByteCounts:
                            {
                                ifd.StripByteCountsType = type;
                                ifd.NumStripByteCounts = (uint)numValues;
                                ifd.StripByteCounts = valueOffset;
                            }
                            break;
                        case IfdTag.PlanarConfiguration:
                            {
                                ifd.PlanarConfiguration = (ushort)valueOffset;
                            }
                            break;
                        case IfdTag.SampleFormat:
                            {
                                ifd.SampleFormat = (ushort)valueOffset;
                            }
                            break;
                        default:
                            {
                                Console.WriteLine($"Tag: {tag} Type: {type} NumValues: {numValues} Value/Offset: {valueOffset}");
                            }
                            break;
                    }
                }
                ifdList.Add(ifd);

                ifdOffset = _reader.ReadUInt32();
            }

            return ifdList.ToArray();
        }

        public EnumerableBitmap<T> ReadImageFile<T>()
        {
            return ReadImageFile<T>(ImageFileDirectories[0]);
        }

        public EnumerableBitmap<T> ReadImageFile<T>(int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            return ReadImageFile<T>(ImageFileDirectories[0], offsetX, offsetY, outputWidth, outputHeight);
        }
        public EnumerableBitmap<T> ReadImageFile<T>(ImageFileDirectory ifd)
        {
            return ReadImageFile<T>(ifd, 0, 0, ifd.ImageWidth, ifd.ImageHeight);
        }

        public EnumerableBitmap<T> ReadImageFile<T>(ImageFileDirectory ifd, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            if(offsetX < 0 || offsetX >= ifd.ImageWidth)
                throw new ArgumentOutOfRangeException("offsetX");
            if(offsetY < 0 || offsetY >= ifd.ImageHeight)
                throw new ArgumentOutOfRangeException("offsetY");
            if(outputWidth <= 0)
                throw new ArgumentOutOfRangeException("outputWidth");
            if(outputHeight <= 0)
                throw new ArgumentOutOfRangeException("outputHeight");
            
            outputWidth = Math.Min(outputWidth, ifd.ImageWidth - offsetX);
            outputHeight = Math.Min(outputHeight, ifd.ImageHeight - offsetY);

            var rows = ReadImageFileInternal(ifd, offsetX, offsetY, outputWidth, outputHeight);

            return new EnumerableBitmap<T>(outputWidth, outputHeight, rows.Select(row => (T[])ConvertRow(ifd, row)));
        }


        private IEnumerable<byte[]> ReadImageFileInternal(ImageFileDirectory ifd, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            if (ifd.PhotometricInterpretation != 1)
                throw new NotSupportedException($"PhotometricInterpretation must be BlackIsZero: {ifd.PhotometricInterpretation}");
            if (ifd.Compression != 1)
                throw new NotSupportedException($"Compression not supported: {ifd.Compression}");
            if (ifd.SamplesPerPixel != 1)
                throw new NotSupportedException($"SamplesPerPixel must be 1: {ifd.SamplesPerPixel}");
            if (ifd.SamplesPerPixel > 1 && ifd.PlanarConfiguration != 1)
                throw new NotSupportedException($"PlanarConfiguration must be ChunkyFormat: {ifd.PlanarConfiguration}");

            _reader.Stream.Position = ifd.StripOffsets;
            var stripOffsets = new long[ifd.NumStripOffsets];
            for (int i = 0; i < ifd.NumStripOffsets; i++)
                stripOffsets[i] = ifd.StripOffsetsType == 16 ? _reader.ReadInt64() : _reader.ReadUInt32();

            _reader.Stream.Position = ifd.StripByteCounts;
            var stripByteCounts = new long[ifd.NumStripByteCounts];
            for (int i = 0; i < ifd.NumStripByteCounts; i++)
                stripByteCounts[i] = ifd.StripOffsetsType == 16 ? _reader.ReadInt64() : _reader.ReadUInt32();

            int bytesPerPixel = ifd.SamplesPerPixel * (ifd.BitsPerSample >> 3);
            int outputBufferSize = bytesPerPixel * outputWidth;

            int y = 0;
            for (uint i = 0; i < ifd.NumStripOffsets; i++)
            {
                long bytesPerRow = stripByteCounts[i] / ifd.RowsPerStrip;
                long position = stripOffsets[i] + offsetX * bytesPerPixel;
                for (uint j = 0; j < ifd.RowsPerStrip; j++)
                {
                    if (y >= offsetY && y < offsetY + outputHeight)
                    {
                        _stream.Position = position;

                        var buffer = new byte[outputBufferSize];
                        _stream.Read(buffer, 0, buffer.Length);
                        yield return buffer;
                    }
                    position += bytesPerRow;
                    y++;
                }
            }
        }


        public class ImageFileDirectory
        {
            public int ImageWidth;
            public int ImageHeight;
            public ushort BitsPerSample;
            public ushort PhotometricInterpretation; // 0 = WhiteIsZero, 1 = BlackIsZero
            public ushort Compression;
            public ushort StripOffsetsType;
            public uint NumStripOffsets;
            public long StripOffsets;
            public ushort SamplesPerPixel;
            public uint RowsPerStrip;
            public ushort StripByteCountsType;
            public uint NumStripByteCounts;
            public long StripByteCounts;
            public ushort PlanarConfiguration; // 1 = Chunky format, 2 = Planar format
            public ushort SampleFormat; // 1 = unsigned integer data, 2 = two’s complement signed integer data, 3 = IEEE floating point data [IEEE]
        }

        private enum IfdTag
        {
            ImageWidth = 256,
            ImageHeight = 257,
            BitsPerSample = 258,
            Compression = 259,
            PhotometricInterpretation = 262,
            StripOffsets = 273,
            SamplesPerPixel = 277,
            RowsPerStrip = 278,
            StripByteCounts = 279,
            PlanarConfiguration = 284,
            SampleFormat = 339,
        }

        private Array ConvertRow(ImageFileDirectory ifd, byte[] row)
        {
            if (ifd.SamplesPerPixel == 1)
            {
                if (ifd.BitsPerSample == 8)
                {
                    if (ifd.SampleFormat == 1) // 1 unsigned, 2 signed, 3 float
                    {
                        // var output = new byte[width];
                        // Buffer.BlockCopy(row, 0, output, 0, width + width);
                        // return output;
                        return row;
                    }
                }
                else if (ifd.BitsPerSample == 16)
                {
                    if (ifd.SampleFormat == 1 || ifd.SampleFormat == 2)
                    {
                        var output = new short[row.Length / 2];
                        if (_reader.StreamIsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            int w2 = row.Length;
                            for (int i = 0; i < w2; i += 2)
                            {
                                byte b = row[i];
                                row[i] = row[i + 1];
                                row[i + 1] = b;
                            }
                        }
                        Buffer.BlockCopy(row, 0, output, 0, row.Length);
                        return output;
                    }
                }
                else if (ifd.BitsPerSample == 32)
                {
                    if (ifd.SampleFormat == 3)
                    {
                        var output = new float[row.Length / 4];
                        if (_reader.StreamIsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            int w4 = row.Length;
                            for (int i = 0; i < w4; i += 4)
                            {
                                byte b = row[i];
                                row[i] = row[i + 3];
                                row[i + 3] = b;

                                b = row[i + 1];
                                row[i + 1] = row[i + 2];
                                row[i + 2] = b;
                            }
                        }
                        Buffer.BlockCopy(row, 0, output, 0, row.Length);
                        return output;
                    }
                }
            }
            return null;
        }

    }
}