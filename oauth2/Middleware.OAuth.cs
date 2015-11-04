using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using container;
using gadfly.auth;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.Caching.Distributed;
using oauth2.Models;
using oauth2.Security;

namespace oauth2
{
    public partial class Middleware
    {
        private string _issuer;
        private string _audience;
        private string _tokenPath;

        public string Name { get; }
        public Version Version { get; }

        public Middleware()
        {
            Name = "auth";
            Version = new Version("0.0.0");
        }

        public void Install(NameValueCollection options, IContainer container)
        {
            _issuer = options["issuer"] ?? "myself";
            _audience = options["audience"] ?? "myself";
            _tokenPath = options["tokenPath"] ?? "/token";

            container.Register<ISecurityService>(r => new SecurityService(), Lifetime.Permanent);
            container.Register(r => new UserService(r.Resolve<ISecurityService>(),  r.Resolve<IUserRepository>(), r.Resolve<IDistributedCache>()), Lifetime.Permanent);
        }

        public void Use(IApplicationBuilder app, IContainer container)
        {
            var key = new RsaSecurityKey(GenerateRsaKeys());
            var signingCredentials = new SigningCredentials(key, JwtAlgorithms.RSA_SHA256, JwtAlgorithms.RSA_SHA256);
           
            // Token Consumer:
            //
            app.UseOAuthBearerAuthentication(options =>
            {
                var handler = new ErrorHandlingTokenValidator(new JwtSecurityTokenHandler());
                var handlers = new List<ISecurityTokenValidator> { handler };

                options.SecurityTokenValidators = handlers;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = key,
                    ValidAudience = _audience,
                    ValidIssuer = _issuer,
                };
            });

            // Token Issuer:
            //
            app.UseMiddleware<OAuthServerMiddleware>(signingCredentials, _audience, _issuer, _tokenPath);
        }
        
        private static RSAParameters GenerateRsaKeys()
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var keys = rsa.ExportParameters(true);
            return keys;
        }
    }
}