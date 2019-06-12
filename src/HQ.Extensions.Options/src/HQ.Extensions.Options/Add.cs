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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Options
{
    public static class Add
    {
        public static IServiceCollection AddValidOptions(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IValidOptions<>), typeof(ValidOptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IValidOptionsSnapshot<>), typeof(ValidOptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IValidOptionsMonitor<>), typeof(ValidOptionsMonitor<>)));
            return services;
        }

        public static IServiceCollection AddSaveOptions(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ISaveOptions<>), typeof(SaveOptionsManager<>)));
            return services;
        }
    }
}