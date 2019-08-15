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
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using CookieOptions = HQ.Platform.Security.Configuration.CookieOptions;

namespace HQ.Platform.Security.AspNetCore.Models
{
	public static class AuthenticationExtensions
	{
		private static readonly object Sync = new object();

		public static void AddAuthentication(this IServiceCollection services, SecurityOptions security, SuperUserOptions superUser, TokenOptions tokens, CookieOptions cookies, ClaimOptions claims)
		{
			lock (Sync)
			{
				AuthenticationBuilder authBuilder = null;

				var scheme = tokens.Scheme;

				if (tokens.Enabled || superUser.Enabled || cookies.Enabled)
				{
					authBuilder = services.AddAuthentication(x =>
					{
						x.DefaultScheme = scheme;
						x.DefaultAuthenticateScheme = scheme;
						x.DefaultForbidScheme = scheme;
						x.DefaultSignInScheme = scheme;
						x.DefaultSignOutScheme = scheme;
						x.DefaultChallengeScheme = scheme;
					});
				}

				if (tokens.Enabled || superUser.Enabled)
				{
					var parameters = BuildTokenValidationParameters(security, tokens, claims);

					authBuilder.AddJwtBearer(scheme, x =>
					{
						x.Events = new JwtBearerEvents();
						x.Events.OnTokenValidated += OnTokenValidated;
						x.Events.OnMessageReceived += OnMessageReceived;
						x.Events.OnAuthenticationFailed += OnAuthenticationFailed;
						x.Events.OnChallenge += OnChallenge;
						x.SaveToken = true;
						x.TokenValidationParameters = parameters;
#if DEBUG
						x.IncludeErrorDetails = true;
						x.RequireHttpsMetadata = false;
#else
				    x.IncludeErrorDetails = false;
				    x.RequireHttpsMetadata = true;
#endif
					});
				}

				if (cookies.Enabled)
				{
					authBuilder.AddCookie(cookies.Scheme ?? scheme, cfg =>
					{
						cfg.LoginPath = cookies.SignInPath;
						cfg.LogoutPath = cookies.SignOutPath;
						cfg.AccessDeniedPath = cookies.ForbidPath;
						cfg.ReturnUrlParameter = cookies.ReturnOperator;
						cfg.Events = new CookieAuthenticationEvents
						{
							OnSigningIn = context =>
							{
								if (string.IsNullOrWhiteSpace(cookies.Domain))
									context.Options.Cookie.Domain = context.HttpContext.Request.Host.Value;
								return Task.CompletedTask;
							}
						};
						cfg.SlidingExpiration = tokens.AllowRefresh;
						cfg.ClaimsIssuer = tokens.Issuer;

						cfg.Cookie.Name = cookies.IdentityName;
						cfg.Cookie.Expiration = TimeSpan.FromSeconds(tokens.TimeToLiveSeconds);
						cfg.Cookie.Path = "/";
						cfg.Cookie.HttpOnly = true;
						cfg.Cookie.IsEssential = true;
						cfg.Cookie.SameSite = SameSiteMode.Strict;
						cfg.Cookie.MaxAge = cfg.Cookie.Expiration;
						cfg.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					});
				}
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
			arg.Fail(arg.Exception);
			return Task.CompletedTask;
		}

		public static string CreateToken<TUser>(this TUser user, IEnumerable<Claim> userClaims, SecurityOptions security) where TUser : IUserIdProvider
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

			MaybeSetSecurityKeys(security);

			var handler = new JwtSecurityTokenHandler();

			if (!security.Tokens.Encrypt)
			{
				var jwt = new JwtSecurityToken(iss, aud, claims, now.UtcDateTime, expires.UtcDateTime, security.Signing);

				return handler.WriteToken(jwt);
			}

			var descriptor = new SecurityTokenDescriptor
			{
				Audience = aud,
				Issuer = iss,
				Subject = new ClaimsIdentity(claims),
				EncryptingCredentials = security.Encrypting
			};

			return handler.CreateEncodedJwt(descriptor);
		}

		internal static void MaybeSetSecurityKeys(SecurityOptions security)
		{
			security.Signing = security.Signing ?? (security.Signing = BuildSigningCredentials(security.Tokens));
			security.Encrypting = security.Encrypting ?? (security.Encrypting = BuildEncryptingCredentials(security.Tokens));
		}

		private static SigningCredentials BuildSigningCredentials(TokenOptions options)
		{
			MaybeSelfCreateMissingKeys(options);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
			return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		}

		private static EncryptingCredentials BuildEncryptingCredentials(TokenOptions options)
		{
			MaybeSelfCreateMissingKeys(options);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.EncryptionKey));
			return new EncryptingCredentials(securityKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);
		}

		public static bool MaybeSelfCreateMissingKeys(TokenOptions options)
		{
			var changed = false;

			if (options.SigningKey == null || options.SigningKey == Constants.Tokens.NoSigningKeySet)
			{
				Trace.TraceWarning("No JWT signing key found, creating temporary key.");
				options.SigningKey = Crypto.GetRandomString(64);
				changed = true;
			}

			if (options.EncryptionKey == null || options.EncryptionKey == Constants.Tokens.NoEncryptionKeySet)
			{
				Trace.TraceWarning("No JWT encryption key found, using signing key.");
				options.EncryptionKey = options.SigningKey;
				changed = true;
			}

			return changed;
		}

		private static TokenValidationParameters BuildTokenValidationParameters(SecurityOptions security, TokenOptions tokens, ClaimOptions claims)
		{
			MaybeSetSecurityKeys(security);

			var name = claims.UserNameClaim;
			var role = claims.RoleClaim;

			if (tokens.Encrypt)
			{
				return new TokenValidationParameters
				{
					TokenDecryptionKeyResolver = (token, securityToken, kid, parameters) => new[] { security.Encrypting.Key },
					ValidateIssuerSigningKey = false,
					ValidIssuer = tokens.Issuer,
					ValidateLifetime = true,
					ValidateAudience = true,
					ValidAudience = tokens.Audience,
					RequireSignedTokens = false,
					IssuerSigningKey = security.Signing.Key,
					TokenDecryptionKey = security.Encrypting.Key,
					ClockSkew = TimeSpan.FromSeconds(tokens.ClockSkewSeconds),
					RoleClaimType = role,
					NameClaimType = name
				};
			}

			return new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				ValidIssuer = tokens.Issuer,
				ValidateLifetime = true,
				ValidateAudience = true,
				ValidAudience = tokens.Audience,
				RequireSignedTokens = true,
				IssuerSigningKey = security.Signing.Key,
				ClockSkew = TimeSpan.FromSeconds(tokens.ClockSkewSeconds),
				RoleClaimType = role,
				NameClaimType = name
			};
		}
	}
}
