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

using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore.Configuration
{
    internal class ConfigureWebServer : IConfigureOptions<KestrelServerOptions>
    {
        private readonly IOptions<SecurityOptions> _options;

        public ConfigureWebServer(IOptions<SecurityOptions> options)
        {
            _options = options;
        }

        public void Configure(KestrelServerOptions options)
        {
            options.AddServerHeader = false;

            // See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2
            options.Limits.MaxConcurrentConnections = _options.Value.WebServer.MaxConcurrentConnections;
            options.Limits.MaxConcurrentUpgradedConnections = _options.Value.WebServer.MaxConcurrentUpgradedConnections;
            options.Limits.MaxRequestBodySize = _options.Value.WebServer.MaxRequestBodySize;
        }
    }
}
