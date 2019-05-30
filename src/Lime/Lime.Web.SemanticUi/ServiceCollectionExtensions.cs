// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Lime.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Lime.Web.SemanticUi
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddSemanticUi(this IServiceCollection services)
		{
			services.AddSingleton(r => UiTheme.Default);

			return services.AddUiResources(typeof(SemanticUi).Assembly);
		}
	}
}