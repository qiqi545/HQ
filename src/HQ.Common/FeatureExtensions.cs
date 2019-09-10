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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Common
{
    public static class FeatureExtensions
    {
	    public static bool FeatureEnabled<TFeature>(this HttpContext context, out TFeature feature)
		    where TFeature : class, IFeatureToggle
	    {
		    return context.RequestServices.FeatureEnabled(out feature);
	    }

		public static bool FeatureEnabled<TFeature, TOptions>(this HttpContext context, out TFeature feature)
            where TFeature : class, IFeatureToggle
            where TOptions : class, new()
        {
            return context.RequestServices.FeatureEnabled<TFeature, TOptions>(out feature);
        }

        public static bool FeatureEnabled<TFeature, TOptions>(this IApplicationBuilder appBuilder, out TFeature feature)
            where TFeature : class, IFeatureToggle
            where TOptions : class, new()
        {
            return appBuilder.ApplicationServices.FeatureEnabled<TFeature, TOptions>(out feature);
        }

        public static bool FeatureEnabled<TFeature>(this IServiceProvider serviceProvider, out TFeature feature)
	        where TFeature : class, IFeatureToggle
        {
	        var options = serviceProvider.GetService(typeof(IOptionsMonitor<TFeature>));
	        if (!(options is IOptionsMonitor<TFeature> o))
	        {
		        feature = default;
		        return false;
	        }
	        feature =  o.CurrentValue;
	        return feature != null && feature.Enabled;
        }

		public static bool FeatureEnabled<TFeature, TOptions>(this IServiceProvider serviceProvider, out TFeature feature)
            where TFeature : class, IFeatureToggle
            where TOptions : class, new()
        {
            var options = serviceProvider.GetService(typeof(IOptionsMonitor<TOptions>));
            if (!(options is IOptionsMonitor<TOptions> o))
            {
                feature = default;
                return false;
            }

            var type = o.CurrentValue.GetType();
            var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
            var featureType = members.SingleOrDefault(x => x.Type == typeof(TFeature));
            if (featureType == null)
            {
                feature = default;
                return false;
            }

            var accessor = ReadAccessor.Create(type);
            feature = accessor[o.CurrentValue, featureType.Name] as TFeature;
            return feature != null && feature.Enabled;
        }
    }
}
