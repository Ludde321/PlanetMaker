
namespace TiffExpress.Tiff
{
    public struct Rational
    {
        public Rational(uint numerator, uint denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
		}
        public uint Numerator;
        public uint Denominator;

        public override string ToString()
        {
            return $"{Numerator}:{Denominator}";
        }
    }

    public struct SRational
    {
        public SRational(int numerator, int denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
		}
        public int Numerator;
        public int Denominator;

        public override string ToString()
        {
            return $"{Numerator}:{Denominator}";
        }
    }

}