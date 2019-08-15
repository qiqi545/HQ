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

using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Schema.Models;
using HQ.Extensions.Logging;
using HQ.Integration.DocumentDb.Schema.Documents;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb.Schema
{
	public class DocumentApplicationVersionStore : IApplicationVersionStore
	{
		private readonly IDocumentDbRepository<ApplicationVersionDocument> _repository;

		public DocumentApplicationVersionStore(IOptionsMonitor<DocumentDbOptions> options, ISafeLogger<DocumentDbRepository<ApplicationVersionDocument>> logger)
		{
			_repository = new DocumentDbRepository<ApplicationVersionDocument>(
				Constants.ConnectionSlots.Schema, options, logger);
		}

		public async Task CreateAsync(ApplicationVersion model)
		{
			var document = new ApplicationVersionDocument(model);
			await _repository.CreateAsync(document);
		}
	}
}
