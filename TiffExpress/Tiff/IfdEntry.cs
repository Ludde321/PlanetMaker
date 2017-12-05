namespace TiffExpress.Tiff
{
    public class IfdEntry
    {
        public IfdTag Tag;
        public FieldType FieldType;
        public long NumValues;
        public long ValueOffset;

        public IfdEntry()
        {
        }
    }

}