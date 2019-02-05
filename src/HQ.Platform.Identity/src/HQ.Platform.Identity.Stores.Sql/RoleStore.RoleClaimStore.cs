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
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Stores.Sql.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class RoleStore<TKey, TRole> : IRoleClaimStore<TRole>
    {
        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<AspNetRoleClaims<TKey>>(new
            {
                TenantId = _tenantId,
                RoleId = role.Id
            });

            _connection.SetTypeInfo(typeof(AspNetRoleClaims<TKey>));
            var claims = await _connection.Current.QueryAsync<AspNetUserClaims<TKey>>(query.Sql, query.Parameters);
            return claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
        }

        public async Task<IList<Claim>> GetAllRoleClaimsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<AspNetRoleClaims<TKey>>(new
            {
                TenantId = _tenantId
            });

            _connection.SetTypeInfo(typeof(AspNetRoleClaims<TKey>));
            var claims = await _connection.Current.QueryAsync<AspNetUserClaims<TKey>>(query.Sql, query.Parameters);
            return claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
        }

        public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            role.ConcurrencyStamp = role.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var query = SqlBuilder.Insert(new AspNetRoleClaims<TKey>
            {
                TenantId = _tenantId,
                RoleId = role.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
            _connection.SetTypeInfo(typeof(AspNetRoleClaims<TKey>));

            var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            Debug.Assert(inserted == 1);
        }

        public async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Delete<AspNetRoleClaims<TKey>>(new
            {
                TenantId = _tenantId,
                RoleId = role.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
            _connection.SetTypeInfo(typeof(TRole));

            var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            Debug.Assert(deleted == 1);
        }
    }
}
