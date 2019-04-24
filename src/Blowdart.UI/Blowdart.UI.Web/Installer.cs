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
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blowdart.UI.Web
{
	public static class Installer
    {
        internal static CompositeFileProvider ProviderContainer;

        public static IServiceCollection AddBlowdartUi(this IServiceCollection services, IHostingEnvironment env, params Assembly[] uiAssemblies)
        {
            AddUiResources(services, env, uiAssemblies);

            services.AddHttpContextAccessor();
            services.AddSignalR(o => { });

            UiConfig.Initialize<HtmlSystem>(services);
            UiConfig.ConfigureServices?.Invoke(services);

            return services;
        }

        internal static IServiceCollection AddUiResources(this IServiceCollection services, params Assembly[] uiAssemblies)
        {
            return AddUiResources(services, null, uiAssemblies);
        }

        internal static IServiceCollection AddUiResources(this IServiceCollection services, IHostingEnvironment env, params Assembly[] uiAssemblies)
        {
            ManifestEmbeddedFileProvider RegisterUiAssembly(Assembly a)
            {
                var prefix = a.GetName().Name;
                Trace.TraceInformation($"Blowdart: Scanning {a.GetName().Name} for UI resources.");
                foreach (var name in a.GetManifestResourceNames())
                    Trace.TraceInformation($"\t{name.Replace(prefix, "")}");
                return new ManifestEmbeddedFileProvider(a);
            }

            var fileProviders = new List<IFileProvider>();

            if (env != null)
                fileProviders.Add(env.ContentRootFileProvider);

            if (ProviderContainer == null)
                fileProviders.Add(RegisterUiAssembly(typeof(UiServer).Assembly));
            
            fileProviders.AddRange(uiAssemblies.Select(RegisterUiAssembly));
            
            if (ProviderContainer == null)
            {
                var composite = new CompositeFileProvider(fileProviders);
                services.AddSingleton<IFileProvider>(composite);
                ProviderContainer = composite;
            }
            else
            {
                var merge = new List<IFileProvider>(ProviderContainer.FileProviders);
                merge.AddRange(fileProviders);

                var composite = new CompositeFileProvider(merge);
                services.Replace(ServiceDescriptor.Singleton<IFileProvider>(composite));
                ProviderContainer = composite;
            }

            return services;
        }

        public static void UseBlowdartUi(this IApplicationBuilder app, Action<LayoutRoot> layout)
        {
            var serviceProvider = app.ApplicationServices;
            var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            if (options.Value.UseServerSideRendering)
                loggerFactory?.AddProvider(new ServerSideLoggerProvider(serviceProvider.GetRequiredService<IHubContext<LoggingHub>>()));

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
                void SetMessagingModel(HttpConnectionDispatcherOptions o)
                {
                    o.Transports = (HttpTransportType) options.Value.MessagingModel;
                }
                r.MapHub<HtmlHub>(options.Value.HubPath, SetMessagingModel);
                if(options.Value.UseServerSideLogging)
                    r.MapHub<LoggingHub>(options.Value.LoggingPath, SetMessagingModel);
            });

            var template = ServerSideRenderer.LoadPageTemplate(serviceProvider, options);
            layout?.Invoke(serviceProvider.GetRequiredService<LayoutRoot>());

            app.Use(async (context, next) =>
            {
                await ServerSideRenderer.BuildUi(serviceProvider.GetRequiredService<LayoutRoot>(), template, context, next);
            });
        }
    }
}