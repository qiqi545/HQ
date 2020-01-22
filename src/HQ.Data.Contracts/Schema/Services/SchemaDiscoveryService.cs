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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Data.Contracts.Schema.Services
{
	public class SchemaDiscoveryService : IHostedService
	{
		private readonly IHostEnvironment _environment;
		private readonly ISafeLogger<SchemaDiscoveryService> _logger;
		private readonly IOptionsMonitor<SchemaOptions> _options;
		private readonly SchemaService _service;

		public SchemaDiscoveryService(SchemaService service, IHostEnvironment environment,
			IOptionsMonitor<SchemaOptions> options, ISafeLogger<SchemaDiscoveryService> logger)
		{
			_service = service;
			_environment = environment;
			_options = options;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (!_options.CurrentValue.Enabled)
				return;

			_logger.Info(() => "Starting schema discovery...");

			var schemaRelativeDir = _options.CurrentValue.SchemaFolder;

			if (!string.IsNullOrWhiteSpace(schemaRelativeDir))
			{
				if (schemaRelativeDir.StartsWith("/") || schemaRelativeDir.StartsWith("\\"))
					schemaRelativeDir = schemaRelativeDir.Substring(1);

				var schemaDir = Path.Combine(_environment.ContentRootPath, schemaRelativeDir);
				if (Directory.Exists(schemaDir))
				{
					_logger.Info(() => "Found schemas in {SchemaDir}", schemaDir);

					var revision = await UpdateSchemaAsync(schemaRelativeDir,
						_options.CurrentValue.ApplicationId ?? Constants.Schemas.DefaultApplicationId);
					if (revision != 0)
						_logger.Info(() => "Schema updated to revision {Revision}", revision);
					else
						_logger.Info(() => "Schema is unchanged");
				}
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		private async Task<ulong> UpdateSchemaAsync(string folder, string applicationId)
		{
			var revisionSet = new List<Models.Schema>();
			var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly)
				.Select(x => new FileInfo(x));
			foreach (var file in files.OrderBy(x => x.LastWriteTimeUtc).ThenBy(x => x.Name))
			{
				var json = File.ReadAllText(file.FullName);
				var schemas = JsonConvert.DeserializeObject<Models.Schema[]>(json);
				foreach (var schema in schemas)
					_logger.Info(() => "{FileName} => {SchemaName}", file.FullName, schema.Name);
				revisionSet.AddRange(schemas);
			}

			return await _service.TrySaveRevisionAsync(applicationId, revisionSet);
		}
	}
}