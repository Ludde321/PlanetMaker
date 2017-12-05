using System.Collections.Generic;

namespace TiffExpress.Tiff
{
    public class ImageFileDirectory
    {
        public readonly Dictionary<IfdTag, IfdEntry> Entries = new Dictionary<IfdTag, IfdEntry>();

        public int ImageWidth;
        public int ImageHeight;
        public ushort BitsPerSample;
        public PhotometricInterpretation PhotometricInterpretation;
        public Compression Compression;
        public ushort SamplesPerPixel;
        public uint RowsPerStrip;
        public ushort PlanarConfiguration; // 1 = Chunky format, 2 = Planar format
        public ushort SampleFormat; // 1 = unsigned integer data, 2 = two’s complement signed integer data, 3 = IEEE floating point data [IEEE]

        public long[] StripOffsets;
        public long[] StripByteCounts;
    }

}