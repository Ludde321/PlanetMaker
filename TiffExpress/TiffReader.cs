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

                int sizeInBytes = (int)numFields * (2 + 2 + 2 * (_bigTiff ? 8 : 4)) + (_bigTiff ? 8 : 4);
                using (var ifdReader = new BinaryReader2(new MemoryStream(_reader.ReadBytes(sizeInBytes))))
                {
                    ifdReader.StreamIsLittleEndian = _reader.StreamIsLittleEndian;

                    for (int i = 0; i < numFields; i++)
                    {
                        IfdTag tag = (IfdTag)ifdReader.ReadUInt16();
                        FieldType fieldType = (FieldType)ifdReader.ReadUInt16();
                        int numValues = _bigTiff ? (int)ifdReader.ReadInt64() : (int)ifdReader.ReadUInt32();

                        long pos = ifdReader.Stream.Position;

                        switch (tag)
                        {
                            case IfdTag.ImageWidth:
                                {
                                    ifd.ImageWidth = (int)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.ImageHeight:
                                {
                                    ifd.ImageHeight = (int)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.BitsPerSample:
                                {
                                    ifd.BitsPerSample = (ushort)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.PhotometricInterpretation:
                                {
                                    ifd.PhotometricInterpretation = (PhotometricInterpretation)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.Compression:
                                {
                                    ifd.Compression = (Compression)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.SamplesPerPixel:
                                {
                                    ifd.SamplesPerPixel = (ushort)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.RowsPerStrip:
                                {
                                    ifd.RowsPerStrip = (uint)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.PlanarConfiguration:
                                {
                                    ifd.PlanarConfiguration = (PlanarConfiguration)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.SampleFormat:
                                {
                                    ifd.SampleFormat = (SampleFormat)ReadFieldValue(ifdReader, fieldType);
                                }
                                break;
                            case IfdTag.StripOffsets:
                                {
                                    _reader.Stream.Position = ReadFieldValue(ifdReader, fieldType);
                                    ifd.StripOffsets = ReadFieldValues(_reader, fieldType, numValues);
                                }
                                break;
                            case IfdTag.StripByteCounts:
                                {
                                    _reader.Stream.Position = ReadFieldValue(ifdReader, fieldType);
                                    ifd.StripByteCounts = ReadFieldValues(_reader, fieldType, numValues);
                                }
                                break;
                            default:
                                {
                                    long valueOffset = ReadFieldValue(ifdReader, fieldType);
                                    ifd.Entries[tag] = new IfdEntry { Tag = tag, FieldType = fieldType, ValueOffset = valueOffset };
                                    Console.WriteLine($"Tag: {tag} Type: {fieldType} NumValues: {numValues} ValueOffset: {valueOffset}");
                                }
                                break;
                        }

                        ifdReader.Stream.Position = pos + (_bigTiff ? 8 : 4);

                    }

                    if (_bigTiff)
                        ifdOffset = ifdReader.ReadInt64();
                    else
                        ifdOffset = ifdReader.ReadUInt32();
                }
                ifdList.Add(ifd);
            }

            return ifdList.ToArray();
        }

        // private Array ReadArrayValues(BinaryReader2 reader, FieldType fieldType, int numValues)
        // {
        //     switch(fieldType)
        //     {
        //         case FieldType.Byte:
        //         case FieldType.Ascii:
        //             return reader.ReadBytes(numValues);
        //         case FieldType.SByte:
        //             return reader.ReadSBytes(numValues);
        //         case FieldType.UInt16:
        //         {
        //             var array = new ushort[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadUInt16();
        //             return array;
        //         }
        //         case FieldType.Int16:
        //         {
        //             var array = new short[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadInt16();
        //             return array;
        //         }
        //         case FieldType.UInt32:
        //         {
        //             var array = new uint[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadUInt32();
        //             return array;
        //         }
        //         case FieldType.Int32:
        //         {
        //             var array = new int[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadInt32();
        //             return array;
        //         }
        //         case FieldType.Float:
        //         {
        //             var array = new float[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadSingle();
        //             return array;
        //         }
        //         case FieldType.Double:
        //         {
        //             var array = new double[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadDouble();
        //             return array;
        //         }
        //         case FieldType.UInt64:
        //         {
        //             var array = new ulong[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadUInt64();
        //             return array;
        //         }
        //         case FieldType.Int64:
        //         {
        //             var array = new long[numValues];
        //             for(int i=0;i<numValues;i++)
        //                 array[i] = reader.ReadInt64();
        //             return array;
        //         }
        //         default:
        //             Console.WriteLine($"Unknown FieldType: {fieldType} NumValues: {numValues}");
        //         break;
        //     }
        //     return null;
        // }

        private long ReadFieldValue(BinaryReader2 reader, FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Int64:
                    return reader.ReadInt64();
                case FieldType.UInt64:
                    return reader.ReadInt64();
                case FieldType.Int32:
                    return reader.ReadInt32();
                case FieldType.UInt32:
                    return reader.ReadUInt32();
                case FieldType.Int16:
                    return reader.ReadInt16();
                case FieldType.UInt16:
                    return reader.ReadUInt16();
                case FieldType.Byte:
                    return reader.ReadByte();
                case FieldType.SByte:
                    return reader.ReadSByte();
            }
            Console.WriteLine($"Unknown field type {fieldType}");
            return 0;
        }

        private long[] ReadFieldValues(BinaryReader2 reader, FieldType fieldType, int numValues)
        {
            var array = new long[numValues];

            switch (fieldType)
            {
                case FieldType.Int64:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadInt64();
                    }
                    break;
                case FieldType.UInt64:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadInt64();
                    }
                    break;
                case FieldType.Int32:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadInt32();
                    }
                    break;
                case FieldType.UInt32:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadUInt32();
                    }
                    break;
                case FieldType.Int16:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadInt16();
                    }
                    break;
                case FieldType.UInt16:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadUInt16();
                    }
                    break;
                case FieldType.SByte:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadSByte();
                    }
                    break;
                case FieldType.Byte:
                    {
                        for (int i = 0; i < numValues; i++)
                            array[i] = _reader.ReadByte();
                    }
                    break;
                default:
                    throw new Exception($"Unknown field type {fieldType}");
            }

            return array;
        }

        public static Bitmap<T> LoadBitmap<T>(Stream stream)
        {
            using(var tiffReader = new TiffReader(stream))
            {
                return tiffReader.ReadImageFile<T>().ToBitmap();
            }
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