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

using System.Linq;
using HQ.Common.Configuration;
using HQ.Common.FastMember;
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
                feature = default;
                return false;
            }

            var accessor = TypeAccessor.Create(o.Value.GetType());
            var featureType = accessor.GetMembers().SingleOrDefault(x => x.Type == typeof(TFeature));
            if (featureType == null)
            {
                feature = default;
                return false;
            }

            feature = accessor[o.Value, featureType.Name] as TFeature;
            return feature != null && feature.Enabled;
        }
    }
}
