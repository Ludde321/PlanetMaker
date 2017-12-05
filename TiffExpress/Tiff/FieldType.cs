namespace TiffExpress.Tiff
{
    public enum FieldType
    {
        Byte = 1,
        Ascii = 2,
        UInt16 = 3,
        UInt32 = 4,
        Rational = 5, // Two LONGs: the first represents the numerator of a fraction; the second, the denominator
        SByte = 6,
        Undefined8 = 7,
        Int16 = 8,
        Int32 = 9,
        SRational = 10,
        Float = 11,
        Double = 12,

        UInt64 = 16,
        Int64 = 17,
        IFD8 = 18, // being a new unsigned 8byte IFD offset.
    }

}