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

using System.Globalization;

namespace HQ.Platform.Identity
{
	internal sealed class Resources
	{
		/// <summary></summary>
		internal static string FormatInvalidPhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidPhoneNumber, phoneNumber);
		}

		/// <summary></summary>
		internal static string FormatDuplicatePhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicatePhoneNumber, phoneNumber);
		}

		/// <summary></summary>
		internal static string FormatInvalidTenantName(string tenantName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidTenantName, tenantName);
		}

		/// <summary></summary>
		internal static string FormatDuplicateTenantName(string tenantName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicateTenantName, tenantName);
		}

		/// <summary></summary>
		internal static string FormatInvalidApplicationName(string ApplicationName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidApplicationName, ApplicationName);
		}

		/// <summary></summary>
		internal static string FormatDuplicateApplicationName(string ApplicationName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicateApplicationName, ApplicationName);
		}
	}
}