// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using System.Threading.Tasks;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace HQ.Tokens.AspNetCore.Mvc.Filters
{
	public class RequireClaimActionFilter : IAsyncActionFilter
	{
		private readonly string _type;
		private readonly string _value;
		private readonly IOptions<SecurityOptions> _options;

		public RequireClaimActionFilter(string type, string value, IOptions<SecurityOptions> options)
		{
			_type = type;
			_value = value;
			_options = options;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if ((_options.Value.SuperUser?.Enabled ?? false) && context.HttpContext.User.HasClaim(ClaimTypes.Role, ClaimValues.SuperUser))
				await next();
			else if (!context.HttpContext.User.HasClaim(_type, _value))
				context.Result = new ForbidResult();
			else
				await next();
		}
	}
}
