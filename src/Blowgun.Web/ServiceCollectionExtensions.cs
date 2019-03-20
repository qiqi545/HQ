// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Blowgun.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlowgun(this IServiceCollection services)
        {
            services.AddSingleton<Elements>(r => new HtmlElements(r));

            return services;
        }
    }
}