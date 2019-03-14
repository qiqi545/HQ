// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blowdart.UI
{
    public class UiConfig
    {
        public static Action<UiSettings> Settings = null;
        public static Action<IServiceCollection> ConfigureServices = null;

        public static void Initialize<TSystem>(IServiceCollection services) where TSystem : UiSystem
        {
            services.AddSingleton(r =>
            {
                var settings = new UiSettings(r);
                Settings?.Invoke(settings);
                if (settings.System == null)
                    settings.System = Activator.CreateInstance<TSystem>();
                return settings;
            });
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().System);
            services.AddSingleton(r => new LayoutRoot(r));
            services.AddSingleton(r =>
            {
                var settings = r.GetRequiredService<UiSettings>();

                var assemblies = settings.ComponentAssemblies ?? new[] {typeof(UiConfig).Assembly};
                var exportedTypes = assemblies.SelectMany(x => x.GetExportedTypes());
                var componentTypes = exportedTypes
                    .Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent)));

                var components = componentTypes
                    .Select(x => (UiComponent) Activator.CreateInstance(x))
                    .ToDictionary(k => k.Name ?? k.GetType().Name, StringComparer.OrdinalIgnoreCase);

                return components;
            });

            ConfigureServices?.Invoke(services);
        }
    }
}