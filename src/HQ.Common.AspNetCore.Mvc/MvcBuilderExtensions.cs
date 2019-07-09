using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Common.AspNetCore.Mvc
{
	public static class MvcBuilderExtensions
	{
		public static IMvcBuilder AddFeature<T>(this IMvcBuilder mvcBuilder)
		{
            // See: https://github.com/aspnet/Mvc/issues/5992
			mvcBuilder.AddApplicationPart(typeof(T).Assembly);
			mvcBuilder.Services.AddOptions<MvcOptions>()
				.Configure<IEnumerable<IDynamicComponent>>((o, x) =>
				{
					o.Conventions.RemoveType<DynamicComponentConvention>();
					o.Conventions.Add(new DynamicComponentConvention(x));
				});
			return mvcBuilder;
		}
	}
}
