﻿using System;
using HQ.Common.AspNetCore.MergePatch.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

#if NETCOREAPP3_0
namespace HQ.Common.AspNetCore.MergePatch
{
	public static class MvcBuilderExtensions
	{
		private static void AddJsonMergePatch(this IServiceCollection services, Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
		}

		public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddJsonMergePatch(configure);
			return builder;
		}

		public static IMvcCoreBuilder AddJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			//builder.AddJsonFormatters();
			builder.Services.AddJsonMergePatch(configure);
			return builder;
		}
	}
}
#endif