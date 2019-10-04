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

using HQ.Platform.Identity.Configuration;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Extensions
{
	internal static class IdentityOptionsExtensions
	{
		public static void Apply(this IdentityOptions o, IdentityOptionsExtended x)
		{
			o.ClaimsIdentity.RoleClaimType = x.ClaimsIdentity.RoleClaimType;
			o.ClaimsIdentity.SecurityStampClaimType = x.ClaimsIdentity.SecurityStampClaimType;
			o.ClaimsIdentity.UserIdClaimType = x.ClaimsIdentity.UserIdClaimType;
			o.ClaimsIdentity.UserNameClaimType = x.ClaimsIdentity.UserNameClaimType;

			o.Stores.ProtectPersonalData = x.Stores.ProtectPersonalData;
			o.Stores.MaxLengthForKeys = x.Stores.MaxLengthForKeys;

			o.User.AllowedUserNameCharacters = x.User.AllowedUserNameCharacters;
			o.User.RequireUniqueEmail = x.User.RequireUniqueEmail;

			o.Lockout.AllowedForNewUsers = x.Lockout.AllowedForNewUsers;
			o.Lockout.DefaultLockoutTimeSpan = x.Lockout.DefaultLockoutTimeSpan;
			o.Lockout.MaxFailedAccessAttempts = x.Lockout.MaxFailedAccessAttempts;

			o.Password.RequireDigit = x.Password.RequireDigit;
			o.Password.RequireLowercase = x.Password.RequireLowercase;
			o.Password.RequireNonAlphanumeric = x.Password.RequireNonAlphanumeric;
			o.Password.RequireUppercase = x.Password.RequireUppercase;
			o.Password.RequiredLength = x.Password.RequiredLength;
			o.Password.RequiredUniqueChars = x.Password.RequiredUniqueChars;

			o.Tokens.AuthenticatorTokenProvider = x.Tokens.AuthenticatorTokenProvider;
			o.Tokens.ChangeEmailTokenProvider = x.Tokens.ChangeEmailTokenProvider;
			o.Tokens.ChangePhoneNumberTokenProvider = x.Tokens.ChangePhoneNumberTokenProvider;
			o.Tokens.EmailConfirmationTokenProvider = x.Tokens.EmailConfirmationTokenProvider;
			o.Tokens.PasswordResetTokenProvider = x.Tokens.PasswordResetTokenProvider;
			o.Tokens.ProviderMap = x.Tokens.ProviderMap;

			o.SignIn.RequireConfirmedEmail = x.SignIn.RequireConfirmedEmail;
			o.SignIn.RequireConfirmedPhoneNumber = x.SignIn.RequireConfirmedPhoneNumber;
		}
	}
}