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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HQ.Common.Extensions;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HQ.Platform.Security.AspNetCore
{
    public static class JwtSecurity
    {
        private static SigningCredentials credentials;

        public static TokenValidationParameters TokenValidationParameters { get; private set; }

        public static void AddAuthentication(this IServiceCollection services, SecurityOptions options)
        {
            credentials = credentials ?? BuildSigningCredentials(options);

            TokenValidationParameters = TokenValidationParameters ?? BuildTokenValidationParameters(options);

            services
                .AddAuthentication(x =>
                {
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = TokenValidationParameters;
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

        public static string CreateToken<TUser>(TUser user, IEnumerable<Claim> userClaims, SecurityOptions security,
            PublicApiOptions api)
            where TUser : IUserIdProvider
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

            credentials = credentials ?? BuildSigningCredentials(security);

            var handler = new JwtSecurityTokenHandler();
            var jwt = new JwtSecurityToken(iss, aud, claims, now.UtcDateTime, expires.UtcDateTime, credentials);

            return handler.WriteToken(jwt);
        }

        private static SigningCredentials BuildSigningCredentials(SecurityOptions options)
        {
            return new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Tokens.Key)),
                SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        private static TokenValidationParameters BuildTokenValidationParameters(SecurityOptions options)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = options.Tokens.Issuer,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = options.Tokens.Audience,
                RequireSignedTokens = true,
                IssuerSigningKey = credentials.Key,
                ClockSkew = TimeSpan.FromMinutes(5),
                RoleClaimType = options.Claims.RoleClaim,
                NameClaimType = options.Claims.UserNameClaim
            };

            return parameters;
        }
    }
}
