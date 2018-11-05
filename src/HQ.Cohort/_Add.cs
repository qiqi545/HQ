// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Cohort.Configuration;
using HQ.Cohort.Security;
using HQ.Cohort.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort
{
	public static class Add
	{
		private static readonly Action<IdentityOptionsExtended> Defaults;

		static Add()
		{
			Defaults = x =>
			{
				// Sensible defaults not set by ASP.NET Core Identity:
				x.Stores.ProtectPersonalData = true;
				x.Stores.MaxLengthForKeys = 128;
				x.User.RequireUniqueEmail = true;
			};
		}

		public static IdentityBuilder AddIdentityCoreExtended<TUser>(this IServiceCollection services,
			IConfiguration configuration) where TUser : class
		{
			services.Configure<IdentityOptionsExtended>(configuration);

			return AddIdentityCoreExtended<TUser>(services, configuration.Bind);
		}

		public static IdentityBuilder AddIdentityCoreExtended<TUser>(this IServiceCollection services,
			Action<IdentityOptionsExtended> setupAction) where TUser : class
		{
			/*
				services.AddOptions().AddLogging();
			    services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
			    services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
			    services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
			    services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
			    services.TryAddScoped<IdentityErrorDescriber>();
			    services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
			    services.TryAddScoped<UserManager<TUser>, UserManager<TUser>>();
			    if (setupAction != null)
			      services.Configure<IdentityOptions>(setupAction);
			    return new IdentityBuilder(typeof (TUser), services);
			 */
			var identityBuilder = services.AddIdentityCore<TUser>(o =>
			{
				var extended = new IdentityOptionsExtended(o);
				Defaults(extended);
				setupAction.Invoke(extended);
			});

			identityBuilder.AddDefaultTokenProviders();
			identityBuilder.AddPersonalDataProtection<NoLookupProtector, NoLookupProtectorKeyRing>();
			identityBuilder.Services.AddSingleton<IPersonalDataProtector, DefaultPersonalDataProtector>();

			services.AddScoped<IEmailValidator<TUser>, DefaultEmailValidator<TUser>>();
			services.AddScoped<IPhoneNumberValidator<TUser>, DefaultPhoneNumberValidator<TUser>>();
			services.AddScoped<IUsernameValidator<TUser>, DefaultUsernameValidator<TUser>>();

			services.AddScoped<IUserValidator<TUser>, UserValidatorExtended<TUser>>();

			return identityBuilder;
		}
	}
}