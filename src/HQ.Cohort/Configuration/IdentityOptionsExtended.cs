// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Configuration
{
	public class IdentityOptionsExtended : IdentityOptions
	{
		public IdentityOptionsExtended() { }

		public IdentityOptionsExtended(IdentityOptions inner)
		{
			Password = inner.Password;
			Lockout = inner.Lockout;
			Stores = inner.Stores;
			Tokens = inner.Tokens;
			SignIn = inner.SignIn;
			ClaimsIdentity = inner.ClaimsIdentity;

			User = new UserOptionsExtended(inner.User);
		}

		public new UserOptionsExtended User { get; set; } = new UserOptionsExtended();
	}
}