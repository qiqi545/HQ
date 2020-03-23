using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using System.Security.Claims;
using ActiveAuth.Models;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HQ.Platform.Api.Security.Extensions
{
	internal static class UserProviderExtensions
	{
		public static string CreateToken<TUser, TKey>(this TUser user, Func<DateTimeOffset> timestamps, IEnumerable<Claim> userClaims,
			TokenFabricationRequest security) where TUser : IUserIdProvider<TKey> where TKey : IEquatable<TKey>
		{
			var now = timestamps();
			var expires = now.AddSeconds(security.TokenTimeToLiveSeconds);

			/*
                See: https://tools.ietf.org/html/rfc7519#section-4.1
                All claims are optional, but since our JSON conventions elide null values,
                We need to ensure any optional claims are emitted as empty strings.
            */

			// JWT.io claims:
			var sub = user.Id?.ToString() ?? string.Empty;
			var jti = $"{Guid.NewGuid()}";
			var iat = now.ToUnixTimeSeconds().ToString();
			var exp = expires.ToUnixTimeSeconds().ToString();
			var nbf = now.ToUnixTimeSeconds().ToString();
			var iss = security.TokenIssuer ?? string.Empty;
			var aud = security.TokenAudience ?? string.Empty;

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, sub, ClaimValueTypes.String),
				new Claim(JwtRegisteredClaimNames.Jti, jti, ClaimValueTypes.String),
				new Claim(JwtRegisteredClaimNames.Iat, iat, ClaimValueTypes.Integer64),
				new Claim(JwtRegisteredClaimNames.Nbf, nbf, ClaimValueTypes.Integer64),
				new Claim(JwtRegisteredClaimNames.Exp, exp, ClaimValueTypes.Integer64)
			};

			claims.AddRange(userClaims);

			AuthenticationExtensions.MaybeSetSecurityKeys(security);

			var handler = new JwtSecurityTokenHandler();

			if (!security.Encrypt)
			{
				var jwt = new JwtSecurityToken(iss, aud, claims, now.UtcDateTime, expires.UtcDateTime,
					security.SigningKey);

				return handler.WriteToken(jwt);
			}

			var descriptor = new SecurityTokenDescriptor
			{
				Audience = aud,
				Issuer = iss,
				Subject = new ClaimsIdentity(claims),
				EncryptingCredentials = security.EncryptingKey
			};

			return handler.CreateEncodedJwt(descriptor);
		}
	}
}
