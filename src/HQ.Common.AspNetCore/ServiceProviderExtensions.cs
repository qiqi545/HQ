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
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HQ.Common.AspNetCore
{
    public static class ServiceProviderExtensions
    {
        public static bool TryGetRequestAbortCancellationToken(this IServiceProvider services,
            out CancellationToken cancelToken)
        {
            cancelToken = CancellationToken.None;
            var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var token = accessor?.HttpContext?.RequestAborted;
            if (!token.HasValue)
            {
                return false;
            }

            cancelToken = token.Value;
            return true;
        }

        public static bool TryBindOptions(this IServiceProvider serviceProvider, Type optionsWrapperType, out object options)
        {
            // IOptions<T>
            var resolved = optionsWrapperType.GetGenericArguments()[0];
            while (resolved != null && resolved.IsGenericType)
            {
                resolved = resolved.IsGenericTypeDefinition
                    ? resolved.MakeGenericType(optionsWrapperType.GetGenericArguments())    // IOptions<TService<T1,...TN>>
                    : resolved.BaseType;                                                    // HubOptions<THub> -> HubOptions
            }
            resolved = typeof(IOptions<>).MakeGenericType(resolved);

            try
            {
                var instance = serviceProvider.GetService(resolved);
                var property = resolved.GetProperty(nameof(IOptions<object>.Value));
                options = property?.GetValue(instance);
                return options != null;
            }
            catch (Exception e)
            {
                options = new
                {
                    Type = GetInnerGenericTypeName(optionsWrapperType),
                    ErrorType = e.GetType().Name,
                    e.Message,
                    e.StackTrace
                };
                return false;
            }
        }

        public static string GetInnerGenericTypeName(this Type optionsWrapperType)
        {
            var declaringMethod = optionsWrapperType.DeclaringMethod;

            return optionsWrapperType.IsGenericParameter && declaringMethod != null && declaringMethod.IsGenericMethod ?
                declaringMethod.IsGenericMethod ? declaringMethod.GetGenericArguments()[0].Name :
                declaringMethod.Name
                : optionsWrapperType.IsGenericType
                    ? optionsWrapperType.GetGenericArguments()[0].Name
                    : optionsWrapperType.Name;
        }
    }
}
