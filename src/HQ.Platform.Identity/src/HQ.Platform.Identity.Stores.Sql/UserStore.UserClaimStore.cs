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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Common.Extensions;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Stores.Sql.Models;
using HQ.Platform.Security;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
    partial class UserStore<TUser, TKey, TRole> : IUserClaimStore<TUser>
    {
        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var claims = new List<Claim>();

            if (SupportsSuperUser && user.NormalizedUserName?.ToUpperInvariant() ==
                _security.Value.SuperUser?.Username.ToUpperInvariant())
            {
                var superUser = CreateSuperUserInstance();
                claims.TryAddClaim(_security.Value.Claims.UserIdClaim, $"{superUser.Id}");
                claims.TryAddClaim(_security.Value.Claims.UserNameClaim, superUser.UserName);
                claims.TryAddClaim(_security.Value.Claims.EmailClaim, superUser.Email, ClaimValueTypes.Email);
                claims.TryAddClaim(_security.Value.Claims.RoleClaim, ClaimValues.SuperUser, ClaimValueTypes.Email);
                return claims;
            }

            var id = user.Id == null ? string.Empty : $"{user.Id}";

            if (!_tenantId.Equals(default))
                claims.TryAddClaim(_security.Value.Claims.TenantIdClaim, $"{_tenantId}");

            if (!string.IsNullOrWhiteSpace(_tenantName))
            {
                claims.TryAddClaim(_security.Value.Claims.TenantNameClaim, _tenantName);
            }

            claims.TryAddClaim(_security.Value.Claims.UserIdClaim, id);
            claims.TryAddClaim(_security.Value.Claims.UserNameClaim, user.UserName);
            claims.TryAddClaim(_security.Value.Claims.EmailClaim, user.Email, ClaimValueTypes.Email);
            claims.AddRange(await GetUserClaimsAsync(user));

            var roleNames = await GetRolesAsync(user, cancellationToken);

            foreach (var roleName in roleNames)
            {
                var role = await _roles.FindByNameAsync(roleName);
                if (role == null)
                {
                    continue;
                }

                claims.Add(new Claim(_security.Value.Claims.RoleClaim, roleName));

                var roleClaims = await _roles.GetClaimsAsync(role);
                if (!roleClaims.Any())
                {
                    continue;
                }

                foreach (var claim in roleClaims)
                {
                    claims.Add(claim);
                }
            }

            return claims;
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                var query = SqlBuilder.Insert(new AspNetUserClaims<TKey>
                {
                    TenantId = _tenantId,
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });

                _connection.SetTypeInfo(typeof(AspNetUserClaims<TKey>));
                await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            }
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE AspNetUserClaims " +
                               "SET ClaimType = @NewClaimType, " +
                               "    ClaimValue = @NewClaimValue " +
                               "WHERE UserId = @UserId " +
                               "AND ClaimType = @ClaimType " +
                               "AND ClaimValue = @ClaimValue " +
                               "AND TenantId = @TenantId";

            await _connection.Current.ExecuteAsync(sql, new
            {
                TenantId = _tenantId,
                NewClaimType = newClaim.Type,
                NewClaimValue = newClaim.Value,
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
        }

        public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                var query = SqlBuilder.Delete<AspNetUserClaims<TKey>>(new
                    {UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value, TenantId = _tenantId});

                _connection.SetTypeInfo(typeof(AspNetUserClaims<TKey>));
                var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
                Debug.Assert(deleted == 1);
            }
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT u.* FROM AspNetUsers u" +
                               "INNER JOIN AspNetUserClaims uc ON uc.UserId = u.Id " +
                               "WHERE uc.ClaimType = @ClaimType " +
                               "AND uc.ClaimValue = @ClaimValue " +
                               "AND uc.TenantId = @TenantId";

            var users = await _connection.Current.QueryAsync<TUser>(sql, new
            {
                TenantId = _tenantId,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });

            return users.AsList();
        }

        private async Task<IList<Claim>> GetUserClaimsAsync(TUser user)
        {
            var query = SqlBuilder.Select<AspNetUserClaims<TKey>>(new {UserId = user.Id, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(AspNetUserClaims<TKey>));

            var claims = await _connection.Current.QueryAsync<AspNetUserClaims<TKey>>(query.Sql, query.Parameters);
            return claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
        }
    }
}
