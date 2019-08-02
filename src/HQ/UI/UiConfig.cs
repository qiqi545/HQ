#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HQ.UI.Internal;
using Microsoft.Extensions.DependencyInjection;
using TypeKitchen;

namespace HQ.UI
{
	public class UiConfig
	{
		public static Action<UiSettings> Settings = null;
		public static Action<IServiceCollection> ConfigureServices = null;

		public static void Initialize<TSystem>(IServiceCollection services) where TSystem : UiSystem
		{
			services.AddSingleton(r =>
			{
				var settings = new UiSettings();
				Settings?.Invoke(settings);

				if (settings.ComponentAssemblies == null)
					settings.AutoRegisterComponentAssemblies();

				if (settings.DefaultSystem == null)
					settings.DefaultSystem = Instancing.CreateInstance<TSystem>();

				if (settings.Data == null)
					settings.Data = new InvokeUiData();

				Pools.AutoResolver = new NoContainer(settings.ComponentAssemblies);

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

		private static Dictionary<Type, Func<Type, UiComponent>> RegisterComponentsByType(IServiceProvider r)
		{
			var settings = r.GetRequiredService<UiSettings>();
			var componentTypes = r.GetRequiredService<ComponentTypes>();
			var autoResolver = new NoContainer(settings.ComponentAssemblies);
			var byType = componentTypes.ToDictionary(k => k, v =>
			{
				return new Func<Type, UiComponent>(c => autoResolver.GetService(c ?? v) as UiComponent ??
				                                        Instancing.CreateInstance<UiComponent>());
			});
			return byType;
		}

		private static Dictionary<string, Func<UiComponent>> RegisterComponentsByName(IServiceProvider r)
		{
			var settings = r.GetRequiredService<UiSettings>();
			var componentTypes = r.GetRequiredService<ComponentTypes>();
			var autoResolver = new NoContainer(settings.ComponentAssemblies);
			var byName = componentTypes
				.Select(x => new
				{
					Key = x.Name,
					Value = new Func<UiComponent>(() =>
						autoResolver.GetService(x) as UiComponent ?? Instancing.CreateInstance<UiComponent>())
				})
				.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
			return byName;
		}

		private static Dictionary<Type, Func<IViewComponent>> RegisterViewComponents(IServiceProvider r)
		{
			var settings = r.GetRequiredService<UiSettings>();
			var componentTypes = r.GetRequiredService<ViewComponentTypes>();
			var autoResolver = new NoContainer(settings.ComponentAssemblies);

			var lookup = new Dictionary<Type, Func<IViewComponent>>();
			foreach (var componentType in componentTypes)
			foreach (var contract in componentType.GetTypeInfo().ImplementedInterfaces)
			{
				if (!contract.ImplementsGeneric(typeof(IViewComponent<>)))
					continue;

				var valueType = contract.GetGenericArguments()[0];
				lookup[valueType] = () => autoResolver.GetService(contract) as IViewComponent;
			}

			return lookup;
		}

		private static ComponentTypes ResolveComponentTypes(IServiceProvider r)
		{
			var settings = r.GetRequiredService<UiSettings>();
			var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
			var componentTypes = exportedTypes
				.Where(x => !x.IsAbstract && x.GetTypeInfo().IsSubclassOf(typeof(UiComponent))).Distinct();
			return new ComponentTypes(componentTypes);
		}

		private static ViewComponentTypes ResolveViewComponentTypes(IServiceProvider r)
		{
			var settings = r.GetRequiredService<UiSettings>();
			var exportedTypes = settings.ComponentAssemblies.SelectMany(x => x.GetExportedTypes());
			var componentTypes = exportedTypes
				.Where(x => !x.IsInterface && !x.IsAbstract && typeof(IViewComponent).IsAssignableFrom(x)).Distinct();
			return new ViewComponentTypes(componentTypes);
		}

		private class ComponentTypes : IEnumerable<Type>
		{
			private readonly IEnumerable<Type> _types;

			public ComponentTypes(IEnumerable<Type> types) => _types = types;

			public IEnumerator<Type> GetEnumerator()
			{
				return _types.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class ViewComponentTypes : IEnumerable<Type>
		{
			private readonly IEnumerable<Type> _types;

			public ViewComponentTypes(IEnumerable<Type> types) => _types = types;

			public IEnumerator<Type> GetEnumerator()
			{
				return _types.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}