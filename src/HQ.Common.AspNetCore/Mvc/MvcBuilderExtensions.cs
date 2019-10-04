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
				var typeInfo = new List<TypeInfo> {typeof(T).GetTypeInfo()};
				x.ApplicationParts.Add(new DynamicControllerApplicationPart(typeInfo));
			});
			return mvcBuilder;
		}
	}
}