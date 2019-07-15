// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Data.DocumentDb
{
    internal static class StringExtensions
    {
        public static string Truncate(this string value, int length)
        {
            return string.IsNullOrEmpty(value) ? value : value.Length <= length ? value : value.Substring(0, length);
        }
    }
}
