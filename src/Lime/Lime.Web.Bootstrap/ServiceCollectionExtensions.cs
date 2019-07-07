// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Lime.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Lime.Web.Bootstrap
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBootstrap(this IServiceCollection services)
		{
			services.AddSingleton(r => UiTheme.Default);

			return services.AddUiResources(typeof(Bootstrap).Assembly);
		}
	}
}