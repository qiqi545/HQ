// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blowdart.UI.Internal;
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

                if (settings.ComponentAssemblies == null)
                    settings.AutoRegisterComponentAssemblies();

                if (settings.DefaultSystem == null)
                    settings.DefaultSystem = Caches.ActivatorCache.Create<TSystem>();

                if (settings.Data == null)
                    settings.Data = new InvokeUiData(r);

                Pools.AutoResolver = new NoContainer(r, settings.ComponentAssemblies);

                return settings;
            });
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().DefaultSystem);
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().Data);
            services.AddSingleton(r => new LayoutRoot(r));
            services.AddSingleton(r =>
            {
                var settings = r.GetRequiredService<UiSettings>();
                var componentTypes = ResolveComponentTypes(r);
                var autoResolver = new NoContainer(r, settings.ComponentAssemblies);
                var byName = componentTypes
                    .Select(x => autoResolver.GetService(x) as UiComponent ?? Caches.ActivatorCache.Create<UiComponent>())
                    .ToDictionary(k => k.Name ?? k.GetType().Name, StringComparer.OrdinalIgnoreCase);

                return byName;
            });
            services.AddSingleton(r =>
            {
                var settings = r.GetRequiredService<UiSettings>();
                var componentTypes = ResolveComponentTypes(r);
                var autoResolver = new NoContainer(r, settings.ComponentAssemblies);
                var byType = componentTypes.ToDictionary(k => k, v =>
                {
                    return new Func<UiComponent>(() => autoResolver.GetService(v) as UiComponent ?? Caches.ActivatorCache.Create<UiComponent>());
                });
                return byType;
            });

            ConfigureServices?.Invoke(services);
        }

        private static IEnumerable<Type> ResolveComponentTypes(IServiceProvider r)
        {
            var settings = r.GetRequiredService<UiSettings>();
            var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
            var componentTypes = exportedTypes
                .Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent)));
            return componentTypes;
        }
    }
}