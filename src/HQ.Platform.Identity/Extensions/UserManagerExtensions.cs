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
using HQ.Platform.Security.AspNetCore.Mvc.Models;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace HQ.Platform.Identity.Extensions
{
	public static class UserManagerExtensions
	{
		public static Task<int> CountAsync<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			userManager.ThrowIfDisposed();
			if (userManager.Store() is IUserStoreExtended<TUser> extended)
			{
				return extended.GetCountAsync(extended.CancellationToken);
			}

			return null;
		}

		public static bool SupportsSuperUser<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			userManager.ThrowIfDisposed();
			return userManager.Store() is IUserStoreExtended<TUser> extended && extended.SupportsSuperUser;
		}

		public static Task<TUser> FindByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber)
			where TUser : class
		{
			if (userManager.Store() is IUserPhoneNumberStoreExtended<TUser> extended)
			{
				return extended.FindByPhoneNumberAsync(phoneNumber, extended.CancellationToken);
			}

			return null;
		}

		public static IUserStore<TUser> Store<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = ReadAccessor.Create(typeof(UserManager<TUser>));
			var userStore = accessor[userManager, "Store"];
			return userStore as IUserStore<TUser>;
		}

		public static async Task<TUser> FindByIdentityAsync<TUser>(this UserManager<TUser> userManager,
			IdentityType identityType, string identity)
			where TUser : class, IUserEmailProvider, IPhoneNumberProvider
		{
			TUser user;
			switch (identityType)
			{
				case IdentityType.Username:
					user = await userManager.FindByNameAsync(identity);
					break;
				case IdentityType.Email:
					user = await userManager.FindByEmailAsync(identity);
					if (user != null && !user.EmailConfirmed)
						return null;
					break;
				case IdentityType.PhoneNumber:
					user = await userManager.FindByPhoneNumberAsync(identity);
					if (user != null && !user.PhoneNumberConfirmed)
						return null;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return user;
		}

		private static void ThrowIfDisposed<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = ReadAccessor.Create(typeof(UserManager<TUser>));
			var disposedField = accessor[userManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
			{
				throw new ObjectDisposedException(userManager.GetType().Name);
			}
		}
	}
}