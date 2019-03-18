﻿// Copyright (c) Blowdart, Inc. All rights reserved.
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
                if (settings.System == null)
                    settings.System = Activator.CreateInstance<TSystem>();
                if(settings.Data == null)
                    settings.Data = new InvokeUiData(r);
                return settings;
            });
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().System);
            services.AddSingleton(r => new LayoutRoot(r));
            services.AddSingleton(r =>
            {
                var componentTypes = ResolveComponentTypes(r);

                var byName = componentTypes
                    .Select(x => (UiComponent) Activator.CreateInstance(x))
                    .ToDictionary(k => k.Name ?? k.GetType().Name, StringComparer.OrdinalIgnoreCase);

                return byName;
            });
            services.AddSingleton(r =>
            {
                var componentTypes = ResolveComponentTypes(r);

                var byType = componentTypes
                    .ToDictionary(k => k, v => (UiComponent) Activator.CreateInstance(v));

                return byType;
            });

            ConfigureServices?.Invoke(services);
        }

        private static IEnumerable<Type> ResolveComponentTypes(IServiceProvider r)
        {
            var settings = r.GetRequiredService<UiSettings>();
            if (settings.ComponentAssemblies == null)
                settings.AutoRegisterComponentAssemblies();
            var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
            var componentTypes = exportedTypes
                .Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent)));
            return componentTypes;
        }
    }
}