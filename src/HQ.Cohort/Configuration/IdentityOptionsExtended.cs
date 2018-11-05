// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Configuration
{
	public class IdentityOptionsExtended : IdentityOptions
	{
		public IdentityOptionsExtended()
		{
		}

		public IdentityOptionsExtended(IdentityOptions inner)
		{
			User = new UserOptionsExtended(inner.User);
		}

		public new UserOptionsExtended User { get; set; } = new UserOptionsExtended();
	}
}