// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using HQ.Tokens.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Tokens.AspNetCore.Mvc.Attributes
{
	public class RequireClaimAttribute : TypeFilterAttribute
	{
		public RequireClaimAttribute(string type, params string[] values) : base(typeof(RequireClaimActionFilter))
		{
			Arguments = new object[] {type, values};
		}
	}
}
