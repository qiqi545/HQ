// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Globalization;

namespace HQ.Cohort
{
	internal sealed class Resources
	{
		internal static string FormatInvalidPhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidPhoneNumber, phoneNumber);
		}

		/// <summary></summary>
		internal static string FormatDuplicatePhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicatePhoneNumber, phoneNumber);
		}
	}
}