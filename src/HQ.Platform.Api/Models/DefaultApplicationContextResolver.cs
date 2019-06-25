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
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Models
{
    public class DefaultApplicationContextResolver<TApplication> : IApplicationContextResolver<TApplication> where TApplication : class
    {
        private readonly ILogger _logger;
        private readonly IOptions<MultiTenancyOptions> _options;
        private readonly ICache _applicationCache;
        private readonly IApplicationContextStore<TApplication> _applicationContextStore;

        public DefaultApplicationContextResolver(ICache applicationCache, IApplicationContextStore<TApplication> applicationContextStore,
            IOptions<MultiTenancyOptions> options, ILogger<IApplicationContextResolver<TApplication>> logger)
        {
            _applicationCache = applicationCache;
            _applicationContextStore = applicationContextStore;
            _options = options;
            _logger = logger;
        }

        public async Task<ApplicationContext<TApplication>> ResolveAsync(HttpContext http)
        {
            if (string.IsNullOrWhiteSpace(_options.Value.ApplicationHeader) ||
                !http.Request.Headers.TryGetValue(_options.Value.ApplicationHeader, out var tenantKey))
            {
                tenantKey = http?.Request?.Host.Value.ToUpperInvariant();
            }

            var useCache = _options.Value.ApplicationLifetimeSeconds.HasValue;
            if (!useCache)
            {
                return await _applicationContextStore.FindByKeyAsync(tenantKey);
            }

            if (_applicationCache.Get($"{Constants.ContextKeys.Application}:{tenantKey}") is ApplicationContext<TApplication> applicationContext)
            {
                return applicationContext;
            }

            applicationContext = await _applicationContextStore.FindByKeyAsync(tenantKey);
            if (applicationContext == null)
            {
                return null;
            }

            foreach (var identifier in applicationContext.Identifiers ?? Enumerable.Empty<string>())
            {
                _applicationCache.Set($"{Constants.ContextKeys.Application}:{identifier}", applicationContext,
                    TimeSpan.FromSeconds(_options.Value.ApplicationLifetimeSeconds.Value));
            }

            return applicationContext;
        }
    }
}
