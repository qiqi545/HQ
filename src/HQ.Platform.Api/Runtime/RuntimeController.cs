#region 

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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Data.Contracts.Runtime;
using HQ.Data.Contracts.Versioning;
using HQ.Extensions.Caching.AspNetCore.Mvc;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Runtime
{
	[DynamicController(typeof(RuntimeOptions))]
	[DynamicAuthorize(typeof(RuntimeOptions))]
	[Route("objects")]
	[Produces(Constants.MediaTypes.Json, Constants.MediaTypes.Xml)]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Objects", "Provides programmatic access to API resources over HTTP.")]
	[ServiceFilter(typeof(HttpCacheFilterAttribute))]
	public class RuntimeController : DataController, IDynamicComponentEnabled<RuntimeComponent>
	{
		private readonly ITypeResolver _typeResolver;
		private readonly IObjectGetRepository _repository;

		private readonly IOptionsMonitor<RuntimeOptions> _runtimeOptions;

		public RuntimeController(ITypeResolver typeResolver, IObjectGetRepository repository, IOptionsMonitor<RuntimeOptions> runtimeOptions)
		{
			_typeResolver = typeResolver;
			_repository = repository;
			_runtimeOptions = runtimeOptions;
		}

		[FeatureSelector]
		[HttpOptions("")]
		public IActionResult Options()
		{
			IEnumerable<string> availableTypes = new List<string>();

			// add any discovered schema types
			var version = HttpContext.GetVersionContext();
			if (version != null && version != VersionContext.None)
				availableTypes = availableTypes.Concat(version.Map.Keys);

			// add any custom prefabs 
			availableTypes = availableTypes.Concat(_typeResolver.FindByInterface<IObject>().Select(x => x.Name));

			// FIXME: filter out exclusions
			return Ok(new { Data = availableTypes });
		}

		#region GET

		// [VersionSelector] // FIXME: controller name dependent
		[FeatureSelector]
		[QueryContextProviderSelector]
		[FormatFilter]
		[HttpGet("{objectType}")]
		[HttpGet("{objectType}.{format}")]
		public async Task<IActionResult> GetAsync([FromRoute, BindRequired] string objectType)
		{
			var type = _typeResolver.FindFirstByName(objectType); // FIXME: allow pluralized type names
			if (type == null || !typeof(IObject).IsAssignableFrom(type))
				return NotFound();

			if (!(HttpContext.Items[nameof(QueryContextProviderSelectorAttribute)] is IQueryContextProvider provider))
				return InternalServerError(ErrorEvents.PlatformError, "Did not correctly resolve query context provider");

			var contexts = provider.Parse(type, User, Request.GetEncodedPathAndQuery());
			var results = new List<object>();
			foreach (var context in contexts)
				results.Add(await context.GetAsync(_repository));
			return provider.ToResult(results);
		}

		#endregion
	}
}