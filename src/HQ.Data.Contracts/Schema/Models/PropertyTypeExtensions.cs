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

namespace HQ.Data.Contracts.Schema.Models
{
	public static class PropertyTypeExtensions
	{
		public static bool IsString(this PropertyType propertyType)
		{
			switch (propertyType)
			{
				case PropertyType.String:
				case PropertyType.Money:
				case PropertyType.Email:
				case PropertyType.Password:
				case PropertyType.CreditCard:
				case PropertyType.Phone:
					return true;
				default:
					return false;
			}
		}

		public static string ToLayoutField(this PropertyType propertyType)
		{
			switch (propertyType)
			{
				case PropertyType.String:
				case PropertyType.Boolean:
				case PropertyType.Byte:
				case PropertyType.Int32:
				case PropertyType.Int16:
				case PropertyType.Int64:
				case PropertyType.Float:
				case PropertyType.Double:
				case PropertyType.Decimal:
				case PropertyType.TimeSpan:
					return Enum.GetName(typeof(PropertyType), propertyType) + "Field";

				case PropertyType.Date:
				case PropertyType.DateTime:
					return "DateTimeOffsetField";

				case PropertyType.Money:
				case PropertyType.Email:
				case PropertyType.Password:
				case PropertyType.CreditCard:
				case PropertyType.Phone:
					return "StringField";

				case PropertyType.Enum:
					return "Int64Field";

				case PropertyType.Object:
				case PropertyType.View:
					throw new NotSupportedException();

				default:
					throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
			}
		}
	}
}