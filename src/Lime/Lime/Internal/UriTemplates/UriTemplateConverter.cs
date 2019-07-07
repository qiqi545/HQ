// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Lime.Internal.UriTemplates
{
	/// <inheritdoc />
	/// <summary>
	///     Converts to <see cref="T:Lime.UI.Web.Internal.UriTemplates.UriTemplate" /> instances from other representations.
	/// </summary>
	public sealed class UriTemplateConverter : TypeConverter
	{
		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		/// <inheritdoc />
		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value)
		{
			if (value == null) return null;

			var template = value as string;
			if (template != null)
			{
				if (template.Length == 0)
					// For TypeConverter purposes, an empty string is "no value."
					return null;

				return new UriTemplate(template);
			}

			throw (NotSupportedException) GetConvertFromException(value);
		}
	}
}