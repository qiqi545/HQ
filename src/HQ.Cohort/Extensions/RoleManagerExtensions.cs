// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using FastMember;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Extensions
{
	/// <summary>
	///     Provides conventional RoleManager access to Zero extensions to the ASP.NET Identity system.
	/// </summary>
	public static class RoleManagerExtensions
	{
		private static void ThrowIfDisposed<TUser>(this RoleManager<TUser> roleManager) where TUser : class
		{
			var accessor = TypeAccessor.Create(typeof(RoleManager<TUser>), true);
			var disposedField = accessor[roleManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
				throw new ObjectDisposedException(roleManager.GetType().Name);
		}
	}
}