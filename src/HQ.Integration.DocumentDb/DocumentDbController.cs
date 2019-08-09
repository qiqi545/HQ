using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Mvc;
using HQ.Integration.DocumentDb.SessionManagement;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Integration.DocumentDb.Sql
{
    public abstract class DocumentDbController<T> : DataController where T : IDocument
    {
        protected DocumentDbRepository<T> Repository;

        protected DocumentDbController(string slot, IOptionsMonitor<DocumentDbOptions> options)
        {
            Repository = new DocumentDbRepository<T>(slot, options);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] T document)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var created = await Repository.CreateAsync(document);

            // See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
            var location = $"{typeof(T).Name.Pluralize()}/{created.Id}";
            if (Request.Headers.TryGetValue(Constants.HttpHeaders.Prefer, out var prefer) && prefer.ToString()
                    .ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
            {
                Response.Headers.Add(Constants.HttpHeaders.PreferenceApplied, "true");
                return Created(location);
            }

            return Created(location, document);
        }

        [HttpGet("")]
        public async Task<IActionResult> RetrieveAsync()
        {
            var documents = await Repository.RetrieveAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> RetrieveAsync(string id)
        {
            var document = await Repository.RetrieveAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        [HttpPut("")]
        public async Task<ActionResult> UpdateAsync(T item)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            await Repository.UpdateAsync(item.Id, item);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var document = await Repository.RetrieveAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            await Repository.DeleteAsync(id);
            return NoContent();
        }
    }
}