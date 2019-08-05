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

using HQ.Data.Contracts.Runtime;
using HQ.Platform.Runtime.Rest.Attributes;
using HQ.Platform.Runtime.Rest.Filters;
using HQ.Platform.Runtime.Rest.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Platform.Runtime.Rest
{
    public static class Add
    {
        public static void AddRestRuntime(this IServiceCollection services)
        {
	        services.TryAddEnumerable(ServiceDescriptor.Singleton<IQueryContextProvider, RestQueryContextProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMutationContextProvider, RestMutationContextProvider>());
            
            services.TryAddSingleton<RestFieldsFilter>();
            services.TryAddSingleton<RestFilterFilter>();
            services.TryAddSingleton<RestPageFilter>();
            services.TryAddSingleton<RestStreamFilter>();
            services.TryAddSingleton<RestProjectionFilter>();
            services.TryAddSingleton<RestSearchFilter>();
            services.TryAddSingleton<RestSortFilter>();
            services.TryAddSingleton<ChildResourceFilterAttribute>();

            TryAddFilter<RestFieldsFilter>(services);
            TryAddFilter<RestFilterFilter>(services);
            TryAddFilter<RestPageFilter>(services);
            TryAddFilter<RestStreamFilter>(services);
            TryAddFilter<RestProjectionFilter>(services);
            TryAddFilter<RestSearchFilter>(services);
            TryAddFilter<RestSortFilter>(services);
        }

        private static void TryAddFilter<TFilter>(IServiceCollection services) where TFilter : class, IRestFilter
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IRestFilter, TFilter>(r => r.GetRequiredService<TFilter>()));
        }
	}
}
