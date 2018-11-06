// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Configuration
{
	public class UserOptionsExtended : UserOptions
	{
		public UserOptionsExtended() { }

		public UserOptionsExtended(UserOptions inner)
		{
			AllowedUserNameCharacters = inner.AllowedUserNameCharacters;
			RequireUniqueEmail = inner.RequireUniqueEmail;
		}

		public bool RequireUniqueUsername { get; set; } = true;
		public bool RequireUniquePhoneNumber { get; set; } = false;

		public string AllowedPhoneNumberCharacters { get; set; } = "()123456789-+#";

		public bool RequireEmail { get; set; } = true;
		public bool RequirePhoneNumber { get; set; } = false;
		public bool RequireUsername { get; set; } = true;
		public bool RequireEmailPhoneNumberOrUsername { get; set; } = false;
	}
}