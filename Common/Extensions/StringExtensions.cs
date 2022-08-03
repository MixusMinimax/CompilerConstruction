namespace Common.Extensions;

public static class StringExtensions
{
    public static IFormattable Center<T>(this T self, int width)
    {
        return new CenterHelper<T>(self, width);
    }

    private class CenterHelper<T> : IFormattable
    {
        private readonly T _value;
        private readonly int _width;

        internal CenterHelper(T value, int width)
        {
            _value = value;
            _width = width;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            string basicString;
            if (_value is IFormattable formattable)
                basicString = formattable.ToString(format, formatProvider) ?? "";
            else if (_value != null)
                basicString = _value.ToString() ?? "";
            else
                basicString = "";

            var numberOfMissingSpaces = _width - basicString.Length;
            return numberOfMissingSpaces <= 0
                ? basicString
                : basicString.PadLeft(_width - numberOfMissingSpaces / 2).PadRight(_width);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }
    }
}
