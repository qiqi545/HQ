// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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
                    settings.Data = new InvokeUiData();

                Pools.AutoResolver = new NoContainer(r, settings.ComponentAssemblies);

                return settings;
            });
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().DefaultSystem);
			services.AddSingleton(r => r.GetRequiredService<UiSettings>().DefaultSystem);
            services.AddSingleton(r => r.GetRequiredService<UiSettings>().Data);

            services.AddSingleton(r => new LayoutRoot());

            services.AddSingleton(ResolveComponentTypes);
			services.AddSingleton(RegisterComponentsByName);
            services.AddSingleton(RegisterComponentsByType);

            services.AddSingleton(ResolveViewComponentTypes);
            services.AddSingleton(RegisterViewComponents);

			ConfigureServices?.Invoke(services);
        }

        private static Dictionary<Type, Func<UiComponent>> RegisterComponentsByType(IServiceProvider r)
        {
	        var settings = r.GetRequiredService<UiSettings>();
	        var componentTypes = r.GetRequiredService<ComponentTypes>();
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
			var componentTypes = r.GetRequiredService<ComponentTypes>();
			var autoResolver = new NoContainer(r, settings.ComponentAssemblies);
			var byName = componentTypes
		        .Select(x => new { Key = x.Name, Value = new Func<UiComponent>(() => autoResolver.GetService(x) as UiComponent ?? Instancing.CreateInstance<UiComponent>()) })
		        .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
			return byName;
        }

        private static Dictionary<Type, Func<IViewComponent>> RegisterViewComponents(IServiceProvider r)
        {
	        var settings = r.GetRequiredService<UiSettings>();
	        var componentTypes = r.GetRequiredService<ViewComponentTypes>();
	        var autoResolver = new NoContainer(r, settings.ComponentAssemblies);

	        var lookup = new Dictionary<Type, Func<IViewComponent>>();
	        foreach (var componentType in componentTypes)
	        {
		        foreach (var contract in componentType.GetTypeInfo().ImplementedInterfaces)
		        {
			        if (!contract.ImplementsGeneric(typeof(IViewComponent<>)))
				        continue;

			        var valueType = contract.GetGenericArguments()[0];
			        lookup[valueType] = () => autoResolver.GetService(contract) as IViewComponent;
		        }
	        }
	        return lookup;
        }

		private class ComponentTypes : IEnumerable<Type>
        {
	        private readonly IEnumerable<Type> _types;

	        public ComponentTypes(IEnumerable<Type> types)
	        {
		        _types = types;
	        }

	        public IEnumerator<Type> GetEnumerator()
	        {
		        return _types.GetEnumerator();
	        }

	        IEnumerator IEnumerable.GetEnumerator()
	        {
		        return GetEnumerator();
	        }
        }

        private static ComponentTypes ResolveComponentTypes(IServiceProvider r)
        {
            var settings = r.GetRequiredService<UiSettings>();
            var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
            var componentTypes = exportedTypes.Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent))).Distinct();
            return new ComponentTypes(componentTypes);
        }

        private class ViewComponentTypes : IEnumerable<Type>
        {
	        private readonly IEnumerable<Type> _types;

	        public ViewComponentTypes(IEnumerable<Type> types)
	        {
		        _types = types;
	        }

	        public IEnumerator<Type> GetEnumerator()
	        {
		        return _types.GetEnumerator();
	        }

	        IEnumerator IEnumerable.GetEnumerator()
	        {
		        return GetEnumerator();
	        }
        }

        private static ViewComponentTypes ResolveViewComponentTypes(IServiceProvider r)
        {
	        var settings = r.GetRequiredService<UiSettings>();
	        var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
	        var componentTypes = exportedTypes.Where(x => !x.IsInterface && !x.IsAbstract && typeof(IViewComponent).IsAssignableFrom(x)).Distinct();
	        return new ViewComponentTypes(componentTypes);
        }
	}
}