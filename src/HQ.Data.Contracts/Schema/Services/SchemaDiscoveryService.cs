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
using System.Threading;
using System.Threading.Tasks;
using HQ.Data.Contracts.Schema.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Data.Contracts.Schema.Services
{
	public class SchemaDiscoveryService : IHostedService
	{
		private readonly SchemaService _service;
		private readonly IOptionsMonitor<SchemaOptions> _options;

		public SchemaDiscoveryService(SchemaService service, IOptionsMonitor<SchemaOptions> options)
		{
			_service = service;
			_options = options;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			const string applicationId = "default";

			if (_options.CurrentValue.Enabled &&
			    !string.IsNullOrWhiteSpace(_options.CurrentValue.SchemaFolder) &&
			    Directory.Exists(_options.CurrentValue.SchemaFolder))
			{
				await UpdateSchemaAsync(_options.CurrentValue.SchemaFolder, applicationId);
			}
		}

		private async Task UpdateSchemaAsync(string folder, string applicationId)
		{
			var revisionSet = new List<Models.Schema>();
			var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
			foreach (var file in files)
			{
				var json = File.ReadAllText(file);
				var schemas = JsonConvert.DeserializeObject<Models.Schema[]>(json);
				revisionSet.AddRange(schemas);
			}
			await _service.TrySaveRevisionAsync(applicationId, revisionSet);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}