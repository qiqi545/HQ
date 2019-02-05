using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HQ.Platform.Api.Configuration;
using HQ.Extensions.Caching;

namespace HQ.Platform.Api.Models
{
    public class DefaultTenantContextResolver<TTenant> : ITenantContextResolver<TTenant> where TTenant : class
    {
        private readonly ICache _tenantCache;
        private readonly ITenantContextStore<TTenant> _tenantContextStore;
        private readonly IOptions<MultiTenancyOptions> _options;
        private readonly ILogger _logger;

        public DefaultTenantContextResolver(ICache tenantCache, ITenantContextStore<TTenant> tenantContextStore, IOptions<MultiTenancyOptions> options, ILogger<ITenantContextResolver<TTenant>> logger)
        {
            _tenantCache = tenantCache;
            _tenantContextStore = tenantContextStore;
            _options = options;
            _logger = logger;
        }

        public async Task<TenantContext<TTenant>> ResolveAsync(HttpContext http)
        {
            if (string.IsNullOrWhiteSpace(_options.Value.TenantHeader) || !http.Request.Headers.TryGetValue(_options.Value.TenantHeader, out var tenantKey))
                tenantKey = http?.Request?.Host.Value.ToUpperInvariant();

            var useCache = _options.Value.TenantLifetimeSeconds.HasValue;

            if (useCache)
            {
                if (_tenantCache.Get(tenantKey) is TenantContext<TTenant> tenantContext)
                    return tenantContext;

                tenantContext = await _tenantContextStore.FindByKeyAsync(tenantKey);

                if (tenantContext != null)
                {
                    foreach (var identifier in tenantContext.Identifiers ?? Enumerable.Empty<string>())
                        _tenantCache.Set(identifier, tenantContext, TimeSpan.FromSeconds(_options.Value.TenantLifetimeSeconds.Value));
                }

                return tenantContext;
            }
            else
            {
                return await _tenantContextStore.FindByKeyAsync(tenantKey);
            }
        }
    }
}
