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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using HQ.Common;
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.DocumentDb.Options
{
	public static class DocumentConfigurationHelper
    {
        public static void MigrateToLatest(DocumentDbOptions options, SaveConfigurationOptions saveConfig,
	        IConfiguration configSeed = null, SeedStrategy strategy = SeedStrategy.InsertIfNotExists)
        {
	        var runner = new DocumentDbMigrationRunner(options);

	        if (saveConfig.CreateIfNotExists)
	        {
		        runner.CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
			}

	        if (saveConfig.MigrateOnStartup)
	        {
		        runner.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
			}
	        
	        if (configSeed != null)
	        {
		        var repository = new DocumentDbRepository<ConfigurationDocument>(Constants.Options.DefaultCollection, 
			        new OptionsMonitorShim<DocumentDbOptions>(options), null);

				switch (strategy)
		        {
			        case SeedStrategy.InsertIfNotExists:
			        {
				        InsertIfNotExists(repository);
				        break;
			        }

			        case SeedStrategy.InsertIfEmpty:
			        {
				        var count = repository.CountAsync().GetAwaiter().GetResult();
				        if (count == 0)
				        {
							InsertIfNotExists(repository);
				        }
				        break;
			        }
				        
			        default:
				        throw new ArgumentOutOfRangeException();
		        }
	        }

	        void InsertIfNotExists(IDocumentDbRepository<ConfigurationDocument> repository)
	        {
		        var manifest = repository.RetrieveAsync()
			        .GetAwaiter().GetResult().Select(x => x.Key).ToImmutableHashSet();

		        var changedKeys = new HashSet<string>();
				foreach (var document in configSeed.AsEnumerable())
				{
					if (manifest.Contains(document.Key))
						continue;

			        repository.CreateAsync(new ConfigurationDocument
			        {
				        Key = document.Key,
				        Value = document.Value
			        }).GetAwaiter().GetResult();

			        changedKeys.Add(document.Key);
				}

				Trace.TraceInformation(changedKeys.Count > 0
					? $"Configuration updated the following keys: {string.Join(",", changedKeys)}"
					: $"Configuration is up to date.");
	        }
        }
    }
}
