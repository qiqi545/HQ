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
using System.Threading.Tasks;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace HQ.Platform.Identity.Extensions
{
	/// <summary>
	///     Provides conventional RoleManager access to Zero extensions to the ASP.NET Identity system.
	/// </summary>
	public static class RoleManagerExtensions
	{
		public static Task<int> GetCountAsync<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			roleManager.ThrowIfDisposed();
			if (roleManager.GetStore() is IRoleStoreExtended<TRole> extended)
			{
				return extended.GetCountAsync(extended.CancellationToken);
			}

			return null;
		}

		public static IRoleStore<TRole> GetStore<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			var accessor = ReadAccessor.Create(typeof(RoleManager<TRole>));
			var roleStore = accessor[roleManager, "Store"];
			return roleStore as IRoleStore<TRole>;
		}

		private static void ThrowIfDisposed<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			var accessor = ReadAccessor.Create(typeof(RoleManager<TRole>));
			var disposedField = accessor[roleManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
			{
				throw new ObjectDisposedException(roleManager.GetType().Name);
			}
		}
	}
}