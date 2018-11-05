// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Security
{
	internal sealed class NoLookupProtector : ILookupProtector
	{
		public string Protect(string keyId, string data)
		{
			return data;
		}

		public string Unprotect(string keyId, string data)
		{
			return data;
		}
	}
}