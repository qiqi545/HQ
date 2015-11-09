using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace tuxedo
{
    internal static class StringExtensions
    {
        private const string DefaultSeparator = ", ";

        public static string Concat(this IEnumerable<object> list, string separator = DefaultSeparator)
        {
            return string.Join(separator, list);
        }

        public static string Concat(this IEnumerable list, string separator = DefaultSeparator)
        {
            return Concat(list.Cast<object>(), separator);
        }

        public static string Qualify(this string value, IDialect dialect)
        {
            return string.IsNullOrWhiteSpace(value) ? value : string.Concat(dialect.StartIdentifier, value, dialect.EndIdentifier).Trim();
        }

        public static string ConcatQualified(this IEnumerable<string> list, IDialect dialect, string separator = DefaultSeparator)
        {
            var qualified = list.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Qualify(dialect));
            return string.Join(separator, qualified);
        }
    }
}