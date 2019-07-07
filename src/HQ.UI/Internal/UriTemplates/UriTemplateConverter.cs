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
using System.ComponentModel;
using System.Globalization;

namespace HQ.UI.Internal.UriTemplates
{
	/// <inheritdoc />
	/// <summary>
	///     Converts to <see cref="T:HQ.UI.Web.Internal.UriTemplates.UriTemplate" /> instances from other representations.
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