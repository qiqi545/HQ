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
using HQ.Common;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sodium;

namespace HQ.Platform.Security.AspNetCore.Models
{
    public static class JwtSecurity
    {
        private static SigningCredentials _signing;
        private static EncryptingCredentials _encrypting;
        
        public static void AddAuthentication(this IServiceCollection services, SecurityOptions options)
        {
            _signing = _signing ?? BuildSigningCredentials(options);
            _encrypting = _encrypting ?? BuildEncryptingCredentials(options);

            services
                .AddAuthentication(x =>
                {
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = BuildTokenValidationParameters(options);
#if DEBUG
                    x.IncludeErrorDetails = true;
                    x.RequireHttpsMetadata = false;
#else
					x.IncludeErrorDetails = false;
					x.RequireHttpsMetadata = true;
#endif
                })
                .AddCookie(cfg => cfg.SlidingExpiration = true);
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
            var key = options.Tokens?.SigningKey ?? SelfCreateMissingKeys(options);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        private static EncryptingCredentials BuildEncryptingCredentials(SecurityOptions options)
        {
            var key = options.Tokens.EncryptionKey ?? options.Tokens.SigningKey ?? SelfCreateMissingKeys(options);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            return new EncryptingCredentials(securityKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        private static string SelfCreateMissingKeys(SecurityOptions options)
        {
            if (options.Tokens.SigningKey == null)
            {
                Trace.TraceWarning("No JWT signing key found, creating temporary key.");
                options.Tokens.SigningKey = Encoding.UTF8.GetString(SodiumCore.GetRandomBytes(32));
            }

            if (options.Tokens.EncryptionKey == null)
            {
                Trace.TraceWarning("No JWT encryption key found, using signing key.");
                options.Tokens.EncryptionKey = options.Tokens.SigningKey;
            }

            return options.Tokens.SigningKey;
        }

        private static TokenValidationParameters BuildTokenValidationParameters(SecurityOptions options)
        {
            if (options.Tokens.Encrypt)
            {
                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = options.Tokens.Issuer,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidAudience = options.Tokens.Audience,
                    RequireSignedTokens = false,
                    TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Tokens.EncryptionKey)),
                    ClockSkew = TimeSpan.FromSeconds(options.Tokens.ClockSkewSeconds),
                    RoleClaimType = options.Claims.RoleClaim,
                    NameClaimType = options.Claims.UserNameClaim
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
                RoleClaimType = options.Claims.RoleClaim,
                NameClaimType = options.Claims.UserNameClaim
            };
        }
    }
}
