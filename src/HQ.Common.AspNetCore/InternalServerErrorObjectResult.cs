// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HQ.Common.AspNetCore
{
	/// <summary>
	/// An <see cref="ObjectResult" /> that when executed will produce an Internal Server Error (500) response.
	/// </summary>
	public class InternalServerErrorObjectResult : ObjectResult
	{
		/// <summary>
		/// Creates a new <see cref="InternalServerErrorObjectResult" /> instance.
		/// </summary>
		/// <param name="error">Contains the errors to be returned to the client.</param>
		public InternalServerErrorObjectResult(object error)
			: base(error)
		{
			StatusCode = (int)HttpStatusCode.InternalServerError;
		}

		/// <summary>
		/// Creates a new <see cref="InternalServerErrorObjectResult" /> instance.
		/// </summary>
		/// <param name="modelState"><see cref="ModelStateDictionary" /> containing the validation errors.</param>
		public InternalServerErrorObjectResult(ModelStateDictionary modelState)
			: base(new SerializableError(modelState))
		{
			if (modelState == null)
				throw new ArgumentNullException(nameof(modelState));
			StatusCode = (int)HttpStatusCode.InternalServerError;
		}
	}
}