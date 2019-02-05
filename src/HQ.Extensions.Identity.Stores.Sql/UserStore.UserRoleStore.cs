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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Extensions.Identity.Stores.Sql.Models;
using HQ.Lingo.Queries;
using HQ.Tokens;
using Microsoft.AspNetCore.Identity;

namespace HQ.Extensions.Identity.Stores.Sql
{
    partial class UserStore<TUser, TKey, TRole> : IUserRoleStore<TUser>
    {
        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByNameAsync(roleName);

            if (roleId != null)
            {
                var query = SqlBuilder.Insert(new AspNetUserRoles<TKey>
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    TenantId = _tenantId
                });

                _connection.SetTypeInfo(typeof(AspNetUserRoles<TKey>));
                var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
                Debug.Assert(inserted == 1);
            }
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByNameAsync(roleName);

            if (roleId != null)
            {
                var query = SqlBuilder.Delete<AspNetUserRoles<TUser>>(new
                    {UserId = user.Id, RoleId = roleId, TenantId = _tenantId});

                _connection.SetTypeInfo(typeof(AspNetUserRoles<TKey>));
                var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
                Debug.Assert(deleted == 1);
            }
        }

        private static readonly List<string> SuperUserRoles = new List<string> { ClaimValues.SuperUser };

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user.NormalizedUserName == _security?.Value.SuperUser?.Username?.ToUpperInvariant())
                return SuperUserRoles;

            var mappingQuery = SqlBuilder.Select<AspNetUserRoles<TKey>>(new
            {
                UserId = user.Id,
                TenantId = _tenantId
            });

            // Mapping:
            _connection.SetTypeInfo(typeof(AspNetUserRoles<TKey>));
            var roleMapping =
                (await _connection.Current.QueryAsync<AspNetUserRoles<TKey>>(mappingQuery.Sql, mappingQuery.Parameters))
                .AsList();

            // Roles:
            if (roleMapping.Any())
            {
                var descriptor = SqlBuilder.GetDescriptor<TRole>();
                var roleQuery = SqlBuilder.Select<TRole>(descriptor, new { TenantId = _tenantId });
                roleQuery.Sql += $" AND {descriptor.Table}.Id IN @RoleIds";
                roleQuery.Parameters.Add("@RoleIds", roleMapping.Select(x => x.RoleId));
                _connection.SetTypeInfo(typeof(TRole));

                var roles = await _connection.Current.QueryAsync<TRole>(roleQuery.Sql, roleQuery.Parameters);
                return roles.Select(x => x.Name).ToList();
            }

            return new List<string>(0);
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var userRoles = await GetRolesAsync(user, cancellationToken);
            return userRoles.Contains(roleName);
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            const string sql = "SELECT u.* " +
                               "FROM AspNetRoles r " +
                               "INNER JOIN AspNetUserRoles AS ur ON ur.RoleId = r.Id " +
                               "INNER JOIN AspNetUsers u ON u.Id = ur.UserId " +
                               "WHERE r.NormalizedName = @NormalizedName " +
                               "AND r.TenantId = @TenantId";

            var users = await _connection.Current.QueryAsync<TUser>(sql, new
            {
                NormalizedName = roleName,
                TenantId = _tenantId
            });

            return users.AsList();
        }

        private async Task<TKey> GetRoleIdByNameAsync(string roleName)
        {
            var query = SqlBuilder.Select<TRole>(new {NormalizedName = roleName?.ToUpperInvariant(), TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));
            var role = await _connection.Current.QuerySingleOrDefaultAsync<TRole>(query.Sql, query.Parameters);
            return role.Id;
        }
    }
}
