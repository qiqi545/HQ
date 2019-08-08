﻿using System.Drawing;
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
		
		public static string ToRgbHexString(this Color color)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"; // NOTE: RGBA
		}
	}
}
