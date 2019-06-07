using System.Net;
using HQ.Common.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace HQ.Extensions.Metrics.SplitTesting
{
    /// <summary>
    /// A default identity that is based on the user first, if they are authenticated,
    /// and then the request's anonymous ID. If both of these identity methods fail,
    /// then the inbound IP address is used.
    /// </summary>

    public class HttpRequestIdentifier : ICohortIdentifier
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpRequestIdentifier(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

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
