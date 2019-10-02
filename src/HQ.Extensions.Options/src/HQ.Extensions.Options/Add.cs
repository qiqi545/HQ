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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options
{
    public static class Add
    {
        public static IServiceCollection AddValidOptions(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IValidOptions<>), typeof(ValidOptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IValidOptionsSnapshot<>), typeof(ValidOptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IValidOptionsMonitor<>), typeof(ValidOptionsMonitor<>)));
            return services;
        }

        public static IServiceCollection AddSaveOptions(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ISaveOptions<>), typeof(SaveOptionsManager<>)));
            return services;
        }

		public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfiguration config) where TOptions : class
			=> services.Configure<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, config);

		public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, IConfiguration config) where TOptions : class
			=> services.Configure<TOptions>(name, config, _ => { });

		public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfiguration config, Action<BinderOptions> configureBinder)
			where TOptions : class
			=> services.Configure<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, config, configureBinder);

		public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, IConfiguration config, Action<BinderOptions> configureBinder)
			where TOptions : class
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			if (config == null)
				throw new ArgumentNullException(nameof(config));

			services.AddOptions();
			services.AddSingleton<IOptionsChangeTokenSource<TOptions>>(new ConfigurationChangeTokenSource<TOptions>(name, config));
			return services.AddSingleton<IConfigureOptions<TOptions>>(new FastNamedConfigureFromConfigurationOptions<TOptions>(name, config, configureBinder));
		}
	}
}
