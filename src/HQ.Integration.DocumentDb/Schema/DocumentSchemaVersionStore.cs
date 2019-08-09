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
using HQ.Common;
using HQ.Data.Contracts.Schema.Models;
using HQ.Integration.DocumentDb.Schema.Documents;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb.Schema
{
	public class DocumentSchemaVersionStore : ISchemaVersionStore
	{
		private readonly IDocumentDbRepository<SchemaVersionDocument> _repository;

		public DocumentSchemaVersionStore(IOptionsMonitor<DocumentDbOptions> options)
		{
			_repository = new DocumentDbRepository<SchemaVersionDocument>(
				Constants.ConnectionSlots.Schema, options);
		}

		public async Task<IEnumerable<SchemaVersion>> GetByApplicationId(string applicationId)
		{
			var current = (await _repository.RetrieveAsync(q =>
					q.Where(x => x.ApplicationId == applicationId)
						.OrderByDescending(x => x.Timestamp)
				)).GroupBy(k => k.Name).Select(g => g.First()).ToList(); // have to group outside DocumentDB :(

			return current.Select(x => x.Model);
		}

		public async Task<SchemaVersion> GetByFingerprintAsync(ulong hash)
		{
			var document = (await _repository.RetrieveAsync(x => x.Fingerprint == hash)).SingleOrDefault();
			return document?.Model;
		}

		public async Task CreateAsync(SchemaVersion model)
		{
			var document = new SchemaVersionDocument(model);
			await _repository.CreateAsync(document);
		}

		public async Task<SchemaVersion> GetLastRevisionAsync(SchemaType type, string @namespace, string name)
		{
			var query = (await _repository.RetrieveAsync(q => q
				.Where(x =>
					x.Type.ToString() == type.ToString() && // Azure SDK does not respect serializer settings: force conversion
					x.Namespace == @namespace &&
					x.Name == name)
				.OrderByDescending(x => x.Revision)));

			var lastRevision = query.FirstOrDefault();
			return lastRevision?.Model;
		}
	}
}