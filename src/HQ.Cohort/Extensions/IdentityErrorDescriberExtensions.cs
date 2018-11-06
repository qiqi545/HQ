// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Extensions
{
	public static class IdentityErrorDescriberExtensions
	{
		public static IdentityError InvalidPhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(InvalidPhoneNumber),
				Description = Resources.FormatInvalidPhoneNumber(phoneNumber)
			};
		}

		public static IdentityError DuplicatePhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(DuplicatePhoneNumber),
				Description = Resources.FormatDuplicatePhoneNumber(phoneNumber)
			};
		}

		public static IdentityError MustHaveEmailPhoneOrUsername(this IdentityErrorDescriber describer)
		{
			return new IdentityError
			{
				Code = nameof(MustHaveEmailPhoneOrUsername),
				Description = "Must have an email address, phone number, or email address to register." ?? ErrorStrings.MustHaveEmailPhoneOrUsername
			};
		}
	}
}