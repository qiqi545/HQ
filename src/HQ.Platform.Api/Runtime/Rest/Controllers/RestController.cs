using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Caching.AspNetCore.Mvc;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Runtime.Rest.Attributes;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch;

namespace HQ.Platform.Api.Runtime.Rest.Controllers
{
	[ApiExplorerSettings(IgnoreApi = true)]
	[ServiceFilter(typeof(HttpCacheFilterAttribute))]
	[Produces(Constants.MediaTypes.Json, Constants.MediaTypes.Xml)]
	public class RestController<T> : DataController, IObjectController<T> where T : class, IObject
	{
		private readonly IOptionsMonitor<QueryOptions> _queryOptions;
		private readonly IOptionsMonitor<PlatformApiOptions> _apiOptions;
		private readonly IObjectRepository<T> _repository;

		// ReSharper disable once StaticMemberInGenericType
		private static readonly Lazy<FieldOptions> IdField = new Lazy<FieldOptions>(() =>
		{
			var fields = new FieldOptions();
			fields.Fields.Add(nameof(IObject.Id));
			return fields;
		});

		public Type ObjectType => typeof(T);

		public RestController(IObjectRepository<T> repository, IOptionsMonitor<QueryOptions> queryOptions, IOptionsMonitor<PlatformApiOptions> apiOptions)
		{
			_repository = repository;
			_queryOptions = queryOptions;
			_apiOptions = apiOptions;
		}

		#region GET

		[VersionSelector]
		[HttpGet("X"), HttpGet("X.{format}"), ResourceFilter, FormatFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedCollectionBody<IPage<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeCollectionBody<IPage<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedCollectionBody<IStream<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeCollectionBody<IStream<IObject>>))]
		public async Task<IActionResult> GetAsync(SortOptions sort, PageOptions page, StreamOptions stream, FieldOptions fields, FilterOptions filter, ProjectionOptions projection, SegmentOptions segment, [FromQuery] string query = null)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			if (segment.Ids != null && (segment.Count > 0 || segment.Ids.Any()))
			{
				// ReSharper disable once PossibleMultipleEnumeration
				var slice = await _repository.GetAsync(segment, fields, filter, projection);
				if (slice?.Data?.Count == 0)
					return NotFound();
				Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, _queryOptions.CurrentValue, slice?.Data, slice?.Errors, out var body);
				return Ok(body);
			}
			else
			{
				var slice = await _repository.GetAsync(query, sort, page, fields, filter, projection);
				if (slice?.Data?.Count == 0)
					return NotFound();
				Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, _queryOptions.CurrentValue, slice?.Data, slice?.Errors, out var body);
				return Ok(body);
			}
		}

		[VersionSelector]
		[HttpGet("X/{id}"), HttpGet("X/{id}.{format}"), FieldsFilter, ProjectionFilter, FormatFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<IObject>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<IObject>))]
		public async Task<IActionResult> GetAsync([FromRoute, BindRequired] long id, FieldOptions fields, ProjectionOptions projections)
		{
			var @object = await _repository.GetAsync(id, fields);
			if (@object?.Data == null)
				return NotFound();
			Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, _queryOptions.CurrentValue, @object.Data, @object.Errors, out var body);
			return Ok(body);
		}

		#endregion

		#region POST

		[VersionSelector]
		[HttpPost("X")]
		[ProducesResponseType((int) HttpStatusCode.SeeOther)]
		[ProducesResponseType((int) HttpStatusCode.Forbidden)]
		[ProducesResponseType((int) HttpStatusCode.Created)]
		[ProducesResponseType((int) HttpStatusCode.Created, Type = typeof(IObject))]
		public async Task<IActionResult> PostAsync([FromBody] T @object)
		{
			if (@object.Id != 0)
			{
				if (await GetAsync(@object.Id, IdField.Value, ProjectionOptions.Empty) != null)
				{
					// If trying to create an existing @object by mistake, use REST constraints:
					// https://tools.ietf.org/html/rfc7231#section-4.3.3
					return SeeOther($"{Request.Path}/{@object.Id}");
				}
				var error = new Error(ErrorEvents.InvalidRequest, "Cannot create an object with a pre-specified identifier.", HttpStatusCode.Forbidden);
				return new ErrorResult(error);
			}
			return await SaveAsync(@object);
		}

		[VersionSelector]
		[HttpPost("X/batch")]
		[ProducesResponseType((int) 422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public async Task<IActionResult> PostAsync([FromBody] IEnumerable<T> objects, long startingAt = 0, int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return Error(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave, 422 /*HttpStatusCode.UnprocessableEntity*/));
			// ReSharper disable once PossibleMultipleEnumeration
			return await SaveAsync(objects, startingAt, count, BatchSaveStrategy.Insert);
		}

		#endregion

		#region PUT

		[VersionSelector]
		[HttpPut("X/{id}")]
		[ProducesResponseType((int) 422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IObject))]
		public async Task<IActionResult> PutAsync([FromRoute, BindRequired] long id, T @object)
		{
			if (@object == null)
				return Error(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave, 422 /*HttpStatusCode.UnprocessableEntity*/));
			var existing = await _repository.GetAsync(id);
			if (existing == null)
				return NotFound();
			@object.Id = id;
			return await SaveAsync(@object);
		}

		[VersionSelector]
		[HttpPut("X")]
		[ProducesResponseType((int) 422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public async Task<IActionResult> PutAsync([FromBody] IEnumerable<T> objects, long startingAt = 0, int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return Error(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave, 422 /*HttpStatusCode.UnprocessableEntity*/));
			return await SaveAsync(objects, startingAt, count, BatchSaveStrategy.Upsert);
		}

		#endregion

		#region PATCH

		[VersionSelector]
		[HttpPatch("X/{id}")]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<IObject>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<IObject>))]
		public async Task<IActionResult> PatchAsync([FromRoute, BindRequired] long id, JsonPatchDocument<T> patch)
		{
			if (!Valid(patch, out var error))
				return error;

			var @object = await _repository.GetAsync(id);
			if (@object?.Data == null)
				return NotFound();

			patch.ApplyTo(@object.Data);
			return await SaveAsync(@object.Data);
		}

		[VersionSelector]
		[HttpPatch("X/{id}/merge")]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<Task>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<Task>))]
		public async Task<IActionResult> PatchAsync([FromRoute, BindRequired] long id, JsonMergePatchDocument<T> patch)
		{
			if (!Valid(patch, out var error))
				return error;

			var @object = await _repository.GetAsync(id);
			if (@object?.Data == null)
				return NotFound();

			patch.ApplyTo(@object.Data);
			return await SaveAsync(@object.Data);
		}

		[VersionSelector]
		[HttpPatch("X")]
		[ProducesResponseType((int) 422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public virtual async Task<IActionResult> PatchAsync([FromBody] IEnumerable<T> objects, long startingAt = 0, int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return Error(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave, 422 /*HttpStatusCode.UnprocessableEntity*/));
			return await SaveAsync(objects, startingAt, count, BatchSaveStrategy.Update);
		}

		#endregion

		#region DELETE

		[VersionSelector]
		[HttpDelete("X"), FilterFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public virtual async Task<IActionResult> DeleteAsync(FilterOptions filter)
		{
			var toDelete = await _repository.GetAsync(null, SortOptions.Empty, PageOptions.Empty, FieldOptions.Empty, filter, ProjectionOptions.Empty);
			if (toDelete?.Data == null)
				return NotFound();

			var operation = await _repository.DeleteAsync(toDelete.Data);
			return operation.ToResult();
		}

		[VersionSelector]
		[HttpDelete("X/{id}")]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.Gone)]
		[ProducesResponseType((int) HttpStatusCode.NoContent)]
		public virtual async Task<IActionResult> DeleteAsync([FromRoute, BindRequired] long id)
		{
			var toDelete = await _repository.GetAsync(id, FieldOptions.Empty, ProjectionOptions.Empty);
			if (toDelete?.Data == null)
				return NotFound();

			var operation = await _repository.DeleteAsync(toDelete.Data);
			return operation.ToResult(body =>
			{
				switch (operation.Data)
				{
					case ObjectDelete.NotFound:
						return NotFound();
					case ObjectDelete.Deleted:
						return NoContent();
					case ObjectDelete.Gone:
						return Gone();
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		[VersionSelector]
		[HttpDelete("X/{ids}")]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public virtual async Task<IActionResult> DeleteAsync(SegmentOptions segment)
		{
			var toDelete = await _repository.GetAsync(segment, FieldOptions.Empty, FilterOptions.Empty, ProjectionOptions.Empty);
			if (toDelete == null)
				return NotFound();

			var operation = await _repository.DeleteAsync(toDelete.Data);
			return operation.ToResult();
		}

		#endregion

		private async Task<IActionResult> SaveAsync(T @object)
		{
			if (!Valid(@object, out var error))
				return error;

			var operation = await _repository.SaveAsync(@object);
			return operation.ToResult(body =>
			{
				// See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
				if (Request.Headers.TryGetValue(Constants.HttpHeaders.Prefer, out var prefer) && prefer.ToString().ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
				{
					body = null;
					Response.Headers.Add(Constants.HttpHeaders.PreferenceApplied, "true");
				}

				switch (operation.Data)
				{
					case ObjectSave.NotFound:
						return NotFound();
					case ObjectSave.NoChanges:
						return NotModified();
					case ObjectSave.Updated:
						return Ok(body);
					case ObjectSave.Created:
						return Created(new Uri($"{Request.Path}/{@object.Id}", UriKind.Relative), body);
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		private async Task<IActionResult> SaveAsync([FromBody] IEnumerable<T> objects, long startingAt, int? count = null, BatchSaveStrategy strategy = BatchSaveStrategy.Insert)
		{
			var errors = new List<Error>();
			objects = objects.Where(x =>
			{
				if (TryValidateModel(x, $"{x.Id}"))
					return true;
				errors.Add(ConvertModelStateToError());
				return false;
			});

			var result = await _repository.SaveAsync(objects, strategy, startingAt, count);
			foreach (var error in errors)
				result.Errors.Add(error);

			// See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
			if (Request.Headers.TryGetValue(Constants.HttpHeaders.Prefer, out var prefer) && prefer.ToString().ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
			{
				Response.Headers.Add(Constants.HttpHeaders.PreferenceApplied, "true");
				return Ok();
			}

			Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, _queryOptions.CurrentValue, result.Errors, out var body);
			return Ok(body);
		}
	}
}
