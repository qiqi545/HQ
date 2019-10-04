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

using System.Drawing;
using System.Globalization;

namespace HQ.Common
{
	public static class ColorExtensions
	{
		public static Color FromHex(string hex)
		{
			if (string.IsNullOrWhiteSpace(hex))
				return Color.Empty;

			hex = hex.Replace("0x", string.Empty).TrimStart('#');
			if (hex.Length < 8)
				hex = $"FF{hex}";

			if (hex.Length != 8)
				return Color.Empty;

			return int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb)
				? Color.FromArgb(argb)
				: Color.Empty;
		}

		public static string ToRgbaHexString(this Color color)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
		}
	}
}