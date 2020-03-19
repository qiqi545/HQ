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
using System.Net;
using ActiveErrors;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Data.Contracts.Mvc
{
	public static class OperationExtensions
	{
		public static IActionResult ToResult(this Operation operation)
		{
			switch (operation.Result)
			{
				case OperationResult.Succeeded:
					return new OkResult();
				case OperationResult.Refused:
					return new ForbidResult();

				case OperationResult.Error:
				case OperationResult.SucceededWithErrors:
				{
					var errors = operation.Errors.Count > 1
						? new Error(ErrorEvents.AggregateErrors, ErrorStrings.AggregateErrors,
							HttpStatusCode.InternalServerError, operation.Errors)
						: operation.Errors[0];

					var error = new ErrorObjectResult(errors);
					if (operation.Succeeded)
						error.StatusCode = 200;

					return error;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static IActionResult ToResult<T>(this Operation<T> operation,
			Func<object, IActionResult> inner = null,
			HttpStatusCode forbidStatusCode = HttpStatusCode.Forbidden)
		{
			switch (operation.Result)
			{
				case OperationResult.Refused:
					switch (forbidStatusCode)
					{
						case HttpStatusCode.Unauthorized:
							return new UnauthorizedObjectResult(AggregateErrors(operation));
					}

					return new ForbidResult( /* Forbid does not permit a body */);
				case OperationResult.Succeeded:
					return inner == null ? new OkObjectResult(operation.Data) : inner.Invoke(operation.Data);
			}

			if (operation.Data == null)
				return new NotFoundObjectResult(new ErrorObjectResult(
					new Error(ErrorEvents.ResourceMissing, "Resource not found.", HttpStatusCode.NotFound,
						operation.Errors)
				));

			var error = AggregateErrors(operation);

			if (operation.Result == OperationResult.Error) return new ErrorObjectResult(error);

			return inner != null ? inner(new {operation.Data, Errors = error}) :
				operation.Succeeded ? new ErrorAndObjectResult<T>(operation.Data, error) : new ErrorObjectResult(error);
		}
		
		private static Error AggregateErrors(Operation operation)
		{
			var error = operation.HasErrors
				? new Error(ErrorEvents.AggregateErrors, ErrorStrings.AggregateErrors,
					HttpStatusCode.InternalServerError, operation.Errors)
				: operation.Errors[0];
			return error;
		}
	}
}