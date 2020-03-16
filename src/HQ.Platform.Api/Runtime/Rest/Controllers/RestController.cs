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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ActiveRoutes;
using ActiveErrors;
using HQ.Common.AspNetCore.MergePatch;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Configuration;
using HQ.Extensions.Caching;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Models;
using HQ.Platform.Api.Runtime.Rest.Attributes;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Error = ActiveErrors.Error;

namespace HQ.Platform.Api.Runtime.Rest.Controllers
{
	[ApiExplorerSettings(IgnoreApi = true)]
	[ServiceFilter(typeof(HttpCacheFilterAttribute))]
	[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
	public class RestController<TObject, TKey> : Controller, IObjectController where TObject : class, IObject<TKey> where TKey : IEquatable<TKey>
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly Lazy<FieldOptions> IdField = new Lazy<FieldOptions>(() =>
		{
			var fields = new FieldOptions();
			fields.Fields.Add(nameof(IObject.Id));
			return fields;
		});

		private readonly IOptionsMonitor<ApiOptions> _apiOptions;
		private readonly IOptionsMonitor<QueryOptions> _queryOptions;
		private readonly IObjectRepository<TObject, TKey> _repository;

		public RestController(IObjectRepository<TObject, TKey> repository, IOptionsMonitor<QueryOptions> queryOptions,
			IOptionsMonitor<ApiOptions> apiOptions)
		{
			_repository = repository;
			_queryOptions = queryOptions;
			_apiOptions = apiOptions;
		}

		public Type ObjectType => typeof(TObject);

		private async Task<IActionResult> SaveAsync(TObject @object)
		{
			if (!this.TryValidateModelOrError(@object, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed, out var error))
				return error;

			var operation = await _repository.SaveAsync(@object);
			return operation.ToResult(body =>
			{
				// See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
				if (Request.Headers.TryGetValue(HttpHeaders.Prefer, out var prefer) && prefer.ToString()
					    .ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
				{
					body = null;
					Response.Headers.Add(HttpHeaders.PreferenceApplied, "true");
				}

				switch (operation.Data)
				{
					case ObjectSave.NotFound:
						return NotFound();
					case ObjectSave.NoChanges:
						return this.NotModified();
					case ObjectSave.Updated:
						return Ok(body);
					case ObjectSave.Created:
						return Created(new Uri($"{Request.Path}/{@object.Id}", UriKind.Relative), body);
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		private async Task<IActionResult> SaveAsync([FromBody] IEnumerable<TObject> objects, long startingAt,
			int? count = null, BatchSaveStrategy strategy = BatchSaveStrategy.Insert)
		{
			var errors = new List<Error>();
			objects = objects.Where(x =>
			{
				if (TryValidateModel(x, $"{x.Id}"))
					return true;
				errors.Add(this.ConvertModelStateToError(ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed));
				return false;
			});

			var result = await _repository.SaveAsync(objects, strategy, startingAt, count);
			foreach (var error in errors)
				result.Errors.Add(error);

			// See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
			if (Request.Headers.TryGetValue(HttpHeaders.Prefer, out var prefer) && prefer.ToString()
				    .ToUpperInvariant().Replace(" ", string.Empty).Equals("RETURN=MINIMAL"))
			{
				Response.Headers.Add(HttpHeaders.PreferenceApplied, "true");
				return Ok();
			}

			Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, _queryOptions.CurrentValue, result.Errors,
				out var body);
			return Ok(body);
		}

		#region GET

		[VersionSelector]
		[DynamicHttpGet("X")]
		[DynamicHttpGet("X.{format}")]
		[ResourceFilter]
		[FormatFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedCollectionBody<IPage<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeCollectionBody<IPage<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedCollectionBody<IStream<IObject>>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeCollectionBody<IStream<IObject>>))]
		public async Task<IActionResult> GetAsync(SortOptions sort, PageOptions page, StreamOptions stream,
			FieldOptions fields, FilterOptions filter, ProjectionOptions projection, SegmentOptions segment,
			[FromQuery] string query = null)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			if (segment.Ids != null && (segment.Count > 0 || segment.Ids.Any()))
			{
				// ReSharper disable once PossibleMultipleEnumeration
				var slice = await _repository.GetAsync(segment, fields, filter, projection);
				Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, slice?.Data, slice?.Errors, out var body);
				return Ok(body);
			}
			else
			{
				var slice = await _repository.GetAsync(query, sort, page, fields, filter, projection);
				Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, slice?.Data, slice?.Errors, out var body);
				return Ok(body);
			}
		}

		[VersionSelector]
		[DynamicHttpGet("X/{id}")]
		[DynamicHttpGet("X/{id}.{format}")]
		[FieldsFilter]
		[ProjectionFilter]
		[FormatFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<IObject>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<IObject>))]
		public async Task<IActionResult> GetAsync([FromRoute] [BindRequired] TKey id, FieldOptions fields,
			ProjectionOptions projections)
		{
			var @object = await _repository.GetAsync(id, fields);
			if (@object?.Data == null)
				return NotFound();
			Response.MaybeEnvelope(Request, _apiOptions.CurrentValue, @object.Data, @object.Errors, out var body);
			return Ok(body);
		}

		#endregion

		#region POST

		[VersionSelector]
		[DynamicHttpPost("X")]
		[ProducesResponseType((int) HttpStatusCode.SeeOther)]
		[ProducesResponseType((int) HttpStatusCode.Forbidden)]
		[ProducesResponseType((int) HttpStatusCode.Created)]
		[ProducesResponseType((int) HttpStatusCode.Created, Type = typeof(IObject))]
		public async Task<IActionResult> PostAsync([FromBody] TObject @object)
		{
			if (!@object.Id.Equals(default))
			{
				if (await GetAsync(@object.Id, IdField.Value, ProjectionOptions.Empty) != null)
				{
					// If trying to create an existing @object by mistake, use REST constraints:
					// https://tools.ietf.org/html/rfc7231#section-4.3.3
					return this.SeeOther($"{Request.Path}/{@object.Id}");
				}

				var error = new Error(ErrorEvents.InvalidRequest,
					"Cannot create an object with a pre-specified identifier.", HttpStatusCode.Forbidden);
				return new ErrorObjectResult(error);
			}

			return await SaveAsync(@object);
		}

		[VersionSelector]
		[DynamicHttpPost("X/batch")]
		[ProducesResponseType(422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public async Task<IActionResult> PostAsync([FromBody] IEnumerable<TObject> objects, long startingAt = 0,
			int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return new ErrorObjectResult(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave,
					422 /*HttpStatusCode.UnprocessableEntity*/));
			// ReSharper disable once PossibleMultipleEnumeration
			return await SaveAsync(objects, startingAt, count);
		}

		#endregion

		#region PUT

		[VersionSelector]
		[DynamicHttpPut("X/{id}")]
		[ProducesResponseType(422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IObject))]
		public async Task<IActionResult> PutAsync([FromRoute] [BindRequired] TKey id, TObject @object)
		{
			if (@object == null)
				return new ErrorObjectResult(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave,
					422 /*HttpStatusCode.UnprocessableEntity*/));
			var existing = await _repository.GetAsync(id);
			if (existing == null)
				return NotFound();
			@object.Id = id;
			return await SaveAsync(@object);
		}

		[VersionSelector]
		[DynamicHttpPut("X")]
		[ProducesResponseType(422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public async Task<IActionResult> PutAsync([FromBody] IEnumerable<TObject> objects, long startingAt = 0,
			int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return new ErrorObjectResult(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave,
					422 /*HttpStatusCode.UnprocessableEntity*/));
			return await SaveAsync(objects, startingAt, count, BatchSaveStrategy.Upsert);
		}

		#endregion

		#region PATCH

		[VersionSelector]
		[DynamicHttpPatch("X/{id}")]
		[Consumes(MediaTypeNames.Application.JsonPatch)]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<IObject>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<IObject>))]
		public async Task<IActionResult> PatchAsync([FromRoute] [BindRequired] TKey id, JsonPatchDocument<TObject> patch)
		{
			if (!this.TryValidateModelOrError(patch, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed, out var error))
				return error;

			var @object = await _repository.GetAsync(id);
			if (@object?.Data == null)
				return NotFound();

			patch.ApplyTo(@object.Data);
			return await SaveAsync(@object.Data);
		}

		[VersionSelector]
		[DynamicHttpPatch("X/{id}/merge")]
		[Consumes(MediaTypeNames.Application.JsonMergePatch)]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.NotModified)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(EnvelopeBody<Task>))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(NestedBody<Task>))]
		public async Task<IActionResult> PatchAsync([FromRoute] [BindRequired] TKey id, JsonMergePatchDocument<TObject> patch)
		{
			if (!this.TryValidateModelOrError(patch, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed, out var error))
				return error;

			var @object = await _repository.GetAsync(id);
			if (@object?.Data == null)
				return NotFound();

			patch.ApplyTo(@object.Data);
			return await SaveAsync(@object.Data);
		}

		[VersionSelector]
		[DynamicHttpPatch("X")]
		[ProducesResponseType(422 /*HttpStatusCode.UnprocessableEntity*/)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Envelope))]
		[ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(Nested))]
		public virtual async Task<IActionResult> PatchAsync([FromBody] IEnumerable<TObject> objects, long startingAt = 0,
			int? count = null)
		{
			if (objects == null || count.HasValue && count.Value == 0 || !objects.Any())
				return new ErrorObjectResult(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ResourceMissingInSave,
					422 /*HttpStatusCode.UnprocessableEntity*/));
			return await SaveAsync(objects, startingAt, count, BatchSaveStrategy.Update);
		}

		#endregion

		#region DELETE

		[VersionSelector]
		[DynamicHttpDelete("X")]
		[FilterFilter]
		[ProducesResponseType((int) HttpStatusCode.BadRequest)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public virtual async Task<IActionResult> DeleteAsync(FilterOptions filter)
		{
			var toDelete = await _repository.GetAsync(null, SortOptions.Empty, PageOptions.Empty, FieldOptions.Empty,
				filter, ProjectionOptions.Empty);
			if (toDelete?.Data == null)
				return NotFound();

			var operation = await _repository.DeleteAsync(toDelete.Data);
			return operation.ToResult();
		}

		[VersionSelector]
		[DynamicHttpDelete("X/{id}")]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		[ProducesResponseType((int) HttpStatusCode.Gone)]
		[ProducesResponseType((int) HttpStatusCode.NoContent)]
		public virtual async Task<IActionResult> DeleteAsync([FromRoute] [BindRequired] TKey id)
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
						return this.Gone();
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		[VersionSelector]
		[DynamicHttpDelete("X/{ids}")]
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
	}
}