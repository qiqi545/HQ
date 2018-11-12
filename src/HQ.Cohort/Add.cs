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
using System.Diagnostics;
using System.Linq;
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
            services.Configure<IdentityOptions>(configuration);
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

            var validator = services.SingleOrDefault(x => x.ServiceType == typeof(IUserValidator<TUser>));
            Debug.Assert(validator != null);
            Debug.Assert(services.Remove(validator));

            services.AddScoped<IUserValidator<TUser>, UserValidatorExtended<TUser>>();

            return identityBuilder;
        }
    }
}
