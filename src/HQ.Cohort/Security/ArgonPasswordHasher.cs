// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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

			if (_previous?.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success)
				return PasswordVerificationResult.SuccessRehashNeeded;

			return PasswordVerificationResult.Failed;
		}
	}
}