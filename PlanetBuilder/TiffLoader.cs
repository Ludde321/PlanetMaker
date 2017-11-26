using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetBuilder
{
    // https://www.itu.int/itudoc/itu-t/com16/tiff-fx/docs/tiff6.pdf
    // https://www.awaresystems.be/imaging/tiff/bigtiff.html


    public class TiffLoader
    {
        private Stream _stream;
        private BinaryReader2 _reader;

        private bool _bigTiff = false;

        public TiffLoader(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader2(_stream, true);
        }

        public ImageFileDirectory[] ReadImageFileDirectories()
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

            if(_bigTiff)
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
                    long numValues = _bigTiff ? _reader.ReadInt64() :_reader.ReadUInt32();
                    long valueOffset = _bigTiff ? _reader.ReadInt64() :_reader.ReadUInt32();

                    switch ((IfdTag)tag)
                    {
                        case IfdTag.ImageWidth:
                            {
                                ifd.ImageWidth = (uint)valueOffset;
                            }
                            break;
                        case IfdTag.ImageHeight:
                            {
                                ifd.ImageHeight = (uint)valueOffset;
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

        public IEnumerable<short[]> ReadImageFileAsInt16(ImageFileDirectory ifd)
        {
            var rows = ReadImageFile(ifd);
            foreach (var row in rows)
                yield return ConvertRowToInt16(ifd, row);
        }
        public IEnumerable<float[]> ReadImageFileAsFloat(ImageFileDirectory ifd)
        {
            var rows = ReadImageFile(ifd);
            foreach (var row in rows)
                yield return ConvertRowToFloat(ifd, row);
        }

        public IEnumerable<byte[]> ReadImageFile(ImageFileDirectory ifd)
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
            var stripOffsets = new uint[ifd.NumStripOffsets];
            for (int i = 0; i < ifd.NumStripOffsets; i++)
                stripOffsets[i] = _reader.ReadUInt32();

            _reader.Stream.Position = ifd.StripByteCounts;
            var stripByteCounts = new uint[ifd.NumStripByteCounts];
            for (int i = 0; i < ifd.NumStripByteCounts; i++)
                stripByteCounts[i] = _reader.ReadUInt32();

            for (uint i = 0; i < ifd.NumStripOffsets; i++)
            {
                _stream.Position = stripOffsets[i];

                uint bytesPerRow = stripByteCounts[i] / ifd.RowsPerStrip;

                for (uint j = 0; j < ifd.RowsPerStrip; j++)
                {
                    var buffer = new byte[bytesPerRow];
                    _stream.Read(buffer, 0, (int)bytesPerRow);
                    yield return buffer;
                }
            }
        }


        public class ImageFileDirectory
        {
            public uint ImageWidth;
            public uint ImageHeight;
            public ushort BitsPerSample;
            public ushort PhotometricInterpretation; // 0 = WhiteIsZero, 1 = BlackIsZero
            public ushort Compression;
            public uint NumStripOffsets;
            public long StripOffsets;
            public ushort SamplesPerPixel;
            public uint RowsPerStrip;
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

        private short[] ConvertRowToInt16(ImageFileDirectory ifd, byte[] row)
        {
            int width = (int)ifd.ImageWidth;
            if (ifd.SamplesPerPixel == 1)
            {
                if (ifd.BitsPerSample == 16)
                {
                    if (ifd.SampleFormat == 1 || ifd.SampleFormat == 2)
                    {
                        var output = new short[width];
                        if (_reader.StreamIsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            int w2 = width + width;
                            for (int i = 0; i < w2; i += 2)
                            {
                                byte b = row[i];
                                row[i] = row[i + 1];
                                row[i + 1] = b;
                            }
                        }
                        Buffer.BlockCopy(row, 0, output, 0, width + width);
                        return output;
                    }
                }
            }
            return null;
        }

        private float[] ConvertRowToFloat(ImageFileDirectory ifd, byte[] row)
        {
            int width = (int)ifd.ImageWidth;
            if (ifd.SamplesPerPixel == 1)
            {
                if (ifd.BitsPerSample == 32)
                {
                    if (ifd.SampleFormat == 3)
                    {
                        var output = new float[width];
                        if (_reader.StreamIsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            int w4 = 4 * width;
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
                        Buffer.BlockCopy(row, 0, output, 0, width + width);
                        return output;
                    }
                }
            }
            return null;
        }
    }
}