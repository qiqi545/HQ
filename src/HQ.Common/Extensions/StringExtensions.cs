using System.Globalization;

namespace HQ.Common.Extensions
{
    internal static class StringExtensions
    {
        public static string ToTitleCase(this string value)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);
        }
    }
}
