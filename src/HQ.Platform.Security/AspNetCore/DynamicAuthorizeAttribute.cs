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
using HQ.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class DynamicAuthorizeAttribute : Attribute, IAuthorizeData
	{
		private readonly Type _policyProviderType;
		private readonly string[] _segments;

		public DynamicAuthorizeAttribute(Type policyProviderType, params string[] segments)
		{
			_policyProviderType = policyProviderType;
			_segments = segments;
		}

		public IServiceProvider ServiceProvider { get; set; }

		public string Policy
		{
			get => ResolvePolicyName();
			set => throw new NotSupportedException("Dynamic authorization does not support directly setting policy.");
		}

		private string ResolvePolicyName()
		{
			if (ServiceProvider == null)
				return null; // don't attempt to resolve if opted-out/disabled

			var optionsType = typeof(IOptionsMonitor<>).MakeGenericType(_policyProviderType);
			var options = ServiceProvider.GetRequiredService(optionsType);

			var currentValueProperty = optionsType.GetProperty(nameof(IOptionsMonitor<object>.CurrentValue));
			var currentValue = currentValueProperty?.GetValue(options);

			var policyProperty = _policyProviderType.GetProperty(nameof(IProtectedFeature.Policy));
			var policy = policyProperty?.GetValue(currentValue);
			return policy as string ?? Constants.Security.Policies.NoPolicy;
		}

		public string Roles { get; set; }
		public string AuthenticationSchemes { get; set; }
	}
}