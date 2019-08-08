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
using System.Drawing;
using HQ.Common;

namespace HQ.Data.Contracts.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ColorAttribute : Attribute
	{
		public Color Color { get; set; }

		private static readonly Color UnknownColor = Color.CornflowerBlue;
		
		public ColorAttribute(string nameOrHexColor)
		{
			var color = Color.FromName(nameOrHexColor);
			if (!color.IsNamedColor)
				color = ColorExtensions.FromHex(nameOrHexColor);
			if (color == Color.Empty)
				color = UnknownColor;

			Color = color;
		}

		public ColorAttribute(uint argb)
		{
			var color = Color.FromArgb((int) argb);
			if(color.IsEmpty)
				color = UnknownColor;

			Color = color;
		}

		public ColorAttribute(int argb)
		{
			var color = Color.FromArgb(argb);
			if (color.IsEmpty)
				color = UnknownColor;

			Color = color;
		}

		public ColorAttribute(int r, int g, int b)
		{
			var color = Color.FromArgb(r, g, b);
			if (Color.IsEmpty)
				color = UnknownColor;

			Color = color;
		}

		public ColorAttribute(int a, int r, int g, int b)
		{
			var color = Color.FromArgb(a, r, g, b);
			if (color.IsEmpty)
				color = UnknownColor;

			Color = color;
		}
	}
}