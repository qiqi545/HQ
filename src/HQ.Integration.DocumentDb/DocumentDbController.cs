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
using ActiveLogging;
using ActiveRoutes;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Integration.DocumentDb.SessionManagement;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb
{
	public abstract class DocumentDbController<T> : DataController where T : IDocument
	{
		protected DocumentDbRepository<T> Repository;

		protected DocumentDbController(string slot, IOptionsMonitor<DocumentDbOptions> options,
			ISafeLogger<DocumentDbRepository<T>> logger) =>
			Repository = new DocumentDbRepository<T>(slot, options, logger);

		[DynamicHttpPost("")]
		public async Task<IActionResult> CreateAsync([FromBody] T document)
		{
			if (!ValidModelState(out var error))
			{
				return error;
			}

			var created = await Repository.CreateAsync(document);

			// See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
			var location = $"{typeof(T).Name.Pluralize()}/{created.Id}";
			if (Request.Headers.TryGetValue(HttpHeaders.Prefer, out var prefer) && prefer.ToString()
				    .ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
			{
				Response.Headers.Add(HttpHeaders.PreferenceApplied, "true");
				return Created(location);
			}

			return Created(location, document);
		}

		[DynamicHttpGet("")]
		public async Task<IActionResult> RetrieveAsync()
		{
			var documents = await Repository.RetrieveAsync(predicate: null, CancellationToken);
			return Ok(documents);
		}

		[DynamicHttpGet("{id}")]
		public async Task<IActionResult> RetrieveAsync(string id)
		{
			if (id == null)
				return BadRequest();

			var document = await Repository.RetrieveAsync(id, CancellationToken);
			if (document == null)
			{
				return NotFound();
			}

			return Ok(document);
		}

		[DynamicHttpPut("")]
		public async Task<ActionResult> UpdateAsync(T item)
		{
			if (item == null)
				return BadRequest();

			if (!ValidModelState(out var error))
			{
				return error;
			}

			await Repository.UpdateAsync(item.Id, item, CancellationToken);
			return Ok();
		}

		[DynamicHttpDelete("{id}")]
		public async Task<ActionResult> DeleteAsync(string id)
		{
			if (id == null)
				return BadRequest();

			var document = await Repository.RetrieveAsync(id, CancellationToken);
			if (document == null)
				return NotFound();

			await Repository.DeleteAsync(id, CancellationToken);
			return NoContent();
		}
	}
}