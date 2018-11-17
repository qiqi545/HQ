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

using Microsoft.AspNetCore.Identity;
using Sodium;

namespace HQ.Cohort.Security
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides libsodium-powered Argon2i password hashing.
    ///     If a previous hashing implementation is provided, it provides graceful upgrade via ASP.NET Identity.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class ArgonPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private readonly IPasswordHasher<TUser> _previous;
        private readonly PasswordHash.StrengthArgon _strength;

        public ArgonPasswordHasher(IPasswordHasher<TUser> previous = null,
            PasswordHash.StrengthArgon strength = PasswordHash.StrengthArgon.Moderate)
        {
            _previous = previous;
            _strength = strength;
        }

        public string HashPassword(TUser user, string password)
        {
            return PasswordHash.ArgonHashString(password, _strength);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword,
            string providedPassword)
        {
            if (PasswordHash.ArgonHashStringVerify(hashedPassword, providedPassword))
                return PasswordVerificationResult.Success;

            if (_previous?.VerifyHashedPassword(user, hashedPassword, providedPassword) ==
                PasswordVerificationResult.Success)
                return PasswordVerificationResult.SuccessRehashNeeded;

            return PasswordVerificationResult.Failed;
        }
    }
}
