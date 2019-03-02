// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Blowdart.UI.Web.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Blowdart.UI.Web
{
    public static class Installer
    {
        public static IServiceCollection AddBlowdartUi(this IServiceCollection services, IHostingEnvironment env,
            params Assembly[] uiAssemblies)
        {
            var fileProviders = new List<IFileProvider>
            {
                env.ContentRootFileProvider,
                RegisterUiAssembly(typeof(UiServer).Assembly)
            };

            fileProviders.AddRange(uiAssemblies.Select(RegisterUiAssembly));

            var composite = new CompositeFileProvider(fileProviders);
            services.AddSingleton<IFileProvider>(composite);
            services.AddSignalR(o => { });

            UiConfig.Initialize<HtmlSystem>(services);
            UiConfig.ConfigureServices?.Invoke(services);

            return services;

            ManifestEmbeddedFileProvider RegisterUiAssembly(Assembly a)
            {
                var prefix = a.GetName().Name;
                Trace.TraceInformation($"Blowdart: Scanning {a.GetName().Name} for UI resources.");
                foreach (var name in a.GetManifestResourceNames())
                    Trace.TraceInformation($"\t{name.Replace(prefix, "")}");
                return new ManifestEmbeddedFileProvider(a);
            }
        }

        public static void UseBlowdartUi(this IApplicationBuilder app, Action<LayoutRoot> layout)
        {
            var serviceProvider = app.ApplicationServices;
            var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();

            app.UseStaticFiles();
            app.Map("/~", x =>
            {
                x.UseStaticFiles(new StaticFileOptions(new SharedOptions
                {
                    FileProvider = x.ApplicationServices.GetRequiredService<IFileProvider>()
                }));
            });
            app.UseSignalR(r =>
            {
                r.MapHub<HtmlHub>(options.Value.HubPath,
                    o => { o.Transports = (HttpTransportType) options.Value.MessagingModel; });
            });
            var template = ServerSideRenderer.LoadPageTemplate(serviceProvider, options);
            layout?.Invoke(serviceProvider.GetRequiredService<LayoutRoot>());

            app.Use(async (context, next) =>
            {
                await ServerSideRenderer.BuildUi(serviceProvider.GetRequiredService<LayoutRoot>(), template,
                    context, next);
            });
        }
    }
}