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
using ActiveRoutes;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Security.AspNetCore.Mvc.Configuration;
using HQ.Platform.Security.AspNetCore.Mvc.Controllers;
using HQ.Platform.Security.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Security.AspNetCore.Mvc
{
	public static class Add
	{
		public static IServiceCollection AddSuperUserTokenController<TUser, TKey>(this IServiceCollection services) where TKey : IEquatable<TKey>
		{
			var mvcBuilder = services.AddMvcCore();
			mvcBuilder.AddSuperUserTokenController<TUser, TKey>();
			return services;
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TUser, TKey>(this IMvcCoreBuilder mvcBuilder) where TKey : IEquatable<TKey>
		{
			mvcBuilder.AddActiveRoute<SuperUserTokenController<TKey>, SuperUserComponent, SuperUserOptions>();
			return mvcBuilder;
		}
	}
}