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

using HQ.Platform.Runtime.Rest.Attributes;
using HQ.Platform.Runtime.Rest.Filters;
using HQ.Platform.Runtime.Rest.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Runtime.Rest
{
    public static class Add
    {
        public static void AddRestRuntime(this IServiceCollection services)
        {
            services.AddSingleton<RestFieldsFilter>();
            services.AddSingleton<RestFilterFilter>();
            services.AddSingleton<RestPageFilter>();
            services.AddSingleton<RestStreamFilter>();
            services.AddSingleton<RestProjectionFilter>();
            services.AddSingleton<RestSearchFilter>();
            services.AddSingleton<RestSortFilter>();
            services.AddSingleton<ChildResourceFilterAttribute>();

            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestFieldsFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestFilterFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestPageFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestStreamFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestProjectionFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestSearchFilter>());
            services.AddSingleton<IRestFilter>(r => r.GetRequiredService<RestSortFilter>());
        }
    }
}
