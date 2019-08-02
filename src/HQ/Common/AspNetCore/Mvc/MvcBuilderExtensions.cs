using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Common.AspNetCore.Mvc
{
	public static class MvcBuilderExtensions
	{
		public static IMvcBuilder AddControllerFeature<T>(this IMvcBuilder mvcBuilder)
		{
            // See: https://github.com/aspnet/Mvc/issues/5992
			mvcBuilder.AddApplicationPart(typeof(T).Assembly);
			mvcBuilder.Services.AddOptions<MvcOptions>()
				.Configure<IEnumerable<IDynamicComponent>>((o, x) =>
				{
					o.Conventions.RemoveType<DynamicComponentConvention>();
					o.Conventions.Add(new DynamicComponentConvention(x));
				});
			mvcBuilder.ConfigureApplicationPartManager(x =>
			{
				var typeInfo = new List<TypeInfo> { typeof(T).GetTypeInfo() };
				x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
			});
			return mvcBuilder;
		}
	}
}
