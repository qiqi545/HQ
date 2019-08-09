using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Schema.Models;
using HQ.Integration.DocumentDb.Schema.Documents;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb.Schema
{
	public class DocumentApplicationVersionStore : IApplicationVersionStore
	{
		private readonly IDocumentDbRepository<ApplicationVersionDocument> _repository;

		public DocumentApplicationVersionStore(IOptionsMonitor<DocumentDbOptions> options)
		{
			_repository = new DocumentDbRepository<ApplicationVersionDocument>(
				Constants.ConnectionSlots.Schema, options);
		}

		public async Task CreateAsync(ApplicationVersion model)
		{
			var document = new ApplicationVersionDocument(model);
			await _repository.CreateAsync(document);
		}
	}
}
