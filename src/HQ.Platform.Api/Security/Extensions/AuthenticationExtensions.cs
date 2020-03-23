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
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Models;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sodium;
using CookieOptions = HQ.Platform.Api.Security.Configuration.CookieOptions;

namespace HQ.Platform.Api.Security.Extensions
{
	public static class AuthenticationExtensions
	{
		private static readonly object Sync = new object();

		public static void AddAuthentication(this IServiceCollection services, 
			SecurityOptions security,
			SuperUserOptions superUser, 
			TokenOptions tokens,
			CookieOptions cookies, 
			ClaimOptions claims)
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
			arg.Success();
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

		internal static void MaybeSetSecurityKeys(ITokenCredentials request)
		{
			request.SigningKey ??= request.SigningKey = BuildSigningCredentials(request);
			request.EncryptingKey ??= (request.EncryptingKey = BuildEncryptingCredentials(request));
		}

		private static SigningCredentials BuildSigningCredentials(ITokenCredentials request)
		{
			MaybeSelfCreateMissingKeyStrings(request);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.SigningKeyString));
			return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		}

		private static EncryptingCredentials BuildEncryptingCredentials(ITokenCredentials request)
		{
			MaybeSelfCreateMissingKeyStrings(request);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.EncryptingKeyString));
			return new EncryptingCredentials(securityKey, JwtConstants.DirectKeyUseAlg,
				SecurityAlgorithms.Aes256CbcHmacSha512);
		}

		public static bool MaybeSelfCreateMissingKeyStrings(ITokenCredentials feature)
		{
			var changed = false;

			if (feature.SigningKey == null || feature.SigningKeyString == ActiveAuth.Constants.Tokens.NoSigningKeySet)
			{
				Trace.TraceWarning("No JWT signing key found, creating temporary key.");
				feature.SigningKeyString = Encoding.UTF8.GetString(SodiumCore.GetRandomBytes(128));
				changed = true;
			}

			if (feature.EncryptingKey == null || feature.EncryptingKeyString == ActiveAuth.Constants.Tokens.NoEncryptingKeySet)
			{
				Trace.TraceWarning("No JWT encryption key found, using signing key.");
				feature.EncryptingKeyString = feature.SigningKeyString;
				changed = true;
			}

			return changed;
		}

		private static TokenValidationParameters BuildTokenValidationParameters(SecurityOptions security, TokenOptions tokens, ClaimOptions claims)
		{
			MaybeSetSecurityKeys(new TokenFabricationRequest
			{
				TokenAudience = tokens.Audience,
				Encrypt = tokens.Encrypt,
				EncryptingKey = security.Encrypting,
				EncryptingKeyString = tokens.EncryptingKey,
				SigningKey = security.Signing,
				SigningKeyString = tokens.SigningKey,
				TokenIssuer = tokens.Issuer,
				TokenTimeToLiveSeconds = tokens.TimeToLiveSeconds
			});

			var name = claims.UserNameClaim;
			var role = claims.RoleClaim;

			if (tokens.Encrypt)
			{
				return new TokenValidationParameters
				{
					TokenDecryptionKeyResolver = (token, securityToken, kid, parameters) => new[] {security.Encrypting.Key},
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