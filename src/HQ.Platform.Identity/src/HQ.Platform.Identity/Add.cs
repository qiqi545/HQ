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
using HQ.Common;
using HQ.Platform.Api.Models;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Security;
using HQ.Platform.Identity.Services;
using HQ.Platform.Identity.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Platform.Identity
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

        public static IdentityBuilder AddIdentityExtended<TUser, TRole, TTenant, TKey>(this IServiceCollection services,
            IConfiguration configuration)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            AddIdentityPreamble(services);

            return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TKey>(configuration);
        }

        public static IdentityBuilder AddIdentityExtended<TUser, TRole, TTenant, TKey>(this IServiceCollection services,
            Action<IdentityOptionsExtended> configureIdentityExtended = null)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            AddIdentityPreamble(services);

            return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TKey>(configureIdentityExtended: o =>
            {
                configureIdentityExtended?.Invoke(o);
            });
        }

        private static void AddIdentityPreamble(IServiceCollection services)
        {
            var authBuilder = services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var cookiesBuilder = authBuilder.AddIdentityCookies(o => { });
        }

        public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole, TTenant, TKey>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            services.Configure<IdentityOptions>(configuration);
            services.Configure<IdentityOptionsExtended>(configuration);

            return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TKey>(configuration.Bind,
                configuration.Bind);
        }

        public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole, TTenant, TKey>(
            this IServiceCollection services,
            Action<IdentityOptions> configureIdentity = null,
            Action<IdentityOptionsExtended> configureIdentityExtended = null)
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TKey : IEquatable<TKey>
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
                var x = new IdentityOptionsExtended(o);
                Defaults(x);
                configureIdentityExtended?.Invoke(x);
                o.Apply(x);
            });

            if (configureIdentityExtended != null)
            {
                services.Configure(configureIdentityExtended);
            }

            if (configureIdentity != null)
            {
                services.Configure(configureIdentity);
            }

            identityBuilder.AddDefaultTokenProviders();

            // https://github.com/blowdart/AspNetCoreIdentityEncryption
            identityBuilder.AddPersonalDataProtection<NoLookupProtector, NoLookupProtectorKeyRing>();
            identityBuilder.Services.AddSingleton<IPersonalDataProtector, DefaultPersonalDataProtector>();

            services.AddScoped<IEmailValidator<TUser>, DefaultEmailValidator<TUser>>();
            services.AddScoped<IPhoneNumberValidator<TUser>, DefaultPhoneNumberValidator<TUser>>();
            services.AddScoped<IUsernameValidator<TUser>, DefaultUsernameValidator<TUser>>();

            var validator = services.SingleOrDefault(x => x.ServiceType == typeof(IUserValidator<TUser>));
            var removed = services.Remove(validator);
            Debug.Assert(validator != null);
            Debug.Assert(removed);

            services.AddScoped<IUserValidator<TUser>, UserValidatorExtended<TUser>>();
            services.AddScoped<ITenantValidator<TTenant, TUser, TKey>, TenantValidator<TTenant, TUser, TKey>>();

            services.AddScoped<IUserService<TUser>, UserService<TUser, TKey>>();
            services.AddScoped<IVersionService, IdentityVersionService>();
            services.AddScoped<ITenantService<TTenant>, TenantService<TTenant, TUser, TKey>>();
            services.AddScoped<IRoleService<TRole>, RoleService<TRole, TKey>>();
            services.AddScoped<ISignInService<TUser>, SignInService<TUser, TKey>>();

            services.TryAddSingleton<IServerTimestampService, LocalServerTimestampService>();

            return identityBuilder;
        }

        public static IServiceCollection AddIdentityTenantContextStore<TTenant>(this IServiceCollection services)
            where TTenant : IdentityTenant
        {
            services.AddScoped<ITenantContextStore<TTenant>, IdentityTenantContextStore<TTenant>>();
            return services;
        }
    }
}
