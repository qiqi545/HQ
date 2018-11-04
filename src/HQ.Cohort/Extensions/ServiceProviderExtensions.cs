// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace HQ.Cohort.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static void TryGetRequestAbortCancellationToken(this IServiceProvider services,
			out CancellationToken cancelToken)
		{
			cancelToken = CancellationToken.None;
			var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
			var token = accessor?.HttpContext?.RequestAborted;
			if (!token.HasValue)
				return;
			cancelToken = token.Value;
		}
	}
}