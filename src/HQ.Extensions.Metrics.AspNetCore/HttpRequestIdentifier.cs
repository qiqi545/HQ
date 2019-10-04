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

using System.Net;
using HQ.Common.AspNetCore;
using HQ.Extensions.Metrics.SplitTesting;
using Microsoft.AspNetCore.Http;

namespace HQ.Extensions.Metrics.AspNetCore
{
	/// <summary>
	///     A default identity that is based on the user first, if they are authenticated,
	///     and then the request's anonymous ID. If both of these identity methods fail,
	///     then the inbound IP address is used.
	/// </summary>
	public class HttpRequestIdentifier : ICohortIdentifier
	{
		private readonly IHttpContextAccessor _accessor;

		public HttpRequestIdentifier(IHttpContextAccessor accessor) => _accessor = accessor;

		public string Identify()
		{
			var context = _accessor.HttpContext;
			if (context == null)
				return null;

			var request = context.Request;

			var identity = context.User != null && context.User.Identity.IsAuthenticated
				? context.User.Identity.Name
				: context.Items.TryGetValue(Common.Constants.ContextKeys.AnonymousUserId, out var anonymousId)
					? anonymousId.ToString()
					: null;

			if (identity == null)
			{
				return request.IsLocal()
					? IPAddress.Loopback.ToString()
					: (context.Connection.RemoteIpAddress ?? IPAddress.Loopback).ToString();
			}

			return identity;
		}
	}
}