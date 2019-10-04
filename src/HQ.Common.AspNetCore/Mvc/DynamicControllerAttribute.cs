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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Common.AspNetCore.Mvc
{
	public sealed class DynamicControllerAttribute : Attribute, IControllerModelConvention, IDynamicAttribute
	{
		private readonly Type _featureProviderType;
		private readonly string[] _segments;

		public DynamicControllerAttribute(Type featureProviderType, params string[] segments)
		{
			_featureProviderType = featureProviderType;
			_segments = segments;
		}

		public bool Enabled => Resolve(ServiceProvider);

		public void Apply(ControllerModel controller)
		{
			if (!controller.ControllerType.IsGenericType)
				return;

			var types = controller.ControllerType.GetGenericArguments();
			if (types.Length == 0)
				return;

			controller.ControllerName = Pooling.StringBuilderPool.Scoped(sb =>
			{
				var controllerName = controller.ControllerType.Name.Replace($"Controller`{types.Length}", string.Empty);
				sb.Append(controllerName);
				foreach (var type in types)
					sb.Append($"_{type.Name}");
			});
		}

		public IServiceProvider ServiceProvider { get; set; }

		public bool Resolve(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return false; // don't attempt to resolve if opted-out/disabled

			var optionsType = typeof(IOptionsMonitor<>).MakeGenericType(_featureProviderType);
			var options = serviceProvider.GetRequiredService(optionsType);

			var currentValueProperty = optionsType.GetProperty(nameof(IOptionsMonitor<object>.CurrentValue));
			var currentValue = currentValueProperty?.GetValue(options);

			var featureProperty = _featureProviderType.GetProperty(nameof(IFeatureToggle.Enabled));
			if (featureProperty == null)
			{
				var reads = ReadAccessor.Create(_featureProviderType, out var members);
				currentValue = WalkFeatureRecursive(0, currentValue, reads, members);
				featureProperty = currentValue.GetType().GetProperty(nameof(IFeatureToggle.Enabled));
			}

			var enabled = featureProperty?.GetValue(currentValue);
			return enabled is bool toggle && toggle;
		}

		private object WalkFeatureRecursive(int segmentIndex, object currentValue, ITypeReadAccessor reads,
			AccessorMembers members)
		{
			foreach (var member in members)
			{
				var key = member.Name;

				if (_segments.Length < segmentIndex + 1 ||
				    _segments[segmentIndex] != key ||
				    !member.CanRead ||
				    !reads.TryGetValue(currentValue, key, out var segment))
					continue;

				if (segment is IFeatureToggle featureToggle)
				{
					currentValue = featureToggle;
					return currentValue;
				}

				currentValue = segment;
				segmentIndex++;
				var segmentReads = ReadAccessor.Create(segment, out var segmentMembers);
				WalkFeatureRecursive(segmentIndex, segment, segmentReads, segmentMembers);
			}

			return currentValue;
		}
	}
}