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
using ActiveCaching;
using ActiveCaching.Internal;
using ActiveErrors;
using ActiveRoutes;
using ActiveRoutes.Meta;
using ActiveStorage;
using ActiveText;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Schema.Models;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Schemas
{
	[DynamicController(typeof(SchemaOptions))]
	[Route("schemas")]
	[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Objects", "Provides programmatic access to weak-typed object schemas.")]
	[ServiceFilter(typeof(HttpCacheFilterAttribute))]
	public class SchemaController : Controller, IDynamicComponentEnabled<SchemaComponent>
	{
		private readonly IOptionsMonitor<ApiOptions> _apiOptions;
		private readonly IOptionsMonitor<QueryOptions> _queryOptions;
		private readonly ISchemaVersionStore _store;

		public SchemaController(ISchemaVersionStore store, IOptionsMonitor<ApiOptions> apiOptions,
			IOptionsMonitor<QueryOptions> queryOptions)
		{
			_store = store;
			_apiOptions = apiOptions;
			_queryOptions = queryOptions;
		}

		[DynamicHttpGet("{applicationId}")]
		public async Task<IActionResult> GetSchemas([FromRoute] [BindRequired] string applicationId)
		{
			var data = (await _store.GetByApplicationId(applicationId)).AsList();
			var count = data.Count;
			var page = new Page<SchemaVersion>(data, count, 0, count, count);
			var slice = new Operation<IPage<SchemaVersion>>(page);
			Response.MaybeEnvelope(Request, _apiOptions.CurrentValue.JsonConversion, slice.Data, slice.Errors, out var body);
			return Ok(body);
		}
	}
}