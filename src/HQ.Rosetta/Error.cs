// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.Net;

namespace HQ.Rosetta
{
	public class Error
	{
		public Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
			params Error[] errors)
		{
			Message = message;
			StatusCode = statusCode;
			Errors = errors;
		}

		public Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
			IEnumerable<Error> errors = null)
		{
			Message = message;
			StatusCode = statusCode;
			Errors = errors;
		}

		public string Message { get; internal set; }
		public HttpStatusCode StatusCode { get; }
		public IEnumerable<Error> Errors { get; }
	}
}