using System;
using System.Collections.Generic;
using System.Text;

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

        public ushort FillOrder = 1;

        public long[] StripOffsets;
        public long[] StripByteCounts;

        // --
        public string Artist;
        public string Copyright;
        public DateTime DateTime;
        public string Software;
        public string DocumentName;
        public ushort ExtraSamples;
        public string HostComputer;
        public string ImageDescription;
        public string Make;
        public string Model;
        public ushort Orientation;

        // private T GetValue<T>(IfdTag tag, T defaultValue = default(T))
        // {
        //     if (Entries.TryGetValue(tag, out var entry))
        //         if (entry.Values.Length == 1)
        //             return (T)Convert.ChangeType(entry.Values.GetValue(0), typeof(T));
        //     return defaultValue;
        // }

        // private void SetValue<T>(IfdTag tag, FieldType fieldType, T value)
        // {
        //     Entries[tag] = new IfdEntry { Tag = tag, FieldType = fieldType, Values = new T[] { value } };
        // }

        // private T[] GetValues<T>(IfdTag tag)
        // {
        //     if (Entries.TryGetValue(tag, out var entry))
        //     {
        //         var array = new T[entry.Values.Length];
        //         for(int i=0;i<entry.Values.Length;i++)
        //             array[i] = (T)Convert.ChangeType(entry.Values.GetValue(i), typeof(T));
        //         return array;
        //     }
        //     return null;
        // }

    }

}