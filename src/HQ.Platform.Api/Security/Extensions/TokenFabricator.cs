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
using System.Security.Claims;
using ActiveAuth.Configuration;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using HQ.Platform.Api.Security.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Security.Extensions
{
	public sealed class TokenFabricator<TKey> : ITokenFabricator<TKey> where TKey : IEquatable<TKey>
	{
		private readonly Func<DateTimeOffset> _timestamps;
		private readonly IHttpContextAccessor _http;
		private readonly IUserIdProvider<TKey> _userIdProvider;
		private readonly IOptionsSnapshot<SecurityOptions> _security;
		private readonly IOptionsSnapshot<TokenOptions> _tokens;

		public TokenFabricator(Func<DateTimeOffset> timestamps, IHttpContextAccessor http, IUserIdProvider<TKey> userIdProvider,
			IOptionsSnapshot<SecurityOptions> security, IOptionsSnapshot<TokenOptions> tokens)
		{
			_timestamps = timestamps;
			_http = http;
			_userIdProvider = userIdProvider;
			_security = security;
			_tokens = tokens;
		}

		#region Implementation of ITokenFabricator

		public string CreateToken(IUserIdProvider<TKey> user, IEnumerable<Claim> claims = null)
		{ 
			//var claims = _http.HttpContext.User.Claims;
			var identity = user.QuackLike<IUserIdProvider<TKey>>();

			var request = new TokenFabricationRequest();

			request.TokenIssuer = _tokens.Value.Issuer;
			request.TokenAudience = _tokens.Value.Audience;
			request.TokenTimeToLiveSeconds = _tokens.Value.TimeToLiveSeconds;
			
			request.SigningKeyString = _tokens.Value.SigningKey;
			request.EncryptingKeyString = _tokens.Value.EncryptingKey;

			var token = identity.CreateToken<IUserIdProvider<TKey>, TKey>(_timestamps, claims, request);
			return token;
		}

		#endregion
	}
}