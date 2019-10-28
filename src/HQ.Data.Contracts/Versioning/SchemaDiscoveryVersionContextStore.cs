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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Schema.Models;
using HQ.Extensions.Cryptography;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Data.Contracts.Versioning
{
	public class SchemaDiscoveryVersionContextStore : IVersionContextStore
	{
		private readonly IOptionsMonitor<SchemaOptions> _options;
		private readonly ISchemaVersionStore _store;

		public SchemaDiscoveryVersionContextStore(ISchemaVersionStore store, IOptionsMonitor<SchemaOptions> options)
		{
			_store = store;
			_options = options;
		}

		public bool SupportsFallbackVersion => true;

		public async Task<VersionContext> FindByKeyAsync(string versionKey)
		{
			var versions = await _store.GetByApplicationId(_options.CurrentValue.ApplicationId);

			//
			// Match by revision:
			if (int.TryParse(versionKey, out var revision))
			{
				// ReSharper disable once PossibleMultipleEnumeration
				foreach (var version in versions)
				{
					if (version.Revision == revision)
					{
						return MatchedContext(version);
					}
				}

				return VersionContext.None;
			}

			//
			// Match by fingerprint:
			if (ulong.TryParse(versionKey, out var fingerprint))
			{
				// ReSharper disable once PossibleMultipleEnumeration
				foreach (var version in versions)
				{
					if (version.Fingerprint == fingerprint)
					{
						return MatchedContext(version);
					}
				}

				return VersionContext.None;
			}

			//
			// Resolve to latest version:
			// ReSharper disable once PossibleMultipleEnumeration
			var latest = versions.LastOrDefault();
			if (latest == null)
				return VersionContext.None;

			return MatchedContext(latest);
		}

		private static VersionContext MatchedContext(SchemaVersion version)
		{
			var context = new VersionContext
			{
				Group = new VersionGroup($"{version.Revision}"), Map = new Dictionary<string, Version>()
			};

			context.Map.Add(version.Data.Name, new Version {Major = version.Fingerprint, Minor = 0});

			foreach (var entry in version.Data.GetMap())
			{
				context.Map.Add(entry.Value.Name, new Version {Major = ValueHash.ComputeHash(entry.Value), Minor = 0});
			}

			context.Identifiers = new string[0];
			return context;
		}
	}
}