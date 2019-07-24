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
using TypeKitchen;

namespace HQ.Data.Contracts.Attributes
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

			object policy = null;
			var segmentIndex = 0;
			var policyProperty = _policyProviderType.GetProperty(nameof(IProtectedFeature.Policy));
			if (policyProperty == null)
			{
				policyProperty = _policyProviderType.GetProperty("Policies");
				if (policyProperty != null)
				{
					var reads = ReadAccessor.Create(_policyProviderType, out var members);
					currentValue = WalkPoliciesRecursive(segmentIndex, currentValue, reads, members, ref policy);
				}
			}

			policy = policy ?? policyProperty?.GetValue(currentValue);
			return policy as string ?? Constants.Security.Policies.NoPolicy;
		}

		private object WalkPoliciesRecursive(int segmentIndex, object currentValue, ITypeReadAccessor reads, AccessorMembers members, ref object policy)
		{
			foreach (var member in members)
			{
				var key = member.Name;

				if (_segments.Length >= segmentIndex + 1 &&
				    _segments[segmentIndex] == key &&
				    member.CanRead &&
				    reads.TryGetValue(currentValue, key, out var segment))
				{
					if (segment is IProtectedFeature feature)
						policy = feature.Policy;

					currentValue = segment;
					segmentIndex++;
					var segmentReads = ReadAccessor.Create(segment, out var segmentMembers);
					WalkPoliciesRecursive(segmentIndex, segment, segmentReads, segmentMembers, ref policy);
				}
			}

			return currentValue;
		}

		public string Roles { get; set; }
		public string AuthenticationSchemes { get; set; }
	}
}