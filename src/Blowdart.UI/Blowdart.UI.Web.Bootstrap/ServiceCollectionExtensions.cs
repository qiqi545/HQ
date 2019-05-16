// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Blowdart.UI.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Blowdart.UI.Web.Bootstrap
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBootstrap(this IServiceCollection services)
        {
            services.AddSingleton(r => UiTheme.Default);

            return services.AddUiResources(typeof(Bootstrap).Assembly);
        }
    }
}