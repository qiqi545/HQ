#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HQ.Lingo.Dialects;

namespace HQ.Lingo
{
    internal static class StringExtensions
    {
        public const string DefaultSeparator = ", ";

        public static string Concat(this IEnumerable<object> list, string separator = DefaultSeparator)
        {
            return string.Join(separator, list);
        }

        public static string Concat(this IEnumerable list, string separator = DefaultSeparator)
        {
            return Concat(list.Cast<object>(), separator);
        }

        public static string Qualify(this string value, ISqlDialect dialect)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : string.Concat(dialect.StartIdentifier, value, dialect.EndIdentifier).Trim();
        }

        public static string ConcatQualified(this IEnumerable<string> list, ISqlDialect dialect,
            string separator = DefaultSeparator)
        {
            var qualified = list.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Qualify(dialect));
            return string.Join(separator, qualified);
        }
    }
}
