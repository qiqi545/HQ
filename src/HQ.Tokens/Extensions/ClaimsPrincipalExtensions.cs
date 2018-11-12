using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;

namespace HQ.Tokens.Extensions
{
    public static class ClaimsPrincipalExtensions
	{
		public static ExpandoObject GetClaims(this ClaimsPrincipal user)
		{
			IDictionary<string, object> result = new ExpandoObject();

			if (user?.Identity == null || !user.Identity.IsAuthenticated)
				return (ExpandoObject)result;

			var claims = user.GetClaimsList();

			foreach (var claim in claims)
				if (claim.Value.Count == 1)
					result.Add(claim.Key, claim.Value[0]);
				else
					result.Add(claim.Key, claim.Value);

			return (ExpandoObject)result;
		}

		private static IDictionary<string, IList<string>> GetClaimsList(this ClaimsPrincipal user)
		{
			IDictionary<string, IList<string>> claims = new Dictionary<string, IList<string>>();
			foreach (var claim in user?.Claims ?? Enumerable.Empty<Claim>())
			{
				if (!claims.TryGetValue(claim.Type, out var list))
					claims.Add(claim.Type, list = new List<string>());
				list.Add(claim.Value);
			}
			return claims;
		}
	}
}
