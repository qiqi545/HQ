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
using System.Data;
using ActiveTenant;
using HQ.Data.SessionManagement;
using HQ.Extensions.DependencyInjection;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Identity.Stores.Sql
{
	public static class Add
	{
		public static IdentityBuilder AddSqlIdentityStores<TDatabase, TKey, TUser, TRole, TTenant, TApplication>
		(
			this IdentityBuilder identityBuilder,
			string connectionString,
			ConnectionScope scope,
			Action<IDbCommand, Type, IServiceProvider> onCommand = null,
			Action<IDbConnection, IServiceProvider> onConnection = null
		)
			where TDatabase : class, IConnectionFactory, new()
			where TKey : IEquatable<TKey>
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
		{
			var services = identityBuilder.Services;

			if (scope == ConnectionScope.ByRequest)
				services.AddHttpContextAccessor();

			services.AddDatabaseConnection<IdentityBuilder, TDatabase>(connectionString, scope, new[] { new HttpAccessorExtension() }, onConnection, onCommand);
			
			services.AddTransient<IUserStoreExtended<TUser>, UserStore<TUser, TKey, TRole>>();
			services.AddTransient<IUserStore<TUser>>(r => r.GetRequiredService<IUserStoreExtended<TUser>>());

			services.AddTransient<IRoleStoreExtended<TRole>, RoleStore<TKey, TRole>>();
			services.AddTransient<IRoleStore<TRole>>(r => r.GetRequiredService<IRoleStoreExtended<TRole>>());

			services.AddTransient<ITenantStore<TTenant>, TenantStore<TTenant, TKey>>();
			services.AddScoped<TenantManager<TTenant, TUser, TKey>>();

			services.AddTransient<IApplicationStore<TApplication>, ApplicationStore<TApplication, TKey>>();
			services.AddScoped<ApplicationManager<TApplication, TUser, TKey>>();

			return identityBuilder
				.AddRoles<TRole>()
				.AddUserManager<UserManager<TUser>>()
				.AddRoleManager<RoleManager<TRole>>()
				.AddSignInManager<SignInManager<TUser>>();
		}
	}
}