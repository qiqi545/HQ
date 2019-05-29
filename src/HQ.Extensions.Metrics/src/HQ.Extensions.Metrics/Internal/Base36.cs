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
using HQ.Common;
using TypeKitchen;

namespace HQ.Extensions.Metrics.Internal
{
    internal static class Base36
    {
        private const int Base = 36;

        private static readonly char[] Chars =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
            'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static string ToBase36(this ulong input)
        {
            return Pooling.StringBuilderPool.Scoped(sb =>
            {
                while (input != 0)
                {
                    sb.Append(Chars[input % Base]);
                    input /= Base;
                }
            });
        }

        public static ulong FromBase36(this string input)
        {
            var result = 0UL;
            var pos = 0;
            for (var i = input.Length - 1; i >= 0; i--)
            {
                result += input[i] * (ulong) Math.Pow(Base, pos);
                pos++;
            }

            return result;
        }
    }
}
