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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HQ.Common
{
    public static class EnumerableExtensions
    {
        public static SelfEnumerable<T> Enumerate<T>(this List<T> inner)
        {
            return new SelfEnumerable<T>(inner);
        }

        public static FuncEnumerable<T, TResult> Enumerate<T, TResult>(this List<T> inner, Func<T, TResult> func)
        {
            return new FuncEnumerable<T, TResult>(inner, func);
        }

        public static PredicateEnumerable<T> Enumerate<T>(this List<T> inner, Predicate<T> predicate)
        {
            return new PredicateEnumerable<T>(inner, predicate);
        }

        public static List<T> MaybeList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as List<T> ?? enumerable.ToList();
        }

        public static bool Contains(this StringValues stringValues, string input, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach (var value in stringValues)
            {
                if (value.Equals(input, comparison))
                    return true;
            }
            return false;
        }

        public static bool AnyStartWith(this StringValues stringValues, string input, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach (var value in stringValues)
            {
                if (value.StartsWith(input, comparison))
                    return true;
            }
            return false;
        }
    }
}
