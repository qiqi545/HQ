// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blowdart.UI.Internal;
using Microsoft.Extensions.DependencyInjection;
using TypeKitchen;

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
                    settings.DefaultSystem = Instancing.CreateInstance<TSystem>();

                if (settings.Data == null)
                    settings.Data = new InvokeUiData(r);

                Pools.AutoResolver = new NoContainer(r, settings.ComponentAssemblies);

                return settings;
            });
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().DefaultSystem);
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().Data);
            services.AddSingleton(r => new LayoutRoot(r));
            services.AddSingleton(RegisterComponentsByName);
            services.AddSingleton(RegisterComponentsByType);

            ConfigureServices?.Invoke(services);
        }

        private static Dictionary<Type, Func<UiComponent>> RegisterComponentsByType(IServiceProvider r)
        {
	        var settings = r.GetRequiredService<UiSettings>();
	        var componentTypes = ResolveComponentTypes(r);
	        var autoResolver = new NoContainer(r, settings.ComponentAssemblies);
	        var byType = componentTypes.ToDictionary(k => k, v =>
	        {
		        return new Func<UiComponent>(() => autoResolver.GetService(v) as UiComponent ?? Instancing.CreateInstance<UiComponent>());
	        });
	        return byType;
        }

        private static Dictionary<string, Func<UiComponent>> RegisterComponentsByName(IServiceProvider r)
        {
	        var settings = r.GetRequiredService<UiSettings>();
	        var componentTypes = ResolveComponentTypes(r);
	        var autoResolver = new NoContainer(r, settings.ComponentAssemblies);
			var byName = componentTypes
		        .Select(x => new { Key = x.Name, Value = new Func<UiComponent>(() => autoResolver.GetService(x) as UiComponent ?? Instancing.CreateInstance<UiComponent>()) })
		        .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
			return byName;
        }

        private static IEnumerable<Type> ResolveComponentTypes(IServiceProvider r)
        {
            var settings = r.GetRequiredService<UiSettings>();
            var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
            var componentTypes = exportedTypes.Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent)));
            return componentTypes;
        }
    }
}