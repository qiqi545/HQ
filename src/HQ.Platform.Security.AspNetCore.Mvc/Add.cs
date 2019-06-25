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

using System.Collections.Generic;
using System.Reflection;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Security.AspnetCore.Mvc.Controllers;
using HQ.Platform.Security.AspNetCore.Mvc.Configuration;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Security.AspNetCore.Mvc
{
    public static class Add
    {
        public static IMvcBuilder AddSuperUserTokenController(this IMvcBuilder mvc)
        {
            var services = mvc.Services;

            var typeInfo = new List<TypeInfo>
            {
                typeof(SuperUserTokenController).GetTypeInfo(),
            };

            mvc.ConfigureApplicationPartManager(x =>
            {
                x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
            });
            
            services.AddSingleton(r =>
            {
                var o = r.GetRequiredService<IOptions<SecurityOptions>>();
                return new SuperUserComponent
                {
                    Namespace = () => o.Value.Tokens?.Path ?? string.Empty
                };
            });

            services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureSuperUserController>();
            return mvc;
        }
    }
}
