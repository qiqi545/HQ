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

using System.Threading.Tasks;
using HQ.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore
{
	internal sealed class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
	{
		private readonly AuthorizationOptions _options;

		public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
		{
			_options = options.Value;
		}

		public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
		{
			AuthorizationPolicy policy;

			if (policyName != Constants.Security.Policies.NoPolicy)
			{
				policy = await base.GetPolicyAsync(policyName);
				if (policy != null)
					return policy;
			}

			policy = await base.GetPolicyAsync(Constants.Security.Policies.NoPolicy);
			if (policy != null)
				return policy;

			policy = new AuthorizationPolicyBuilder()
				.RequireAssertion(context => true)
				.Build();

			_options.AddPolicy(policyName, policy);

			return policy;
		}
	}
}