// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Blowgun.Web.SemanticUi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlowgun(this IServiceCollection services)
        {
            services.AddTransient<Elements>(r => r.GetRequiredService<SemanticUiElements>());
            services.AddTransient<HtmlElements>(r => r.GetRequiredService<SemanticUiElements>());
            services.AddSingleton<SemanticUiElements>(r => new SemanticUiElements(r));
            return services;
        }
    }
}