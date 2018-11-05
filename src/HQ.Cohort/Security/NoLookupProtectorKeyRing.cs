// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Security
{
	internal sealed class NoLookupProtectorKeyRing : ILookupProtectorKeyRing
	{
		private const string ProtectorContextKey = "None";

		public IEnumerable<string> GetAllKeyIds()
		{
			return new[] {ProtectorContextKey};
		}

		public string CurrentKeyId => ProtectorContextKey;

		public string this[string keyId] => ProtectorContextKey;
	}
}