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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Common.AspNetCore.Mvc
{
	public static class Add
	{
		public static IMvcBuilder AddComponentFeature<TComponent, TComponentOptions>(this IMvcBuilder mvcBuilder) 
			where TComponent : IDynamicComponent
			where TComponentOptions : IComponentOptions
		{
			mvcBuilder.Services.AddSingleton<IDynamicComponent>(r =>
			{
				var instance = Activator.CreateInstance<TComponent>();
				instance.RouteTemplate = () =>
				{
					var o = r.GetRequiredService<IOptionsMonitor<TComponentOptions>>();
					return o.CurrentValue.RootPath ?? string.Empty;
				};
				return instance;
			});
			return mvcBuilder;
		}

		public static IServiceCollection AddLocalTimestamps(this IServiceCollection services)
		{
			services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();
			return services;
		}

		public static IServiceCollection AddTypeDiscovery(this IServiceCollection services)
		{
			services.TryAddSingleton<ITypeResolver>(r => new ReflectionTypeResolver(AppDomain.CurrentDomain.GetAssemblies()));
			services.TryAddSingleton<ITypeRegistry, TypeRegistry>();
			return services;
		}
	}
}
