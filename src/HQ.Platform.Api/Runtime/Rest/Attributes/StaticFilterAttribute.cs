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
using System.Net;
using ActiveErrors;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Runtime;
using HQ.Platform.Api.Runtime.Rest.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Runtime.Rest.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     An adapter for running runtime filters in a static context.
	/// </summary>
	public abstract class StaticFilterAttribute : ActionFilterAttribute
	{
		internal static void Execute<T>(ActionExecutingContext context, Func<T, string> getOperator,
			Func<QueryContext, IQueryValidator> getValidator)
			where T : IRestFilter
		{
			Execute(context, getOperator, getValidator,
				QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value),
				new QueryContext(context.HttpContext.User), true);
		}

		internal static void Execute<T>(ActionExecutingContext context, Func<T, StringValues> getOperators,
			Func<QueryContext, IQueryValidator> getValidator)
			where T : IRestFilter
		{
			Execute(context, getOperators, getValidator,
				QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value),
				new QueryContext(context.HttpContext.User), true);
		}

		internal static void Execute<T>(ActionExecutingContext context, Func<T, StringValues> getOperators,
			Func<QueryContext, IQueryValidator> getValidator, IDictionary<string, StringValues> qs, QueryContext qc,
			bool isolated = false) where T : IRestFilter
		{
			var filter = context.HttpContext.RequestServices.GetRequiredService<T>();
			var fields = getOperators(filter);
			foreach (var field in fields)
			{
				if (!context.ActionArguments.ContainsKey(field))
				{
					return;
				}
			}

			filter.Execute(qs, ref qc);

			if (isolated && qc.Errors?.Count > 0)
			{
				context.Result = new ErrorObjectResult(new Error(ErrorEvents.ValidationFailed,
					ErrorStrings.ValidationFailed, (HttpStatusCode) 422, qc.Errors));
			}

			foreach (var field in fields)
			{
				context.ActionArguments[field] = getValidator(qc);
			}
		}

		internal static void Execute<T>(ActionExecutingContext context, Func<T, string> getOperator,
			Func<QueryContext, IQueryValidator> getValidator, IDictionary<string, StringValues> qs, QueryContext qc,
			bool isolated = false) where T : IRestFilter
		{
			var filter = context.HttpContext.RequestServices.GetRequiredService<T>();
			var field = getOperator(filter);
			if (!context.ActionArguments.ContainsKey(field))
			{
				return;
			}

			filter.Execute(qs, ref qc);

			if (isolated && qc.Errors?.Count > 0)
			{
				context.Result = new ErrorObjectResult(new Error(ErrorEvents.ValidationFailed,
					ErrorStrings.ValidationFailed, (HttpStatusCode) 422, qc.Errors));
			}

			context.ActionArguments[field] = getValidator(qc);
		}
	}
}