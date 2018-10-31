// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Linq;
using FastMember;
using HQ.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HQ.Common.Extensions
{
	public static class HttpContextExtensions
	{
		public static bool FeatureEnabled<TFeature, TOptions>(this HttpContext context, out TFeature feature)
			where TFeature : FeatureToggle<TOptions>
			where TOptions : class, new()
		{
			var options = context.RequestServices.GetService(typeof(IOptions<TOptions>));
			if (!(options is IOptions<TOptions> o))
			{
				feature = default(TFeature);
				return false;
			}

			var accessor = TypeAccessor.Create(o.Value.GetType());
			var featureType = accessor.GetMembers().SingleOrDefault(x => x.Type == typeof(TFeature));
			if (featureType == null)
			{
				feature = default(TFeature);
				return false;
			}

			feature = accessor[o.Value, featureType.Name] as TFeature;
			return feature != null && feature.Enabled;
		}
	}
}