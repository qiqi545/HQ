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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class UserStore<TUser, TKey, TRole> : IUserEmailStoreExtended<TUser>, IUserEmailStore<TUser>
    {
        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user?.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (SupportsSuperUser && normalizedEmail?.ToUpperInvariant() ==
                _security?.Value.SuperUser?.Username.ToUpperInvariant())
            {
                return CreateSuperUserInstance();
            }

            var query = SqlBuilder.Select<TUser>(new {NormalizedEmail = normalizedEmail, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));
            var user = await _connection.Current.QuerySingleOrDefaultAsync<TUser>(query.Sql, query.Parameters);
            return user;
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult<object>(null);
        }

        public async Task<IEnumerable<TUser>> FindAllByEmailAsync(string normalizedEmail,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (SupportsSuperUser && normalizedEmail?.ToUpperInvariant() ==
                _security?.Value.SuperUser?.Username.ToUpperInvariant())
            {
                return new[] {CreateSuperUserInstance()};
            }

            var query = SqlBuilder.Select<TUser>(new {NormalizedEmail = normalizedEmail});
            _connection.SetTypeInfo(typeof(TUser));
            var users = await _connection.Current.QueryAsync<TUser>(query.Sql, query.Parameters);
            return users;
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = SqlBuilder.Count<TUser>();
            _connection.SetTypeInfo(typeof(TUser));
            var count = await _connection.Current.ExecuteScalarAsync<int>(query.Sql, query.Parameters);
            return count;
        }
    }
}
