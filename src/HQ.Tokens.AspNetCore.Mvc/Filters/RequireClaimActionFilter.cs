#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace HQ.Tokens.AspNetCore.Mvc.Filters
{
    public class RequireClaimActionFilter : IAsyncActionFilter
    {
        private readonly IOptions<SecurityOptions> _options;
        private readonly string _type;
        private readonly string[] _oneOf;

        public RequireClaimActionFilter(string type, string[] oneOf, IOptions<SecurityOptions> options)
        {
            _type = type;
            _oneOf = oneOf;
            _options = options;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (SupportsSuperUser && IsSuperUser(context))
                await next();
            else if (!context.HttpContext.User.HasClaim(c => _type == c.Type && _oneOf.Contains(c.Value, StringComparer.OrdinalIgnoreCase)))
                context.Result = new ForbidResult();
            else
                await next();
        }

        private static bool IsSuperUser(ActionContext context)
        {
            return context.HttpContext.User.HasClaim(Constants.ClaimTypes.Role, Constants.ClaimValues.SuperUser);
        }

        private bool SupportsSuperUser => _options.Value.SuperUser?.Enabled ?? false;
    }
}
