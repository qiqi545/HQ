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

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Configuration
{
    internal class PlatformApiRouteOptions : IConfigureOptions<RouteOptions>
    {
        private readonly IOptions<PlatformApiOptions> _options;

        public PlatformApiRouteOptions(IOptions<PlatformApiOptions> options)
        {
            _options = options;
        }

        public void Configure(RouteOptions options)
        {
            options.AppendTrailingSlash = _options.Value.CanonicalRoutes.AppendTrailingSlash;
            options.LowercaseUrls = _options.Value.CanonicalRoutes.LowercaseUrls;
            options.LowercaseQueryStrings = _options.Value.CanonicalRoutes.LowercaseQueryStrings;
        }
    }
}
