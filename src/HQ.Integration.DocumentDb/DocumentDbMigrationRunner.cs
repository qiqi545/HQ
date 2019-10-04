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

using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb
{
	public class DocumentDbMigrationRunner
	{
		private readonly DocumentDbOptions _options;

		public DocumentDbMigrationRunner(DocumentDbOptions options)
		{
			_options = options;
			Client = new DocumentClient(options.AccountEndpoint, options.AccountKey, Defaults.JsonSettings);
		}

		public DocumentClient Client { get; }

		public async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_options.DatabaseId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == HttpStatusCode.NotFound)
				{
					await Client.CreateDatabaseAsync(new Database {Id = _options.DatabaseId});
				}
				else
				{
					throw;
				}
			}
		}

		public async Task CreateCollectionIfNotExistsAsync()
		{
			try
			{
				await Client.ReadDocumentCollectionAsync(
					UriFactory.CreateDocumentCollectionUri(_options.DatabaseId, _options.CollectionId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == HttpStatusCode.NotFound)
				{
					await Client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_options.DatabaseId),
						new DocumentCollection
						{
							Id = _options.CollectionId,
							PartitionKey =
								new PartitionKeyDefinition
								{
									Paths = new Collection<string>(_options.PartitionKeyPaths),
									Version = PartitionKeyDefinitionVersion.V2
								}
						},
						new RequestOptions {OfferThroughput = _options.OfferThroughput});
				}
				else
				{
					throw;
				}
			}
		}
	}
}