using System.Collections.Generic;

namespace TiffExpress.Tiff
{
    public class ImageFileDirectory
    {
        public readonly Dictionary<IfdTag, IfdEntry> Entries = new Dictionary<IfdTag, IfdEntry>();

        public int ImageWidth;
        public int ImageHeight;
        public ushort BitsPerSample;
        public PhotometricInterpretation PhotometricInterpretation = PhotometricInterpretation.BlackIsZero;
        public Compression Compression = Compression.NoCompression;
        public ushort SamplesPerPixel = 1;
        public uint RowsPerStrip;
        public PlanarConfiguration PlanarConfiguration = PlanarConfiguration.Chunky;
        public SampleFormat SampleFormat = SampleFormat.Unsigned; // 1 = unsigned integer data, 2 = two’s complement signed integer data, 3 = IEEE floating point data [IEEE]

        public long[] StripOffsets;
        public long[] StripByteCounts;
    }

}