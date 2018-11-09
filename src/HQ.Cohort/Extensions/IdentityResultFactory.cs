// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using HQ.Common.FastMember;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Extensions
{
	internal static class IdentityResultFactory
	{
		/// <summary>
		///     Provides an <see cref="IdentityResult" /> in a failed state, with errors.
		///     This prevents an allocation by forcing the error collection to an array.
		/// </summary>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static IdentityResult Failed(ICollection<IdentityError> errors)
		{
			var accessor = TypeAccessor.Create(typeof(IdentityResult), true);
			var result = new IdentityResult();
			var list = accessor[result, "_errors"] as List<IdentityError>;
			list?.AddRange(errors);
			accessor[result, "Succeeded"] = false;
			return result;
		}
	}
}