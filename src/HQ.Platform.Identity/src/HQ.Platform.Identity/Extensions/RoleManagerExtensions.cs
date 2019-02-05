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
using HQ.Common.FastMember;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Extensions
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
