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
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveCaching;
using HQ.Common;
using HQ.Data.Contracts.Versioning;
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Models
{
	/// <summary>
	///     Resolves a version context from metadata and a context store.
	///     See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning
	/// </summary>
	public class DefaultVersionContextResolver : IVersionContextResolver
	{
		private readonly ILogger _logger;
		private readonly IOptionsMonitor<VersioningOptions> _options;
		private readonly ICache _versionCache;
		private readonly IVersionContextStore _versionContextStore;

		public DefaultVersionContextResolver(ICache versionCache, IVersionContextStore versionContextStore,
			IOptionsMonitor<VersioningOptions> options, ILogger<IVersionContextResolver> logger)
		{
			_versionCache = versionCache;
			_versionContextStore = versionContextStore;
			_options = options;
			_logger = logger;
		}

		public async Task<VersionContext> ResolveAsync(HttpContext http)
		{
			if (http == null)
				return null; // fail early, developer error

			if (!_options.CurrentValue.Enabled || _options.CurrentValue.RequireExplicitVersion &&
			    !_options.CurrentValue.EnableVersionHeader &&
			    !_options.CurrentValue.EnableVersionParameter &&
			    !_options.CurrentValue.EnableVersionPath)
			{
				return null; // fail early, no versioning, or no explicit version
			}

			StringValues versionKey;

			//
			// Explicit Version:
			{
				if (_options.CurrentValue.EnableVersionHeader &&
				    !string.IsNullOrWhiteSpace(_options.CurrentValue.VersionHeader))
					http.Request.Headers.TryGetValue(_options.CurrentValue.VersionHeader, out versionKey);

				if (_options.CurrentValue.EnableVersionParameter &&
				    !string.IsNullOrWhiteSpace(_options.CurrentValue.VersionParameter) &&
				    http.Request.QueryString.HasValue)
					http.Request.Query.TryGetValue(_options.CurrentValue.VersionParameter, out versionKey);

				if (_options.CurrentValue.EnableVersionPath &&
				    !string.IsNullOrWhiteSpace(_options.CurrentValue.VersionPathPrefix) &&
				    http.Request.PathBase.HasValue)
					versionKey = http.Request.PathBase.Value;
			}

			if (versionKey == default(StringValues))
			{
				//
				// Implicit Version:
				if (_options.CurrentValue.EnableUserVersions &&
				    !string.IsNullOrWhiteSpace(_options.CurrentValue.UserVersionClaim))
				{
					var claim = http.User.FindFirst(x => x.Type == ClaimTypes.Version);
					if (claim != null)
						versionKey = claim.Value;
				}
			}

			if (versionKey == default(StringValues) && !_versionContextStore.SupportsFallbackVersion)
			{
				return null; // no derivable key
			}

			var useCache = _options.CurrentValue.VersionLifetimeSeconds.HasValue;
			if (!useCache)
			{
				return await _versionContextStore.FindByKeyAsync(versionKey);
			}

			if (_versionCache.Get($"{Constants.ContextKeys.Version}:{versionKey}") is VersionContext versionContext)
			{
				return versionContext;
			}

			versionContext = await _versionContextStore.FindByKeyAsync(versionKey);
			if (versionContext == null)
			{
				return null;
			}

			foreach (var identifier in versionContext.Identifiers ?? Enumerable.Empty<string>())
			{
				_versionCache.Set($"{Constants.ContextKeys.Version}:{identifier}", versionContext,
					TimeSpan.FromSeconds(_options.CurrentValue.VersionLifetimeSeconds.Value));
			}

			return versionContext;
		}
	}
}