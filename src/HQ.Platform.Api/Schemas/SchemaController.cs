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

using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Extensions.Caching.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Platform.Api.Schemas
{
	[DynamicController(typeof(SchemaOptions))]
	[DynamicAuthorize(typeof(SchemaOptions))]
	[Route("schemas")]
	[Produces(Constants.MediaTypes.Json, Constants.MediaTypes.Xml)]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Objects", "Provides programmatic access to weak-typed object schemas.")]
	[ServiceFilter(typeof(HttpCacheFilterAttribute))]
	public class SchemaController : DataController
	{

	}
}