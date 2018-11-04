// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;
using FastMember;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Extensions
{
	/// <summary>
	///     Provides conventional UserManager access to Zero extensions to the ASP.NET Identity system.
	/// </summary>
	public static class UserManagerExtensions
	{
		public static bool SupportsSuperUser<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			userManager.ThrowIfDisposed();
			return userManager.GetStore() is IUserStoreExtended<TUser> extended && extended.SupportsSuperUser;
		}

		public static Task<TUser> FindByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber)
			where TUser : class
		{
			if (userManager.GetStore() is IUserPhoneNumberStoreExtended<TUser> extended)
				return extended.FindByPhoneNumberAsync(phoneNumber, extended.CancellationToken);
			return null;
		}

		public static IUserStore<TUser> GetStore<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = TypeAccessor.Create(typeof(UserManager<TUser>), true);
			var userStore = accessor[userManager, "Store"];
			return userStore as IUserStore<TUser>;
		}

		private static void ThrowIfDisposed<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = TypeAccessor.Create(typeof(UserManager<TUser>), true);
			var disposedField = accessor[userManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
				throw new ObjectDisposedException(userManager.GetType().Name);
		}
	}
}