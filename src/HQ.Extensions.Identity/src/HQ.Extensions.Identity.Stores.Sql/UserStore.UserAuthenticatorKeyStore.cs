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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Extensions.Identity.Stores.Sql.Models;
using HQ.Lingo.Queries;
using Microsoft.AspNetCore.Identity;

namespace HQ.Extensions.Identity.Stores.Sql
{
    partial class UserStore<TUser, TKey, TRole> : IUserAuthenticatorKeyStore<TUser>
    {
        public virtual async Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.ConcurrencyStamp = user.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            _connection.SetTypeInfo(typeof(AspNetUserTokens<TKey>));

            var exists = SqlBuilder.Select<AspNetUserTokens<TKey>>(new
            {
                TenantId = _tenantId,
                UserId = user.Id,
                Name = "AuthenticatorKey",
                LoginProvider = "[AspNetUserStore]"
            });

            var token = await _connection.Current.QuerySingleOrDefaultAsync<AspNetUserTokens<TKey>>(exists.Sql,
                exists.Parameters);

            if (string.IsNullOrWhiteSpace(token.Value))
            {
                token = new AspNetUserTokens<TKey>
                {
                    TenantId = _tenantId,
                    UserId = user.Id,
                    LoginProvider = "[AspNetUserStore]",
                    Name = "AuthenticatorKey",
                    Value = key
                };
                var query = SqlBuilder.Insert(token);
                await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            }
            else
            {
                var query = SqlBuilder.Update(token, new {UserId = user.Id, TenantId = _tenantId });
                await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            }
        }

        public async Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<AspNetUserTokens<TKey>>(new
            {
                TenantId = _tenantId,
                UserId = user.Id,
                Name = "AuthenticatorKey",
                LoginProvider = "[AspNetUserStore]"
            });
            _connection.SetTypeInfo(typeof(AspNetUserTokens<TKey>));

            var token = await _connection.Current.QuerySingleOrDefaultAsync<AspNetUserTokens<TKey>>(query.Sql,
                query.Parameters);

            return token.Value;
        }
    }
}
