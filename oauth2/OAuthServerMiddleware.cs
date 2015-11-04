using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using gadfly.auth;
using Microsoft.AspNet.Authentication.OAuthBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

namespace oauth2
{
    /// <summary>
    /// Implements OAuth 2.0 Resource Password Grant Flow
    /// </summary>
    public class OAuthServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SigningCredentials _signingCredentials;
        private readonly string _audience;
        private readonly string _issuer;
        private readonly PathString _path;

        public OAuthServerMiddleware(RequestDelegate next, 
            SigningCredentials signingCredentials, 
            string audience, 
            string issuer,
            string path)
        {
            _next = next;
            _signingCredentials = signingCredentials;
            _audience = audience;
            _issuer = issuer;
            _path = new PathString(path);
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals(_path))
            {
                // Preflight:
                //
                {
                    // http://tools.ietf.org/html/draft-ietf-oauth-v2-22#section-4.3
                    if (!context.Request.HasFormContentType ||
                        !context.Request.Form.ContainsKey("username") ||
                        !context.Request.Form.ContainsKey("password") ||
                        !context.Request.Form.ContainsKey("grant_type"))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return;
                    }
                    string grantType = context.Request.Form["grant_type"];
                    if (grantType.ToLower() != "password")
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                        return;
                    }
                }

                // Validate Credentials:
                //
                var username = context.Request.Form["username"];
                var password = context.Request.Form["password"];
                var users = context.ApplicationServices.GetRequiredService<UserService>();
                var user = await users.GetValidatedAsync(username, password);
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                // Produce Claims (claims will now exist in distributed cache during user's token-based session):
                //
                //var roles = await users.GetRolesAsync(user);
                //var claims = new List<Claim>(new Claim[] { new Claim(ClaimTypes.Name, user.UserName) });
                //foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
                //foreach (var c in user.Claims) claims.Add(new Claim(c.ClaimType, c.ClaimValue));
                //var identity = new ClaimsIdentity(claims, authScheme);
                
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, "bob") };
                var identity = new ClaimsIdentity(claims, OAuthBearerAuthenticationDefaults.AuthenticationScheme);

                // Generate Bearer Token:
                //
                {
                    var audience = _audience ?? "myself";
                    var issuer = _issuer ?? "myself";

                    var now = DateTime.UtcNow;
                    var expiration = now.AddMinutes(20);
                    var handler = new JwtSecurityTokenHandler();
                    var secToken = handler.CreateToken(audience, issuer, identity, now, expiration, _signingCredentials);
                    var token = handler.WriteToken(secToken);

                    // http://tools.ietf.org/html/draft-ietf-oauth-v2-22#section-7.1
                    const string tokenType = "bearer";
                    var expiresIn = (expiration - now).TotalSeconds;
                    await context.Response.WriteAsync($"{{\"token_type\":\"{tokenType}\",\"access_token\":\"{token}\",\"expires_in\":\"{expiresIn}\"}}");
                }
            }

            await _next.Invoke(context);
        }
    }
}