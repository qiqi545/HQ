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
using HQ.Data.Contracts;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Services
{
    public class SignInService<TUser, TKey> : ISignInService<TUser> where TUser :
        IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly SignInManager<TUser> _signInManager;

        public SignInService(SignInManager<TUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<Operation> SignIn(TUser user, bool isPersistent, string authenticationMethod = null)
        {
            await _signInManager.SignInAsync(user, isPersistent, authenticationMethod);
            return Operation.CompletedWithoutErrors;
        }

        public async Task<Operation> SignOut()
        {
            /*
            await _signInManager.Context.SignOutAsync(IdentityConstants.ApplicationScheme);
            await _signInManager.Context.SignOutAsync(IdentityConstants.ExternalScheme);
            await _signInManager.Context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            */

            await _signInManager.SignOutAsync();
            return Operation.CompletedWithoutErrors;
        }
    }
}
