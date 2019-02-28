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
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Models
{
    public class DefaultTenantContextResolver<TTenant> : ITenantContextResolver<TTenant> where TTenant : class
    {
        private readonly ILogger _logger;
        private readonly IOptions<MultiTenancyOptions> _options;
        private readonly ICache _tenantCache;
        private readonly ITenantContextStore<TTenant> _tenantContextStore;

        public DefaultTenantContextResolver(ICache tenantCache, ITenantContextStore<TTenant> tenantContextStore,
            IOptions<MultiTenancyOptions> options, ILogger<ITenantContextResolver<TTenant>> logger)
        {
            _tenantCache = tenantCache;
            _tenantContextStore = tenantContextStore;
            _options = options;
            _logger = logger;
        }

        public async Task<TenantContext<TTenant>> ResolveAsync(HttpContext http)
        {
            if (string.IsNullOrWhiteSpace(_options.Value.TenantHeader) ||
                !http.Request.Headers.TryGetValue(_options.Value.TenantHeader, out var tenantKey))
            {
                tenantKey = http?.Request?.Host.Value.ToUpperInvariant();
            }

            var useCache = _options.Value.TenantLifetimeSeconds.HasValue;
            if (!useCache)
            {
                return await _tenantContextStore.FindByKeyAsync(tenantKey);
            }

            if (_tenantCache.Get(tenantKey) is TenantContext<TTenant> tenantContext)
            {
                return tenantContext;
            }

            tenantContext = await _tenantContextStore.FindByKeyAsync(tenantKey);
            if (tenantContext == null)
            {
                return null;
            }

            foreach (var identifier in tenantContext.Identifiers ?? Enumerable.Empty<string>())
            {
                _tenantCache.Set(identifier, tenantContext,
                    TimeSpan.FromSeconds(_options.Value.TenantLifetimeSeconds.Value));
            }

            return tenantContext;
        }
    }
}
