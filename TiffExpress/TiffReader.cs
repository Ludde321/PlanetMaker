using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TiffExpress.Tiff;

namespace TiffExpress
{
    // https://www.itu.int/itudoc/itu-t/com16/tiff-fx/docs/tiff6.pdf
    // https://www.awaresystems.be/imaging/tiff/bigtiff.html


    public partial class TiffReader : IDisposable
    {
        private Stream _stream;
        private BinaryReader2 _reader;

        private bool _bigTiff = false;

        public ImageFileDirectory[] ImageFileDirectories { get; private set; }

        public TiffReader(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader2(_stream, true);
            ReadImageFileHeader();
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

        private void ReadImageFileHeader()
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
                if (_reader.ReadUInt16() != 8)
                    throw new Exception("Offset should be size 8 in BigTIFF");
                _reader.ReadUInt16(); // always 0
            }
        }

        private ImageFileDirectory[] ReadImageFileDirectories()
        {
            var ifdList = new List<ImageFileDirectory>();

            long ifdOffset = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();
            while (ifdOffset > 0)
            {
                _reader.Stream.Position = ifdOffset;

                var ifd = new ImageFileDirectory();

                long numFields = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt16();

                for (int i = 0; i < numFields; i++)
                {
                    IfdTag tag = (IfdTag)_reader.ReadUInt16();
                    FieldType type = (FieldType)_reader.ReadUInt16();
                    long numValues = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();
                    long valueOffset = _bigTiff ? _reader.ReadInt64() : _reader.ReadUInt32();

                    ifd.Entries.Add(tag, new IfdEntry { Tag = tag, FieldType = type, NumValues = numValues, ValueOffset = valueOffset });

                    switch (tag)
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
                                ifd.PhotometricInterpretation = (PhotometricInterpretation)valueOffset;
                            }
                            break;
                        case IfdTag.Compression:
                            {
                                ifd.Compression = (Compression)valueOffset;
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
                        case IfdTag.PlanarConfiguration:
                            {
                                ifd.PlanarConfiguration = (PlanarConfiguration)valueOffset;
                            }
                            break;
                        case IfdTag.SampleFormat:
                            {
                                ifd.SampleFormat = (SampleFormat)valueOffset;
                            }
                            break;
                        case IfdTag.StripOffsets:
                        case IfdTag.StripByteCounts:
                            break;
                        default:
                            {
                                Console.WriteLine($"Tag: {tag} Type: {type} NumValues: {numValues} Value/Offset: {valueOffset}");
                            }
                            break;
                    }
                }
                ifdList.Add(ifd);

                if (_bigTiff)
                    ifdOffset = _reader.ReadInt64();
                else
                    ifdOffset = _reader.ReadUInt32();


                IfdEntry entry;
                if (ifd.Entries.TryGetValue(IfdTag.StripOffsets, out entry))
                    ifd.StripOffsets = ReadArrayField(entry);

                if (ifd.Entries.TryGetValue(IfdTag.StripByteCounts, out entry))
                    ifd.StripByteCounts = ReadArrayField(entry);
            }

            return ifdList.ToArray();
        }

        private long[] ReadArrayField(IfdEntry entry)
        {
            _reader.Stream.Position = entry.ValueOffset;
            var array = new long[entry.NumValues];

            if (entry.FieldType == FieldType.UInt64)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    array[i] = _reader.ReadInt64();
            }
            else if (entry.FieldType == FieldType.UInt32)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    array[i] = _reader.ReadUInt32();
            }
            else if (entry.FieldType == FieldType.UInt16)
            {
                for (int i = 0; i < entry.NumValues; i++)
                    array[i] = _reader.ReadUInt16();
            }
            else
                throw new Exception($"Unknown field type {entry.FieldType} for tag {entry.Tag}");

            return array;
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
            if (offsetX < 0 || offsetX >= ifd.ImageWidth)
                throw new ArgumentOutOfRangeException("offsetX");
            if (offsetY < 0 || offsetY >= ifd.ImageHeight)
                throw new ArgumentOutOfRangeException("offsetY");
            if (outputWidth <= 0)
                throw new ArgumentOutOfRangeException("outputWidth");
            if (outputHeight <= 0)
                throw new ArgumentOutOfRangeException("outputHeight");

            outputWidth = Math.Min(outputWidth, ifd.ImageWidth - offsetX);
            outputHeight = Math.Min(outputHeight, ifd.ImageHeight - offsetY);

            var rows = ReadImageFileInternal(ifd, offsetX, offsetY, outputWidth, outputHeight);

            return new EnumerableBitmap<T>(outputWidth, outputHeight, rows.Select(row => (T[])ConvertRow(ifd, row)));

            // var blockingQueue = new BlockingCollection<byte[]>(8);
            // Task.Run(() => 
            // {
            //     var rows = ReadImageFileInternal(ifd, offsetX, offsetY, outputWidth, outputHeight);
            //     foreach(var row in rows)
            //        blockingQueue.Add(row);
            //     blockingQueue.CompleteAdding(); 
            // });

            // return new EnumerableBitmap<T>(outputWidth, outputHeight, blockingQueue.GetConsumingEnumerable().Select(row => (T[])ConvertRow(ifd, row)));
        }

        private IEnumerable<byte[]> ReadImageFileInternal(ImageFileDirectory ifd, int offsetX, int offsetY, int outputWidth, int outputHeight)
        {
            if (ifd.PhotometricInterpretation != PhotometricInterpretation.BlackIsZero)
                throw new NotSupportedException($"PhotometricInterpretation must be BlackIsZero: {ifd.PhotometricInterpretation}");
            if (ifd.Compression != Compression.NoCompression)
                throw new NotSupportedException($"Compression not supported: {ifd.Compression}");
            if (ifd.SamplesPerPixel != 1)
                throw new NotSupportedException($"SamplesPerPixel must be 1: {ifd.SamplesPerPixel}");
            if (ifd.SamplesPerPixel > 1 && ifd.PlanarConfiguration != PlanarConfiguration.Chunky)
                throw new NotSupportedException($"PlanarConfiguration must be ChunkyFormat: {ifd.PlanarConfiguration}");

            int bytesPerPixel = ifd.SamplesPerPixel * (ifd.BitsPerSample >> 3);
            int outputBufferSize = bytesPerPixel * outputWidth;

            int y = 0;
            for (uint i = 0; i < ifd.StripOffsets.Length; i++)
            {
                long bytesPerRow = ifd.StripByteCounts[i] / ifd.RowsPerStrip;
                long position = ifd.StripOffsets[i] + offsetX * bytesPerPixel;
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


        private Array ConvertRow(ImageFileDirectory ifd, byte[] row)
        {
            if (ifd.SamplesPerPixel == 1)
            {
                if (ifd.BitsPerSample == 8)
                {
                    if (ifd.SampleFormat == SampleFormat.Unsigned || ifd.SampleFormat == SampleFormat.Signed)
                    {
                        // var output = new byte[width];
                        // Buffer.BlockCopy(row, 0, output, 0, width + width);
                        // return output;
                        return row;
                    }
                }
                else if (ifd.BitsPerSample == 16)
                {
                    if (ifd.SampleFormat == SampleFormat.Unsigned || ifd.SampleFormat == SampleFormat.Signed)
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
                    if (ifd.SampleFormat == SampleFormat.FloatingPoint)
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