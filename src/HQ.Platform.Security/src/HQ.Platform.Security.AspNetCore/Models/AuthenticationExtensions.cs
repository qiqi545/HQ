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
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Cryptography;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HQ.Platform.Security.AspNetCore.Models
{
    public static class AuthenticationExtensions
    {
        private static SigningCredentials _signing;
        private static EncryptingCredentials _encrypting;

        public static void AddAuthentication(this IServiceCollection services, SecurityOptions options)
        {
            _signing = _signing ?? BuildSigningCredentials(options);
            _encrypting = _encrypting ?? BuildEncryptingCredentials(options);

            var authBuilder = services.AddAuthentication(x =>
            {
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            if (options.Tokens.Enabled)
            {
                authBuilder.AddJwtBearer(x =>
                {
                    x.Events = new JwtBearerEvents();
                    x.Events.OnTokenValidated += OnTokenValidated;
                    x.Events.OnMessageReceived += OnMessageReceived;
                    x.Events.OnAuthenticationFailed += OnAuthenticationFailed;
                    x.Events.OnChallenge += OnChallenge;

                    x.SaveToken = true;

                    x.TokenValidationParameters = BuildTokenValidationParameters(options);
#if DEBUG
                    x.IncludeErrorDetails = true;
                    x.RequireHttpsMetadata = false;
#else
				    x.IncludeErrorDetails = false;
				    x.RequireHttpsMetadata = true;
#endif
                });
            }

            if (options.Cookies.Enabled)
            {
                authBuilder.AddCookie(cfg =>
                {
                    cfg.Cookie.Name = options.Cookies.IdentityName;
                    cfg.LoginPath = options.Cookies.SignInPath;
                    cfg.LogoutPath = options.Cookies.SignOutPath;
                    cfg.AccessDeniedPath = options.Cookies.ForbidPath;
                    cfg.ReturnUrlParameter = options.Cookies.ReturnOperator;
                    cfg.Events = new CookieAuthenticationEvents
                    {
                        OnSigningIn = context =>
                        {
                            if (string.IsNullOrWhiteSpace(options.Cookies.Domain))
                                context.Options.Cookie.Domain = context.HttpContext.Request.Host.Value;
                            return Task.CompletedTask;
                        }
                    };
                    cfg.SlidingExpiration = options.Tokens.AllowRefresh;
                    cfg.ClaimsIssuer = options.Tokens.Issuer;
                    cfg.Cookie.Expiration = TimeSpan.FromSeconds(options.Tokens.TimeToLiveSeconds);

                    cfg.Cookie.Path = "/";
                    cfg.Cookie.HttpOnly = true;
                    cfg.Cookie.IsEssential = true;
                    cfg.Cookie.SameSite = SameSiteMode.Strict;
                    cfg.Cookie.MaxAge = cfg.Cookie.Expiration;
                    cfg.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }
        }

        private static Task OnTokenValidated(TokenValidatedContext arg)
        {
            Trace.TraceInformation(arg.ToString());
            return Task.CompletedTask;
        }

        private static Task OnMessageReceived(MessageReceivedContext arg)
        {
            Trace.TraceInformation(arg.ToString());
            return Task.CompletedTask;
        }

        private static Task OnChallenge(JwtBearerChallengeContext arg)
        {
            Trace.TraceInformation(arg.ToString());
            return Task.CompletedTask;
        }

        private static Task OnAuthenticationFailed(AuthenticationFailedContext arg)
        {
            Trace.TraceInformation(arg.ToString());
            return Task.CompletedTask;
        }

        public static string CreateToken<TUser>(TUser user, IEnumerable<Claim> userClaims, SecurityOptions security, PlatformApiOptions api) where TUser : IUserIdProvider
        {
            var now = DateTimeOffset.Now;
            var expires = now.AddSeconds(security.Tokens.TimeToLiveSeconds);

            /*
                See: https://tools.ietf.org/html/rfc7519#section-4.1
                All claims are optional, but since our JSON conventions elide null values,
                We need to ensure any optional claims are emitted as empty strings.
            */

            // JWT.io claims:
            var sub = user.Id ?? string.Empty;
            var jti = $"{Guid.NewGuid()}";
            var iat = now.ToUnixTimeSeconds().ToString();
            var exp = expires.ToUnixTimeSeconds().ToString();
            var nbf = now.ToUnixTimeSeconds().ToString();
            var iss = security.Tokens?.Issuer ?? string.Empty;
            var aud = security.Tokens?.Audience ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, sub, ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Jti, jti, ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Iat, iat, ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Nbf, nbf, ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, exp, ClaimValueTypes.Integer64)
            };

            claims.AddRange(userClaims);

            claims.TryAddClaim(security.Claims.ApplicationIdClaim, api.ApiVersion);
            claims.TryAddClaim(security.Claims.ApplicationNameClaim, api.ApiName);

            _signing = _signing ?? BuildSigningCredentials(security);
            _encrypting = _encrypting ?? BuildEncryptingCredentials(security);

            var handler = new JwtSecurityTokenHandler();

            if (security.Tokens.Encrypt)
            {
                var descriptor = new SecurityTokenDescriptor
                {
                    Audience = aud,
                    Issuer = iss,
                    Subject = new ClaimsIdentity(claims),
                    EncryptingCredentials = _encrypting
                };

                return handler.CreateEncodedJwt(descriptor);
            }

            return handler.WriteToken(new JwtSecurityToken(iss, aud, claims, now.UtcDateTime, expires.UtcDateTime, _signing));
        }

        private static SigningCredentials BuildSigningCredentials(SecurityOptions options)
        {
            MaybeSelfCreateMissingKeys(options);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Tokens.SigningKey));
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        private static EncryptingCredentials BuildEncryptingCredentials(SecurityOptions options)
        {
            MaybeSelfCreateMissingKeys(options);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Tokens.EncryptionKey));
            return new EncryptingCredentials(securityKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        public static bool MaybeSelfCreateMissingKeys(SecurityOptions options)
        {
            bool changed = false;

            if (options.Tokens.SigningKey == null || options.Tokens.SigningKey == Constants.Tokens.NoSigningKeySet)
            {
                Trace.TraceWarning("No JWT signing key found, creating temporary key.");
                options.Tokens.SigningKey = Crypto.GetRandomString(64);
                changed = true;
            }

            if (options.Tokens.EncryptionKey == null || options.Tokens.EncryptionKey == Constants.Tokens.NoEncryptionKeySet)
            {
                Trace.TraceWarning("No JWT encryption key found, using signing key.");
                options.Tokens.EncryptionKey = options.Tokens.SigningKey;
                changed = true;
            }

            return changed;
        }

        private static TokenValidationParameters BuildTokenValidationParameters(SecurityOptions options)
        {
            var name = options.Claims.UserNameClaim;
            var role = options.Claims.RoleClaim;

            if (options.Tokens.Encrypt)
            {
                return new TokenValidationParameters
                {
                    TokenDecryptionKeyResolver = TokenDecryptionKeyResolver,
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = options.Tokens.Issuer,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidAudience = options.Tokens.Audience,
                    RequireSignedTokens = false,
                    IssuerSigningKey = _signing.Key,
                    TokenDecryptionKey = _encrypting.Key,
                    ClockSkew = TimeSpan.FromSeconds(options.Tokens.ClockSkewSeconds),
                    RoleClaimType = role,
                    NameClaimType = name
                };
            }

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = options.Tokens.Issuer,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = options.Tokens.Audience,
                RequireSignedTokens = true,
                IssuerSigningKey = _signing.Key,
                ClockSkew = TimeSpan.FromSeconds(options.Tokens.ClockSkewSeconds),
                RoleClaimType = role,
                NameClaimType = name
            };
        }

        private static IEnumerable<SecurityKey> TokenDecryptionKeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters parameters)
        {
            yield return _encrypting.Key;
        }
    }
}
