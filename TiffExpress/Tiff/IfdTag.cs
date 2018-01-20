namespace TiffExpress.Tiff
{
    public enum IfdTag
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

        // Unused
        ResolutionUnit = 296,
        XResolution = 282,
        YResolution = 283,
        Artist = 315,
        CellLength = 265,
        CellWidth = 264,
        ColorMap = 320,
        Copyright = 33432,
        DateTime = 306,
        ExtraSamples = 338,
        FillOrder = 266,
        FreeByteCounts = 289,
        FreeOffsets = 288,
        GrayResponseCurve = 291,
        GrayResponseUnit = 290,
        HostComputer = 316,
        ImageDescription = 270,
        Make = 271,
        MaxSampleValue = 281,
        MinSampleValue = 280,
        Model = 272,
        NewSubfileType = 254,
        Orientation = 274,
        Software = 305,
        SubfileType = 255,
        Threshholding = 263,
        DocumentName = 269,

        // Adobe
        XMP = 700,
        IPTC = 33723,
        Photoshop = 34377,
        Exif_IFD = 34665,
        ICC_Profile = 34675,

        // GeoTiff
        ModelPixelScaleTag     = 33550,
        ModelTiepointTag       = 33922,
        GeoKeyDirectoryTag     = 34735,

        GeoDoubleParamsTag      = 34736,
        GeoAsciiParamsTag       = 34737,
        GDAL_NODATA             = 42113,
    }

}