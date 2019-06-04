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

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;

namespace HQ.Platform.Security.Internal.Extensions
{
    internal static class ClaimsPrincipalExtensions
    {
        public static IDictionary<string, object> Claims(this ClaimsPrincipal user)
        {
            IDictionary<string, object> result = new ExpandoObject();

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return (ExpandoObject) result;
            }

            var claims = user.ClaimsList();

            foreach (var claim in claims)
            {
                if (claim.Value.Count == 1)
                {
                    result.Add(claim.Key, claim.Value[0]);
                }
                else
                {
                    result.Add(claim.Key, claim.Value);
                }
            }

            return (ExpandoObject) result;
        }

        private static IDictionary<string, IList<string>> ClaimsList(this ClaimsPrincipal user)
        {
            IDictionary<string, IList<string>> claims = new Dictionary<string, IList<string>>();
            foreach (var claim in user?.Claims ?? Enumerable.Empty<Claim>())
            {
                if (!claims.TryGetValue(claim.Type, out var list))
                {
                    claims.Add(claim.Type, list = new List<string>());
                }

                list.Add(claim.Value);
            }

            return claims;
        }
    }
}
