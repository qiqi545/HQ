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

using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TypeKitchen;

namespace HQ.Data.Contracts.Runtime.Mvc
{
	[DynamicController]
	[DynamicAuthorize(typeof(RuntimeOptions))]
	[Route("objects")]
	[Produces(Constants.MediaTypes.Json, Constants.MediaTypes.Xml)]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Objects", "Provides programmatic access to API resources over HTTP.")]
	public class RuntimeController : DataController
	{
		private readonly ITypeResolver _typeResolver;
		private readonly IObjectGetRepository _repository;

		public RuntimeController(ITypeResolver typeResolver, IObjectGetRepository repository)
		{
			_typeResolver = typeResolver;
			_repository = repository;
		}

		[HttpGet("{objectType}")]
		[RuntimeGetActionMethodSelector]
		public async Task<IActionResult> GetAsync([FromRoute, BindRequired] string objectType)
		{
			if (HttpContext.Items[nameof(RuntimeGetActionMethodSelectorAttribute)] is IQueryContextProvider provider)
			{
				var type = _typeResolver.FindFirstByName(objectType); // FIXME: pluralize
				var contexts = provider.Parse(type, User, Request.GetEncodedPathAndQuery());
				foreach (var context in contexts)
				{
					var slice = await context.ExecuteAsync(_repository);
					if (slice?.Data?.Count == 0)
						return NotFound();
					//Response.MaybeEnvelope(Request, _apiOptions.Value, _queryOptions.Value, slice?.Data, slice?.Errors, out var body);
					return Ok(slice?.Data);
				}
			}
			else
			{
				return InternalServerError(ErrorEvents.PlatformError,
					"Did not correctly resolve query context provider");
			}

			return NotImplemented();
		}
	}
}