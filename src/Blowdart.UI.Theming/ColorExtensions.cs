// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Blowdart.UI.Theming
{
    internal static class ColorExtensions
    {
        public static Color ToColor(this string hex)
        {
            var match = Regex.Match(hex, "^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);
            if (match.Success)
            {
                return Color.FromArgb(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber));
            }
            Trace.TraceError($"Could not parse color {hex}.");
            return Color.Red;
        }
    }
}